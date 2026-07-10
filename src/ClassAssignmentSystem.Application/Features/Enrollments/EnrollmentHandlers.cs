using ClassAssignmentSystem.Application.Common.Results;
using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Application.Features;
using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Enums;
using ClassAssignmentSystem.Domain.Exceptions;
using ClassAssignmentSystem.Domain.Repositories;
using MediatR;

namespace ClassAssignmentSystem.Application.Features.Enrollments;

// Submit Enrollment Request (Student)
public record SubmitEnrollmentRequestCommand(Guid StudentId, Guid CourseId) : IRequest<Result<EnrollmentRequestDto>>;
public class SubmitEnrollmentRequestHandler : IRequestHandler<SubmitEnrollmentRequestCommand, Result<EnrollmentRequestDto>>
{
    private readonly ICourseRepository _courses;
    private readonly IEnrollmentRequestRepository _enrollments;
    private readonly IUserRepository _users;
    public SubmitEnrollmentRequestHandler(ICourseRepository courses, IEnrollmentRequestRepository enrollments, IUserRepository users)
    { _courses = courses; _enrollments = enrollments; _users = users; }

    public async Task<Result<EnrollmentRequestDto>> Handle(SubmitEnrollmentRequestCommand request, CancellationToken ct)
    {
        var course = await _courses.GetByIdAsync(request.CourseId, ct)
            ?? throw new NotFoundException(nameof(Course), request.CourseId);

        if (course.Status == CourseStatus.Inactive)
            return Result<EnrollmentRequestDto>.Failure(Error.Validation("InActive","This course is not currently active."));

        if (course.TeacherId is null)
            return Result<EnrollmentRequestDto>.Failure(Error.Validation("Unavailable","This course has no assigned teacher. Enrollment is not available yet."));

        var existing = await _enrollments.GetPendingByStudentAndCourseAsync(request.StudentId, request.CourseId, ct);
        if (existing is not null)
            return Result<EnrollmentRequestDto>.Failure(Error.Conflict("Duplicate Request","You already have a pending enrollment request for this course."));

        var approvedCount = await _enrollments.GetApprovedCountByCourseAsync(request.CourseId, ct);
        if (!course.HasAvailableSeats(approvedCount))
            return Result<EnrollmentRequestDto>.Failure(Error.Validation("MaxLimitExceeded",$"No seats available. Course is at capacity ({course.TotalSeats} seats)."));

        var er = EnrollmentRequest.Create(request.CourseId, request.StudentId);
        await _enrollments.AddAsync(er, ct);
        await _enrollments.SaveChangesAsync(ct);

        var student = await _users.GetByIdAsync(request.StudentId, ct);
        return Result<EnrollmentRequestDto>.Success(er.ToDto(course.Title, student?.FullName ?? ""));
    }
}

// Approve Enrollment (Teacher)
public record ApproveEnrollmentCommand(Guid TeacherId, Guid EnrollmentRequestId) : IRequest<Result>;
public class ApproveEnrollmentHandler : IRequestHandler<ApproveEnrollmentCommand, Result>
{
    private readonly IEnrollmentRequestRepository _enrollments;
    private readonly ICourseRepository _courses;
    public ApproveEnrollmentHandler(IEnrollmentRequestRepository enrollments, ICourseRepository courses)
    { _enrollments = enrollments; _courses = courses; }

