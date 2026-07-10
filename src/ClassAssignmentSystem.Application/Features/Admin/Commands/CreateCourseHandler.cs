using ClassAssignmentSystem.Application.Common.Results;
using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassAssignmentSystem.Application.Features.Admin.Commands
{

    public record CreateCourseCommand(CreateCourseDto Dto) : IRequest<Result<CourseDto>>;

    public class CreateCourseHandler : IRequestHandler<CreateCourseCommand, Result<CourseDto>>
    {
        private readonly ICourseRepository _courses;

        public CreateCourseHandler(ICourseRepository courses) => _courses = courses;

        public async Task<Result<CourseDto>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            var course = Course.Create(request.Dto.Title, request.Dto.Description, request.Dto.TotalSeats);
            await _courses.AddAsync(course, cancellationToken);
            await _courses.SaveChangesAsync(cancellationToken);

            return Result<CourseDto>.Success(course.ToDto(approvedCount: 0));
        }
    }
}
