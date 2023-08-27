using AwesomeCompany.Entities;
using Microsoft.EntityFrameworkCore;
using static System.Linq.Enumerable;

namespace AwesomeCompany;

public sealed class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(b =>
        {
            b.ToTable("Companies");
            b
            .HasMany(c => c.Employees)
            .WithOne()
            .HasForeignKey(e => e.CompanyId)
            .IsRequired();
            b.HasData(new Company
            {
                Id = 1,
                Name = "Awesome Company"
            });
        });

        modelBuilder.Entity<Employee>(b =>
        {
            b.ToTable("Employees");

            var employees = Range(1, 1000)
            .Select(i => new Employee
            {
                Id = i,
                Name = $"Employee #{i}",
                Salary = 100,
                CompanyId = 1
            })
            .ToList();

            b.HasData(employees);
        });
    }
}
