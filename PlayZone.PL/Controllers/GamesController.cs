using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PlayZone.BLL.Interfaces;
using PlayZone.DAL.Models;
using PlayZone.PL.Helpers;
using PlayZone.PL.ViewModels;

namespace PlayZone.PL.Controllers
{
    public class GamesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public GamesController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index()
        {
            var games = await _unitOfWork.GameRepository.GetAllAsync();
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync();
            var devices = await _unitOfWork.DeviceRepository.GetAllAsync();

            return View(games);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync();
            var devices = await _unitOfWork.DeviceRepository.GetAllAsync();

            var viewModel = new CreateGameFormViewModel
            {
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }),

                Devices = devices.Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                })

            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateGameFormViewModel game)
        {
            if (ModelState.IsValid)
            {
                string coverFileName = null;
                if (game.Cover != null)
                {
                    coverFileName = await DocumentSettings.UploadFileAsync(game.Cover, "assets/images/games");
                }

                var gameEntity = new Game
                {
                    Name = game.Name,
                    Description = game.Description,
                    CategoryId = game.CategoryId,
                    Cover = coverFileName,
                    Device = game.SelectedDevices.Select(d => new GameDevice
                    {
                        DeviceId = d
                    }).ToList(),
                };

                await _unitOfWork.GameRepository.CreateAsync(gameEntity);
                await _unitOfWork.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            var categories = await _unitOfWork.CategoryRepository.GetAllAsync();
            var devices = await _unitOfWork.DeviceRepository.GetAllAsync();

            var viewModel = new CreateGameFormViewModel
            {
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }),

                Devices = devices.Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                })
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid Id)
        {
            if (Id == Guid.Empty)
            {
                return NotFound();
            }
            var game = await _unitOfWork.GameRepository.GetByIdAsync(Id);
            if (game == null)
            {
                return NotFound();
            }
            return View(game);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var game = await _unitOfWork.GameRepository.GetByIdAsync(id);
            if (game == null)
                return NotFound();

            var categories = await _unitOfWork.CategoryRepository.GetAllAsync();
            var devices = await _unitOfWork.DeviceRepository.GetAllAsync();

            // Get the devices associated with this game
            var gameDevices = game.Device.Select(d => d.DeviceId).ToList();

            var viewModel = new EditGameFormViewModel
            {
                Id = game.Id,
                Name = game.Name,
                Description = game.Description,
                CategoryId = game.CategoryId,
                CurrentCover = game.Cover,
                SelectedDevices = gameDevices,
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }),
                Devices = devices.Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                })
            };

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditGameFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Get the existing game
                var existingGame = await _unitOfWork.GameRepository.GetByIdAsync(model.Id);
                if (existingGame == null)
                    return NotFound();

                // Handle cover image - if no new image is uploaded, keep the current one
                if (model.Cover != null)
                {
                    // Store the old cover filename for deletion
                    string oldCoverFileName = existingGame.Cover;

                    // Upload new image
                    existingGame.Cover = await DocumentSettings.UploadFileAsync(model.Cover, "assets/images/games");

                    // Delete the old image if it exists and is different from the default image
                    if (!string.IsNullOrEmpty(oldCoverFileName) && oldCoverFileName != "default.jpg")
                    {
                        DeleteOldImage(oldCoverFileName, "assets/images/games");
                    }
                }

                // Update other properties
                existingGame.Name = model.Name;
                existingGame.Description = model.Description;
                existingGame.CategoryId = model.CategoryId;

                // Handle devices
                existingGame.Device.Clear();
                foreach (var deviceId in model.SelectedDevices)
                {
                    existingGame.Device.Add(new GameDevice
                    {
                        DeviceId = deviceId,
                        GameId = existingGame.Id
                    });
                }

                // Update the game
                _unitOfWork.GameRepository.UpdateAsync(existingGame);
                await _unitOfWork.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            // If ModelState is invalid, reload the form with necessary data
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync();
            var devices = await _unitOfWork.DeviceRepository.GetAllAsync();

            model.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            });

            model.Devices = devices.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name
            });

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var game = await _unitOfWork.GameRepository.GetByIdAsync(id);
            if (game == null)
                return NotFound();

            // Delete the cover image if it exists and is not the default image
            if (!string.IsNullOrEmpty(game.Cover))
            {
                DeleteOldImage(game.Cover, "assets/images/games");
            }

            _unitOfWork.GameRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        private void DeleteOldImage(string fileName, string folderPath)
        {
            try
            {
                // Get the full path to the file
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderPath, fileName);

                // Check if file exists before attempting to delete
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error deleting file: {ex.Message}");
            }
        }

    }
}
