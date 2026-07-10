using ClassAssignmentSystem.Application.Common.Results;
using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Exceptions;
using MediatR;

namespace ClassAssignmentSystem.Application.Features.Courses;

public record GetAllCoursesQuery : IRequest<IReadOnlyList<CourseDto>>;
public class GetAllCoursesHandler : IRequestHandler<GetAllCoursesQuery, IReadOnlyList<CourseDto>>
{
    private readonly ICourseRepository _courses;
    private readonly IEnrollmentRequestRepository _enrollments;
    public GetAllCoursesHandler(ICourseRepository courses, IEnrollmentRequestRepository enrollments)
    { _courses = courses; _enrollments = enrollments; }
    public async Task<IReadOnlyList<CourseDto>> Handle(GetAllCoursesQuery request, CancellationToken ct)
    {
        var courses = await _courses.GetAllAsync(ct);
        var result = new List<CourseDto>();
        foreach (var c in courses)
        {
            var count = await _enrollments.GetApprovedCountByCourseAsync(c.Id, ct);
            result.Add(c.ToDto(count));
        }
        return result;
    }
}

public record GetCourseByIdQuery(Guid CourseId) : IRequest<CourseDto?>;
public class GetCourseByIdHandler : IRequestHandler<GetCourseByIdQuery, CourseDto?>
{
    private readonly ICourseRepository _courses;
    private readonly IEnrollmentRequestRepository _enrollments;
    public GetCourseByIdHandler(ICourseRepository courses, IEnrollmentRequestRepository enrollments)
    { _courses = courses; _enrollments = enrollments; }
    public async Task<CourseDto?> Handle(GetCourseByIdQuery request, CancellationToken ct)
    {
        var course = await _courses.GetByIdAsync(request.CourseId, ct);
        if (course is null) return null;
        var count = await _enrollments.GetApprovedCountByCourseAsync(course.Id, ct);
        return course.ToDto(count);
    }
}

public record UpdateCourseCommand(Guid CourseId, UpdateCourseDto Dto) : IRequest<Result<CourseDto>>;
public class UpdateCourseHandler : IRequestHandler<UpdateCourseCommand, Result<CourseDto>>
{
    private readonly ICourseRepository _courses;
    private readonly IEnrollmentRequestRepository _enrollments;
    public UpdateCourseHandler(ICourseRepository courses, IEnrollmentRequestRepository enrollments)
    { _courses = courses; _enrollments = enrollments; }
    public async Task<Result<CourseDto>> Handle(UpdateCourseCommand request, CancellationToken ct)
    {
        var course = await _courses.GetByIdAsync(request.CourseId, ct)
            ?? throw new NotFoundException(nameof(Course), request.CourseId);
        var approvedCount = await _enrollments.GetApprovedCountByCourseAsync(course.Id, ct);
        if (request.Dto.TotalSeats < approvedCount)
            return Result<CourseDto>.Failure(Error.Validation("Limit Exceeded",$"Cannot reduce seats below current enrolled count ({approvedCount})."));
        course.Update(request.Dto.Title, request.Dto.Description, request.Dto.TotalSeats);
        await _courses.SaveChangesAsync(ct);
        return Result<CourseDto>.Success(course.ToDto(approvedCount));
    }
}
