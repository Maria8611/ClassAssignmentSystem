using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassAssignmentSystem.Application.Features.Admin.Queries
{

    public record GetStudentsQuery : IRequest<IReadOnlyList<UserDto>>;

    public class GetStudentsHandler : IRequestHandler<GetStudentsQuery, IReadOnlyList<UserDto>>
    {
        private readonly IUserRepository _users;
        private readonly HybridCache _hybridCache;

        public GetStudentsHandler(IUserRepository users, HybridCache hybridCache)
        {
            _users = users;
            _hybridCache = hybridCache;
        }

        public async Task<IReadOnlyList<UserDto>> Handle(GetStudentsQuery request, CancellationToken cancellationToken)
        {
            var students = await _hybridCache.GetOrCreateAsync(
                "students",
                async token => {
                    var domainStudents = await _users.GetByRoleAsync(Domain.Enums.UserRole.Student, token);
                    return domainStudents.Select(s => s.ToDto()).ToList();
                },
                cancellationToken: cancellationToken
            );
            return students;
        }
    }
}
