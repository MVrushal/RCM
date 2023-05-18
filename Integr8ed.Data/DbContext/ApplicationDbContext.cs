using System;
using System.Collections.Generic;
using System.Text;
using Integr8ed.Data.DbModel;
using Integr8ed.Data.DbModel.SuperAdmin;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Integr8ed.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<long>, long>
    {

        #region Db Sets
     public   DbSet<ErrorLog> ErrorLog { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Database> Databases { get; set; }

        #endregion


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
           
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            // Change Default filed datatype & length
            modelBuilder.Entity<ApplicationUser>().Property(c => c.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Role>().Property(c => c.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<UserClaim>().Property(x => x.ClaimType).HasMaxLength(50);
            modelBuilder.Entity<UserClaim>().Property(x => x.ClaimValue).HasMaxLength(200);

            modelBuilder.Entity<ApplicationUser>().Property(x => x.Email).HasMaxLength(100);
            modelBuilder.Entity<ApplicationUser>().Property(x => x.UserName).HasMaxLength(100);
            modelBuilder.Entity<ApplicationUser>().Property(x => x.PhoneNumber).HasMaxLength(12);

          

            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseLazyLoadingProxies();
            base.OnConfiguring(optionsBuilder);
            
        }

    }
}
