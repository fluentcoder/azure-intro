
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using UserCreator;

namespace FunctionApptest
{
    public static class Function1
    {
        public static SqlConnection connection = new SqlConnection();
        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {

            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            string name = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
                .Value;

            //if (name == null)
            //{
            //    // Get request body
            //    dynamic data = await req.Content.ReadAsAsync<object>();
            //    name = data?.name;
            //}

            try
            {
                SqlCommand cmd = new SqlCommand();

                string con = "Server=tcp:myserverforwebapplication.database.windows.net,1433;Initial Catalog=TempDbForWebTest;Persist Security Info=False;User ID=dim97;Password=Janedimanoavesta1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
                //string con = "Server=(localdb)\\mssqllocaldb;Database=usersdbstore;Trusted_Connection=True;MultipleActiveResultSets=true";

                SqlDataReader reader;

                if (connection.ConnectionString.Length == 0)
                {
                    connection.ConnectionString = con;
                }

                await connection.OpenAsync();

                cmd.CommandText = "SELECT * FROM Users";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = connection;

                reader = cmd.ExecuteReader();

                UserCreator.UserCreator creator = new UserCreator.UserCreator();
                List<User> users = new List<User>();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        users.Add(creator.CreateUser(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2)));
                    }
                }

                XmlSerializer serializer = new XmlSerializer(typeof(List<User>));

                StringWriter stringWriter = new StringWriter();

                serializer.Serialize(stringWriter, users);
                string result = stringWriter.ToString();
                cmd.Connection.Close();

                return req.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (SqlException sqlex)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest,
                    $"The following SqlException happened: {sqlex.Message}");
            }
            catch (Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest,
                    $"The following Exception happened: {ex.Message}");
            }

            //return req.CreateResponse(HttpStatusCode.OK, user.name); 
        }
    }
}
