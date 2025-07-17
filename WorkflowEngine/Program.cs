using WorkflowEngine.Models;
using WorkflowEngine.Persistence;
using WorkflowEngine.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register our services
builder.Services.AddSingleton<InMemoryStore>();
builder.Services.AddScoped<WorkflowDefinitionService>();
builder.Services.AddScoped<WorkflowInstanceService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Workflow Definition Endpoints
app.MapPost("/workflows", async (WorkflowDefinition definition, WorkflowDefinitionService service) =>
{
    var result = service.CreateDefinition(definition);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

app.MapGet("/workflows", async (WorkflowDefinitionService service) =>
{
    var result = service.GetAllDefinitions();
    return Results.Ok(result);
});

app.MapGet("/workflows/{id}", async (string id, WorkflowDefinitionService service) =>
{
    var result = service.GetDefinition(id);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
});

// Workflow Instance Endpoints
app.MapPost("/instances", async (StartInstanceRequest request, WorkflowInstanceService service) =>
{
    var result = service.StartInstance(request);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

app.MapGet("/instances", async (WorkflowInstanceService service) =>
{
    var result = service.GetAllInstances();
    return Results.Ok(result);
});

app.MapGet("/instances/{id}", async (string id, WorkflowInstanceService service) =>
{
    var result = service.GetInstance(id);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
});

app.MapPost("/instances/{id}/actions", async (string id, ExecuteActionRequest request, WorkflowInstanceService service) =>
{
    var result = service.ExecuteAction(id, request);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

app.Run();
