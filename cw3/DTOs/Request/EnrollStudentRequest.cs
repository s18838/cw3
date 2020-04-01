using System;
using System.ComponentModel.DataAnnotations;

namespace cw3.DTOs
{
    public class EnrollStudentRequest
    {
        // [Required]
        // [RegularExpression("^[Ss][0-9]+$")]
        public string IndexNumber { get; set; }
        // [Required]
        public string FirstName { get; set; }
        // [Required]
        public string LastName { get; set; }
        // [Required]
        public DateTime BirthDate { get; set; }
        // [Required]
        public string Studies { get; set; }
    }
}