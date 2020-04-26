using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityRolesProject.Models.Localization
{
    public interface ITimeZones
    {
        List<SelectListItem> LoadTimeZones();
    }

    public class TimeZones : ITimeZones
    {
        public List<SelectListItem> LoadTimeZones()
        {
            return TimeZoneInfo.GetSystemTimeZones().Select(a =>
                                  new SelectListItem
                                  {
                                      Value = a.Id,
                                      Text = a.DisplayName
                                  }).OrderBy(x => x.Text).ToList();
        }
    }
}
