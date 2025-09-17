namespace Backend.Models;

public class Todo
{
    public string Id { get; }
    public string Description { get; }

    public Todo(string id, string description)
    {
        Id = id;
        Description = description;
    }
}