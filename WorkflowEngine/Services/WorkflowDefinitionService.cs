using WorkflowEngine.Models;
using WorkflowEngine.Persistence;

namespace WorkflowEngine.Services;

public class WorkflowDefinitionService
{
    private readonly InMemoryStore _store;

    public WorkflowDefinitionService(InMemoryStore store)
    {
        _store = store;
    }

    public ApiResponse<WorkflowDefinition> CreateDefinition(WorkflowDefinition definition)
    {
        var validationErrors = ValidateDefinition(definition);
        if (validationErrors.Any())
        {
            return new ApiResponse<WorkflowDefinition>
            {
                Success = false,
                ValidationErrors = validationErrors
            };
        }

        if (_store.DefinitionExists(definition.Id))
        {
            return new ApiResponse<WorkflowDefinition>
            {
                Success = false,
                Error = $"Workflow definition with ID '{definition.Id}' already exists"
            };
        }

        _store.AddDefinition(definition);
        return new ApiResponse<WorkflowDefinition>
        {
            Success = true,
            Data = definition
        };
    }

    public ApiResponse<WorkflowDefinition> GetDefinition(string id)
    {
        var definition = _store.GetDefinition(id);
        if (definition == null)
        {
            return new ApiResponse<WorkflowDefinition>
            {
                Success = false,
                Error = $"Workflow definition with ID '{id}' not found"
            };
        }

        return new ApiResponse<WorkflowDefinition>
        {
            Success = true,
            Data = definition
        };
    }

    public ApiResponse<List<WorkflowDefinition>> GetAllDefinitions()
    {
        return new ApiResponse<List<WorkflowDefinition>>
        {
            Success = true,
            Data = _store.GetAllDefinitions()
        };
    }

    private List<string> ValidateDefinition(WorkflowDefinition definition)
    {
        var errors = new List<string>();

        // Check for duplicate state IDs
        var stateIds = definition.States.Select(s => s.Id).ToList();
        if (stateIds.Count != stateIds.Distinct().Count())
        {
            errors.Add("Duplicate state IDs found");
        }

        // Check for duplicate action IDs
        var actionIds = definition.Actions.Select(a => a.Id).ToList();
        if (actionIds.Count != actionIds.Distinct().Count())
        {
            errors.Add("Duplicate action IDs found");
        }

        // Check for exactly one initial state
        var initialStates = definition.States.Where(s => s.IsInitial).ToList();
        if (initialStates.Count != 1)
        {
            errors.Add("Must have exactly one initial state");
        }

        // Check that all actions reference valid states
        foreach (var action in definition.Actions)
        {
            if (!stateIds.Contains(action.ToState))
            {
                errors.Add($"Action '{action.Id}' references unknown target state '{action.ToState}'");
            }

            foreach (var fromState in action.FromStates)
            {
                if (!stateIds.Contains(fromState))
                {
                    errors.Add($"Action '{action.Id}' references unknown source state '{fromState}'");
                }
            }
        }

        return errors;
    }
}
