using ClassAssignmentSystem.Application.Common.Results;
using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Application.Interfaces;
using ClassAssignmentSystem.Domain.Entities;
using ClassAssignmentSystem.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassAssignmentSystem.Application.Features.Admin.Commands
{
    public record RegisterTeacherCommand(RegisterTeacherDto Dto) : IRequest<Result<UserDto>>;

    public class RegisterTeacherHandler : IRequestHandler<RegisterTeacherCommand, Result<UserDto>>
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;

        public RegisterTeacherHandler(IUserRepository users, IPasswordHasher hasher)
        {
            _users = users;
            _hasher = hasher;
        }

        public async Task<Result<UserDto>> Handle(RegisterTeacherCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            if (await _users.EmailExistsAsync(dto.Email, cancellationToken))
                return Result<UserDto>.Failure(Error.Conflict("409", "A user with this email already exists."));

            var teacher = User.CreateTeacher(dto.FullName, dto.Email, _hasher.Hash(dto.Password));
            await _users.AddAsync(teacher, cancellationToken);
            await _users.SaveChangesAsync(cancellationToken);

            return Result<UserDto>.Success(teacher.ToDto());
        }
    }
}
