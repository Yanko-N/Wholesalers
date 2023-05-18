using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcService.Models {
    public class Ficheiro {
        [Key]
        public string Hash { get; set; }
    }
}
