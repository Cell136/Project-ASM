using Project.Areas.Identity.Pages.Account;
using Project.Data;
using Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Project.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;

        public AdminController(
            ApplicationDbContext context, 
            RoleManager<ApplicationRole> roleManager, 
            UserManager<ApplicationUser> userManager
            )
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }


        //Roles
        public IActionResult IndexRole()
        {
            var roles = _context.Roles.ToList();
            return View(roles);
        }
        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateRole(ApplicationRole model)
        {
            if (!await _roleManager.RoleExistsAsync(model.Name))
            {
                var role = new ApplicationRole { Name = model.Name };
                var result = await _roleManager.CreateAsync(role);
                return RedirectToAction(nameof(IndexRole));
            }
            return View();
        }
        [HttpGet]
        public IActionResult DeleteRole(string? id)
        {
            var role = _context.Roles.Find(id);
            return View(role);
        }
        [HttpPost]
        public IActionResult DeleteRole(ApplicationRole model)
        {
            _context.Roles.Remove(model);
            _context.SaveChanges();
            return RedirectToAction(nameof(IndexRole));
        }

        //User
        [NonAction]
        private void LoadRole()
        {
            var roles = _roleManager.Roles.ToList();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
        }
        
        public IActionResult IndexUser()
        {
            var users = _context.UserRoles.Include(x => x.User).Include(y => y.Role).ToList();
            return View(users);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return RedirectToAction(nameof(IndexUser));
            }
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(IndexUser));
            }
            return RedirectToAction(nameof(IndexUser));
        }
        //Job
        [HttpGet]
        public IActionResult JobList()
        {
            var jobs = _context.Jobs.Include(x => x.Users).ToList();
            return View(jobs);
        }
        [HttpGet]
        public IActionResult DeleteJob(int? id)
        {
            var job = _context.Jobs.Find(id);
            return View(job);
        }
        [HttpPost]
        public IActionResult DeleteJob(Job job)
        {
            _context.Jobs.Remove(job);
            _context.SaveChanges();
            return RedirectToAction(nameof(JobList));
        }
    }
}
