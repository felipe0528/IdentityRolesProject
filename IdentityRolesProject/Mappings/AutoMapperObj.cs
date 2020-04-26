using AutoMapper;
using IdentityRolesProject.Models.Users;
using IdentityRolesProject.Models.ViewModels.User;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityRolesProject.Mappings
{
    public class AutoMapperObj : Profile
    {
        public AutoMapperObj()
        {
            CreateMap<IdentityRoleVM, IdentityRole>().ReverseMap();
        }
    }
}
