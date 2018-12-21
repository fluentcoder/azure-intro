using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeyVaultController : ControllerBase
    {
        public string Message { get; set; }

        // GET: api/KeyVault/secret/name
        [HttpGet("secret/{nameOfSecret}")]
        public async Task<IActionResult> Get([FromRoute] string nameOfSecret)
        {
            Message = ".";
            try
            {
                /* The below 4 lines of code shows you how to use AppAuthentication library to fetch secrets from your Key Vault*/
                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                KeyVaultClient keyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                var secret = await keyVaultClient
                    .GetSecretAsync("https://mykeyvaultdm.vault.azure.net/", nameOfSecret)
                    .ConfigureAwait(false);
                Message = secret.Value;

                return Ok("Message: " + Message);
            }

            /// <exception cref="KeyVaultErrorException">
            /// Thrown when the operation returned an invalid status code
            /// </exception>
            catch (KeyVaultErrorException keyVaultException)
            {
                Message = keyVaultException.Message;

                return Ok("Exception: "+Message);
            }
        }

    }
}
