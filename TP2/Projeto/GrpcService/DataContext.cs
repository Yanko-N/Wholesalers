using GrpcService.Models;
using Microsoft.EntityFrameworkCore;

namespace GrpcService {
    public class dataContext : DbContext {
        public DbSet<Cobertura> Coberturas { get; set; }
        public DbSet<Logs> Logs { get; set; }
        public DbSet<Ficheiro> Ficheiros { get; set; }
        public DbSet<User> Users { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=DataDB;Trusted_Connection=True;");
        }

    }
}
