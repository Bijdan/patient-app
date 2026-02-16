using MongoDB.Driver;
using PatientApp.Domain.Entities;
using PatientApp.Domain.Interfaces;

namespace PatientApp.Infrastructure.Repositories;

public class HealthLinkSubmissionRepository : IHealthLinkSubmissionRepository
{
    private readonly IMongoCollection<HealthLinkSubmission> _collection;

    public HealthLinkSubmissionRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<HealthLinkSubmission>("HealthLinkSubmissions");
    }

    public async Task CreateAsync(HealthLinkSubmission submission)
    {
        await _collection.InsertOneAsync(submission);
    }

    public async Task<HealthLinkSubmission?> GetByIdAsync(string id)
    {
        return await _collection.Find(s => s.Id == id).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(HealthLinkSubmission submission)
    {
        await _collection.ReplaceOneAsync(s => s.Id == submission.Id, submission);
    }
}
