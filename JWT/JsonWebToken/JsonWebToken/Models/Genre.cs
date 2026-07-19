using System.ComponentModel.DataAnnotations;

namespace JsonWebToken.Models;

public class Genre
{
    [Key]
    public int GenreID { get; set; }
    public string Name { get; set; }

    // Navigation property
    public ICollection<MovieGenre> MovieGenres { get; set; }
}