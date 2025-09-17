using Backend.Models;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this IEndpointRouteBuilder routes, string basePath)
    {
        var group = routes.MapGroup("/todos");

        group.MapGet("/", (List<Todo> todos) => todos);

        group.MapPost("/", (string description, List<Todo> todos) =>
        {
            var todo = new Todo(Guid.NewGuid().ToString(), description);
            todos.Add(todo);
            return Results.Created($"{basePath}/todos/{todo.Id}", todo);
        });

        group.MapGet("/{id}", (string id, List<Todo> todos) =>
        {
            var todo = todos.FirstOrDefault(t => t.Id == id);
            return todo is not null ? Results.Ok(todo) : Results.NotFound();
        });
    }
}