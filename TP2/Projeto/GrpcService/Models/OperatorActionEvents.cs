using System.ComponentModel.DataAnnotations;

namespace GrpcService.Models {
    public class OperatorActionEvents {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public User Operator { get; set; }
        public Cobertura Cobertura { get; set; }
        public string Action { get; set; }
    }
}
