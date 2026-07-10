using ClassAssignmentSystem.Application.Common.Results;
using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Exceptions;
using ClassAssignmentSystem.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassAssignmentSystem.Application.Features.Admin.Commands
{

    public record AssignTeacherCommand(Guid CourseId, Guid TeacherId) : IRequest<Result>;

    public class AssignTeacherHandler : IRequestHandler<AssignTeacherCommand, Result>
    {
        private readonly ICourseRepository _courses;
        private readonly IUserRepository _users;

        public AssignTeacherHandler(ICourseRepository courses, IUserRepository users)
        {
            _courses = courses;
            _users = users;
        }

        public async Task<Result> Handle(AssignTeacherCommand request, CancellationToken cancellationToken)
        {
            var course = await _courses.GetByIdAsync(request.CourseId, cancellationToken)
                ?? throw new NotFoundException(nameof(Course), request.CourseId);

            var teacher = await _users.GetByIdAsync(request.TeacherId, cancellationToken)
                ?? throw new NotFoundException(nameof(User), request.TeacherId);

            if (teacher.Role != Domain.Enums.UserRole.Teacher)
                return Result.Failure(Error.Validation("Failed Validation","The specified user is not a Teacher."));

            course.AssignTeacher(request.TeacherId);
            await _courses.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
