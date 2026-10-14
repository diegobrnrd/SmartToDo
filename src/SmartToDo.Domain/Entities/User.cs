using SmartToDo.Domain.Exceptions;

namespace SmartToDo.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    private readonly List<TodoTask> _tasks = new();
    public IReadOnlyCollection<TodoTask> Tasks => _tasks.AsReadOnly();

    // Construtor vazio protegido exigido pelo Entity Framework Core
    protected User() { }

    public User(string name, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome do usuário é obrigatório.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("Um e-mail válido é obrigatório.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("O hash da senha é obrigatório.");

        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
    }
}