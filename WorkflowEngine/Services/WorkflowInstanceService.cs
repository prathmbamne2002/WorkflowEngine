using WorkflowEngine.Models;
using WorkflowEngine.Persistence;

namespace WorkflowEngine.Services;

public class WorkflowInstanceService
{
    private readonly InMemoryStore _store;

    public WorkflowInstanceService(InMemoryStore store)
    {
        _store = store;
    }

    public ApiResponse<WorkflowInstance> StartInstance(StartInstanceRequest request)
    {
        var definition = _store.GetDefinition(request.DefinitionId);
        if (definition == null)
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = $"Workflow definition with ID '{request.DefinitionId}' not found"
            };
        }

        var initialState = definition.States.FirstOrDefault(s => s.IsInitial);
        if (initialState == null)
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = "No initial state found in workflow definition"
            };
        }

        var instance = new WorkflowInstance
        {
            DefinitionId = request.DefinitionId,
            CurrentStateId = initialState.Id
        };

        _store.AddInstance(instance);
        return new ApiResponse<WorkflowInstance>
        {
            Success = true,
            Data = instance
        };
    }

    public ApiResponse<WorkflowInstance> GetInstance(string id)
    {
        var instance = _store.GetInstance(id);
        if (instance == null)
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = $"Workflow instance with ID '{id}' not found"
            };
        }

        return new ApiResponse<WorkflowInstance>
        {
            Success = true,
            Data = instance
        };
    }

    public ApiResponse<List<WorkflowInstance>> GetAllInstances()
    {
        return new ApiResponse<List<WorkflowInstance>>
        {
            Success = true,
            Data = _store.GetAllInstances()
        };
    }

    public ApiResponse<WorkflowInstance> ExecuteAction(string instanceId, ExecuteActionRequest request)
    {
        var instance = _store.GetInstance(instanceId);
        if (instance == null)
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = $"Workflow instance with ID '{instanceId}' not found"
            };
        }

        var definition = _store.GetDefinition(instance.DefinitionId);
        if (definition == null)
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = "Workflow definition not found"
            };
        }

        var currentState = definition.States.FirstOrDefault(s => s.Id == instance.CurrentStateId);
        if (currentState == null)
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = "Current state not found in definition"
            };
        }

        if (currentState.IsFinal)
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = "Cannot execute actions on workflow instance in final state"
            };
        }

        var action = definition.Actions.FirstOrDefault(a => a.Id == request.ActionId);
        if (action == null)
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = $"Action '{request.ActionId}' not found in workflow definition"
            };
        }

        if (!action.Enabled)
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = $"Action '{request.ActionId}' is disabled"
            };
        }

        if (!action.FromStates.Contains(instance.CurrentStateId))
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = $"Action '{request.ActionId}' cannot be executed from current state '{instance.CurrentStateId}'"
            };
        }

        var targetState = definition.States.FirstOrDefault(s => s.Id == action.ToState);
        if (targetState == null)
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = $"Target state '{action.ToState}' not found"
            };
        }

        if (!targetState.Enabled)
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = $"Target state '{action.ToState}' is disabled"
            };
        }

        // Execute the action
        var historyEntry = new ActionHistory
        {
            ActionId = request.ActionId,
            FromState = instance.CurrentStateId,
            ToState = action.ToState,
            Timestamp = DateTime.UtcNow
        };

        instance.History.Add(historyEntry);
        instance.CurrentStateId = action.ToState;

        _store.UpdateInstance(instance);

        return new ApiResponse<WorkflowInstance>
        {
            Success = true,
            Data = instance
        };
    }
}
