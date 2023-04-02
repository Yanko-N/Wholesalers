using Aula_2___Sockets.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aula_2___Sockets
{
    public class dataContext : DbContext
    {
        public DbSet<Cobertura> Coberturas { get; set; }
        public DbSet<Logs> Logs { get; set; }
        public DbSet<Ficheiro> Ficheiros { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=DataDB;Trusted_Connection=True;");
        }
        
    }
}
