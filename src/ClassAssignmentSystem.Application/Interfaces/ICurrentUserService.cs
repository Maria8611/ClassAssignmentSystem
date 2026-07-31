using System;
using System.Collections.Generic;
using System.Text;

namespace ClassAssignmentSystem.Application.Interfaces
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? Username { get; }
        bool IsInRole(string role);
        IEnumerable<string> GetRoles();
    }
}
