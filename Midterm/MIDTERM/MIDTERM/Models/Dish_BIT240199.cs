using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIDTERM.Models;

[Table("Dish_BIT240199")]
public class Dish_BIT240199 {
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Dish name is required.")]
    [StringLength(100)]
    public string Name { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Preparation time is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Preparation time must be greater than 0 minutes.")]
    public int PreparationTime { get; set; }

    public bool IsAvailable { get; set; } = true;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Please select a dish category.")]
    public int DishCategoryId { get; set; }

    // Navigation properties
    [ForeignKey("DishCategoryId")]
    public virtual DishCategory_BIT240199? DishCategory { get; set; }

    public virtual ICollection<DishImage_BIT240199> DishImages { get; set; } = new List<DishImage_BIT240199>();

    // Calculated property (Requirement 3) - Not mapped to Database
    [NotMapped]
    public string PreparationType => PreparationTime <= 15 ? "Fast" : "Normal";
}