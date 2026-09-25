using System.Net;
using System.Net.Http.Json;
using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace ClassAssignmentSystem.IntegrationTests.API;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithSeededAdminCredentials_ReturnsOkAndToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto(
            "admin@classassign.local",
            "Admin@123456"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body.Should().NotBeNull();
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto(
            "admin@classassign.local",
            "WrongPassword123"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
