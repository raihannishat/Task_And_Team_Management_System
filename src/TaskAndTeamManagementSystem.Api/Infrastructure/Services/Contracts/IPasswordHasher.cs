namespace TaskAndTeamManagementSystem.Api.Infrastructure.Services.Contracts;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

