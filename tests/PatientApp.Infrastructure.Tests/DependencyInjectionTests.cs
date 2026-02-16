using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PatientApp.Application.Interfaces;
using PatientApp.Domain.Interfaces;
using PatientApp.Infrastructure;

namespace PatientApp.Infrastructure.Tests;

public class DependencyInjectionTests
{
    private static IConfiguration BuildConfiguration(
        string connectionString = "mongodb://localhost:27017",
        string databaseName = "TestDb")
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["MongoDbSettings:ConnectionString"] = connectionString,
            ["MongoDbSettings:DatabaseName"] = databaseName,
            ["FileStorageSettings:BasePath"] = "/tmp/test-health-links"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public void Given_MissingMongoDbSettings_When_AddInfrastructure_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act
        var act = () => services.AddInfrastructure(config);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*MongoDbSettings*");
    }

    [Fact]
    public void Given_ValidConfig_When_AddInfrastructure_Then_RegistersIPatientRepository()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = BuildConfiguration();

        // Act
        services.AddInfrastructure(config);

        // Assert
        services.Should().Contain(sd =>
            sd.ServiceType == typeof(IPatientRepository) &&
            sd.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void Given_ValidConfig_When_AddInfrastructure_Then_RegistersIPatientService()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = BuildConfiguration();

        // Act
        services.AddInfrastructure(config);

        // Assert
        services.Should().Contain(sd =>
            sd.ServiceType == typeof(IPatientService) &&
            sd.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void Given_ValidConfig_When_AddInfrastructure_Then_RegistersIMongoClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = BuildConfiguration();

        // Act
        services.AddInfrastructure(config);

        // Assert
        services.Should().Contain(sd =>
            sd.ServiceType == typeof(IMongoClient) &&
            sd.Lifetime == ServiceLifetime.Singleton);
    }
}
