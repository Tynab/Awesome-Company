using AwesomeCompany.Entities;
using Microsoft.EntityFrameworkCore;
using static System.Linq.Enumerable;

namespace AwesomeCompany;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<Company>(b =>
        {
            _ = b.ToTable("Companies");
            _ = b.HasMany(c => c.Employees).WithOne().HasForeignKey(e => e.CompanyId).IsRequired();

            _ = b.HasData(new Company
            {
                Id = 1,
                Name = "Awesome Company"
            });
        });

        _ = modelBuilder.Entity<Employee>(b =>
        {
            _ = b.ToTable("Employees");
            _ = b.Property(e => e.Salary).HasColumnType("decimal(18, 2)");

            _ = b.HasData(Range(1, 1000).Select(i => new Employee
            {
                Id = i,
                Name = $"Employee #{i}",
                Salary = 100,
                CompanyId = 1
            }).ToList());
        });
    }
}
