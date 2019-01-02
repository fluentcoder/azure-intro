using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.Text;
using System.Threading.Tasks;
using WebApplication.Controllers;

namespace Tests
{
    public class KeyVaultControllerTest
    {
        private KeyVaultController _keyVaultController;
        public IConfigurationRoot Configuration { get; set; }


        [SetUp]
        public void Setup()
        {
            _keyVaultController = new KeyVaultController(null);
            _keyVaultController.UpdateKeyVaultUri("https://mykeyvaultdm.vault.azure.net/");
        }

        [Test]
        public async Task Create_and_get_key_should_create_key_then_get_it()
        {
            //Arrange
            IActionResult expected = null;
            IActionResult actual = null;
            string keyName = "TestTest";

            //Act
            async Task Act()
            {
                expected = await _keyVaultController.CreateKey(new string[] { keyName, "RSA" });
                actual = await _keyVaultController.GetKey(keyName);
            }
            await Act();

            //Assert
            Assert.DoesNotThrowAsync(Act);
            Assert.IsNotInstanceOf<BadRequestObjectResult>(expected);
            Assert.IsNotInstanceOf<BadRequestObjectResult>(actual);
            Assert.IsInstanceOf<OkObjectResult>(actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task Delete_key_should_return_message()
        {
            //Arrange
            string expected = "Key removed";
            string actual = null;
            string keyName = "TestTest";

            //Act
            async Task Act()
            {
                IActionResult actualActionResult = await _keyVaultController.DeleteKey(keyName);
                actual = actualActionResult.ToString();
            }
            await Act();

            //Assert
            Assert.DoesNotThrowAsync(Act);
            Assert.IsNotInstanceOf<BadRequestObjectResult>(actual);
            Assert.IsInstanceOf<OkObjectResult>(actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task Encrypt_and_decrypt_should_create_key_then_encrypt_and_decrypt_data()
        {
            //Arrange
            string data = "some data";
            string expected = data;
            string actual = null;
            string keyName = "TestTest";
            IActionResult actualActionResult = null;

            //Act
            async Task Act()
            {
                await _keyVaultController.CreateKey(new string[] { keyName, "RSA" });
                IActionResult encryptedData = await _keyVaultController.EncryptData(keyName, data);
                actualActionResult =
                    await _keyVaultController.DecryptData(keyName, Encoding.ASCII.GetBytes(encryptedData.ToString()));
                actual = actualActionResult.ToString();
            }
            await Act();

            //Assert
            Assert.DoesNotThrowAsync(Act);
            Assert.IsNotInstanceOf<BadRequestObjectResult>(actualActionResult);
            Assert.IsInstanceOf<OkObjectResult>(actualActionResult);
            Assert.AreEqual(expected, actual);
        }
        [Test]
        public async Task Create_and_get_secret_should_create_new_sectret_then_get_it()
        {
            //Arrange
            string data = "some data";
            IActionResult expected = new OkObjectResult(data);
            IActionResult actual1 = null;
            IActionResult actual2 = null;
            string secretName = "TestTest";

            //Act
            async Task Act()
            {
                actual1 = await _keyVaultController.CreateSecret(secretName, data);
                actual2 = await _keyVaultController.Get(secretName);
            }
            await Act();

            //Assert
            Assert.DoesNotThrowAsync(Act);
            Assert.IsNotInstanceOf<BadRequestObjectResult>(actual1);
            Assert.IsNotInstanceOf<BadRequestObjectResult>(actual2);
            Assert.IsInstanceOf<OkObjectResult>(actual1);
            Assert.IsInstanceOf<OkObjectResult>(actual2);
            Assert.AreEqual(expected, actual1);
            Assert.AreEqual(expected, actual2);
        }
    }
}