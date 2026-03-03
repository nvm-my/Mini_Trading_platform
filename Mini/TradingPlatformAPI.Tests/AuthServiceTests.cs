using System.IdentityModel.Tokens.Jwt;
using TradingPlatform.Config;
using TradingPlatform.Models;
using TradingPlatform.Repositories;
using TradingPlatform.Services;
using Moq;
using MongoDB.Driver;

namespace TradingPlatformAPI.Tests;

public class AuthServiceTests
{
    // ------------------------------------------------------------------ //
    // Helpers
    // ------------------------------------------------------------------ //

    private const string TestSecretKey = "super-secret-key-that-is-at-least-32-chars";

    private static (AuthService svc, Mock<IMongoCollection<User>> mockUsers)
        BuildService()
    {
        var mockUsers = MongoTestHelper.CreateCollection<User>();
        var mockDb = MongoTestHelper.RegisterCollection<User>(null, "Users", mockUsers);
        var userRepo = new UserRepository(mockDb.Object);
        var jwtSettings = new JwtSettings { SecretKey = TestSecretKey };
        return (new AuthService(userRepo, jwtSettings), mockUsers);
    }

    // ------------------------------------------------------------------ //
    // Register
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Register_ReturnsNonEmptyJwtToken()
    {
        var (svc, _) = BuildService();

        var user = new User
        {
            Id = "user-001",
            Name = "Alice",
            Email = "alice@example.com",
            PasswordHash = "plaintext-password",
            Role = "Client",
        };

        var token = await svc.Register(user);

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Register_HashesPassword_BeforeInsert()
    {
        var (svc, mockUsers) = BuildService();

        const string plainPassword = "my-plain-password";
        var user = new User
        {
            Id = "user-002",
            Name = "Bob",
            Email = "bob@example.com",
            PasswordHash = plainPassword,
            Role = "Client",
        };

        await svc.Register(user);

        // The stored hash must not equal the original plain-text password
        Assert.NotEqual(plainPassword, user.PasswordHash);

        // BCrypt hashes start with "$2"
        Assert.StartsWith("$2", user.PasswordHash);

        // InsertOneAsync must have been called once
        mockUsers.Verify(
            c => c.InsertOneAsync(
                It.IsAny<User>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Register_GeneratedToken_ContainsRoleClaim()
    {
        var (svc, _) = BuildService();

        var user = new User
        {
            Id = "user-003",
            Name = "Carol",
            Email = "carol@example.com",
            PasswordHash = "password123",
            Role = "Admin",
        };

        var token = await svc.Register(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Contains(jwt.Claims,
            c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
              && c.Value == "Admin");
    }

    // ------------------------------------------------------------------ //
    // Login
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Login_ValidCredentials_ReturnsJwtToken()
    {
        var (svc, mockUsers) = BuildService();

        const string rawPassword = "correct-password";
        var storedUser = new User
        {
            Id = "user-004",
            Name = "Dave",
            Email = "dave@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword),
            Role = "Client",
        };

        MongoTestHelper.SetupFind(mockUsers, new[] { storedUser });

        var token = await svc.Login(storedUser.Email, rawPassword);

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsException()
    {
        var (svc, mockUsers) = BuildService();

        var storedUser = new User
        {
            Id = "user-005",
            Name = "Eve",
            Email = "eve@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-password"),
            Role = "Client",
        };

        MongoTestHelper.SetupFind(mockUsers, new[] { storedUser });

        await Assert.ThrowsAsync<Exception>(
            () => svc.Login(storedUser.Email, "wrong-password"));
    }

    [Fact]
    public async Task Login_UnknownEmail_ThrowsException()
    {
        var (svc, mockUsers) = BuildService();

        // Find returns no user (empty list)
        MongoTestHelper.SetupFind(mockUsers, Array.Empty<User>());

        await Assert.ThrowsAsync<Exception>(
            () => svc.Login("nobody@example.com", "any-password"));
    }
}
