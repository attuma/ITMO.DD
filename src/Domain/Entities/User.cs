using StudentTracker.Domain.Common;
using StudentTracker.Domain.Enums;

namespace StudentTracker.Domain.Entities;

public class User : BaseEntity
{
    public string UserName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public SystemRole SystemRole { get; private set; }

    private User()
    {
        UserName = string.Empty;
        Email = string.Empty;
        PasswordHash = string.Empty;
        SystemRole = SystemRole.Student;
    }

    public User(string userName, string email, string passwordHash, SystemRole systemRole = SystemRole.Student)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name cannot be empty.", nameof(userName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        if (!Enum.IsDefined(systemRole))
            throw new ArgumentException("Invalid system role.", nameof(systemRole));

        UserName = userName;
        Email = email;
        PasswordHash = passwordHash;
        SystemRole = systemRole;
    }

    public void ChangeRole(SystemRole systemRole)
    {
        if (!Enum.IsDefined(systemRole))
            throw new ArgumentException("Invalid system role.", nameof(systemRole));

        SystemRole = systemRole;
    }
}
