using System.ComponentModel.DataAnnotations;
using System.Runtime.Versioning;

namespace API.DTOs
  {

    public class RegisterDto
      {

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

      }

  }