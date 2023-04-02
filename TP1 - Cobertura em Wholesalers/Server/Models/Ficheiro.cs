using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aula_2___Sockets.Models {
    public class Ficheiro {
        [Key]
        public string Hash { get; set; }
    }
}
