using BloodDonor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
namespace BloodDonor.Controllers
{
    public class UserController : Controller
    {

        readonly AdminDbContext _adminDbContext;

        public UserController(AdminDbContext adminDbContext)
        {
            _adminDbContext = adminDbContext;
        }

       
      

        [HttpGet]
        public IActionResult UserHomePage()
        {
            ViewData["AdminName"] = TempData["Name"];
            TempData.Keep("Name");
            ViewBag.upt = TempData["upt"];
            TempData.Peek("upt");



            return View();
        }

        [HttpGet]
        public IActionResult DetailsOfUser()
        {

            return View();
        }
        [HttpPost]
        public IActionResult DetailsOfUser(int donorId)
        {
            if(donorId != 0)
            {
                var DonorData = _adminDbContext.BloodDonors
                                     .Where(e => e.DonorId == donorId)
                                     .ToList();



                return PartialView("DonorDetails", DonorData);

            }

            return View();
        }

        [HttpGet]
        
        public async Task<IActionResult> Upadate_User(int id=0)
        {
            ViewData["AdminName"] = TempData["Name"];
            TempData.Keep("Name");
            await PopulateDropdowns(); // Populates dropdowns like States and BloodGroups.

       

            if (id != 0) // Check if a donor ID is passed for editing.
            {
                var donor = await _adminDbContext.BloodDonors.FindAsync(id);
                if (donor != null)
                {
                    BloodDonorProperty bloodDonorProperty = new BloodDonorProperty()
                    {
                        DonorId = donor.DonorId,
                        FullName = donor.FullName,
                        Email = donor.Email,
                        Gender = donor.Gender,
                        State = donor.State,
                        City = donor.City,
                        Age = donor.Age,
                        BloodGroup = donor.BloodGroup,
                        PhoneNumber = donor.PhoneNumber
                    };

                    ViewBag.State1 = bloodDonorProperty.State;
                    ViewBag.City1 = bloodDonorProperty.City;
                    ViewBag.pwd = donor.Password;
                    ViewBag.cpwd = donor.ConfirmPassword;
                    ViewBag.buttonName = "Update";
                    ViewBag.IsEdit = false; // Flag to determine form mode.
                    
                    return View(bloodDonorProperty);
                }
            }
            else
            {
                ViewBag.buttonName = "Register";
                ViewBag.IsEdit = true; // Form is in Add mode.
            }

            return View();


        }


   
        private async Task<string> FindStateName(string StateID)
        {
            int stateID = 0; // Default value if no valid state is provided
            if (!string.IsNullOrEmpty(StateID))
            {
                stateID = int.Parse(StateID);  // Parse the string to integer
            }

            var stateName = await _adminDbContext.State
                .Where(e => e.Id == stateID)
                .Select(e => e.StateName)
                .FirstOrDefaultAsync();

            string statename = (stateName == null) ? "" : stateName.ToString();

            return statename;

        }
        [HttpPost]
        public async Task<IActionResult> Upadate_User(BloodDonorProperty obj)
        {
            await PopulateDropdowns();

            ViewBag.IsLayout= "/Views/Shared/UserLayout.cshtml";

            if (!ModelState.IsValid && obj.DonorId == 0)
            {
                return View();
            }

            string? checkId = obj.State;

            if (checkId.Length == 1)
            {
                int stateID = 0; // Default value if no valid state is provided
                if (!string.IsNullOrEmpty(obj.State))
                {
                    stateID = int.Parse(obj.State);  // Parse the string to integer
                }

                var stateName = await _adminDbContext.State
                    .Where(e => e.Id == stateID)
                    .Select(e => e.StateName)
                    .FirstOrDefaultAsync();

                obj.State = stateName;
            }
            if (obj.DonorId > 0)
            {


                _adminDbContext.Update(obj);
                await _adminDbContext.SaveChangesAsync();

                TempData["upt"] = "Donor " + obj.DonorId + " information updated successfully.";

                return RedirectToAction("DetailsOfUser","User");

            }
            else
            {


                await _adminDbContext.AddAsync(obj);
                await _adminDbContext.SaveChangesAsync();
                ViewBag.Msg = "Record not inserted.";
                TempData["DataInsert"] = $"Last Inserted Record is {obj.DonorId}";
                return View();
            }


           

        }

        

        private async Task PopulateDropdowns()
        {

            if (ViewBag.BloodGroups == null || ViewBag.BloodGroups == null)
            {
                var bloodGroups = await _adminDbContext.BloodGroup.ToListAsync();
                var states = await _adminDbContext.State.ToListAsync();
                ViewBag.BloodGroups = new SelectList(bloodGroups, "BloodGroupName", "BloodGroupName");
                ViewBag.States = new SelectList(states, "Id", "StateName");
            }




        }

    }
}
