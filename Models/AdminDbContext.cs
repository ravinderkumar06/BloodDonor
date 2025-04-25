
using Microsoft.EntityFrameworkCore;
namespace BloodDonor.Models
{
    public class AdminDbContext:DbContext
    {

        public DbSet<LoginProperties> AdmingTbl { get; set; }
        public DbSet<StateProprties> State { get; set; }
        public DbSet<City> Cities { get; set; }
     
        public DbSet<BloodGroup> BloodGroup { get; set; } 
            public DbSet<BloodDonorProperty> BloodDonors { get; set; }

        public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options) { }

            

    }
}
