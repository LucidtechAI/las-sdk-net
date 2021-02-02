using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

using NUnit.Framework;

using Newtonsoft.Json;
using Moq;
using Moq.Protected;

using Lucidtech.Las;
using Lucidtech.Las.Core;
using Lucidtech.Las.Utils;

namespace Test
{
    [TestFixture]
    public class TestClient 
    {
        private Client Toby { get; set; }
        private Dictionary<string, object> CreateDocResponse { get; set; }

        private static void CheckKeys(string[] expected, object response)
        {
            var res = JsonSerialPublisher.ObjectToDict<Dictionary<string, object>>(response);
            foreach (var key in expected)
            {
                Assert.IsTrue(res.ContainsKey(key), $"{key}: {res[key]}");
            }
        }

        [OneTimeSetUp]
        public void InitClient()
        {
          var mockCreds = new Mock<Credentials>("test", "test", "test", "test", "http://localhost:4010");

          mockCreds
            .Protected()
            .Setup<(string, DateTime)>("GetClientCredentials")
            .Returns(("foobar", DateTime.Now));
          mockCreds
            .Protected()
            .Setup("CommonConstructor");

          Toby = new Client(mockCreds.Object);
        }

        [SetUp]
        public void Setup()
        {
            byte[] body = File.ReadAllBytes(Example.DocPath());
            var response = Toby.CreateDocument(body, Example.ContentType(), Example.ConsentId());
            CreateDocResponse = JsonSerialPublisher.ObjectToDict<Dictionary<string, object>>(response);
        }

        [TestCase("name", "description")]
        [TestCase("", "")]
        [TestCase(null, null)]
        public void TestCreateAsset(string? name, string? description) {
            var bytes = BitConverter.GetBytes(12345);
            var parameters = new Dictionary<string, string?>{
                {"name", name},
                {"description", description}
            };
            var response = Toby.CreateAsset(bytes, parameters);
            CheckKeys(new [] {"assetId"}, response);
        }

        [TestCase("foo", 3)]
        [TestCase(null, null)]
        public void TestListAssets(string nextToken, int maxResults) {
            var response = Toby.ListAssets(nextToken: nextToken, maxResults: maxResults);
            CheckKeys(new [] {"nextToken", "assets"}, response);
        }


        [Test]
        public void TestGetAssetById() {
            var assetId = $"las:asset:{Guid.NewGuid().ToString()}";
            var response = Toby.GetAsset(assetId);
            var expectedKeys = new [] {"assetId", "content"};
            CheckKeys(expectedKeys, response);
        }

        [TestCase("name", "description")]
        [TestCase("", "")]
        public void TestUpdateAsset(string? name, string? description) {
            var assetId = $"las:asset:{Guid.NewGuid().ToString()}";
            var content = BitConverter.GetBytes(123456);
            var response = Toby.UpdateAsset(assetId, content, new Dictionary<string?, string?>{
                {"name", name},
                {"description", description}
            });
            var expectedKeys = new [] {"assetId"};
            CheckKeys(expectedKeys, response);
        }

        [Test]
        public void TestCreateDocument()
        {
            var expectedKeys = new [] {"documentId", "contentType", "consentId", "batchId"};
            CheckKeys(expectedKeys, CreateDocResponse);
        }

        [TestCase("foo", 3, null, null)]
        [TestCase(null, null, "las:consent:08b49ae64cd746f384f05880ef5de72f", null)]
        [TestCase(null, null, null, "las:batch:08b49ae64cd746f384f05880ef5de72f")]
        [TestCase("foo", 2, null, "las:batch:08b49ae64cd746f384f05880ef5de72f")]
        [TestCase("foo", 2, "las:consent:08b49ae64cd746f384f05880ef5de72f", null)]
        public void TestListDocuments(string nextToken, int maxResults, string consentId, string batchId) {
            var response = Toby.ListDocuments(
                nextToken: nextToken, 
                maxResults: maxResults,
                consentId: consentId,
                batchId: batchId
            );
            var expectedKeys = new [] {"documents"};
            CheckKeys(expectedKeys, response);
        }

