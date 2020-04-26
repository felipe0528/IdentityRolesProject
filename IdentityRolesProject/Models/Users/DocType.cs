using IdentityRolesProject.Models.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityRolesProject.Models.Users
{
    public class DocType
    {
        [Key]
        public int DocTypeId { get; set; }
        public string Name { get; set; }
        public int CountryId { get; set; }
        public Country Country { get; set; }
    }
}
