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

        // Bind settings
        var settings = configuration
            .GetSection("MongoDbSettings")
            .Get<MongoDbSettings>()
            ?? throw new InvalidOperationException("MongoDbSettings configuration is missing.");

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

        // Register application services
        services.AddScoped<IPatientService, PatientService>();

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
    }
}
