using IdentityRolesProject.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityRolesProject.Models.ViewModels.User
{
    public class IdentityRoleVM
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public List<ApplicationUser> Users { get; set; }

        public string NewUserName { get; set; }
    }
}
