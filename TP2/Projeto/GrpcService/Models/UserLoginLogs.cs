using System.ComponentModel.DataAnnotations;

namespace GrpcService.Models {

    public class UserLoginLogs {
        [Key]
        public int Id { get; set; }
        [Required]
        public string User { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string LogMessage { get; set; }
        public string? Token { get; set; }
    }
}
