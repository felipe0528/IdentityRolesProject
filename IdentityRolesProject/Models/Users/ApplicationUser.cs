using IdentityRolesProject.Models.Localization;
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace IdentityRolesProject.Models.Users
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser() : base()
        {
            CountryID = 86;
            TimeZone = "SA Pacific Standard Time";
        }
        [MaxLength(150)]
        [PersonalData]
        public string FirstName { get; set; }
        [MaxLength(150)]
        [PersonalData]
        public string LastName { get; set; }

        [MaxLength(150)]
        [PersonalData]
        public string City { get; set; }

        public int CountryID { get; set; }
        public Country Country { get; set; }

        [Display(Name = "Time Zone")]
        public string TimeZone { get; set; }

        public DocType DocType { get; set; }

        [PersonalData]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateOfBirth { get; set; }

        [PersonalData]
        [MaxLength(150)]
        public string DocId { get; set; }

        public Guid? ImageFrontDocId { get; set; }
        public Guid? ImageBackDocId { get; set; }

        public Guid? ImageFaceDocId { get; set; }

        public bool DocValidated { get; set; }

        public bool PendingValidation { get; set; }

        public bool DataApproval { get; set; }

        public DateTime UserDataSubmitionDate { get; set; }

        public DateTime UserApprovalDate { get; set; }

        public DateTime UserCreationDateUTC { get; set; }
    }
}
