using System;
using System.Threading.Tasks;
using ImagineHackStorage.Models;
using ImagineHackStorage.Other;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ImagineHackStorage.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DemoController : ControllerBase
    {
        private readonly string _storageConnString;
        private readonly string _cosmosMongoConnString;

        public DemoController(IConfiguration configuration)
        {
            _storageConnString = configuration["ConnectionStrings:StorageAccount"];
            _cosmosMongoConnString = configuration["ConnectionStrings:CosmosDBMongo"];
        }

        [HttpGet("table")]
        public async Task<IActionResult> BasicAzureTable()
        {
            var storageTable = await StorageUtils.CreateTableAsync(_storageConnString, "Customers");
            var customer = new CustomerEntity("Black", "Jack")
            {
                Email = "jackblack@mail.com",
                PhoneNumber = "323 204 912"
            };

            customer = await StorageUtils.InsertOrMergeEntityAsync(storageTable, customer);

            customer.PhoneNumber = "231 444 023";
            await StorageUtils.InsertOrMergeEntityAsync(storageTable, customer);

            customer = await StorageUtils.RetrieveEntityUsingPointQueryAsync(storageTable, "Black", "Jack");

            await StorageUtils.DeleteEntityAsync(storageTable, customer);
            return Ok(customer);
        }

        [HttpGet("blob")]
        public async Task<IActionResult> BasicBlob()
        {
            var containerClient = await StorageUtils.CreateBlobContainerAsync(_storageConnString, "demoblobs");
            var blobFilePath = await StorageUtils.CreateBlobAsync();
            var blobClient = await StorageUtils.UploadBlobAsync(containerClient, blobFilePath);
            await StorageUtils.ListBlobsAsync(containerClient);
            var downloadFilePath = await StorageUtils.DownloadBlobAsync(blobClient, blobFilePath);
            await StorageUtils.CleanupAsync(blobFilePath, downloadFilePath, containerClient);
            return Ok();
        }

        [HttpGet("cosmos")]
        public async Task<IActionResult> CosmosMongo()
        {
            var invoice = new InvoiceEntity()
            {
                Id = "5deb1cf715755c43e4d1a615",
                Article = new Article()
                {
                    Name = "Forklift",
                    Price = 54000
                },
                AmountPaid = 24000,
                AmountToPay = 30000
            };

            
            
            var database = CosmosUtils.ConnectToDatabase(_cosmosMongoConnString, "Invoices");
            var collection = database.GetCollection<InvoiceEntity>("Invoices");
            await CosmosUtils.AddDocumentAsync(collection, invoice);
            invoice.Article.Name = "Forklift X320";
            await CosmosUtils.UpdateDocumentAsync(collection, invoice);
            foreach (var invoiceEntity in await CosmosUtils.GetAllAsync(collection))
            {
                Console.WriteLine(invoiceEntity.Article.Name);
            }

            await CosmosUtils.DeleteAsync(collection, invoice);
            return Ok();
        }
    }
}
