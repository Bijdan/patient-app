using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using PatientApp.Application.Interfaces;
using PatientApp.Application.Services;
using PatientApp.Domain.Common;
using PatientApp.Domain.Entities;
using PatientApp.Domain.Interfaces;
using PatientApp.Infrastructure.Repositories;
using PatientApp.Infrastructure.Services;
using PatientApp.Infrastructure.Settings;

namespace PatientApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure MongoDB class maps (keeps Domain entities free of MongoDB attributes)
        RegisterClassMaps();

        // Bind MongoDB settings
        var settings = configuration
            .GetSection("MongoDbSettings")
            .Get<MongoDbSettings>()
            ?? throw new InvalidOperationException("MongoDbSettings configuration is missing.");

        // Bind file storage settings
        var fileStorageSettings = configuration
            .GetSection("FileStorageSettings")
            .Get<FileStorageSettings>()
            ?? throw new InvalidOperationException("FileStorageSettings configuration is missing.");
        services.AddSingleton(fileStorageSettings);

        // Bind health link settings
        var healthLinkSettings = configuration
            .GetSection("HealthLinkSettings")
            .Get<HealthLinkSettings>()
            ?? new HealthLinkSettings();
        services.AddSingleton(healthLinkSettings);

        // Register MongoClient as singleton
        services.AddSingleton<IMongoClient>(_ => new MongoClient(settings.ConnectionString));

        // Register IMongoDatabase
        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(settings.DatabaseName);
        });

        // Register repositories
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IHealthLinkSubmissionRepository, HealthLinkSubmissionRepository>();

        // Register infrastructure services
        services.AddScoped<IFhirBundleParser, FhirBundleParser>();
        services.AddScoped<IEncryptionService, AesGcmEncryptionService>();
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IJweService, JweService>();

        // Register application services
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IHealthLinkService>(sp =>
            new HealthLinkService(
                sp.GetRequiredService<IFhirBundleParser>(),
                sp.GetRequiredService<IEncryptionService>(),
                sp.GetRequiredService<IFileStorageService>(),
                sp.GetRequiredService<IJweService>(),
                sp.GetRequiredService<IHealthLinkSubmissionRepository>(),
                healthLinkSettings.DefaultExpiryHours));

        return services;
    }

    private static void RegisterClassMaps()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(BaseEntity)))
        {
            BsonClassMap.RegisterClassMap<BaseEntity>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id)
                    .SetIdGenerator(StringObjectIdGenerator.Instance)
                    .SetSerializer(new StringSerializer(BsonType.ObjectId));
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(Patient)))
        {
            BsonClassMap.RegisterClassMap<Patient>(cm =>
            {
                cm.AutoMap();
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(HealthLinkSubmission)))
        {
            BsonClassMap.RegisterClassMap<HealthLinkSubmission>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id)
                    .SetSerializer(new StringSerializer(BsonType.String));
            });
        }
    }
}
