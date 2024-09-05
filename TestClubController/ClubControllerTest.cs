using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProgettoTest.Controllers;
using ProgettoTest.Data.Enum;
using ProgettoTest.Interfaces;
using ProgettoTest.Models;
using ProgettoTest.ViewModels;

namespace TestClubController
{
    public class ClubControllerTest
    {
        private readonly Mock<IClubRepository> _mockRepo;
        private readonly Mock<IPhotoService> _mockPhotoService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly ClubController _clubController;

        public ClubControllerTest()
        {
            _mockRepo = new Mock<IClubRepository>();
            _mockPhotoService = new Mock<IPhotoService>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _clubController = new ClubController(_mockRepo.Object, _mockPhotoService.Object, _mockHttpContextAccessor.Object);
        }

        #region Get - Unit Test
        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfClubs()
        {
            /* Arrange */
            var clubs = new List<Club>
            {
                new Club
                {
                    Id = 1,
                    Title = "Title prova",
                    Description = "Description test",
                    Image = "img-test.jpg",
                    AddressId = 1,
                    Address = new Address
                    {
                        Id= 1,
                        Street = "Street test",
                        City = "City test",
                        State = "state test"
                    },
                    ClubCategory = ClubCategory.Endurance,
                    AppUserId = "user-1",
                    AppUser = new AppUser
                    {
                        Id = "user1",
                        UserName = "User 1",
                        Pace = 5,
                        Mileage = 100,
                        ProfileImageUrl = "profile1.jpg",
                        City = "City 1",
                        State = "State 1",
                        AddressId = 1,
                        Address = new Address
                        {
                            Id = 1, 
                            Street = "Street 1", 
                            City = "City 1"
                        }
                    }
                },

                new Club
                {
                    Id = 2,
                    Title = "Title prova 2",
                    Description = "Description test 2",
                    Image = "img-test-2.jpg",
                    AddressId = 1,
                    Address = new Address
                    {
                        Id= 1,
                        Street = "Street test 2",
                        City = "City test 2",
                        State = "state test 2"
                    },
                    ClubCategory = ClubCategory.Endurance,
                    AppUserId = "user-2",
                    AppUser = new AppUser
                    {
                        Id = "user2",
                        UserName = "User 2",
                        Pace = 5,
                        Mileage = 100,
                        ProfileImageUrl = "profile2.jpg",
                        City = "City 2",
                        State = "State 2",
                        AddressId = 1,
                        Address = new Address
                        {
                            Id = 1, 
                            Street = "Street 2", 
                            City = "City 2"
                        }
                    }
                }
            };

            _mockRepo.Setup(repo => repo.GetAll())
                .ReturnsAsync(clubs);

            /* Act */
            var result = await _clubController.Index();

            /* Assert */
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Club>>(viewResult.Model);

            Assert.Equal(2, model.Count());
        }
        #endregion

        #region Get By Id - Unit Test
        [Fact]
        public async Task Detail_ReturnsViewResult_WithClub()
        {
            /* Arrange */
            var club = new Club
            {
                Id = 1,
                Title = "Title prova",
                Description = "Description test",
                Image = "img-test.jpg",
                AddressId = 1,
                Address = new Address
                {
                    Id = 1,
                    Street = "Street test",
                    City = "City test",
                    State = "state test"
                },
                ClubCategory = ClubCategory.Endurance,
                AppUserId = "user-1",
                AppUser = new AppUser
                {
                    Id = "user1",
                    UserName = "User 1",
                    Pace = 5,
                    Mileage = 100,
                    ProfileImageUrl = "profile1.jpg",
                    City = "City 1",
                    State = "State 1",
                    AddressId = 1,
                    Address = new Address
                    {
                        Id = 1,
                        Street = "Street 1",
                        City = "City 1"
                    }
                }
            };

            _mockRepo.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(club);

            /* Act */
            var result = await _clubController.Detail(1);

            /* Assert */
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Club>(viewResult.Model);

            Assert.Equal(club.Title, model.Title);
        }
        #endregion

