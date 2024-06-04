using Project.Data;
using Project.Models;
using Project.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Diagnostics;

namespace Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var jobs = _context.Jobs.ToList();
            return View(jobs);
        }
        [HttpGet]
        public IActionResult DetailJob(int? id)
        {
            var job = _context.Jobs.Find(id);
            var jobs = _context.Jobs.ToList();
            var viewmodel = new ViewModelDetailJob
            {
                JobList = jobs,
                job = job
            };
            return View(viewmodel);
        }
        [HttpPost]
        public IActionResult Search(string SearchJob, string SearchLocation)
        {
            var jobSearch = _context.Jobs.ToList();
            if(!String.IsNullOrEmpty(SearchJob) && String.IsNullOrEmpty(SearchLocation))
            {
                jobSearch = jobSearch.Where(n => n.JobTitle.Contains(SearchJob)).ToList();
            }
            else if(!String.IsNullOrEmpty(SearchLocation) && String.IsNullOrEmpty(SearchJob))
            {
                jobSearch = jobSearch.Where(n => n.Location.Contains(SearchLocation)).ToList();
            }
            else if(!String.IsNullOrEmpty(SearchLocation) && !String.IsNullOrEmpty(SearchJob))
            {
                jobSearch = jobSearch.Where(n => n.JobTitle.Contains(SearchJob) && n.Location.Contains(SearchLocation)).ToList();
            } else
            {
                return View(jobSearch);
            }
            return View(jobSearch); 
        }
        [NonAction]
        private string UploadedFile(JobApplication model)
        {
            string uniqueFileName = null;
            if (model.CurriculumVitae != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.CurriculumVitae.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.CurriculumVitae.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }
        [NonAction]
        private void LoadJob()
        {
            var jobs = _context.Jobs.ToList();
            ViewBag.Jobs = new SelectList(jobs, "Id", "JobTitle");
        }
        [HttpGet]
        public IActionResult CreateCVFromList()
        {
            var user = _userManager.GetUserId(User);
            ViewBag.UserId = user;
            LoadJob();
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "Seeker")]
        public IActionResult CreateCV(int? id)
        {
            var user = _userManager.GetUserId(User);
            var jobId = _context.Jobs.Find(id);
            ViewBag.JobId = jobId.Id;
            ViewBag.UserId = user;
            return View();
        }
        [HttpPost]
        public IActionResult CreateCV(JobApplication model)
        {
            string uniqueFileName = UploadedFile(model);
            JobApplication jobApplication = new JobApplication
            {
                Title = model.Title,
                Status = model.Status,
                JobId = model.JobId,
                Introduction = model.Introduction,
                CVUrl = uniqueFileName,
                UserId = model.UserId,
            };
            _context.JobApplications.Add(jobApplication);
            _context.SaveChanges();
            return RedirectToAction(nameof(MyCV));
        }

        [HttpGet]
        [Authorize(Roles = "Seeker")]
        public IActionResult MyCV()
        {
            var jobApplication = _context.JobApplications.ToList();
            var job = _context.Jobs.ToList();
            var viewModel = new ViewModelJobAndCV
            {
                job = job,
                jobApplication = jobApplication,
            };
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult DetailCV(int? id)
        {
            var jobApplication = _context.JobApplications.Find(id);
            return View(jobApplication);
        }

        [HttpGet]
        public IActionResult DeleteJobApp(int? id)
        {
            var model = _context.JobApplications.Find(id);
            var jobApplication = _context.JobApplications.FirstOrDefault(j => j.Id == model.Id);
            if (jobApplication == null)
            {
                return NotFound();
            }
            string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", jobApplication.CVUrl);

            // Kiểm tra xem tập tin ảnh cũ có tồn tại không và sau đó xóa nó
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _context.JobApplications.Remove(jobApplication);
            _context.SaveChanges();
            return RedirectToAction(nameof(MyCV));
        }
    }
}
