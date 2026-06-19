using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIDTERM.Models;

[Table("DishImage_BIT240199")]
public class DishImage_BIT240199 {
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Image URL is required.")]
    public string ImageUrl { get; set; }

    public bool IsThumbnail { get; set; } = false;

    [Required]
    public int DishId { get; set; }

    [ForeignKey("DishId")]
    public virtual Dish_BIT240199? Dish { get; set; }
}