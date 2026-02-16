// MongoDB seed script
// Run with: mongosh < scripts/mongo-seed.js
// Run after: mongosh < scripts/mongo-init.js

const dbName = "PatientDb";
const db = db.getSiblingDB(dbName);

print(`Seeding database: ${dbName}`);

const now = new Date();

const patients = [
  {
    FirstName: "John",
    LastName: "Doe",
    DateOfBirth: new Date("1985-03-15"),
    Email: "john.doe@example.com",
    Phone: "+1-555-0101",
    CreatedAt: now,
    UpdatedAt: now
  },
  {
    FirstName: "Jane",
    LastName: "Smith",
    DateOfBirth: new Date("1990-07-22"),
    Email: "jane.smith@example.com",
    Phone: "+1-555-0102",
    CreatedAt: now,
    UpdatedAt: now
  },
  {
    FirstName: "Robert",
    LastName: "Johnson",
    DateOfBirth: new Date("1978-11-08"),
    Email: "robert.johnson@example.com",
    Phone: "+1-555-0103",
    CreatedAt: now,
    UpdatedAt: now
  },
  {
    FirstName: "Emily",
    LastName: "Williams",
    DateOfBirth: new Date("1995-01-30"),
    Email: "emily.williams@example.com",
    Phone: "+1-555-0104",
    CreatedAt: now,
    UpdatedAt: now
  },
  {
    FirstName: "Michael",
    LastName: "Brown",
    DateOfBirth: new Date("1982-09-12"),
    Email: "michael.brown@example.com",
    Phone: "+1-555-0105",
    CreatedAt: now,
    UpdatedAt: now
  }
];

const result = db.Patients.insertMany(patients);
print(`Inserted ${result.insertedIds.length} patient records`);
print("Seed data inserted successfully.");
