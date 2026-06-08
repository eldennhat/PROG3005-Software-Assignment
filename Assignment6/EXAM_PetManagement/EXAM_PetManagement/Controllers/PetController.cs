using Microsoft.AspNetCore.Mvc;
using EXAM_PetManagement.Models;
using System.Collections.Generic;
using System.Linq;

namespace EXAM_PetManagement.Controllers;

public class PetController : Controller {
        // Giả lập Database bằng static List
        private static List<Pet> pets = new List<Pet>
        {
            new Pet { Id = 1, Name = "Milo", Species = "Corgi", Age = 2, Price = 5000 },
            new Pet { Id = 2, Name = "Mim", Species = "British Shorthair", Age = 1, Price = 3000 }
        };

        // 1. List and Search
        public IActionResult Index(string searchString)
        {
            var petList = pets.ToList();
            
            // filter by searchString
            if (!string.IsNullOrEmpty(searchString))
            {
                petList = petList.Where(p => p.Name.ToLower().Contains(searchString.ToLower()) || 
                                             p.Species.ToLower().Contains(searchString.ToLower())).ToList();
            }

            return View(petList);
        }

        // 2. Detail
        public IActionResult Detail(int id)
        {
            var pet = pets.FirstOrDefault(p => p.Id == id);
            if (pet == null) return NotFound();
            return View(pet);
        }

        // 3. CREATE
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Pet model)
        {
            if (ModelState.IsValid)
            {
                // Tự động tăng Id
                model.Id = pets.Count > 0 ? pets.Max(p => p.Id) + 1 : 1;
                pets.Add(model);

                // Success Notification
                TempData["SuccessMessage"] = "Pet added successfully!";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // 4. EDIT
        public IActionResult Edit(int id)
        {
            var pet = pets.FirstOrDefault(p => p.Id == id);
            if (pet == null) return NotFound();
            return View(pet);
        }

        [HttpPost]
        public IActionResult Edit(Pet model)
        {
            if (ModelState.IsValid)
            {
                var existingPet = pets.FirstOrDefault(p => p.Id == model.Id);
                if (existingPet != null)
                {
                    existingPet.Name = model.Name;
                    existingPet.Species = model.Species;
                    existingPet.Age = model.Age;
                    existingPet.Price = model.Price;

                    TempData["SuccessMessage"] = "Information updated successfully!";
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        // 5. DELETE
        public IActionResult Delete(int id)
        {
            var pet = pets.FirstOrDefault(p => p.Id == id);
            if (pet == null) return NotFound();
            return View(pet); // Trả về trang xác nhận xóa
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var pet = pets.FirstOrDefault(p => p.Id == id);
            if (pet != null)
            {
                pets.Remove(pet);
                TempData["SuccessMessage"] = "Pet deleted successfully!";
            }
            return RedirectToAction("Index");
        }
    }