        [Test]
        public void TestCreatePredictionBareMinimum()
        {
            var response = Toby.CreatePrediction(
                (string)CreateDocResponse["documentId"],
                Example.ModelId()
            );
            var expectedKeys = new [] {"documentId", "predictions"};
            CheckKeys(expectedKeys, response);
        }

        [Test]
        public void TestCreatePredictionMaxPages()
        {
            var response = Toby.CreatePrediction(
                (string)CreateDocResponse["documentId"],
                Example.ModelId(),
                maxPages: 2
            );
            var expectedKeys = new [] {"documentId", "predictions"};
            CheckKeys(expectedKeys, response);
        }

        [Test]
        public void TestCreatePredictionAutoRotate()
        {
            var response = Toby.CreatePrediction(
                (string)CreateDocResponse["documentId"],
                Example.ModelId(),
                autoRotate: true
            );
            var expectedKeys = new [] {"documentId", "predictions"};
            CheckKeys(expectedKeys, response);
        }

        [Test]
        public void TestGetDocument()
        {
            var response = Toby.GetDocument((string)CreateDocResponse["documentId"]);
            var expectedKeys = new [] {"documentId", "contentType", "consentId"};
            CheckKeys(expectedKeys, response);
        }

        [TestCase("54.50", "2007-07-30")]
        public void TestUpdateDocument(string total_amount, string purchase_date)
        {
            var ground_truth = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string>(){{"label", "total_amount"},{"value", total_amount}},
                new Dictionary<string, string>(){{"label", "purchase_date"},{"value", purchase_date}}
            };
            var response = Toby.UpdateDocument((string)CreateDocResponse["documentId"], ground_truth);
            var expectedKeys = new [] {"documentId", "consentId", "contentType", "groundTruth"};
            CheckKeys(expectedKeys, response);
        }

        [Test]
        [Ignore("delete endpoints doesn't work")]
        public void TestDeleteDocuments() {
            var response = Toby.DeleteDocuments();
            var expectedKeys = new [] {"documents"};
            CheckKeys(expectedKeys, response);
        }

        [Test]
        public void TestCreateBatch()
        {
            var response = Toby.CreateBatch(Example.Description());
            var expectedKeys = new [] {"batchId", "description"};
            CheckKeys(expectedKeys, response);
        }

        [TestCase("foo", 3)]
        [TestCase(null, null)]
        public void TestListModels(string nextToken, int maxResults) {
            var response = Toby.ListModels(nextToken: nextToken, maxResults: maxResults);
            var expectedKeys = new [] {"models"};
            CheckKeys(expectedKeys, response);
        }


        [TestCase("foo", 3)]
        [TestCase(null, null)]
        public void TestListPredictions(string nextToken, int maxResults) {
            var response = Toby.ListPredictions(nextToken: nextToken, maxResults: maxResults);
            var expectedKeys = new [] {"predictions"};
            CheckKeys(expectedKeys, response);
        }

        [TestCase("foo", "bar")]
        public void TestCreateSecret(string username, string password) {
            var data = new Dictionary<string, string>(){
                {"username", username},
                {"password", password}
            };
            var response = Toby.CreateSecret(data);
            var expectedKeys = new [] {"secretId"};
            CheckKeys(expectedKeys, response); 
        }

        [TestCase("foo", 3)]
        [TestCase(null, null)]
        public void TestListSecrets(string nextToken, int maxResults) {
            var response = Toby.ListSecrets(nextToken: nextToken, maxResults: maxResults);
            var expectedKeys = new [] {"secrets"};
            CheckKeys(expectedKeys, response);
        }

