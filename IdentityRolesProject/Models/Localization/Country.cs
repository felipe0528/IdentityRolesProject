using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using IdentityRolesProject.Models.Users;

namespace IdentityRolesProject.Models.Localization
{
    public class Country
    {
        [Key]
        public int CountryId { get; set; }
        public int LCID { get; set; }
        public string EnglishName { get; set; }
        public List<DocType> DocTypes { get; set; }
    }
}
