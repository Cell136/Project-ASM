using Project.Data;
using Project.Models;
using Project.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Project.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ClientController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var listJob = _context.Jobs.ToList();
            var user = _userManager.GetUserId(User);
            ViewBag.UserId = user;
            return View(listJob);
        }
        [HttpGet]
        public IActionResult CreateJob()
        {
            var user = _userManager.GetUserId(User);
            if (user != null)
            {
                ViewBag.UserId = user;
                return View();
            } else
            {
                return RedirectToAction(nameof(Index));
            }
        }
        [NonAction]
        private string UploadedFile(Job model)
        {
            string uniqueFileName = null;
            if (model.Avatar != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Avatar.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Avatar.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }
        [HttpPost]
        public IActionResult CreateJob(Job model)
        {
            string uniqueFileName = UploadedFile(model);
            Job job = new Job
            {
                JobTitle = model.JobTitle,
                Location = model.Location,
                Industry = model.Industry,
                AvatarUrl = uniqueFileName,
                Description = model.Description,
                Requiered1 = model.Requiered1,
                Requiered2 = model.Requiered2,
                ApplicationDeadline = model.ApplicationDeadline,
                LowestPrice = model.LowestPrice,
                HighestPrice = model.HighestPrice,
                UserId = model.UserId,
            };
            _context.Jobs.Add(job);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
            
        }
        [HttpGet]
        public IActionResult DetailJob(int? id)
        {
            ViewBag.JobId = id;
            var job = _context.Jobs.Find(id);
            var jobs = _context.Jobs.ToList();
            var viewmodel = new ViewModelDetailJob
            {
                JobList = jobs,
                job = job
            };
            return View(viewmodel);
        }
        [HttpGet]
        public IActionResult EditJob(int? id)
        {
            if (id != null)
            {
                var editJob = _context.Jobs.Find(id);
                return View(editJob);   
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public IActionResult EditJob(Job model)
        {
            var jobToUpdate = _context.Jobs.FirstOrDefault(j => j.Id == model.Id);
            if (jobToUpdate == null)
            {
                return NotFound();
            }
            string uniqueFileName = jobToUpdate.AvatarUrl;
            // Xác định đường dẫn đến thư mục chứa ảnh cũ
            string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", jobToUpdate.AvatarUrl);
        
            if (model.Avatar != null)
            {
                // Kiểm tra xem tập tin ảnh cũ có tồn tại không và sau đó xóa nó
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
                uniqueFileName = UploadedFile(model);
            }
                jobToUpdate.JobTitle = model.JobTitle;
                jobToUpdate.Location = model.Location;
                jobToUpdate.Industry = model.Industry;
                jobToUpdate.AvatarUrl = uniqueFileName;
                jobToUpdate.Description = model.Description;
                jobToUpdate.Requiered1 = model.Requiered1;
                jobToUpdate.Requiered2 = model.Requiered2;
                jobToUpdate.ApplicationDeadline = model.ApplicationDeadline;
                jobToUpdate.LowestPrice = model.LowestPrice;
                jobToUpdate.HighestPrice = model.HighestPrice;
                jobToUpdate.UserId = model.UserId;
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult DeleteJob(int? id)
        {
            if (id != null)
            {
                var deleteJob = _context.Jobs.Find(id);
                return View(deleteJob);
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public IActionResult DeleteJob(Job model)
        {
            var jobToDelete = _context.Jobs.FirstOrDefault(j => j.Id == model.Id);
            if (jobToDelete == null)
            {
                return NotFound();
            }
            string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", jobToDelete.AvatarUrl);

            // Kiểm tra xem tập tin ảnh cũ có tồn tại không và sau đó xóa nó
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _context.Jobs.Remove(jobToDelete);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult CheckCV(int? id)
        {
            ViewBag.JobId = id;
            var jobApplication = _context.JobApplications.Include(x => x.Users).ToList();
            return View(jobApplication);
        }
        [HttpPost]
        public IActionResult DetailCV(int jobId,int jobAppId)
        {
            ViewBag.JobId = jobId;
            var jobApplication = _context.JobApplications.Find(jobAppId);
            return View(jobApplication);
        }
        [HttpPost]
        public IActionResult AcceptJob(int jobId,int jobAppId)
        {
            var model = _context.JobApplications.Find(jobAppId);
            model.Status = "Accepted";
            _context.SaveChanges();
            int id = jobId;
            return RedirectToAction(nameof(CheckCV), new { id });
        }
        [HttpPost]
        public IActionResult RejectJob(int jobId, int jobAppId)
        {
            var model = _context.JobApplications.Find(jobAppId);
            model.Status = "Rejected";
            _context.SaveChanges();
            int id = jobId;
            return RedirectToAction(nameof(CheckCV), new { id });
        }
    }
}