        [TestCase("foo", "bar", "name", "description")]
        [TestCase("foo", "bar", "name", "")]
        public void TestUpdateSecret(string username, string password, string? name = null, string? description = null) {
            var secretId = $"las:model:{Guid.NewGuid().ToString()}";
            var data = new Dictionary<string, string>() {
                {"username", username},
                {"password", password}
            };
            var expectedKeys = new [] {"secretId"};
            var response = Toby.UpdateSecret(secretId, data, new Dictionary<string, string?>{
                {"name", name},
                {"description", description}
            });
            CheckKeys(expectedKeys, response);
        }
        
        [TestCase("docker", "name", "description")]
        [TestCase("manual", "name", "description")]
        [TestCase("docker", null, null)]
        public void TestCreateTransition(string transitionType, string name, string description) {
            var schema = new Dictionary<string, string>() {
                {"schema", "https://json-schema.org/draft-04/schema#"},
                {"title", "response"}
            };

            var inputSchema = schema;
            var outputSchema = schema;
            var optionalParameters = new Dictionary<string, string>{
                {"name", name},
                {"description", description}
            };
            var parametersVariants = new [] {null, new Dictionary<string, object>{
                {"cpu", 256},
                {"imageUrl", "image_url"}
            }};

            foreach (var parameters in parametersVariants) {
                var response = Toby.CreateTransition(transitionType, inputSchema, outputSchema, parameters, optionalParameters);
                CheckKeys(new [] {"name", "transitionId", "transitionType"}, response);
            }
        }

        [TestCase("docker")]
        [TestCase("manual")]
        [TestCase(null)]
        public void TestListTransitions(string? transitionType) {
            var response = Toby.ListTransitions(transitionType);
            CheckKeys(new [] {"transitions"}, response);
        }

        public void TestListTransitions() {
            var response = Toby.ListTransitions(new List<string>{"docker", "manual"});
            CheckKeys(new [] {"transitions"}, response);
        }

        [TestCase("foo", "bar")]
        [TestCase(null, null)]
        public void TestUpdateTransition(string? name, string? description) {
            var schema = new Dictionary<string, string>() {
                {"schema", "https://json-schema.org/draft-04/schema#"},
                {"title", "response"}
            };
            var inputSchema = schema;
            var outputSchema = schema;
            var transitionId = $"las:transition:{Guid.NewGuid().ToString()}";
            var parameters = new Dictionary<string, string?>{
                {"name", name},
                {"description", description}
            };
            var response = Toby.UpdateTransition(transitionId, inputSchema, outputSchema, parameters);
            CheckKeys(new [] {"name", "transitionId", "transitionType"}, response);
        }

        public void TestGetTransitionExecution() {
            var executionId = $"las:transition-execution:{Guid.NewGuid().ToString()}";
            var transitionId = $"las:transition:{Guid.NewGuid().ToString()}";
            var response = Toby.GetTransitionExecution(transitionId, executionId);
            CheckKeys(new [] {"transitionId", "executionId", "status"}, response);
        }

        [Test]
        public void TestExecuteTransition() {
            var transitionId = $"las:transition-execution:{Guid.NewGuid().ToString()}";
            var response = Toby.ExecuteTransition(transitionId);
            CheckKeys(new [] {"transitionId", "executionId", "status"}, response);
        }

        [TestCase(
            "running",
            "las:transition-execution:08b49ae64cd746f384f05880ef5de72f",
            3,
            null,
            "startTime",
            "ascending"
        )]
        public void TestListTransitionExecutions(
            string? status = null,
            string? executionId = null,
            int? maxResults = null,
            string? nextToken = null,
            string? sortBy = null,
            string? order = null
        ) {
            var transitionId = $"las:transition:{Guid.NewGuid().ToString()}";
            var response = Toby.ListTransitionExecutions(
                transitionId,
                new List<string>{ status },
                new List<string>{ executionId },
                maxResults,
                nextToken,
                sortBy,
                order
            );
            var expectedKeys = new [] {"executions"};
            CheckKeys(expectedKeys, response);
        }

