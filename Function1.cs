using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Npgsql;
using Azure.Communication.Email;
using Azure;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using System;

namespace LocalFunctionProj2
{
    public class HttpExample
    {
        private readonly ILogger _logger;

        public HttpExample(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HttpExample>();
        }

        [Function("HttpExample")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            var Host = @"PostgreSQLのホスト名";
            var User = "user";
            var DBname = "postgres";
            var Password = "Password";
            var Port = "5432";
            string connString =
                String.Format(
                    "Server={0};Username={1};Database={2};Port={3};Password={4};SSLMode=Prefer",
                    Host,
                    User,
                    DBname,
                    Port,
                    Password);
            using (var conn = new NpgsqlConnection(connString))

            {
                Console.Out.WriteLine("Opening connection");
                conn.Open();
                using (var command = new NpgsqlCommand("select * from inventory", conn))
                {

                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Console.WriteLine(reader.GetString(2).ToString());
                        sendmail(reader.GetString(2).ToString());
                    }
                    reader.Close();
                }
                async void sendmail(string adress)
                {

                    var connectionString = @"接続文字列";
                    // EmailContent�̕���������̓��[���^�C�g��
                    var subject = "Azure Mail Test";
                    var sender = "送付元のメールアドレス";
                    var htmlContent = "<html><h4>This email message is sent from Azure Communication Service Email.</h4></html>";
                    var recipient = adress;
                    try
                    {
                        Console.WriteLine("Sending email...");
                        EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                            Azure.WaitUntil.Completed,
                            sender,
                            recipient,
                            subject,
                            htmlContent);
                        EmailSendResult statusMonitor = emailSendOperation.Value;

                        Console.WriteLine($"Email Sent. Status = {emailSendOperation.Value.Status}");

                        /// Get the OperationId so that it can be used for tracking the message for troubleshooting
                        string operationId = emailSendOperation.Id;
                        Console.WriteLine($"Email operation id = {operationId}");
                    }
                    catch (RequestFailedException ex)
                    {
                        /// OperationID is contained in the exception message and can be used for troubleshooting purposes
                        Console.WriteLine($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
                    }

                }
                response.WriteString("Welcome to Azure Functions!");

                return response;
            }
        }
    }
}