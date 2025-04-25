using BloodDonor.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
namespace BloodDonor.Services
{
    public class MyService
    {
        readonly AdminDbContext _adminDbContext;

        public MyService(AdminDbContext adminDbContext)
        {
            _adminDbContext = adminDbContext;
        }
        public async Task PopulateDropdowns(ViewDataDictionary viewData)
        {
            var bloodGroups = await _adminDbContext.BloodGroup.ToListAsync();
            var states = await _adminDbContext.State.ToListAsync();

            viewData["BloodGroups"] = new SelectList(bloodGroups, "BloodGroupName", "BloodGroupName");
            viewData["States"] = new SelectList(states, "Id", "StateName");
        }

    }
}
