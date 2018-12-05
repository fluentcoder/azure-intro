
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Configuration;
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
using Dapper;

namespace FunctionApptest
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            try
            {
                return req.CreateResponse(HttpStatusCode.OK, GetUsers());
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

        }

        public static List<User> GetUsers()
        {
            using (IDbConnection db = new SqlConnection(ConfigurationManager.AppSettings.Get("ConnectionString")))
            {
                return db.Query<User> ("Select * From Users").ToList();
            }
        }
    }
}
