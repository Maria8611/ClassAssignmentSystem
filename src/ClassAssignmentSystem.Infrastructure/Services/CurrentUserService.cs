using ClassAssignmentSystem.Application.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ClassAssignmentSystem.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Safely reads the NameIdentifier claim (User ID) from the current HTTP request
        public string? UserId => _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Safely reads the Name claim (Username)
        public string? Username => _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.Name)?.Value;

        Guid? ICurrentUserService.UserId => Guid.TryParse(UserId, out var guid) ? guid : null;

        // Checks if the user has a specific role claim
        public bool IsInRole(string role) => _httpContextAccessor.HttpContext?.User?
            .IsInRole(role) ?? false;

        // Retrieves all assigned roles
        public IEnumerable<string> GetRoles()
        {
            return _httpContextAccessor.HttpContext?.User?
                .FindAll(ClaimTypes.Role)
                .Select(c => c.Value) ?? Enumerable.Empty<string>();
        }
    }

}