        [TestCase(
            3,
            null,
            "startTime",
            "ascending"
        )]
        public void TestListTransitionExecutions(
            int? maxResults = null,
            string? nextToken = null,
            string? sortBy = null,
            string? order = null
        ) {
            var transitionId = $"las:transition:{Guid.NewGuid().ToString()}";
            var statuses = new List<string>{ "running", "succeeded" };
            var executionIds = new List<string>{
                $"las:transition-execution:{Guid.NewGuid().ToString()}",
                $"las:transition-execution:{Guid.NewGuid().ToString()}"
            };
            var response = Toby.ListTransitionExecutions(
                transitionId,
                statuses,
                executionIds,
                maxResults,
                nextToken,
                sortBy,
                order
            );
            var expectedKeys = new [] {"executions"};
            CheckKeys(expectedKeys, response);
        }

        static object[] UpdateTransitionExecutionSources = {
            new object[] { "succeeded", new Dictionary<string, string>{{"foo", "bar"}}, null },
            new object[] { "failed", null, new Dictionary<string, string>{{"message", "foobar"}} }
        };

        [Test, TestCaseSource("UpdateTransitionExecutionSources")]
        public void TestUpdateTransitionExecution(
            string status,
            Dictionary<string, string>? output = null,
            Dictionary<string, string>? error = null
        ) {
            var transitionId = $"las:transition:{Guid.NewGuid().ToString()}";
            var executionId = $"las:transition-execution:{Guid.NewGuid().ToString()}";
            var response = Toby.UpdateTransitionExecution(
                transitionId,
                executionId,
                status,
                output,
                error
            );
            CheckKeys(new [] {
                "completedBy",
                "endTime",
                "executionId",
                "input",
                "logId",
                "startTime",
                "status",
                "transitionId"
            }, response);
        }

        [TestCase("foo@bar.com")]
        public void TestCreateUser(string email) {
            var response = Toby.CreateUser(email);
            CheckKeys(new [] {"email", "userId"}, response);
        }

        [TestCase("foo", 3)]
        [TestCase(null, null)]
        public void TestListUsers(string nextToken, int maxResults) {
            var response = Toby.ListUsers(nextToken: nextToken, maxResults: maxResults);
            CheckKeys(new [] {"nextToken", "users"}, response);
        }

        [Test]
        public void TestGetUser() {
            var userId = $"las:user:{Guid.NewGuid().ToString()}";
            var response = Toby.GetUser(userId);
            CheckKeys(new [] {"userId", "email"}, response);
        }

        [Test]
        [Ignore("delete endpoints doesn't work")]
        public void TestDeleteUser() {
            var userId = $"las:user:{Guid.NewGuid().ToString()}";
            var response = Toby.DeleteUser(userId);
            CheckKeys(new [] {"userId", "email"}, response);
        }

        [TestCase("name", "description")]
        [TestCase("", "description")]
        [TestCase("name", "")]
        [TestCase(null, null)]
        public void TestCreateWorkflow(string name, string description) {
            var spec = new Dictionary<string, object>{
                {"definition", new Dictionary<string, object>()}
            };
            var errorConfig = new Dictionary<string, string>{
                {"email", "foo@bar.com"}
            };
            var parameters = new Dictionary<string, string?>{
                {"name", name},
                {"description", description}
            };
            var response = Toby.CreateWorkflow(spec, errorConfig, parameters);
            CheckKeys(new [] {"workflowId", "name", "description"}, response);
        }

        [TestCase(100, "foo")]
        [TestCase(null, "foo")]
        [TestCase(100, null)]
        public void TestListWorkflows(
            int? maxResults = null,
            string? nextToken = null
        ) {
            var response = Toby.ListWorkflows(maxResults, nextToken);
            CheckKeys(new [] {"workflows"}, response);
        }

        [TestCase("name", "description")]
        [TestCase("", "description")]
        [TestCase("name", "")]
        [TestCase(null, null)]
        public void TestUpdateWorkflow(string name, string description) {
            var workflowId = $"las:workflow:{Guid.NewGuid().ToString()}";
            var response = Toby.UpdateWorkflow(workflowId, new Dictionary<string, string?>{
                {"name", name},
                {"description", description}
            });
            CheckKeys(new [] {"workflowId", "name", "description"}, response);
        }

