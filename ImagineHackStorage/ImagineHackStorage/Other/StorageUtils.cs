using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ImagineHackStorage.Models;
using Microsoft.Azure.Cosmos.Table;

namespace ImagineHackStorage.Other
{
    public class StorageUtils
    {

        #region AzureTable
        public static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }

        public static async Task<CloudTable> CreateTableAsync(string connString, string tableName)
        {
            // Retrieve storage account information from connection string.
            var storageAccount = CreateStorageAccountFromConnectionString(connString);

            // Create a table client for interacting with the table service
            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            Console.WriteLine("Create a Table for the demo");

            // Create a table client for interacting with the table service 
            var table = tableClient.GetTableReference(tableName);
            if (await table.CreateIfNotExistsAsync())
            {
                Console.WriteLine("Created Table named: {0}", tableName);
            }
            else
            {
                Console.WriteLine("Table {0} already exists", tableName);
            }

            Console.WriteLine();
            return table;
        }

        public static async Task<CustomerEntity> InsertOrMergeEntityAsync(CloudTable table, CustomerEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            try
            {
                // Create the InsertOrReplace table operation
                var insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

                // Execute the operation.
                var result = await table.ExecuteAsync(insertOrMergeOperation);
                var insertedCustomer = result.Result as CustomerEntity;

                // Get the request units consumed by the current operation. RequestCharge of a TableResult is only applied to Azure Cosmos DB
                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of InsertOrMerge Operation: " + result.RequestCharge);
                }

                return insertedCustomer;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        public static async Task<CustomerEntity> RetrieveEntityUsingPointQueryAsync(CloudTable table, string partitionKey, string rowKey)
        {
            try
            {
                var retrieveOperation = TableOperation.Retrieve<CustomerEntity>(partitionKey, rowKey);
                var result = await table.ExecuteAsync(retrieveOperation);
                var customer = result.Result as CustomerEntity;
                if (customer != null)
                {
                    Console.WriteLine("\t{0}\t{1}\t{2}\t{3}", customer.PartitionKey, customer.RowKey, customer.Email, customer.PhoneNumber);
                }

                // Get the request units consumed by the current operation. RequestCharge of a TableResult is only applied to Azure CosmoS DB 
                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of Retrieve Operation: " + result.RequestCharge);
                }

                return customer;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        public static async Task DeleteEntityAsync(CloudTable table, CustomerEntity deleteEntity)
        {
            try
            {
                if (deleteEntity == null)
                {
                    throw new ArgumentNullException("deleteEntity");
                }

                var deleteOperation = TableOperation.Delete(deleteEntity);
                var result = await table.ExecuteAsync(deleteOperation);

                // Get the request units consumed by the current operation. RequestCharge of a TableResult is only applied to Azure CosmoS DB 
                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of Delete Operation: " + result.RequestCharge);
                }

            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        #endregion


        #region BlobStorage

        public static async Task<BlobContainerClient> CreateBlobContainerAsync(string connString, string containerName)
        {
            // Create a BlobServiceClient object which will be used to create a container client
            var blobServiceClient = new BlobServiceClient(connString);

            // Create the container and return a container client object
            return await blobServiceClient.CreateBlobContainerAsync(containerName);
        }

        public static async Task<string> CreateBlobAsync()
        {
            // Create a local file in the ./data/ directory for uploading and downloading
            var localPath = "./data/";
            var fileName = "blob" + Guid.NewGuid().ToString() + ".txt";
            var localFilePath = Path.Combine(localPath, fileName);

            // Write text to the file
            await File.WriteAllTextAsync(localFilePath, "Hello, World!");
            return localFilePath;
        }

        public static async Task<BlobClient> UploadBlobAsync(BlobContainerClient containerClient, string filepath)
        {
            // Get a reference to a blob
            var fileName = Path.GetFileName(filepath);
            var blobClient = containerClient.GetBlobClient(fileName);

            Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

            // Open the file and upload its data
            using var uploadFileStream = File.OpenRead(filepath);
            await blobClient.UploadAsync(uploadFileStream);
            uploadFileStream.Close();
            return blobClient;
        }

        public static async Task ListBlobsAsync(BlobContainerClient containerClient)
        {
            Console.WriteLine("Listing blobs...");

            // List all blobs in the container
            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                Console.WriteLine("\t" + blobItem.Name);
            }
        }

        public static async Task<string> DownloadBlobAsync(BlobClient blobClient, string localFilePath)
        {
            // Download the blob to a local file
            // Append the string "DOWNLOAD" before the .txt extension so you can see both files in MyDocuments
            var downloadFilePath = localFilePath.Replace(".txt", "DOWNLOAD.txt");

            Console.WriteLine("\nDownloading blob to\n\t{0}\n", downloadFilePath);

            // Download the blob's contents and save it to a file
            BlobDownloadInfo download = await blobClient.DownloadAsync();

            using var downloadFileStream = File.OpenWrite(downloadFilePath);
            await download.Content.CopyToAsync(downloadFileStream);
            downloadFileStream.Close();
            return downloadFilePath;
        }

        public static async Task CleanupAsync(string localFilePath, string downloadFilePath,
                                              BlobContainerClient containerClient)
        {
            Console.WriteLine("Deleting blob container...");
            await containerClient.DeleteAsync();

            Console.WriteLine("Deleting the local source and downloaded files...");
            File.Delete(localFilePath);
            File.Delete(downloadFilePath);

            Console.WriteLine("Done");
        }

        #endregion
    }
}