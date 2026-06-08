using System.ComponentModel.DataAnnotations;

namespace EXAM_PetManagement.Models;

public class Pet {
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Cannot be empty pet name")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "Cannot be empty pet species (e.g. Dog, Cat, Bird, etc.)")]
    public string Species { get; set; }
    
    [Range(1, 100, ErrorMessage = "Age must be between 1 and 100")]
    public int Age { get; set; }
    
    [Range(0, 1000000, ErrorMessage = "Price must be more than 0")]
    public decimal Price { get; set; }
}