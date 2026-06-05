using System.ComponentModel.DataAnnotations;

namespace Validation.Models;

public class BookViewModel {
    public int Id { get; set; }
    [Required(ErrorMessage = "Tên sách không được để trống")]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Giá sách phải lớn hơn 0")] //Quy tắc validation
    public int Price { get; set; }
}