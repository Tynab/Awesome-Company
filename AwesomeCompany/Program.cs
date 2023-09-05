using AwesomeCompany;
using AwesomeCompany.Entities;
using AwesomeCompany.Models;
using AwesomeCompany.Options;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureOptions<DatabaseOptionsSetup>();

builder.Services.AddDbContext<DatabaseContext>((p, o) =>
{
    var databaseOptions = p.GetService<IOptions<DatabaseOptions>>()!.Value;

    _ = o.UseSqlServer(databaseOptions.ConnectionString, a =>
    {
        _ = a.EnableRetryOnFailure(databaseOptions.MaxRetryCount);
        _ = a.CommandTimeout(databaseOptions.CommandTimeout);
    });

    _ = o.EnableDetailedErrors(databaseOptions.EnableDetailedErrors);
    _ = o.EnableSensitiveDataLogging(databaseOptions.EnableSensitiveDataLogging);
});

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("companies/{companyId:int}", async (int companyId, DatabaseContext dbContext) =>
{
    var company = await dbContext.Set<Company>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == companyId);

    return company is null ? Results.NotFound($"The company with Id '{companyId}' was not found.") : Results.Ok(new CompanyResponse(company.Id, company.Name));
});

app.MapPut("increase-salaries", async (int companyId, DatabaseContext dbContext) =>
{
    var company = await dbContext.Set<Company>().Include(c => c.Employees).FirstOrDefaultAsync(c => c.Id == companyId);

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
    var company = await dbContext.Set<Company>().FirstOrDefaultAsync(c => c.Id == companyId);

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
    var company = await dbContext.Set<Company>().FirstOrDefaultAsync(c => c.Id == companyId);

    if (company is null)
    {
        return Results.NotFound($"The company with Id '{companyId}' was not found.");
    }

    _ = await dbContext.Database.GetDbConnection().ExecuteAsync($"UPDATE Employees SET Salary = Salary * 1.1 WHERE CompanyId = @CompanyId", new
    {
        CompanyId = company.Id
    }, (await dbContext.Database.BeginTransactionAsync()).GetDbTransaction());

    company.LastSalaryUpdateUtc = DateTime.UtcNow;
    _ = await dbContext.SaveChangesAsync();
    await dbContext.Database.CommitTransactionAsync();

    return Results.NoContent();
});

app.Run();
