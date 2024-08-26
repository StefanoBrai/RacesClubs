using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgettoTest.Data;
using ProgettoTest.Interfaces;
using ProgettoTest.Models;
using ProgettoTest.ViewModels;

namespace ProgettoTest.Controllers
{
    public class RaceController : Controller
    {
        private readonly IRaceRepository _raceRepository;
        private readonly IPhotoService _photoService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RaceController(IRaceRepository raceRepository, IPhotoService photoService, IHttpContextAccessor httpContextAccessor)
        {
            _raceRepository = raceRepository;
            _photoService = photoService;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Get All
        public async Task<IActionResult> Index()
        {
            var races = await _raceRepository.GetAll();

            return View(races);
        }
        #endregion

        #region Get By Id
        public async Task<IActionResult> Detail(int id)
        {
            var race = await _raceRepository.GetByIdAsync(id);

            return View(race);
        }
        #endregion

        #region Create
        public IActionResult Create() 
        {
            var currentUserId = _httpContextAccessor.HttpContext.User.GetUserId();
            var createRaceVM = new CreateRaceViewModel()
            {
                AppUserId = currentUserId
            };

            return View(createRaceVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRaceViewModel raceVM)
        {
            if (ModelState.IsValid)
            {
                var result = await _photoService.AddPhotoAsync(raceVM.Image);

                var race = new Race
                {
                    Title = raceVM.Title,
                    Description = raceVM.Description,
                    Image = result.Url.ToString(),
                    AppUserId = raceVM.AppUserId,
                    Address = new Address
                    {
                        Street = raceVM.Address.Street,
                        City = raceVM.Address.City,
                        State = raceVM.Address.State
                    }
                };

                _raceRepository.Add(race);

                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Photo upload failed");
            }

            return View(raceVM);
        }
        #endregion

        #region Edit
        public async Task<IActionResult> Edit(int id)
        {
            var race = await _raceRepository.GetByIdAsync(id);

            if(race == null) return View("Error");

            var raceVM = new EditRaceViewModel
            {
                Title = race.Title,
                Description = race.Description,
                AddressId = race.AddressId,
                Address = race.Address,
                URL = race.Image,
                RaceCategory = race.RaceCategory
            };

            return View(raceVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditRaceViewModel raceVM)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to edit race");

                return View("Edit", raceVM);
            }

            var race = await _raceRepository.GetByIdAsyncNoTracking(id);

            if (race == null)
            {
                return View("Error");
            }

            if (raceVM.Image != null)
            {
                if (!string.IsNullOrEmpty(race.Image))
                {
                    try
                    {
                        await _photoService.DeletePhotoAsync(race.Image);
                    }
                    catch (Exception ex)
                    {

                        ModelState.AddModelError("", "Could not delete photo");

                        return View(raceVM);
                    }

                }

                var photoResult = await _photoService.AddPhotoAsync(raceVM.Image);
                race.Image = photoResult.Url.ToString();
            }

            race.Id = id;
            race.Title = raceVM.Title;
            race.Description = raceVM.Description;
            race.AddressId = raceVM?.AddressId;
            race.Address = raceVM.Address;

            _raceRepository.Update(race);

            return RedirectToAction("Index");
        }
        #endregion

        #region Delete
        public async Task<IActionResult> Delete(int id)
        {
            var raceDetails = await _raceRepository.GetByIdAsync(id);

            if (raceDetails == null) return View("Error");

            return View(raceDetails);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteRace(int id)
        {
            var clubdetails = await _raceRepository.GetByIdAsync(id);

            if(clubdetails == null) return View("error");

            _raceRepository.Delete(clubdetails);

            return RedirectToAction("Index");
        }
        #endregion

        #region // Create - Prima di PhotoServices
        //public IActionResult Create()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public async Task<IActionResult> Create(Race race)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(race);
        //    }

        //    _raceRepository.Add(race);

        //    return RedirectToAction("Index");
        //}
        #endregion

        #region // Prima di implementare Interfaces & Repository

        /*private readonly ApplicationDbContext _context;

        public RaceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var races = _context.Races.ToList();
            return View(races);
        }

        public IActionResult Detail(int id) 
        {
            var race = _context.Races.Include(a => a.Address).FirstOrDefault(r => r.Id == id);

            return View(race);
        }*/
        #endregion
    }
}
