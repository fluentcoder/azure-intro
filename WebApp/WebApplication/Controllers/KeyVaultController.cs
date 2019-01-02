using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeyVaultController : ControllerBase
    {
        private string _keyVaultUri;

        public string Message { get; set; }

        public KeyVaultController(IConfiguration iConfig)
        {
            _keyVaultUri = (iConfig != null)
                ? iConfig.GetSection("AppSettings").GetSection("KeyVaultUri").Value
                : string.Empty;
        }

        public void UpdateKeyVaultUri(string newUri)
        {
            _keyVaultUri = newUri;
        }

        // GET: api/KeyVault/secret/name
        [HttpGet("secret/{nameOfSecret}")]
        public async Task<IActionResult> Get([FromRoute] string nameOfSecret)
        {
            Message = ".";
            try
            {
                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                KeyVaultClient keyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                var secret = await keyVaultClient
                    .GetSecretAsync(_keyVaultUri, nameOfSecret)
                    .ConfigureAwait(false);
                Message = secret.Value;

                return Ok(Message);
            }

            /// <exception cref="KeyVaultErrorException">
            /// Thrown when the operation returned an invalid status code
            /// </exception>
            catch (KeyVaultErrorException keyVaultException)
            {
                Message = keyVaultException.Message;

                return BadRequest("Exception: " + Message);
            }
        }
        // POST: api/KeyVault/secret/name
        [HttpPost("secret/{nameOfSecret}")]
        public async Task<IActionResult> CreateSecret([FromRoute] string nameOfSecret, [FromBody] string content)
        {
            Message = ".";
            try
            {
                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                KeyVaultClient keyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                var secret = await keyVaultClient.SetSecretWithHttpMessagesAsync(_keyVaultUri, nameOfSecret, content);

                return Ok(secret.Body.Value);
            }

            /// <exception cref="KeyVaultErrorException">
            /// Thrown when the operation returned an invalid status code
            /// </exception>
            catch (KeyVaultErrorException keyVaultException)
            {
                Message = keyVaultException.Message;

                return BadRequest("Exception: " + Message);
            }
        }
        // POST: api/KeyVault/key
        [HttpPost("key")]
        public async Task<IActionResult> CreateKey([FromBody] string[] parameters)
        {
            string nameOfKey = parameters[0];
            string keyType = parameters[1];

            Message = ".";
            try
            {
                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                KeyVaultClient keyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

                var key = await keyVaultClient.CreateKeyWithHttpMessagesAsync(_keyVaultUri, nameOfKey, keyType);

                var result = key.Body.Key;

                return Ok(result);
            }

            /// <exception cref="KeyVaultErrorException">
            /// Thrown when the operation returned an invalid status code
            /// </exception>
            catch (Exception exception)
            {
                Message = exception.Message;

                return BadRequest("Exception: " + Message);
            }
        }
        // GET: api/KeyVault/key/name
        [HttpGet("key/{name}")]
        public async Task<IActionResult> GetKey([FromRoute] string name)
        {
            Message = ".";
            try
            {
                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                KeyVaultClient keyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                var lastVersion = keyVaultClient.GetKeyVersionsWithHttpMessagesAsync(_keyVaultUri, name).Result.Body.ToList().FirstOrDefault().Identifier.Version;
                var key = await keyVaultClient.GetKeyWithHttpMessagesAsync(_keyVaultUri, name, lastVersion);
                var result = key.Body.Key;

                return Ok(result);
                //return Ok("Key received !" + Environment.NewLine +
                //          "Key identifier: " + result.Kid + Environment.NewLine +
                //          "Key type: " + result.Kty + Environment.NewLine +
                //          "Key last version: "+ lastVersion+ Environment.NewLine +
                //          "Request Id: " + key.RequestId 
                //);
            }

            /// <exception cref="KeyVaultErrorException">
            /// Thrown when the operation returned an invalid status code
            /// </exception>
            catch (Exception exception)
            {
                Message = exception.Message;

                return BadRequest("Exception: " + exception);
            }
        }
        // DELETE: api/KeyVault/key/name
        [HttpDelete("key/{name}")]
        public async Task<IActionResult> DeleteKey([FromRoute] string name)
        {
            Message = ".";
            try
            {
                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                KeyVaultClient keyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

                await keyVaultClient.DeleteKeyWithHttpMessagesAsync(_keyVaultUri, name);

                return Ok("Key removed");
            }

            /// <exception cref="KeyVaultErrorException">
            /// Thrown when the operation returned an invalid status code
            /// </exception>
            catch (Exception exception)
            {
                Message = exception.Message;

                return BadRequest("Exception: " + exception);
            }
        }
        // POST: api/KeyVault/encrypt/name
        [HttpPost("encrypt/{name}")]
        public async Task<IActionResult> EncryptData([FromRoute] string name, [FromBody]string input)
        {
            Message = ".";

            try
            {
                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                KeyVaultClient keyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

                byte[] bytes = Encoding.ASCII.GetBytes(input);

                var lastVersion = keyVaultClient.GetKeyVersionsWithHttpMessagesAsync(_keyVaultUri, name).Result.Body.ToList().FirstOrDefault().Identifier.Version;
                var output = await keyVaultClient.EncryptWithHttpMessagesAsync(_keyVaultUri, name, lastVersion, "RSA-OAEP-256", bytes);

                var output1 = await keyVaultClient.DecryptWithHttpMessagesAsync(_keyVaultUri, name, lastVersion, "RSA-OAEP-256", output.Body.Result);
                return Ok(output.Body.Result);

            }

            /// <exception cref="KeyVaultErrorException">
            /// Thrown when the operation returned an invalid status code
            /// </exception>
            catch (Exception exception)
            {
                Message = exception.Message;

                return BadRequest("Exception: " + exception);
            }
        }
        // GET: api/KeyVault/key/decrypt/name
        [HttpGet("key/decrypt/{name}")]
        public async Task<IActionResult> DecryptData([FromRoute] string name, [FromBody]byte[] data)
        {
            Message = ".";

            try
            {
                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                KeyVaultClient keyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

                var lastVersion = keyVaultClient.GetKeyVersionsWithHttpMessagesAsync(_keyVaultUri, name).Result.Body.ToList().FirstOrDefault().Identifier.Version;
                var output = await keyVaultClient.DecryptWithHttpMessagesAsync(_keyVaultUri, name, lastVersion, "RSA-OAEP-256", data);
                return Ok(Encoding.UTF8.GetString(output.Body.Result));
            }

            /// <exception cref="KeyVaultErrorException">
            /// Thrown when the operation returned an invalid status code
            /// </exception>
            catch (Exception exception)
            {
                Message = exception.Message;

                return BadRequest("Exception: " + exception);
            }
        }
    }
}
