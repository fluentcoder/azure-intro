using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.WindowsAzure.Storage.Table;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
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

        // GET: api/Storage/table/name/partitionKey
        [Microsoft.AspNetCore.Mvc.HttpGet("table/{tableName}/{partitionKey}")]
        public async Task<IActionResult> GetDataFromTableByPartitionKey([FromRoute] string tableName, [FromRoute] string partitionKey)
        {
            _storageAccount = CloudStorageAccount.Parse(_configuration.GetValue<string>("ConnectionStrings:StorageAccountConnectionString"));
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(tableName);

            TableQuery<Customer> query = new TableQuery<Customer>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            TableContinuationToken token = null;

            TableQuerySegment<Customer> resultSegment = await table.ExecuteQuerySegmentedAsync(query, token);
            return Ok(resultSegment.Results);
        }

        // GET: api/Storage/table/name/partitionKey/rowKey
        [Microsoft.AspNetCore.Mvc.HttpGet("table/{tableName}/{partitionKey}/{rowKey}")]
        public async Task<IActionResult> GetSingleDataFromTable([FromRoute] string tableName, [FromRoute] string partitionKey, [FromRoute] string rowKey)
        {
            _storageAccount = CloudStorageAccount.Parse(_configuration.GetValue<string>("ConnectionStrings:StorageAccountConnectionString"));
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(tableName);

            TableOperation retrieveOperation = TableOperation.Retrieve<Customer>(partitionKey, rowKey);
            TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);

            return Ok((Customer)retrievedResult.Result);
        }

        // DELETE: api/Storage/table/name/partitionKey/rowKey
        [Microsoft.AspNetCore.Mvc.HttpDelete("table/delete/{tableName}/{partitionKey}/{rowKey}")]
        public async Task<IActionResult> DeleteDataFromTable([FromRoute] string tableName, [FromRoute] string partitionKey, [FromRoute] string rowKey)
        {
            _storageAccount = CloudStorageAccount.Parse(_configuration.GetValue<string>("ConnectionStrings:StorageAccountConnectionString"));
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(tableName);

            TableOperation retrieveOperation = TableOperation.Retrieve<Customer>(partitionKey, rowKey);
            TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);
            Customer deleteEntity = (Customer)retrievedResult.Result;

            if (deleteEntity != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(deleteEntity);
                await table.ExecuteAsync(deleteOperation);
                return Ok("Entity was deleted");
            }
            return BadRequest();
        }

        // PUT: api/Storage/table/name/partitionKey/rowKey
        [Microsoft.AspNetCore.Mvc.HttpPut("table/delete/{tableName}/{partitionKey}/{rowKey}")]
        public async Task<IActionResult> UpdateData([Microsoft.AspNetCore.Mvc.FromBody] Customer input, [FromRoute] string tableName, [FromRoute] string partitionKey, [FromRoute] string rowKey)
        {
            _storageAccount = CloudStorageAccount.Parse(_configuration.GetValue<string>("ConnectionStrings:StorageAccountConnectionString"));
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(tableName);
            TableOperation retrieveOperation = TableOperation.Retrieve<Customer>(partitionKey, rowKey);
            TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);
            Customer updateCustomer = (Customer)retrievedResult.Result;

            if (updateCustomer != null)
            {
                updateCustomer.Id = input.Id;
                updateCustomer.Age = input.Age;
                updateCustomer.Name = input.Name;
                TableOperation updateOperation = TableOperation.Replace(updateCustomer);
                await table.ExecuteAsync(updateOperation);
                return Ok("Entity was updated");
            }
            return BadRequest();
        }
    }


}
