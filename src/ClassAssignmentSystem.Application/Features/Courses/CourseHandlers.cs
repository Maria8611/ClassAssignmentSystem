using ClassAssignmentSystem.Application.Common.Results;
using ClassAssignmentSystem.Application.Configurations;
using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;

namespace ClassAssignmentSystem.Application.Features.Courses;

public record GetAllCoursesQuery : IRequest<IReadOnlyList<CourseDto>>;
public class GetAllCoursesHandler : IRequestHandler<GetAllCoursesQuery, IReadOnlyList<CourseDto>>
{
    private readonly ICourseRepository _courses;
    private readonly IEnrollmentRequestRepository _enrollments;
    private readonly HybridCache _hybridCache;

    public GetAllCoursesHandler(ICourseRepository courses, IEnrollmentRequestRepository enrollments, HybridCache hybridCache)
    { _courses = courses; _enrollments = enrollments; _hybridCache = hybridCache; }
    public async Task<IReadOnlyList<CourseDto>> Handle(GetAllCoursesQuery request, CancellationToken ct)
    {
        string cacheKey = "courses:all-with-counts";
        List<CourseDto> cachedResult = await _hybridCache.GetOrCreateAsync(
         cacheKey,
         async token =>
         {
             var courses = await _courses.GetAllAsync(token);
             var result = new List<CourseDto>();

             foreach (var c in courses)
             {
                 token.ThrowIfCancellationRequested();
                 var count = await _enrollments.GetApprovedCountByCourseAsync(c.Id, token);
                 result.Add(c.ToDto(count));
             }
             return result;
         },
        cancellationToken: ct 
    );

        return cachedResult;
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
