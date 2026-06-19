using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIDTERM.Models;

[Table("DishCategory_BIT240199")]
public class DishCategory_BIT240199 {
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Dish category name is required.")]
    [StringLength(100)]
    public string Name { get; set; }

    public string? Description { get; set; }

    // Relationship: One category has many dishes
    public virtual ICollection<Dish_BIT240199> Dishes { get; set; } = new List<Dish_BIT240199>();
}