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
    public record RegisterStudentCommand(RegisterStudentDto Dto) : IRequest<Result<UserDto>>;
 
public class RegisterStudentHandler : IRequestHandler<RegisterStudentCommand, Result<UserDto>>
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;

        public RegisterStudentHandler(IUserRepository users, IPasswordHasher hasher)
        {
            _users = users;
            _hasher = hasher;
        }

        public async Task<Result<UserDto>> Handle(RegisterStudentCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            if (await _users.EmailExistsAsync(dto.Email, cancellationToken))
                return Result<UserDto>.Failure(Error.Conflict("Duplicate Email","A user with this email already exists."));

            var student = User.CreateStudent(dto.FullName, dto.Email, _hasher.Hash(dto.Password));
            await _users.AddAsync(student, cancellationToken);
            await _users.SaveChangesAsync(cancellationToken);

            return Result<UserDto>.Success(student.ToDto());
        }
    }

}
