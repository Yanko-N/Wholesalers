using System.ComponentModel.DataAnnotations;

namespace GrpcService.Models {
    public class User {
        [Required]
        [Key]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