    public async Task<Result> Handle(ApproveEnrollmentCommand request, CancellationToken ct)
    {
        var er = await _enrollments.GetByIdAsync(request.EnrollmentRequestId, ct)
            ?? throw new NotFoundException(nameof(EnrollmentRequest), request.EnrollmentRequestId);
        var course = await _courses.GetByIdAsync(er.CourseId, ct)
            ?? throw new NotFoundException(nameof(Course), er.CourseId);
        if (course.TeacherId != request.TeacherId)
            return Result.Failure(Error.Forbidden("","You are not the assigned teacher for this course."));
        var approvedCount = await _enrollments.GetApprovedCountByCourseAsync(course.Id, ct);
        if (!course.HasAvailableSeats(approvedCount))
            return Result.Failure(Error.Failure("MaxLimitExceeded","Cannot approve: course has reached its seat capacity."));
        er.Approve(request.TeacherId);
        await _enrollments.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// Reject Enrollment (Teacher)
public record RejectEnrollmentCommand(Guid TeacherId, Guid EnrollmentRequestId, string? Reason) : IRequest<Result>;
public class RejectEnrollmentHandler : IRequestHandler<RejectEnrollmentCommand, Result>
{
    private readonly IEnrollmentRequestRepository _enrollments;
    private readonly ICourseRepository _courses;
    public RejectEnrollmentHandler(IEnrollmentRequestRepository enrollments, ICourseRepository courses)
    { _enrollments = enrollments; _courses = courses; }

    public async Task<Result> Handle(RejectEnrollmentCommand request, CancellationToken ct)
    {
        var er = await _enrollments.GetByIdAsync(request.EnrollmentRequestId, ct)
            ?? throw new NotFoundException(nameof(EnrollmentRequest), request.EnrollmentRequestId);
        var course = await _courses.GetByIdAsync(er.CourseId, ct)
            ?? throw new NotFoundException(nameof(Course), er.CourseId);
        if (course.TeacherId != request.TeacherId)
            return Result.Failure(Error.Forbidden("","You are not the assigned teacher for this course."));
        er.Reject(request.TeacherId, request.Reason);
        await _enrollments.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// Get Enrollments for Teacher's Courses
public record GetEnrollmentsByTeacherQuery(Guid TeacherId, EnrollmentStatus? StatusFilter = null) : IRequest<IReadOnlyList<EnrollmentRequestDto>>;
public class GetEnrollmentsByTeacherHandler : IRequestHandler<GetEnrollmentsByTeacherQuery, IReadOnlyList<EnrollmentRequestDto>>
{
    private readonly ICourseRepository _courses;
    private readonly IEnrollmentRequestRepository _enrollments;
    private readonly IUserRepository _users;
    public GetEnrollmentsByTeacherHandler(ICourseRepository courses, IEnrollmentRequestRepository enrollments, IUserRepository users)
    { _courses = courses; _enrollments = enrollments; _users = users; }

    public async Task<IReadOnlyList<EnrollmentRequestDto>> Handle(GetEnrollmentsByTeacherQuery request, CancellationToken ct)
    {
        var teacherCourses = await _courses.GetByTeacherIdAsync(request.TeacherId, ct);
        var result = new List<EnrollmentRequestDto>();
        foreach (var course in teacherCourses)
        {
            var requests = await _enrollments.GetByCourseIdAsync(course.Id, ct);
            var filtered = request.StatusFilter.HasValue ? requests.Where(r => r.Status == request.StatusFilter.Value) : requests;
            foreach (var er in filtered)
            {
                var student = await _users.GetByIdAsync(er.StudentId, ct);
                result.Add(er.ToDto(course.Title, student?.FullName ?? ""));
            }
        }
        return result;
    }
}

// Get Student's Own Enrollments
public record GetMyEnrollmentsQuery(Guid StudentId) : IRequest<IReadOnlyList<EnrollmentRequestDto>>;
public class GetMyEnrollmentsHandler : IRequestHandler<GetMyEnrollmentsQuery, IReadOnlyList<EnrollmentRequestDto>>
{
    private readonly IEnrollmentRequestRepository _enrollments;
    private readonly ICourseRepository _courses;
    private readonly IUserRepository _users;
    public GetMyEnrollmentsHandler(IEnrollmentRequestRepository enrollments, ICourseRepository courses, IUserRepository users)
    { _enrollments = enrollments; _courses = courses; _users = users; }

    public async Task<IReadOnlyList<EnrollmentRequestDto>> Handle(GetMyEnrollmentsQuery request, CancellationToken ct)
    {
        var requests = await _enrollments.GetByStudentIdAsync(request.StudentId, ct);
        var student = await _users.GetByIdAsync(request.StudentId, ct);
        var result = new List<EnrollmentRequestDto>();
        foreach (var er in requests)
        {
            var course = await _courses.GetByIdAsync(er.CourseId, ct);
            result.Add(er.ToDto(course?.Title ?? "", student?.FullName ?? ""));
        }
        return result;
    }
}