        [Test]
        [Ignore("delete endpoints doesn't work")]
        public void TestDeleteWorkflow() {
            var workflowId = $"las:workflow:{Guid.NewGuid().ToString()}";
            var response = Toby.DeleteWorkflow(workflowId);
            CheckKeys(new [] {"workflowId", "name", "description"}, response);
        }

        [Test]
        public void TestExecuteWorkflow() {
            var workflowId = $"las:workflow:{Guid.NewGuid().ToString()}";
            var content = new Dictionary<string, object>();
            var response = Toby.ExecuteWorkflow(workflowId, content);
            var expectedKeys = new [] {
                "workflowId",
                "executionId",
                "startTime",
                "endTime",
                "transitionExecutions"
            };
            CheckKeys(expectedKeys, response);
        }

        [TestCase(
            3,
            null,
            "endTime",
            "ascending"
        )]
        public void TestListWorkflowExecutions(
            int? maxResults = null,
            string? nextToken = null,
            string? sortBy = null,
            string? order = null
        ) {
            var workflowId = $"las:workflow:{Guid.NewGuid().ToString()}";
            var statuses = new List<string>{ "running", "succeeded" };
            var response = Toby.ListWorkflowExecutions(
                workflowId,
                statuses,
                maxResults,
                nextToken,
                sortBy,
                order
            );
            CheckKeys(new [] {"workflowId", "executions"}, response);
        }

        [Test]
        [Ignore("delete endpoints doesn't work")]
        public void TestDeleteWorkflowExecution() {
            var workflowId = $"las:workflow:{Guid.NewGuid().ToString()}";
            var executionId = $"las:workflow-execution:{Guid.NewGuid().ToString()}";
            var response = Toby.DeleteWorkflowExecution(workflowId, executionId);
            var expectedKeys = new [] {
                "workflowId",
                "executionId",
                "startTime",
                "endTime",
                "transitionExecutions"
            };
            CheckKeys(expectedKeys, response);
        }
    }

    public static class Example 
    {
        public static byte[] Content() { return  Encoding.ASCII.GetBytes("%PDF-1.4foobarbaz");; }
        public static string ConsentId() { return "las:consent:abc123def456abc123def456abc123de"; }
        public static string ContentType() { return "image/jpeg"; }
        public static string DocumentId() { return "abcdefghijklabcdefghijklabcdefghijkl"; }
        public static string Description() { return "This is my new batch for receipts july 2020"; }
        public static string ModelType() { return "invoice"; }
        public static string ModelName() { return "invoice"; }
        public static string ModelId() { return "las:model:abc123def456abc123def456abc123de"; }
        public static string Endpoint() { return "http://127.0.0.1:4010"; }
        public static string DocPath() { return Environment.ExpandEnvironmentVariables("Test/Files/example.jpeg"); }
        public static Credentials Creds() 
        {
            return new Credentials("foo", "bar", "baz", "baaz", "http://127.0.0.1:4010"); 
        }
    }

    public static class ExampleDocSplit
    {
        public static string ConsentId() { return "bar"; }
        public static string ContentType() { return "application/pdf"; }
        public static string ModelType() { return "documentSplit"; }
        public static string Endpoint() { return "https://demo.api.lucidtech.ai/v1"; }
        public static string DocPath() { return Environment.ExpandEnvironmentVariables("Test/Files/example.pdf"); }
    }

    public static class ExampleExtraFlags
    {
        public static string ConsentId() { return "bar"; }
        public static string ContentType() { return "application/pdf"; }
        public static string ModelType() { return "invoice"; }
        public static string Endpoint() { return "http://127.0.0.1:4010"; }
        public static string DocPath() 
        { 
            return Environment.ExpandEnvironmentVariables("Test/Files/example.pdf"); 
        }
        public static string apiKey() { return ""; }
        public static string secretKey() { return ""; }
        public static string accessKey() { return ""; }
            
    }

}
