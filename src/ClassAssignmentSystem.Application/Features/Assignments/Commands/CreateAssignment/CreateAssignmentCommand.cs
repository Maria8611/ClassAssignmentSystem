using ClassAssignmentSystem.Application.Common.Interfaces;
using ClassAssignmentSystem.Application.Common.Results;
using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassAssignmentSystem.Application.Features.Assignments.Commands.CreateAssignment
{
    // ─── Create Assignment (Teacher) ──────────────────────────────────────────────

    public record CreateAssignmentCommand(Guid TeacherId, CreateAssignmentDto Dto) : IRequest<Result<AssignmentDto>>;

    public class CreateAssignmentHandler : IRequestHandler<CreateAssignmentCommand, Result<AssignmentDto>>
    {
        private readonly IAssignmentRepository _assignments;
        private readonly ICourseRepository _courses;

        public CreateAssignmentHandler(IAssignmentRepository assignments, ICourseRepository courses)
        { _assignments = assignments; _courses = courses; }

        public async Task<Result<AssignmentDto>> Handle(CreateAssignmentCommand request, CancellationToken ct)
        {
            var course = await _courses.GetByIdAsync(request.Dto.CourseId, ct)
                ?? throw new NotFoundException(nameof(Course), request.Dto.CourseId);

            // Teacher ownership check
            if (course.TeacherId != request.TeacherId)
                return Result<AssignmentDto>.Failure(Error.Forbidden("Forbidden Access", "You can only create assignments for courses assigned to you."));

            var assignment = Assignment.Create(
                request.Dto.Title, request.Dto.Description,
                request.Dto.Deadline, request.Dto.MaxMarks,
                request.Dto.CourseId, request.TeacherId);

            await _assignments.AddAsync(assignment, ct);
            await _assignments.SaveChangesAsync(ct);

            return Result<AssignmentDto>.Success(assignment.ToDto(course.Title, course.Teacher?.FullName ?? ""));
        }
    }

    // ─── Update Assignment (Teacher) ──────────────────────────────────────────────

    public record UpdateAssignmentCommand(Guid TeacherId, Guid AssignmentId, UpdateAssignmentDto Dto) : IRequest<Result<AssignmentDto>>;

    public class UpdateAssignmentHandler : IRequestHandler<UpdateAssignmentCommand, Result<AssignmentDto>>
    {
        private readonly IAssignmentRepository _assignments;
        private readonly ICourseRepository _courses;

        public UpdateAssignmentHandler(IAssignmentRepository assignments, ICourseRepository courses)
        { _assignments = assignments; _courses = courses; }

        public async Task<Result<AssignmentDto>> Handle(UpdateAssignmentCommand request, CancellationToken ct)
        {
            var assignment = await _assignments.GetByIdAsync(request.AssignmentId, ct)
                ?? throw new NotFoundException(nameof(Assignment), request.AssignmentId);

            var course = await _courses.GetByIdAsync(assignment.CourseId, ct)
                ?? throw new NotFoundException(nameof(Course), assignment.CourseId);

            // Domain enforces ownership — throws DomainException if wrong teacher
            assignment.Update(
                request.Dto.Title, request.Dto.Description,
                request.Dto.Deadline, request.Dto.MaxMarks,
                request.TeacherId);

            await _assignments.SaveChangesAsync(ct);

            return Result<AssignmentDto>.Success(assignment.ToDto(course.Title, course.Teacher?.FullName ?? ""));
        }
    }

    // ─── Get Assignments for a Course ─────────────────────────────────────────────

    public record GetAssignmentsByCourseQuery(Guid CourseId) : IRequest<IReadOnlyList<AssignmentDto>>;

    public class GetAssignmentsByCourseHandler : IRequestHandler<GetAssignmentsByCourseQuery, IReadOnlyList<AssignmentDto>>
    {
        private readonly IAssignmentRepository _assignments;
        private readonly ICourseRepository _courses;

        public GetAssignmentsByCourseHandler(IAssignmentRepository assignments, ICourseRepository courses)
        { _assignments = assignments; _courses = courses; }

        public async Task<IReadOnlyList<AssignmentDto>> Handle(GetAssignmentsByCourseQuery request, CancellationToken ct)
        {
            var course = await _courses.GetByIdAsync(request.CourseId, ct)
                ?? throw new NotFoundException(nameof(Course), request.CourseId);

            var assignments = await _assignments.GetByCourseIdAsync(request.CourseId, ct);
            return assignments.Select(a => a.ToDto(course.Title, course.Teacher?.FullName ?? "")).ToList();
        }
    }

    // ─── Get Assignment By Id ─────────────────────────────────────────────────────

    public record GetAssignmentByIdQuery(Guid AssignmentId) : IRequest<AssignmentDto?>;

    public class GetAssignmentByIdHandler : IRequestHandler<GetAssignmentByIdQuery, AssignmentDto?>
    {
        private readonly IAssignmentRepository _assignments;
        private readonly ICourseRepository _courses;

        public GetAssignmentByIdHandler(IAssignmentRepository assignments, ICourseRepository courses)
        { _assignments = assignments; _courses = courses; }

        public async Task<AssignmentDto?> Handle(GetAssignmentByIdQuery request, CancellationToken ct)
        {
            var assignment = await _assignments.GetByIdAsync(request.AssignmentId, ct);
            if (assignment is null) return null;

            var course = await _courses.GetByIdAsync(assignment.CourseId, ct);
            return assignment.ToDto(course?.Title ?? "", course?.Teacher?.FullName ?? "");
        }
    }

    // ─── Get Teacher's Own Assignments ────────────────────────────────────────────

    public record GetMyAssignmentsQuery(Guid TeacherId) : IRequest<IReadOnlyList<AssignmentDto>>;

    public class GetMyAssignmentsHandler : IRequestHandler<GetMyAssignmentsQuery, IReadOnlyList<AssignmentDto>>
    {
        private readonly IAssignmentRepository _assignments;
        private readonly ICourseRepository _courses;

        public GetMyAssignmentsHandler(IAssignmentRepository assignments, ICourseRepository courses)
        { _assignments = assignments; _courses = courses; }

        public async Task<IReadOnlyList<AssignmentDto>> Handle(GetMyAssignmentsQuery request, CancellationToken ct)
        {
            var assignments = await _assignments.GetByTeacherIdAsync(request.TeacherId, ct);
            var result = new List<AssignmentDto>();
            foreach (var a in assignments)
            {
                var course = await _courses.GetByIdAsync(a.CourseId, ct);
                result.Add(a.ToDto(course?.Title ?? "", course?.Teacher?.FullName ?? ""));
            }
            return result;
        }
    }
}
