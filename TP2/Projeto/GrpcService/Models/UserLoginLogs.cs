using System.ComponentModel.DataAnnotations;

namespace GrpcService.Models {

    public class UserLoginLogs {
        [Key]
        public int Id { get; set; }
        public User user;
        public DateTime Date;
        public string LogMessage;
        public string? Token;
    }
}
