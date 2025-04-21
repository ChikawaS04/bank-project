using Microsoft.EntityFrameworkCore;
using BankApp.Models;

namespace BankApp.Data
{
    public class BankDbContext : DbContext
    {
        public BankDbContext(DbContextOptions<BankDbContext> options) : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>()
                .HasMany(c => c.Accounts)
                .WithOne(a => a.Client)
                .HasForeignKey(a => a.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed Clients
            modelBuilder.Entity<Client>().HasData(
                new Client
                {
                    ClientId = 1,
                    ClientFirstName = "John",
                    ClientMiddleName = "A.",
                    ClientLastName = "Doe",
                    ClientPhone = "9051234567",
                    ClientEmail = "john.doe@example.com",
                    ClientStreetAddress = "123 Maple Street",
                    ClientPostalCode = "A1A 1A1",
                    ClientCityAddress = "Toronto",
                    ClientCountryAddress = "Canada"
                },
                new Client
                {
                    ClientId = 2,
                    ClientFirstName = "Jane",
                    ClientMiddleName = null,
                    ClientLastName = "Smith",
                    ClientPhone = "9057654321",
                    ClientEmail = "jane.smith@example.com",
                    ClientStreetAddress = "456 Oak Avenue",
                    ClientPostalCode = "B2B 2B2",
                    ClientCityAddress = "Mississauga",
                    ClientCountryAddress = "Canada"
                }
            );
        }


    }
}
