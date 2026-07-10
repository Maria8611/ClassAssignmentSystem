using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassAssignmentSystem.Application.Features.Admin.Queries
{

    public record GetStudentsQuery : IRequest<IReadOnlyList<UserDto>>;

    public class GetStudentsHandler : IRequestHandler<GetStudentsQuery, IReadOnlyList<UserDto>>
    {
        private readonly IUserRepository _users;
        public GetStudentsHandler(IUserRepository users) => _users = users;

        public async Task<IReadOnlyList<UserDto>> Handle(GetStudentsQuery request, CancellationToken cancellationToken)
        {
            var students = await _users.GetByRoleAsync(Domain.Enums.UserRole.Student, cancellationToken);
            return students.Select(s => s.ToDto()).ToList();
        }
    }
}
