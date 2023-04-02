using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aula_2___Sockets.Models
{
    public class Cobertura
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Operador { get; set; }

        [Required]
        public string Municipio { get; set; }

        [Required]
        public string Rua { get; set; }

        [Required]
        public string Numero { get; set; }

        public string Apartamento { get; set; } = null;

        [Required]
        public Boolean Owner { get; set; }
    }
}
