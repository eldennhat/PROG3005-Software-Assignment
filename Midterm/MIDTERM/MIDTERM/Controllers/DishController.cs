using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MIDTERM.Data;
using MIDTERM.Models;

namespace MIDTERM.Controllers
{
    public class DishController : Controller
    {
        private readonly DishRepository _dishRepository;

        public DishController(DishRepository dishRepository)
        {
            _dishRepository = dishRepository;
        }

        // REQUIREMENT 2 & 3: DISH LIST WITH ADVANCED SEARCH FILTERING
        public IActionResult Index(string? searchTerm, int? categoryId, string? availability, decimal? minPrice, decimal? maxPrice, string? sortBy)
        {
            // Requirement 6: Catch invalid price ranges safely
            if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
            {
                ModelState.AddModelError("", "Invalid price range: Min price cannot be greater than Max price.");
                ViewBag.Categories = new SelectList(_dishRepository.GetAllCategories(), "Id", "Name");
                return View(new List<Dish_BIT240199>());
            }

            // Perform native SQL database queries directly (no loop filters)
            var list = _dishRepository.SearchDishes(searchTerm, categoryId, availability, minPrice, maxPrice, sortBy);

            // Retain search states to pre-populate inputs after postbacks
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SelectedAvailability = availability;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortBy = sortBy;

            // Load category select options dynamically for filter UI elements
            ViewBag.Categories = new SelectList(_dishRepository.GetAllCategories(), "Id", "Name", categoryId);

            // Requirement 6: Catch empty data tracking results
            if (!list.Any())
            {
                ViewBag.NoResultIndexMessage = "No dishes found matching your criteria.";
            }

            return View(list);
        }

        // REQUIREMENT 4 & 6: VIEW MULTIPLE IMAGES IN DETAIL VIEW WITH ERROR HANDLING
        public IActionResult Detail(int id)
        {
            var dish = _dishRepository.GetDishById(id);
            if (dish == null)
            {
                return NotFound("Requirement 6 Error: DishId does not exist.");
            }

            // Load all auxiliary image assets mapped onto this targeted record
            dish.DishImages = _dishRepository.GetImagesByDishId(id);
            return View(dish);
        }

        // GET: CREATE NEW DISH RECORD
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.DishCategoryId = new SelectList(_dishRepository.GetAllCategories(), "Id", "Name");
            return View();
        }

        // POST: CREATE NEW DISH RECORD WITH VALIDATIONS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Dish_BIT240199 dish)
        {
            // Business Rule constraint check: Unique dish name within identical categories
            if (_dishRepository.IsDishNameExists(dish.Name, null, dish.DishCategoryId))
            {
                ModelState.AddModelError("Name", "This dish name already exists within the selected category.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DishCategoryId = new SelectList(_dishRepository.GetAllCategories(), "Id", "Name", dish.DishCategoryId);
                return View(dish);
            }

            _dishRepository.AddDish(dish);

            TempData["SuccessMessage"] = "Add dish successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: CHOOSE TARGET DISH AND OPEN EDIT VIEW
        public IActionResult Edit(int id)
        {
            var dish = _dishRepository.GetDishById(id);
            if (dish == null)
            {
                return NotFound("Requirement 6 Error: DishId does not exist.");
            }

            ViewBag.DishCategoryId = new SelectList(_dishRepository.GetAllCategories(), "Id", "Name", dish.DishCategoryId);
            return View(dish);
        }

        // POST: MODIFY CHOSEN DISH RECORD DATA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Dish_BIT240199 dish)
        {
            if (id != dish.Id)
            {
                return BadRequest();
            }

            // Verify naming availability overlaps across other matching records
            if (_dishRepository.IsDishNameExists(dish.Name, dish.Id, dish.DishCategoryId))
            {
                ModelState.AddModelError("Name", "This dish name enters a conflict with another dish in the same category.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DishCategoryId = new SelectList(_dishRepository.GetAllCategories(), "Id", "Name", dish.DishCategoryId);
                return View(dish);
            }

            if (!_dishRepository.UpdateDish(dish))
            {
                return NotFound("Requirement 6 Error: DishId does not exist.");
            }

            TempData["SuccessMessage"] = "Update dish successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: CHOOSE TARGET DISH AND OPEN CONFIRMATION FOR DELETING
        public IActionResult Delete(int id)
        {
            var dish = _dishRepository.GetDishById(id);
            if (dish == null)
            {
                return NotFound("Requirement 6 Error: DishId does not exist.");
            }

            return View(dish);
        }

        // POST: DELETE DISH FROM PERSISTENT DATABASE STORAGE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!_dishRepository.DeleteDish(id))
            {
                return NotFound("Requirement 6 Error: DishId does not exist.");
            }

            TempData["SuccessMessage"] = "Delete dish successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddImage(int dishId, string? imageUrl, bool isThumbnail, IFormFile? imageFile,
            [FromServices] IWebHostEnvironment env)
        {
            string? finalImageUrl = imageUrl;

            // Handle file upload if a file is provided from local machine
            if (imageFile != null && imageFile.Length > 0) {
                var extension = Path.GetExtension(imageFile.FileName).ToLower();
                if (extension != ".jpg" && extension != ".png" && extension != ".jpeg") {
                    TempData["ErrorImage"] = "Error: Only .jpg, .jpeg and .png files are allowed!";
                    return RedirectToAction(nameof(Detail), new { id = dishId });
                }

                string uploadsFolder = Path.Combine(env.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder)) {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create)) {
                    imageFile.CopyTo(fileStream);
                }

                finalImageUrl = "/images/" + uniqueFileName;
            }

            // Validation check if both options are left empty
            if (string.IsNullOrEmpty(finalImageUrl)) {
                TempData["ErrorImage"] = "Please either provide an image URL link or upload a local file asset.";
                return RedirectToAction(nameof(Detail), new { id = dishId });
            }

            var dish = _dishRepository.GetDishById(dishId);
            if (dish == null) {
                return NotFound("Requirement 6 Error: DishId does not exist.");
            }

            var img = new DishImage_BIT240199 {
                DishId = dishId,
                ImageUrl = finalImageUrl,
                IsThumbnail = isThumbnail
            };

            _dishRepository.AddDishImage(img);
            TempData["SuccessMessage"] = "Image asset attached successfully!";
            return RedirectToAction(nameof(Detail), new { id = dishId });
        }

        public IActionResult ChangeThumbnail(int imageId, int dishId)
        {
            var img = _dishRepository.GetImageById(imageId);
            if (img == null)
            {
                return NotFound("Requirement 6 Error: DishImageId does not exist.");
            }

            _dishRepository.SetThumbnail(imageId, dishId);
            return RedirectToAction(nameof(Detail), new { id = dishId });
        }

        // ==================== DISH CATEGORY CRITERIA HANDLING (Requirement 5) ====================

        public IActionResult Categories()
        {
            var list = _dishRepository.GetAllCategories();
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategory(int id)
        {
            var cat = _dishRepository.GetCategoryById(id);
            if (cat == null)
            {
                return NotFound("Requirement 6 Error: DishCategoryId does not exist.");
            }

            // Requirement 5 constraint rule handling: Block if child items remain attached
            if (_dishRepository.CheckCategoryHasDishes(id))
            {
                TempData["CategoryDeleteError"] = $"Cannot delete category '{cat.Name}' because it currently contains active assigned dishes!";
                return RedirectToAction(nameof(Categories));
            }

            _dishRepository.DeleteCategory(id);
            TempData["SuccessMessage"] = "Dish category was successfully removed.";
            return RedirectToAction(nameof(Categories));
        }
    }
}