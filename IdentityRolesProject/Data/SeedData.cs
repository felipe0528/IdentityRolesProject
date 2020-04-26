using IdentityRolesProject.Models.Localization;
using IdentityRolesProject.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityRolesProject.Data
{
    public class SeedDataValues
    {
        public string AdminUserName { get; set; }
        public string AdminEmail { get; set; }
        public string AdminPassword { get; set; }
        public string AdminRoleName { get; set; }
        public string UserRoleName { get; set; }
    }
    public static class SeedData
    {

        public static async Task SeedAsync(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<SeedDataValues> seedDataValues,
            ApplicationDbContext context)
        {
            context.Database.EnsureCreated();
            await SeedCountries(context);
            SeedRoles(roleManager, seedDataValues.Value);
            await SeedUsersAsync(userManager, seedDataValues.Value);
        }

        private static async Task SeedCountries(ApplicationDbContext context)
        {
            if (context.Country.Count() == 0)
            {
                List<Country> list = new List<Country>();
                CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures | CultureTypes.SpecificCultures);
                foreach (CultureInfo info in cultures)
                {
                    try
                    {
                        if (info.LCID != 127 && !info.IsNeutralCulture)
                        {
                            RegionInfo info2 = new RegionInfo(info.LCID);
                            Country country = new Country();
                            country.LCID = info.LCID;
                            country.EnglishName = info2.EnglishName;
                            if (!list.Exists(x => x.EnglishName == country.EnglishName))
                            {
                                list.Add(country);
                            }
                        }

                    }
                    catch (Exception)
                    {
                    }
                }
                await context.Country.AddRangeAsync(list.OrderBy(x => x.EnglishName));
            }
        }

        private static async Task SeedUsersAsync(UserManager<IdentityUser> userManager,
            SeedDataValues seedDataValues)
        {
            if (userManager.FindByNameAsync(seedDataValues.AdminUserName).Result == null)
            {
                var user = new ApplicationUser
                {
                    UserName = seedDataValues.AdminUserName,
                    Email = seedDataValues.AdminEmail
                };
                var result = userManager.CreateAsync(user, seedDataValues.AdminPassword).Result;
                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, seedDataValues.AdminRoleName).Wait();
                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                    await userManager.ConfirmEmailAsync(user, code);
                }
            }
        }

        private static void SeedRoles(RoleManager<IdentityRole> roleManager,
            SeedDataValues seedDataValues)
        {
            if (!roleManager.RoleExistsAsync(seedDataValues.AdminRoleName).Result)
            {
                var role = new IdentityRole
                {
                    Name = seedDataValues.AdminRoleName
                };
                var result = roleManager.CreateAsync(role).Result;
            }

            if (!roleManager.RoleExistsAsync(seedDataValues.UserRoleName).Result)
            {
                var role = new IdentityRole
                {
                    Name = seedDataValues.UserRoleName
                };
                var result = roleManager.CreateAsync(role).Result;
            }
        }
    }
}
