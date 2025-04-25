using Microsoft.AspNetCore.Mvc;
using BloodDonor.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.IO;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Mono.TextTemplating;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace BloodDonor.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminDbContext _adminDbContext;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ILogger<AdminController> logger, AdminDbContext adminDbContext)
        {
            _logger = logger;
            _adminDbContext = adminDbContext;
        }

      

        public IActionResult AdminHomePage()
        {
            ViewData["AdminName"] = TempData["Name"];
            TempData.Keep("Name");
            return View();
        }

       [HttpGet]
public async Task<IActionResult> Register(int id = 0)
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
                    ViewBag.pwd=donor.Password;
                    ViewBag.cpwd=donor.ConfirmPassword;
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

        private async Task< string> FindStateName(string StateID)
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
           
            string statename=(stateName==null)?"":stateName.ToString();

            return statename;

        }


        [HttpPost]
        public async Task<IActionResult> Register(BloodDonorProperty obj)
        {
            await PopulateDropdowns();

    

            if (!ModelState.IsValid && obj.DonorId==0)
            {
                return View();
            }

            string? checkId=obj.State;

            if(checkId.Length==1)
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
            if (obj.DonorId>0)
            {


                 _adminDbContext.Update(obj);
                await _adminDbContext.SaveChangesAsync();

                TempData["Upt"] = "Donor "+obj.DonorId+" information updated successfully.";
               
                    return RedirectToAction("Details");
               
            }
            else
            {
                

                await _adminDbContext.AddAsync(obj);
                await _adminDbContext.SaveChangesAsync();
                ViewBag.Msg = "Record not inserted.";
                TempData["DataInsert"] = $"Last Inserted Record is {obj.DonorId}";
               
            }

           
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Details(int page = 1, int pageSize = 5)
        {
            ViewData["AdminName"] = TempData["Name"];
            TempData.Keep("Name");
            TempData["upt"] =TempData["Upt"];
            TempData.Keep("Upt");
            var donors = await _adminDbContext.BloodDonors
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int totalRecords = await _adminDbContext.BloodDonors.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("DonorResultsPartial", donors);
            }
            
            ViewBag.Donors = donors;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> DonorSearch()
        {
            ViewData["AdminName"] = TempData["Name"];
            TempData.Keep("Name");
            await PopulateDropdowns();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DonorSearch(BloodDonorProperty obj)
        {
            try
            {
                await PopulateDropdowns();

                int stateID = 0; // Default value if no valid state is provided
                if (!string.IsNullOrEmpty(obj.State))
                {
                    stateID = int.Parse(obj.State);  // Parse the string to integer
                }

               

                // Fetch the state name based on the parsed state ID
                var stateName = await _adminDbContext.State
                    .Where(x => x.Id == stateID)
                    .Select(x => x.StateName)
                    .FirstOrDefaultAsync();

                // Assign the state name back to the obj.State
                obj.State = stateName;

                // Fetch the donor list based on the search criteria
                var donorList = await _adminDbContext.BloodDonors
                    .Where(e =>
                        (string.IsNullOrEmpty(obj.Gender) || e.Gender == obj.Gender) &&
                        (string.IsNullOrEmpty(obj.BloodGroup) || e.BloodGroup == obj.BloodGroup) &&
                        (obj.Age > 0 ? e.Age == obj.Age : true) &&
                        (string.IsNullOrEmpty(obj.State) || e.State == obj.State) &&
                        (string.IsNullOrEmpty(obj.City) || e.City == obj.City))
                    .ToListAsync();

                // Return a Partial View to render the donor list
                var donorTableHtml = await this.RenderViewToStringAsync("_DonorSearchPartialView", donorList);

                return Json(new { donorTableHtml });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DonorSearch: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        private async Task PopulateDropdowns()
        {
            
            if(ViewBag.BloodGroups==null || ViewBag.BloodGroups==null)
            { 
                var bloodGroups = await _adminDbContext.BloodGroup.ToListAsync();
                var states = await _adminDbContext.State.ToListAsync();
                ViewBag.BloodGroups = new SelectList(bloodGroups, "BloodGroupName", "BloodGroupName");
                ViewBag.States = new SelectList(states, "Id", "StateName");
            }




        }

        public IActionResult Logout()
        {
            TempData.Clear();
            return RedirectToAction("Index", "HomePage");
        }


        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0)
            {
                return BadRequest("Invalid ID."); // Return bad request if ID is invalid
            }

            // Find the donor by ID
            var donor = await _adminDbContext.BloodDonors.FirstOrDefaultAsync(d => d.DonorId == id);
            if (donor == null)
            {
                return NotFound("Donor not found."); // Return not found if donor does not exist
            }

            // Remove the donor from the database
            _adminDbContext.BloodDonors.Remove(donor);
            await _adminDbContext.SaveChangesAsync();
            TempData["Upt"] = "delete successfully.";
            // Redirect to the index or list page
            return RedirectToAction("Details");
        }


        public async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            var viewEngine = HttpContext.RequestServices.GetService<ICompositeViewEngine>();
            using var sw = new StringWriter();
            var viewResult = viewEngine.FindView(ControllerContext, viewName, false);
            if (!viewResult.Success) throw new ArgumentNullException($"View '{viewName}' not found.");

            var viewContext = new ViewContext(
                ControllerContext,
                viewResult.View,
                new ViewDataDictionary(new EmptyModelMetadataProvider(), ModelState) { Model = model },
                TempData,
                sw,
                new HtmlHelperOptions()
            );

            await viewContext.View.RenderAsync(viewContext);

            return sw.GetStringBuilder().ToString();
        }
    }
}
