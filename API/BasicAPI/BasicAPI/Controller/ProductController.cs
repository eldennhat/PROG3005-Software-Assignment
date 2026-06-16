using Microsoft.AspNetCore.Mvc;

namespace BasicAPI.Controller;
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase {
    //Mock db for this assignment
    private static List<Product> _products = new List<Product>();
    // POST
    [HttpPost]
    public IActionResult CreateProduct(Product product) {
        product.Id = _products.Count + 1;
        _products.Add(product);
        return Ok();
    }
    //GET
    [HttpGet("{id}")]
    public IActionResult GetProducts(int id) {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null) {
            return NotFound("Product not found");
        }
        return Ok(product);
    }
}