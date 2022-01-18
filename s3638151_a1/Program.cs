using Microsoft.Extensions.Configuration;
using s3638151_a1.Database;
using s3638151_a1.Services;

namespace s3638151_a1;

public static class Program
{
    private static void Main()
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        var connectionString = configuration.GetConnectionString(nameof(s3638151_a1));
        // OR
        //var connectionString = configuration.GetConnectionString("WebServiceAndDatabaseExample");

        DatabaseManager.CreateTables(connectionString);
        RestApiRequest.GetAndSaveCustomers(connectionString);
        new Menu(connectionString).Run();
    }
}