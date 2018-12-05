using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.WindowsAzure.Storage;

namespace WebApplication.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        public string con { get; set; } = "DefaultEndpointsProtocol=https;AccountName=mystorageaccountdm;AccountKey=/u3CY/DAt6m7hHrp51+Tan1UmfMXfT/b9cHU1YFB/ZiWL0dF8OvxZKzuN4pOvMbuCzA7lr45tm0c2yuaA9uoVg==;EndpointSuffix=core.windows.net";

        //// GET: api/Storage
        //[Microsoft.AspNetCore.Mvc.HttpGet]
        //public FileResult Get()
        //{
        //    string fileName = "Test.rtf";
        //    var databytes = System.IO.File.ReadAllBytes(@"files/"+fileName);

        //    FileResult result = new FileContentResult(databytes, "application/octet-stream");
        //    result.FileDownloadName = fileName;

        //    return result;
        //}

        // GET: api/Storage
        [Microsoft.AspNetCore.Mvc.HttpGet("{name}")]
        public async Task<FileResult> GetFromStorage([FromRoute] string name)
        {
            //var filename = name;
            //var storageAccount = CloudStorageAccount.Parse(con);
            //var blobClient = storageAccount.CreateCloudBlobClient();

            //CloudBlobContainer container = blobClient.GetContainerReference("mycontainer");
            //CloudBlockBlob blob = container.GetBlockBlobReference(path);

            //byte[] databytes = new byte[blob.Properties.Length];
            //blob.DownloadRangeToByteArrayAsync(databytes,0);

            //FileResult result = new FileContentResult(databytes, "application/octet-stream");
            //result.FileDownloadName = filename;

            //return result;

            string filename = name;

            var storageAccount = CloudStorageAccount.Parse(con);
            var blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference("mycontainer");
            CloudBlockBlob blob = blobContainer.GetBlockBlobReference(filename);
            Stream blobStream = await blob.OpenReadAsync();

            var memoryStream = new MemoryStream();
            blobStream.CopyTo(memoryStream);

            var databytes = memoryStream.ToArray();

            var result = File(databytes, "application/octet-stream");

            //return Ok(blobStream);
            return result;

        }
    }
}
