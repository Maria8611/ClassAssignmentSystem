using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassAssignmentSystem.Application.Features.Admin.Queries
{

    public record GetTeachersQuery : IRequest<IReadOnlyList<UserDto>>;

    public class GetTeachersHandler : IRequestHandler<GetTeachersQuery, IReadOnlyList<UserDto>>
    {
        private readonly IUserRepository _users;
        public GetTeachersHandler(IUserRepository users) => _users = users;

        public async Task<IReadOnlyList<UserDto>> Handle(GetTeachersQuery request, CancellationToken cancellationToken)
        {
            var teachers = await _users.GetByRoleAsync(Domain.Enums.UserRole.Teacher, cancellationToken);
            return teachers.Select(t => t.ToDto()).ToList();
        }
    }

}
