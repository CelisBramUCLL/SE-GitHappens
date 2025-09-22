namespace Backend.Models;

public class User
{
    public string Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string PasswordHash { get; }
    public DateTime BirthDate { get; }
    public string Username { get; }
    public string Role { get; }

    public User(
        string id,
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        DateTime birthDate,
        string username,
        string role
    )
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        BirthDate = birthDate;
        Username = username;
        Role = role;
    }
}
