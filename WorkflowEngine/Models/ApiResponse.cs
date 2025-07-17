namespace WorkflowEngine.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
}

public class ExecuteActionRequest
{
    public string ActionId { get; set; } = string.Empty;
}

public class StartInstanceRequest
{
    public string DefinitionId { get; set; } = string.Empty;
}
