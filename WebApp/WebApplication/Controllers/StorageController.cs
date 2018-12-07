using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.WindowsAzure.Storage.Table;
using WebApplication.models;

namespace WebApplication.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        
        private CloudStorageAccount _storageAccount;
        private IConfiguration _configuration;

        public StorageController(IConfiguration iConfig)
        {
            _configuration = iConfig;
        }

        // GET: api/Storage/blob/name
        [Microsoft.AspNetCore.Mvc.HttpGet("blob/{name}")]
        public async Task<FileResult> GetFromBlobStorage([FromRoute] string name)
        {
            string con = _configuration.GetSection("ConnectionStrings").GetSection("StorageAccountConnectionString").Value;
            con.ToString();
            _storageAccount = CloudStorageAccount.Parse(con);  
            string filename = name;

            var blobClient = _storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference("mycontainer");
            CloudBlockBlob blob = blobContainer.GetBlockBlobReference(filename);
            Stream blobStream = await blob.OpenReadAsync();

            var memoryStream = new MemoryStream();
            blobStream.CopyTo(memoryStream);

            var databytes = memoryStream.ToArray();

            var result = File(databytes, "application/octet-stream");
           
            return result;

        }
        // GET: api/Storage/file/name
        [Microsoft.AspNetCore.Mvc.HttpGet("file/{name}")]
        public async Task<FileResult> GetFromFileStorage([FromRoute] string name)
        {
            _storageAccount = CloudStorageAccount.Parse(_configuration.GetValue<string>("ConnectionStrings:StorageAccountConnectionString"));
            string filename = name;

            var fileClient = _storageAccount.CreateCloudFileClient();
            CloudFileShare fileShare = fileClient.GetShareReference("fileshare");
            CloudFileDirectory rootDir = fileShare.GetRootDirectoryReference();
            CloudFileDirectory fileDir = rootDir.GetDirectoryReference("Dir1");
            CloudFile file = fileDir.GetFileReference(filename);
            Stream fileStream = await file.OpenReadAsync();

            var memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);

            var databytes = memoryStream.ToArray();

            var result = File(databytes, "application/octet-stream");

            return result;
        }

        // POST: api/Storage/table/create/name
        [Microsoft.AspNetCore.Mvc.HttpPost("table/create")]
        public async Task<IActionResult> CreateTable([Microsoft.AspNetCore.Mvc.FromBody] string name)
        {
            _storageAccount = CloudStorageAccount.Parse(_configuration.GetValue<string>("ConnectionStrings:StorageAccountConnectionString")); ;

            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference(name);
            await table.CreateIfNotExistsAsync();

            return Ok("Table " + name + " was created");
        }

        // POST: api/Storage/table/write/name
        [Microsoft.AspNetCore.Mvc.HttpPost("table/write/{tableName}/{partitionKey?}")]
        public async Task<IActionResult> WriteToTable([Microsoft.AspNetCore.Mvc.FromBody] Customer input, [FromRoute] string tableName, [FromRoute] string partitionKey)
        {
            _storageAccount = CloudStorageAccount.Parse(_configuration.GetValue<string>("ConnectionStrings:StorageAccountConnectionString"));

            Customer customer = new Customer(input);

            if (partitionKey != null)
            {
                customer.PartitionKey = partitionKey;
            }

            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(tableName);
            TableOperation insertOperation = TableOperation.Insert(customer);

            await table.ExecuteAsync(insertOperation);

            return Ok("Entity joined to table");
        }

        // POST: api/Storage/table/writeBatch/name
        [Microsoft.AspNetCore.Mvc.HttpPost("table/writeBatch/{tableName}/{partitionKey?}")]
        public async Task<IActionResult> WriteBatchToTable([Microsoft.AspNetCore.Mvc.FromBody] Customer[] input, [FromRoute] string tableName, [FromRoute] string partitionKey)
        {
            _storageAccount = CloudStorageAccount.Parse(_configuration.GetValue<string>("ConnectionStrings:StorageAccountConnectionString"));

            TableBatchOperation batchOperation = new TableBatchOperation();

            foreach (Customer item in input)
            {
                Customer customer = new Customer(item);
                if (partitionKey != null)
                {
                    customer.PartitionKey = partitionKey;
                }

                batchOperation.Insert(customer);
            }

            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(tableName);

            await table.ExecuteBatchAsync(batchOperation);

            return Ok("Batch items joined to table");
        }
        
        //// GET: api/Storage/table/name
        //[Microsoft.AspNetCore.Mvc.HttpGet("table/{tableName}/{partitionKey}")]
        //public Task<IActionResult>GetDataFromTable([FromRoute] string tableName, [FromRoute] string partitionKey)
        //{
        //    _storageAccount = CloudStorageAccount.Parse(_configuration.GetValue<string>("ConnectionStrings:StorageAccountConnectionString"));
        //    CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
        //    CloudTable table = tableClient.GetTableReference(tableName);

        //    TableQuery<Customer> query = new TableQuery<Customer>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

        //    List<Customer> customers = new List<Customer>();
            
        //    foreach (Customer item in table.())
        //    {

        //    }

        //    return Ok("Batch items joined to table");
        //}
    }


}
