using WorkflowEngine.Models;
using System.Collections.Concurrent;

namespace WorkflowEngine.Persistence;

public class InMemoryStore
{
    private readonly ConcurrentDictionary<string, WorkflowDefinition> _definitions = new();
    private readonly ConcurrentDictionary<string, WorkflowInstance> _instances = new();

    // Workflow Definitions
    public void AddDefinition(WorkflowDefinition definition)
    {
        _definitions[definition.Id] = definition;
    }

    public WorkflowDefinition? GetDefinition(string id)
    {
        return _definitions.TryGetValue(id, out var definition) ? definition : null;
    }

    public List<WorkflowDefinition> GetAllDefinitions()
    {
        return _definitions.Values.ToList();
    }

    public bool DefinitionExists(string id)
    {
        return _definitions.ContainsKey(id);
    }

    // Workflow Instances
    public void AddInstance(WorkflowInstance instance)
    {
        _instances[instance.Id] = instance;
    }

    public WorkflowInstance? GetInstance(string id)
    {
        return _instances.TryGetValue(id, out var instance) ? instance : null;
    }

    public List<WorkflowInstance> GetAllInstances()
    {
        return _instances.Values.ToList();
    }

    public void UpdateInstance(WorkflowInstance instance)
    {
        _instances[instance.Id] = instance;
    }
}
