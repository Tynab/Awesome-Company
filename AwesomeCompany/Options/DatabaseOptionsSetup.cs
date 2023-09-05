using Microsoft.Extensions.Options;

namespace AwesomeCompany.Options;

public sealed class DatabaseOptionsSetup : IConfigureOptions<DatabaseOptions>
{
    private const string ConfigurationSectionName = "DatabaseOptions";
    private readonly IConfiguration _configuration;

    public DatabaseOptionsSetup(IConfiguration configuration) => _configuration = configuration;

    public void Configure(DatabaseOptions options)
    {
        options.ConnectionString = _configuration.GetConnectionString("Default")!;
        _configuration.GetSection(ConfigurationSectionName).Bind(options);
    }
}
