namespace BasicAPI;
using System.ComponentModel.DataAnnotations;

public class Product {
    public int Id { get; set; }
    
    [Required( ErrorMessage = "Product name is required")]
    [MinLength(3, ErrorMessage = "Product name must be at least 3 characters long")]
    public string Name { get; set; }
    
    [Required( ErrorMessage = "Product price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Product price must be greater than 0")]
    public decimal Price { get; set; }
}