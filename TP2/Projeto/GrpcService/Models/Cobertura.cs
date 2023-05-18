using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcService.Models {
    public class Cobertura
    {
        [Key]
        public int Id { get; set; }

        public string Operador { get; set; }

        public string Municipio { get; set; }

        public string Rua { get; set; }

        public string Numero { get; set; }

        public string? Apartamento { get; set; }

        public Boolean Owner { get; set; }
        
        public string Estado { get; set; }
        
    }
}
