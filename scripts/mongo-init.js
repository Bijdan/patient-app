// MongoDB initialization script
// Run with: mongosh < scripts/mongo-init.js

const dbName = "PatientDb";
const db = db.getSiblingDB(dbName);

print(`Setting up database: ${dbName}`);

// Create the Patients collection with schema validation
db.createCollection("Patients", {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["FirstName", "LastName", "Email"],
      properties: {
        FirstName: {
          bsonType: "string",
          description: "Patient first name - required"
        },
        LastName: {
          bsonType: "string",
          description: "Patient last name - required"
        },
        DateOfBirth: {
          bsonType: "date",
          description: "Patient date of birth"
        },
        Email: {
          bsonType: "string",
          description: "Patient email address - required"
        },
        Phone: {
          bsonType: "string",
          description: "Patient phone number"
        },
        CreatedAt: {
          bsonType: "date",
          description: "Record creation timestamp"
        },
        UpdatedAt: {
          bsonType: "date",
          description: "Record last update timestamp"
        }
      }
    }
  }
});

print("Created Patients collection with schema validation");

// Create indexes
db.Patients.createIndex({ Email: 1 }, { unique: true });
db.Patients.createIndex({ LastName: 1 });
db.Patients.createIndex({ DateOfBirth: 1 });

print("Created indexes on Email (unique), LastName, and DateOfBirth");
print("Database initialization complete.");
