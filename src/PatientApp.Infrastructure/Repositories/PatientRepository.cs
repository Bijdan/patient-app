using MongoDB.Bson;
using MongoDB.Driver;
using PatientApp.Domain.Entities;
using PatientApp.Domain.Interfaces;

namespace PatientApp.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly IMongoCollection<Patient> _collection;

    public PatientRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Patient>("Patients");
    }

    public async Task<IEnumerable<Patient>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<Patient?> GetByIdAsync(string id)
    {
        return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Patient?> GetByEmailAsync(string email)
    {
        return await _collection.Find(p => p.Email == email).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Patient patient)
    {
        await _collection.InsertOneAsync(patient);
    }

    public async Task UpdateAsync(Patient patient)
    {
        await _collection.ReplaceOneAsync(p => p.Id == patient.Id, patient);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(p => p.Id == id);
    }
}
