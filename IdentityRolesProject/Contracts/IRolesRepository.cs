using IdentityRolesProject.Models.Users;
using IdentityRolesProject.Repository;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityRolesProject.Contracts
{
    public interface IRolesRepository : IRepositoryBase<IdentityRole>
    {
        Task<IdentityRole> FindByName(string id);

        Task<ICollection<ApplicationUser>> FindAllUsersByRole(string role);

        Task<bool> RemoveUserByRole(string role, string userId);

        Task<bool> AddUserByRole(string role, string userId);
    }
}
