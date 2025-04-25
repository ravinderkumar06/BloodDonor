using Microsoft.AspNetCore.Mvc;
using BloodDonor.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Controllers
{
    public class HomePage : Controller
    {
        private readonly AdminDbContext _adminDbContext;

        public HomePage(AdminDbContext adminDbContext)
        {
            _adminDbContext = adminDbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            try
            {
                ViewBag.Msg = TempData["Record"];
                TempData.Peek("Record");

                var bloodGroups = _adminDbContext.BloodGroup.ToList();
                var states = _adminDbContext.State.ToList();

                if (bloodGroups.Any() && states.Any())
                {
                    // Pass data to the view via ViewBag
                    ViewBag.BloodGroups = new SelectList(bloodGroups, "Id", "BloodGroupName");
                    ViewBag.States = new SelectList(states, "Id", "StateName");
                }
                else
                {
                    ViewBag.BloodGroups = new SelectList(Enumerable.Empty<SelectListItem>());
                    ViewBag.States = new SelectList(Enumerable.Empty<SelectListItem>());
                    ViewBag.ErrorMessage = "No data found for Blood Groups or States.";
                }
            }
            catch (Exception ex)
            {
                // Handle errors gracefully
                ViewBag.ErrorMessage = $"An error occurred while loading data: {ex.Message}";
                ViewBag.BloodGroups = new SelectList(Enumerable.Empty<SelectListItem>());
                ViewBag.States = new SelectList(Enumerable.Empty<SelectListItem>());
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(BloodDonorProperty obj)
        {
            try
            {
                int StateID = 0;

                StateID = int.Parse(obj.State);
                var stateName = await _adminDbContext.State
                    .Where(x => x.Id == StateID)
                    .Select(x => x.StateName) // Assuming the property is "StateName"
                    .FirstOrDefaultAsync(); // Fetch a single result




                if (ModelState.IsValid)
                {
                    obj.State = stateName;

                    _adminDbContext.BloodDonors.Add(obj);
                    _adminDbContext.SaveChanges();
                    TempData["Record"] = "Record has been successfully inserted.";
                    return RedirectToAction("Register", "HomePage");
                }
                else
                {
                    ViewBag.ErrorMessage = "Form validation failed. Please check your inputs.";
                }
            }
            catch (Exception ex)
            {
                // Log the exception and provide feedback to the user
                ViewBag.ErrorMessage = $"An error occurred while saving the data: {ex.Message}";
            }

            // Reload dropdowns if there's an error
            var bloodGroups = _adminDbContext.BloodGroup.ToList();
            var states = _adminDbContext.State.ToList();
            ViewBag.BloodGroups = new SelectList(bloodGroups, "Id", "BloodGroupName");
            ViewBag.States = new SelectList(states, "Id", "StateName");

            return View(obj);
        }

        // Action to fetch cities based on selected state (AJAX)
        public async Task<IActionResult> GetCities(int stateID)
        {
            try
            {
                // Query the cities and project only the required fields
                var cities = await _adminDbContext.Cities
                    .Where(c => c.StateID == stateID)
                    .Select(e => new { e.Id, e.CityName }) // Ensure you're selecting both 'Id' and 'CityName'
                    .ToListAsync();



                return Json(cities); // Return JSON response
            }
            catch (Exception ex)
            {
                // Log the exception (if logging is configured)
                // Example: _logger.LogError(ex, "Failed to fetch cities.");

                // Return an error response with HTTP 500 status code
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }



        [HttpGet]
        public IActionResult UserLogin()
        {


            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UserLogin(LoginProperties obj)
        {
            if (string.IsNullOrWhiteSpace(obj.Email) || string.IsNullOrWhiteSpace(obj.Password))
            {
                ViewBag.ErrorMessage = "Please enter both email and password.";
                return View();
            }

            var data = await _adminDbContext.BloodDonors.FirstOrDefaultAsync(e => e.Email == obj.Email && e.Password == obj.Password);
            if (data != null)
            {

                TempData["Name"] = obj.Email;
                // Redirect to dashboard or other protected area
                return RedirectToAction("UserHomePage","User");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid username or password!";
                return View();
            }
        }


        [HttpGet]
        public IActionResult AdminLogin() => View();

        [HttpPost]
        public async Task<IActionResult> AdminLogin(LoginProperties obj)
        {
            if (string.IsNullOrWhiteSpace(obj.Email) || string.IsNullOrWhiteSpace(obj.Password))
            {
                ViewBag.ErrorMessage = "Please enter both email and password.";
                return View();
            }

            var data = await _adminDbContext.AdmingTbl.FirstOrDefaultAsync(e => e.Email == obj.Email && e.Password == obj.Password);

            if (data != null)
            {
                TempData["Name"] = obj.Email;
                return RedirectToAction("AdminHomePage","Admin");
            }

            ViewBag.ErrorMessage = "Invalid username or password!";
            return View();
        }


    }
}