        #region Create - Unit Test
        [Fact]
        public async Task Create_Post_ReturnsRedirectToActionResult_WhenModelStateIsValid()
        {
            /* Arrange */
            var createClubVM = new CreateClubViewModel
            {
                Title = "Test Club",
                Description = "Description of test club",
                AppUserId = "testUserId",
                Image = new FormFile(null, 0, 0, null, "test.jpg"), // Simula il file di immagine
                Address = new Address
                {
                    Street = "Test Street",
                    City = "Test City",
                    State = "Test State"
                }
            };

            var mockPhotoResult = new ImageUploadResult
            {
                Url = new Uri("http://testurl.com/image.jpg")
            };

            _mockPhotoService.Setup(s => s.AddPhotoAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(mockPhotoResult);

            /* Act */
            var result = await _clubController.Create(createClubVM);

            /* Assert */
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Create_Post_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            /* Arrange */
            var createClubVM = new CreateClubViewModel
            {
                Title = "Test Club",
                Description = "Description of test club",
                AppUserId = "testUserId",
                Image = new FormFile(null, 0, 0, null, "test.jpg"), // Simula il file di immagine
                Address = new Address
                {
                    Street = "Test Street",
                    City = "Test City",
                    State = "Test State"
                }
            };

            _clubController.ModelState.AddModelError("Error", "Errore nel model");

            /* Act */
            var result = await _clubController.Create(createClubVM);

            /* Assert */
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<CreateClubViewModel>(viewResult.Model);
            var modelStateErrors = _clubController.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

            Assert.Equal(createClubVM, model);
            Assert.Contains("Errore nel model", modelStateErrors);
        }
        #endregion

        #region Edit - Unit Test

        #region Edit [Get]
        [Fact]
        public async Task Edit_Get_ReturnsViewResult_WithEditClubViewModel()
        {
            /* Arrange */
            var club = new Club
            {
                Id = 1,
                Title = "Test Club edit",
                Description = "Test Description edit",
                Image = "http://testurl.com/image.jpg",
                AddressId = 1,
                Address = new Address { 
                    Street = "Test Street edit", 
                    City = "Test City edit", 
                    State = "Test State edit"
                },
                ClubCategory = ClubCategory.City
            };

            _mockRepo.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(club);

            /* Act */
            var result = await _clubController.Edit(1);

            /* Assert */
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<EditClubViewModel>(viewResult.Model);

            Assert.Equal(club.Title, model.Title);
            Assert.Equal(club.Description, model.Description);
            Assert.Equal(club.AddressId, model.AddressId);
            Assert.Equal(club.Address, model.Address);
            Assert.Equal(club.Image, model.URL);
            Assert.Equal(club.ClubCategory, model.ClubCategory);
        }

        [Fact]
        public async Task Edit_Get_ReturnsErrorView_WhenClubNotFound()
        {
            /* Arrange */
            _mockRepo.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Club)null);

            /* Act */
            var result = await _clubController.Edit(1);

            /* Assert */
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal("Error", viewResult.ViewName);
        }
        #endregion

        #region Edit - [Post]
        [Fact]
        public async Task Edit_Post_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            // Arrange
            var clubVM = new EditClubViewModel
            {
                Title = "New Title",
                Description = "New Description",
                AddressId = 123,
                Address = new Address
                {
                    Street = "Test Street edit",
                    City = "Test City edit",
                    State = "Test State edit"
                },
                Image = null
            };

            _clubController.ModelState.AddModelError("Error", "Model state non valido");

