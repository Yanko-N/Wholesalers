using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aula_2___Sockets.Models {
    public class Logs {
        [Required]
        public int ID { get; set; }
        [Required]
        public DateTime DataInicio { get; set; }
        [Required]
        public string Ficheiro { get; set; }
        [Required]
        public string Operador { get; set; }
        [Required]
        public string Estado { get; set; }

    }
}
