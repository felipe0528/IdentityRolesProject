using IdentityRolesProject.Contracts;
using IdentityRolesProject.Data;
using IdentityRolesProject.Models.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityRolesProject.Repository
{
    public class RolesRepository : IRolesRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IOptions<SeedDataValues> _seedData;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RolesRepository(ApplicationDbContext db,
            RoleManager<IdentityRole> roleManager,
            IOptions<SeedDataValues> seedData,
            UserManager<IdentityUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _roleManager = roleManager;
            _seedData = seedData;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> Create(IdentityRole entity)
        {
            var result = await _roleManager.CreateAsync(entity);

            if (result == IdentityResult.Success)
            {
                return entity.Id;
            }

            return string.Empty;
        }

        public async Task<bool> Delete(IdentityRole entity)
        {
            if (entity.Name == _seedData.Value.UserRoleName || entity.Name == _seedData.Value.AdminRoleName)
            {
                return false;
            }
            var result = await _roleManager.DeleteAsync(entity);
            return result == IdentityResult.Success;
        }

        public async Task<ICollection<IdentityRole>> FindAll()
        {
            var result = await _roleManager.Roles.ToListAsync();
            return result;
        }

        public async Task<ICollection<ApplicationUser>> FindAllUsersByRole(string roleId)
        {
            var result = await _db.UserRoles.Where(x => x.RoleId == roleId).Select(i => i.UserId).ToListAsync();

            var resultUsers = await _db.ApplicationUsers.Where(p => result.Contains(p.Id)).ToListAsync();
            return resultUsers;
        }

        public async Task<IdentityRole> FindById(string id)
        {
            var result = await _roleManager.FindByIdAsync(id);
            return result;
        }

        public async Task<IdentityRole> FindByName(string id)
        {
            var result = await _roleManager.FindByNameAsync(id);
            return result;
        }

        public async Task<bool> isExists(string id)
        {
            var result = await _roleManager.RoleExistsAsync(id);
            return result;
        }

        public async Task<bool> RemoveUserByRole(string roleId, string userId)
        {
            var role = await FindById(roleId);
            if (role != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var isAdmin = await _userManager.IsInRoleAsync(user, _seedData.Value.AdminRoleName);
                    bool canDelete = true;
                    if (isAdmin)
                    {
                        var username = _httpContextAccessor.HttpContext.User.Identity.Name;
                        //Cant delete an Admin if the current user is not the original Admin
                        if (_seedData.Value.AdminUserName != username)
                        {
                            canDelete = false;
                        }
                        if (username == user.UserName && username != _seedData.Value.AdminUserName)
                        {
                            canDelete = true;
                        }
                    }
                    if (canDelete)
                    {
                        var result = await _userManager.RemoveFromRoleAsync(user, role.Name);
                        var result2 = await _userManager.UpdateSecurityStampAsync(user);
                        return result == IdentityResult.Success && result2 == IdentityResult.Success;
                    }
                }
            }

            return false;
        }

        public async Task<bool> AddUserByRole(string roleId, string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user != null)
            {
                var role = await FindById(roleId);
                if (role != null)
                {
                    var result = await _userManager.AddToRoleAsync(user, role.Name);
                    var result2 = await _userManager.UpdateSecurityStampAsync(user);
                    return result == IdentityResult.Success && result2 == IdentityResult.Success;
                }
            }
            return false;
        }

        public async Task<string> Update(IdentityRole entity)
        {
            var result = await _roleManager.UpdateAsync(entity);
            if (result == IdentityResult.Success)
            {
                return entity.Id;
            }
            return string.Empty;
        }
        public Task<bool> Save()
        {
            throw new NotImplementedException();
        }
    }
}
