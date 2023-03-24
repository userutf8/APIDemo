using APIDemo.Model.DB;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIDemo.Model
{
    public class Context: IdentityUserContext<IdentityUser> // only Users, Claims, Logins and Tokens
    {
        public Context(DbContextOptions<Context> opt) : base(opt) { }
        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientFile> ClientFiles { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<APIDemo.Model.DB.File> Files { get; set; }
        public DbSet<Person> Persons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<Client>().Property(c => c.Id).HasDefaultValueSql("NEWID()");
            //modelBuilder.Entity<ClientFile>().Property(c => c.Id).ValueGeneratedOnAdd();
            //modelBuilder.Entity<APIDemo.Model.DB.File>().Property(c => c.Id).ValueGeneratedOnAdd();
            //modelBuilder.Entity<Company>().Property(c => c.Id).ValueGeneratedOnAdd();
            //modelBuilder.Entity<Person>().Property(c => c.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<IdentityUser>().
                HasMany<Client>().WithOne().
                HasForeignKey(cli => cli.UserId).
                OnDelete(DeleteBehavior.Cascade); // TODO: verify

            modelBuilder.Entity<Client>().
                HasMany<Company>().WithOne().
                HasForeignKey(cli => cli.ClientId).
                OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Client>().
                HasMany<Person>().WithOne().
                HasForeignKey(cli => cli.ClientId).
                OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Client>().
                HasMany<ClientFile>().WithOne().
                HasForeignKey(cli => cli.ClientId).
                OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<APIDemo.Model.DB.File>().
                HasMany<ClientFile>().WithOne().
                HasForeignKey(cli => cli.FileId).
                OnDelete(DeleteBehavior.Cascade);
        }
    }
}
