using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using ProgettoTest.Data;
using ProgettoTest.Extensions;
using ProgettoTest.Interfaces;
using ProgettoTest.Models;
using ProgettoTest.ViewModels;

namespace ProgettoTest.Controllers
{
    public class ClubController : Controller
    {
        private readonly IClubRepository _clubRepository;
        private readonly IPhotoService _photoService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClubController(IClubRepository clubRepository, IPhotoService photoService, IHttpContextAccessor httpContextAccessor)
        {
            _clubRepository = clubRepository;
            _photoService = photoService;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Get All
        public async Task<IActionResult> Index(string sortOrder, int page = 1)
        {
            ViewData["CurrentSort"] = sortOrder;

            ViewData["TitleSortParm"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["LocationSortParm"] = sortOrder == "Location" ? "location_desc" : "Location";

            var clubs = _clubRepository.GetAll();

            switch (sortOrder)
            {
                case "title_desc":
                    clubs = clubs.OrderByDescending(c => c.Title);
                    break;
                default:
                    clubs = clubs.OrderBy(c => c.Title);
                    break;
            }

            var pagedClubs = await clubs.ToPagedListAsync(page, 3);
            return View(pagedClubs);
        }
        #endregion

        #region Get By Id
        public async Task<IActionResult> Detail(int id)
        {
            var club = await _clubRepository.GetByIdAsync(id);

            return View(club);
        }
        #endregion

        #region Create
        public IActionResult Create() 
        {
            var currentUserId = _httpContextAccessor.HttpContext.User.GetUserId();
            var createClubVM = new CreateClubViewModel()
            {
                AppUserId = currentUserId
            };
            return View(createClubVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateClubViewModel clubVM)
        {
            if (ModelState.IsValid)
            {
                var result = await _photoService.AddPhotoAsync(clubVM.Image);

                var club = new Club
                {
                    Title = clubVM.Title,
                    Description = clubVM.Description,
                    Image = result.Url.ToString(),
                    AppUserId = clubVM.AppUserId,
                    Address = new Address
                    {
                        Street = clubVM.Address.Street,
                        City = clubVM.Address.City,
                        State = clubVM.Address.State
                    }
                };

                _clubRepository.Add(club);

                return RedirectToAction("Index");
            }
            else 
            {
                ModelState.AddModelError("", "Photo upload failed");
            }

            return View(clubVM);
        }
        #endregion

        #region Edit
        public async Task<IActionResult> Edit(int id)
        {
            var club = await _clubRepository.GetByIdAsync(id);

            if (club == null) return View("Error");

            var clubVM = new EditClubViewModel
            {
                Title = club.Title,
                Description = club.Description,
                AddressId = club.AddressId,
                Address = club.Address,
                URL = club.Image,
                ClubCategory = club.ClubCategory
            };

            return View(clubVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditClubViewModel clubVM)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to edit club");

                return View("Edit", clubVM);
            }

            var club = await _clubRepository.GetByIdAsyncNoTracking(id);

            if (club == null)
            {
                return View("Error");
            }

            if (clubVM.Image != null)
            {
                if (!string.IsNullOrEmpty(club.Image))
                {
                    try
                    {
                        await _photoService.DeletePhotoAsync(club.Image);
                    }
                    catch (Exception ex)
                    {

                        ModelState.AddModelError("", "Could not delete photo");

                        return View(clubVM);
                    }
                }

                var photoResult = await _photoService.AddPhotoAsync(clubVM.Image);
                club.Image = photoResult.Url.ToString();
            }

            club.Id = id;
            club.Title = clubVM.Title;
            club.Description = clubVM.Description;
            club.AddressId = clubVM?.AddressId;
            club.Address = clubVM.Address;

            _clubRepository.Update(club);

            return RedirectToAction("Index");
        }
        #endregion

        #region Delete
        public async Task<IActionResult> Delete(int id)
        {
            var clubDetails = await _clubRepository.GetByIdAsync(id);

            if (clubDetails == null) return View("Error");

            return View(clubDetails);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteClub(int id)
        {
            var clubDetails = await _clubRepository.GetByIdAsync(id);

            if (clubDetails == null) return View("Error");

            _clubRepository.Delete(clubDetails);

            return RedirectToAction("Index");
        }
        #endregion

        #region // Prima di implementare Interfaces & Repository
        /*private readonly ApplicationDbContext _context;

        public ClubController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var clubs = _context.Clubs.ToList();

            return View(clubs);
        }

        public IActionResult Detail(int id)
        {
            var club = _context.Clubs.Include(a => a.Address).FirstOrDefault(c => c.Id == id);

            return View(club);
        }*/
        #endregion
    }
}
