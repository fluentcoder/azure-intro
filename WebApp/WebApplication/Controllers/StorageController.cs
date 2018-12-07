using System;
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

namespace WebApplication.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        //public string StorageAccountConnectionString { get; set; } = "DefaultEndpointsProtocol=https;AccountName=mystorageaccountdm;AccountKey=/u3CY/DAt6m7hHrp51+Tan1UmfMXfT/b9cHU1YFB/ZiWL0dF8OvxZKzuN4pOvMbuCzA7lr45tm0c2yuaA9uoVg==;EndpointSuffix=core.windows.net";
        
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
    }
}
