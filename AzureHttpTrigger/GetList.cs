using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Net;

namespace AzureHttpTrigger
{
    public class GetList
    {
        private readonly ILogger _logger;

        public GetList(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetList>();
        }

        [Function("GetList")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req, ILogger log)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);

            //We retrieve the id field, which comes as a parameter to the function, by deserializing req.Content.


            var connectionString = Environment.GetEnvironmentVariable("SqlConnection", EnvironmentVariableTarget.Process);

            //Azure SQLDB Log

            var logAdded = true;

            try

            {

                //We get the Connection String in the Function App Settings section we defined.

                using (SqlConnection connection = new SqlConnection(connectionString))

                {

                    //Opens Azure SQL DB connection.

                    connection.Open();

                    string qs = $"SELECT CourseID,CourseName,Rating FROM Course";

                    SqlCommand command = new SqlCommand(qs, connection);

                    string queryop = "";

                    using (SqlDataReader reader = command.ExecuteReader())

                    {

                        queryop = sqlDatoToJson(reader);

                    }
                    response.WriteString(queryop);

                    connection.Close();

                }

            }

            catch (Exception e)

            {

                logAdded = false;

                log.LogError(e.ToString());
                response.WriteString(e.ToString());
            }

            return response;
        }

        static string sqlDatoToJson(SqlDataReader dataReader)

        // transform the returned data to JSON

        {

            var dataTable = new DataTable();

            dataTable.Load(dataReader);

            string JSONString = string.Empty;

            JSONString = JsonConvert.SerializeObject(dataTable);

            return JSONString;

        }
    }
}
