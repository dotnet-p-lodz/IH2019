using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImagineHackStorage.Models;
using MongoDB.Driver;

namespace ImagineHackStorage.Other
{
    public class CosmosUtils
    {
        public static IMongoDatabase ConnectToDatabase(string connectionString, string database)
        {
            var clientOptions = MongoClientSettings.FromConnectionString(connectionString);
            clientOptions.RetryWrites = false;
            var client = new MongoClient(clientOptions);
            return client.GetDatabase(database);
        }

        public static IMongoCollection<InvoiceEntity> GetCollection(IMongoDatabase database, string collection)
        {
            return database.GetCollection<InvoiceEntity>(collection);
        }

        public static async Task AddDocumentAsync(IMongoCollection<InvoiceEntity> collection, InvoiceEntity document)
        {
            await collection.InsertOneAsync(document);
        }

        public static async Task UpdateDocumentAsync(IMongoCollection<InvoiceEntity> collection, InvoiceEntity document)
        {
            await collection.ReplaceOneAsync(x => x.Id == document.Id, document);
        }

        public static async Task<IEnumerable<InvoiceEntity>> GetAllAsync(IMongoCollection<InvoiceEntity> collection)
        {
            return (await collection.FindAsync(FilterDefinition<InvoiceEntity>.Empty)).ToEnumerable();
        }

        public static async Task DeleteAsync(IMongoCollection<InvoiceEntity> collection, InvoiceEntity document)
        {
            await collection.DeleteOneAsync(x => x.Id == document.Id);
        }
    }
}