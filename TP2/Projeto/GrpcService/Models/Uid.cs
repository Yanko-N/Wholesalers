using System.ComponentModel.DataAnnotations;

namespace GrpcService.Models {
    public class Uid {
        [Key]
        public int Id { get; set; }
        public User Operator { get; set; }
        public Cobertura Cobertura { get; set; }
        public string UID { get; set; }
    }
}