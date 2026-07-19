using System.ComponentModel.DataAnnotations;


namespace JsonWebToken.Models;

public class Person
{
    [Key]
    public int PersonID { get; set; }
    public string FullName { get; set; }

}