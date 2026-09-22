using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace ClassAssignmentSystem.IntegrationTests.API;

public class EnrollmentFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public EnrollmentFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task EnrollmentFlow_StudentRequestsAndTeacherApproves_Succeeds()
    {
        using var adminClient = await CreateAuthenticatedClientAsync("admin@classassign.local", "Admin@123456");

        var teacherResponse = await adminClient.PostAsJsonAsync("/api/admin/teachers",
            new RegisterTeacherDto("Flow Teacher", $"teacher-{Guid.NewGuid():N}@test.com", "TeacherPass123!"));
        teacherResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var teacher = await teacherResponse.Content.ReadFromJsonAsync<UserDto>();

        var studentResponse = await adminClient.PostAsJsonAsync("/api/admin/students",
            new RegisterStudentDto("Flow Student", $"student-{Guid.NewGuid():N}@test.com", "StudentPass123!"));
        studentResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var student = await studentResponse.Content.ReadFromJsonAsync<UserDto>();

        var courseResponse = await adminClient.PostAsJsonAsync("/api/admin/courses",
            new CreateCourseDto("Integration Course", "Enrollment flow test", 2));
        courseResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var course = await courseResponse.Content.ReadFromJsonAsync<CourseDto>();

        var assignResponse = await adminClient.PutAsJsonAsync(
            $"/api/admin/courses/{course!.Id}/teacher",
            new AssignTeacherDto(teacher!.Id));
        assignResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var studentClient = await CreateAuthenticatedClientAsync(student!.Email, "StudentPass123!");
        var requestResponse = await studentClient.PostAsync($"/api/enrollments/courses/{course.Id}/request", null);
        requestResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var teacherClient = await CreateAuthenticatedClientAsync(teacher.Email, "TeacherPass123!");
        var enrollments = await teacherClient.GetFromJsonAsync<List<EnrollmentRequestDto>>("/api/enrollments/teacher?status=Pending");
        enrollments.Should().NotBeNull().And.NotBeEmpty();

        var enrollmentId = enrollments!.First().Id;
        var approveResponse = await teacherClient.PutAsync($"/api/enrollments/{enrollmentId}/approve", null);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/enrollments/my");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email, string password)
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginDto(email, password));
        loginResponse.EnsureSuccessStatusCode();
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }
}