            // Act
            var result = await _clubController.Edit(1, clubVM);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.NotNull(result);
            Assert.Equal("Edit", viewResult.ViewName);
            Assert.Same(clubVM, viewResult.Model);
        }

        [Fact]
        public async Task Edit_Post_UpdatesClubAndRedirects_WhenModelStateIsValid()
        {
            // Arrange
            var clubId = 1;
            var clubVM = new EditClubViewModel
            {
                Title = "Updated Title",
                Description = "Updated Description",
                AddressId = 124,
                Address = new Address
                {
                    Street = "Test Street",
                    City = "Test City",
                    State = "Test State"
                },
                Image = new FormFile(null, 0, 0, null, "test.jpg")
            };

            var club = new Club
            {
                Id = clubId,
                Title = "Old Title",
                Description = "Old Description",
                AddressId = 123,
                Address = new Address
                {
                    Street = "Test Street",
                    City = "Test City",
                    State = "Test State"
                },
                Image = "old-image-url"
            };

            var mockPhotoResult = new ImageUploadResult
            {
                Url = new Uri("http://testurl.com/image.jpg")
            };

            _mockRepo.Setup(repo => repo.GetByIdAsyncNoTracking(clubId))
                .ReturnsAsync(club);

            _mockPhotoService.Setup(service => service.DeletePhotoAsync(club.Image))
                .ReturnsAsync(new DeletionResult { Result = "ok" });

            _mockPhotoService.Setup(service => service.AddPhotoAsync(clubVM.Image))
                .ReturnsAsync(mockPhotoResult);

            // Act
            var result = await _clubController.Edit(clubId, clubVM);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.NotNull(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            _mockRepo.Verify(repo => repo.Update(It.Is<Club>(c =>
                c.Title == clubVM.Title &&
                c.Description == clubVM.Description &&
                c.AddressId == clubVM.AddressId &&
                c.Address == clubVM.Address &&
                c.Image == "http://testurl.com/image.jpg"
            )), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_ReturnsErrorView_WhenDeletePhotoFails()
        {
            /* Arrange */
            var clubId = 1;
            var clubVM = new EditClubViewModel
            {
                Title = "Updated Title",
                Description = "Updated Description",
                AddressId = 124,
                Address = new Address
                {
                    Street = "Test Street",
                    City = "Test City",
                    State = "Test State"
                },
                Image = new FormFile(null, 0, 0, null, "test.jpg")
            };

            var club = new Club
            {
                Id = clubId,
                Title = "Old Title",
                Description = "Old Description",
                AddressId = 123,
                Address = new Address
                {
                    Street = "Test Street",
                    City = "Test City",
                    State = "Test State"
                },
                Image = "old-image-url"
            };

            _mockRepo.Setup(repo => repo.GetByIdAsync(clubId))
                .ReturnsAsync(club);

            _mockPhotoService.Setup(s => s.DeletePhotoAsync(club.Image)).ThrowsAsync(new Exception("Delete photo failed"));

            /* Act */
            var result = await _clubController.Edit(clubId, clubVM);

            /* Assert */
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.NotNull(viewResult);
            Assert.Equal("Error", viewResult.ViewName);
        }
        #endregion

        #endregion

        #region Delete - Unit Test

        #region Delete [Get]
        [Fact]
        public async Task Delete_Get_ReturnsErrorView_WhenClubNotFound()
        {
            /* Arrange */
            int cludId = 1;

            _mockRepo.Setup(repo => repo.GetByIdAsync(cludId))
                .ReturnsAsync((Club)null);

            /* Act */
            var result = await _clubController.Delete(cludId);

            /* Assert */
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.NotNull(viewResult);
            Assert.Equal("Error", viewResult.ViewName);
        }

        [Fact]
        public async Task Delete_Get_ReturnsClubDetailsView_WhenClubFound()
        {
            /* Arrange */
            int clubId = 1;
            var club = new Club 
            { 
                Id = clubId, 
                Title = "Test Club" 
            };
            
            _mockRepo.Setup(repo => repo.GetByIdAsync(clubId)).ReturnsAsync(club);

            /* Act */
            var result = await _clubController.Delete(clubId);

            /* Assert */
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal(club, viewResult.Model);
        }
        #endregion

        #region Delete [Post]
        [Fact]
        public async Task DeleteClub_Post_ReturnsErrorView_WhenClubNotFound()
        {
            /* Arrange */
            int clubId = 1;

            _mockRepo.Setup(repo => repo.GetByIdAsync(clubId))
                .ReturnsAsync((Club)null);

            /* Act */
            var result = await _clubController.DeleteClub(clubId);

            /* Assert */
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.NotNull(viewResult);
            Assert.Equal("Error", viewResult.ViewName);
        }

        [Fact]
        public async Task DeleteClub_Post_DeletesClubAndRedirectsToIndex_WhenClubFound()
        {
            /* Arrange */
            int clubId = 1;

            var club = new Club
            {
                Id = 1,
                Title = "Test Club edit",
                Description = "Test Description edit",
                Image = "http://testurl.com/image.jpg",
                AddressId = 1,
                Address = new Address
                {
                    Street = "Test Street edit",
                    City = "Test City edit",
                    State = "Test State edit"
                },
                ClubCategory = ClubCategory.City
            };

            _mockRepo.Setup(repo => repo.GetByIdAsync(clubId))
                .ReturnsAsync(club);

            /* Act */
            var result = await _clubController.DeleteClub(clubId);

            /* Assert */
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirectToActionResult.ActionName);
        }
        #endregion

        #endregion
    }
}