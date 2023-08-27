using AwesomeCompany;
using AwesomeCompany.Entities;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DatabaseContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

app.UseHttpsRedirection();

app.MapPut("increase-salaries", async (int companyId, DatabaseContext dbContext) =>
{
    var company = await dbContext
    .Set<Company>()
    .Include(c => c.Employees)
    .FirstOrDefaultAsync(c => c.Id == companyId);

    if (company is null)
    {
        return Results.NotFound($"The company with Id '{companyId}' was not found.");
    }

    company.Employees?.ForEach(e => e.Salary *= 1.1m);
    company.LastSalaryUpdateUtc = DateTime.UtcNow;
    _ = await dbContext.SaveChangesAsync();

    return Results.NoContent();
});

app.MapPut("increase-salaries-sql", async (int companyId, DatabaseContext dbContext) =>
{
    var company = await dbContext
    .Set<Company>()
    .FirstOrDefaultAsync(c => c.Id == companyId);

    if (company is null)
    {
        return Results.NotFound($"The company with Id '{companyId}' was not found.");
    }

    _ = await dbContext.Database.BeginTransactionAsync();
    _ = await dbContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE Employees SET Salary = Salary * 1.1 WHERE CompanyId = {companyId}");
    company.LastSalaryUpdateUtc = DateTime.UtcNow;
    _ = await dbContext.SaveChangesAsync();
    await dbContext.Database.CommitTransactionAsync();

    return Results.NoContent();
});

app.MapPut("increase-salaries-sql-dapper", async (int companyId, DatabaseContext dbContext) =>
{
    var company = await dbContext
    .Set<Company>()
    .FirstOrDefaultAsync(c => c.Id == companyId);

    if (company is null)
    {
        return Results.NotFound($"The company with Id '{companyId}' was not found.");
    }

    var transaction = await dbContext.Database.BeginTransactionAsync();

    _ = await dbContext.Database.GetDbConnection().ExecuteAsync($"UPDATE Employees SET Salary = Salary * 1.1 WHERE CompanyId = @CompanyId", new
    {
        CompanyId = company.Id
    }, transaction.GetDbTransaction());

    company.LastSalaryUpdateUtc = DateTime.UtcNow;
    _ = await dbContext.SaveChangesAsync();
    await dbContext.Database.CommitTransactionAsync();

    return Results.NoContent();
});

app.Run();
