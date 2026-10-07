using ClassAssignmentSystem.Application.DTOs;
using ClassAssignmentSystem.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassAssignmentSystem.Application.Features.Admin.Queries
{

    public record GetTeachersQuery : IRequest<IReadOnlyList<UserDto>>;

    public class GetTeachersHandler : IRequestHandler<GetTeachersQuery, IReadOnlyList<UserDto>>
    {
        private readonly IUserRepository _users;
        private readonly HybridCache _hybridCache;
        public GetTeachersHandler(IUserRepository users, HybridCache hybridCache)
        {
            _users = users;
            _hybridCache = hybridCache;
        }

        public async Task<IReadOnlyList<UserDto>> Handle(GetTeachersQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = "teachers";
            var teachers = await _hybridCache.GetOrCreateAsync(
                cacheKey, 
                async token =>{ var domainTeachers = await _users.GetByRoleAsync(Domain.Enums.UserRole.Teacher, token);
                                return domainTeachers.Select(t => t.ToDto()).ToList();},
                cancellationToken : cancellationToken
            );
            return teachers;
        }
    }

}
