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
using Test.Service;

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
            var groundTruth = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string>(){{"label", "total_amount"},{"value", "54.50"}},
                new Dictionary<string, string>(){{"label", "purchase_date"},{"value", "2007-07-30"}}
            };
            var response = Toby.CreateDocument(
                body,
                Example.ContentType(),
                Example.ConsentId(),
                groundTruth: groundTruth
            );
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
            var response = Toby.GetAsset(Util.ResourceId("asset"));
            var expectedKeys = new [] {"assetId", "content"};
            CheckKeys(expectedKeys, response);
        }

        [TestCase("name", "description")]
        [TestCase("", "")]
        public void TestUpdateAsset(string? name, string? description) {
            var content = BitConverter.GetBytes(123456);
            var response = Toby.UpdateAsset(Util.ResourceId("asset"), content, new Dictionary<string?, string?>{
                {"name", name},
                {"description", description}
            });
            var expectedKeys = new [] {"assetId"};
            CheckKeys(expectedKeys, response);
        }

        [Ignore("delete endpoints doesn't work")]
        [Test]
        public void TestDeleteAsset() {
            var response = Toby.DeleteAsset(Util.ResourceId("asset"));
            CheckKeys(new [] {"assetId", "name", "description"}, response);
        }

        [Test]
        public void TestCreateDocument()
        {
            var expectedKeys = new [] {"documentId", "contentType", "consentId"};
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

        [TestCase("HIGH")]
        [TestCase("LOW")]
        [TestCase(null)]
        public void TestCreatePredictionBareMinimum(string? imageQuality)
        {
            var response = Toby.CreatePrediction(
                (string)CreateDocResponse["documentId"],
                Example.ModelId(),
                imageQuality: imageQuality
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

        [Ignore("delete endpoints doesn't work")]
        [TestCase(2, "foo", "las:consent:3ac6c39a3f9948a3b1aeb23ae7c73291")]
        public void TestDeleteDocuments(int maxResults, string nextToken, string consentId) {
            var response = Toby.DeleteDocuments(maxResults: maxResults, nextToken: nextToken, consentId: consentId);
            var expectedKeys = new [] {"documents"};
            CheckKeys(expectedKeys, response);
        }

        [TestCase(null, null)]
        [TestCase("name", "description")]
        public void TestCreateBatch(string? name, string? description)
        {
            var response = Toby.CreateBatch(Example.Description());
            var expectedKeys = new [] {"name", "description", "batchId"};
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
            var data = new Dictionary<string, string>() {
                {"username", username},
                {"password", password}
            };
            var expectedKeys = new [] {"secretId"};
            var response = Toby.UpdateSecret(Util.ResourceId("secret"), data, new Dictionary<string, string?>{
                {"name", name},
                {"description", description}
            });
            CheckKeys(expectedKeys, response);
        }

        [Ignore("delete endpoints doesn't work")]
        [Test]
        public void TestDeleteSecret() {
            var response = Toby.DeleteSecret(Util.ResourceId("secret"));
            CheckKeys(new [] {"secretId", "name", "description"}, response);
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
            var attributes = new Dictionary<string, string>{
                {"name", name},
                {"description", description}
            };

            Dictionary<string, object>? parameters = null;

            if (transitionType == "docker") {
                parameters = new Dictionary<string, object>{
                    {"cpu", 256},
                    {"imageUrl", "image_url"}
                };
            }

            var response = Toby.CreateTransition(transitionType, inputSchema, outputSchema, parameters, attributes);
            CheckKeys(new [] {"name", "transitionId", "transitionType"}, response);
        }

        [TestCase("docker")]
        [TestCase("manual")]
        [TestCase(null)]
        public void TestListTransitions(string? transitionType) {
            var response = Toby.ListTransitions(transitionType);
            CheckKeys(new [] {"transitions"}, response);
        }

        [Test]
        public void TestGetTransition() {
            var response = Toby.GetTransition(Util.ResourceId("transition"));
            CheckKeys(new [] {"transitionId", "name", "description", "transitionType"}, response);
        }

        [Ignore("delete endpoints doesn't work")]
        [Test]
        public void TestDeleteTransition() {
            var response = Toby.DeleteTransition(Util.ResourceId("transition"));
            CheckKeys(new [] {"transitionId", "name", "description", "transitionType"}, response);
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
            var assets = new Dictionary<string, string?>{
                {"foo", Util.ResourceId("asset")},
                {"bar", Util.ResourceId("asset")}
            };
            var environment = new Dictionary<string, string?>{
                {"FOO", "FOO"},
                {"BAR", "BAR"}
            };
            var environmentSecrets = new List<string>{ Util.ResourceId("secret")};
            var parameters = new Dictionary<string, string?>{
                {"name", name},
                {"description", description}
            };
            var response = Toby.UpdateTransition(
                Util.ResourceId("transition"),
                inputSchema,
                outputSchema,
                assets,
                environment,
                environmentSecrets,
                parameters);
            CheckKeys(new [] {"transitionId", "name", "description", "transitionType"}, response);
        }

        public void TestGetTransitionExecution() {
            var response = Toby.GetTransitionExecution(Util.ResourceId("transition"), Util.ResourceId("transition-execution"));
            CheckKeys(new [] {"transitionId", "executionId", "status"}, response);
        }

        [Test]
        public void TestExecuteTransition() {
            var response = Toby.ExecuteTransition(Util.ResourceId("transition"));
            CheckKeys(new [] {"transitionId", "executionId", "status"}, response);
        }

        [TestCase(
            "running",
            3,
            null,
            "startTime",
            "ascending"
        )]
        public void TestListTransitionExecutions(
            string? status = null,
            int? maxResults = null,
            string? nextToken = null,
            string? sortBy = null,
            string? order = null
        ) {
            var executionIds = new List<string>{ Util.ResourceId("transition-execution") };
            var response = Toby.ListTransitionExecutions(
                Util.ResourceId("transition"),
                status,
                executionIds,
                maxResults,
                nextToken,
                sortBy,
                order
            );
            var expectedKeys = new [] {"executions"};
            CheckKeys(expectedKeys, response);
        }

        [Ignore("multivalue query parameters don't work with prism")]
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
            var statuses = new List<string>{ "running", "succeeded" };
            var executionIds = new List<string>{
                Util.ResourceId("transition-execution"),
                Util.ResourceId("transition-execution")
            };
            var response = Toby.ListTransitionExecutions(
                Util.ResourceId("transition"),
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
            new object[] { "succeeded", new Dictionary<string, string>{{"foo", "bar"}}, null, new DateTime(2016, 12, 31, 5, 10, 20, DateTimeKind.Utc)},
            new object[] { "failed", null, new Dictionary<string, string>{{"message", "foobar"}}, null }
        };

        [Test, TestCaseSource("UpdateTransitionExecutionSources")]
        public void TestUpdateTransitionExecution(
            string status,
            Dictionary<string, string>? output = null,
            Dictionary<string, string>? error = null,
            DateTime? startTime = null
        ) {
            var response = Toby.UpdateTransitionExecution(
                Util.ResourceId("transition"),
                Util.ResourceId("transition-execution"),
                status,
                output,
                error,
                startTime
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

        [Test]
        public void TestSendHeartbeat() {
            var response = Toby.SendHeartbeat(Util.ResourceId("transition"), Util.ResourceId("transition-execution"));
            CheckKeys(new [] {"Your request executed successfully"}, response);
        }

        [Ignore("")]
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
            var response = Toby.GetUser(Util.ResourceId("user"));
            CheckKeys(new [] {"userId", "email"}, response);
        }

        [TestCase(null, null)]
        [TestCase("name", "avatar")]
        public void TestUpdateUser(string? name, string? avatar) {
            var parameters = new Dictionary<string, object?>{
                {"name", name},
                {"avatar", avatar},
            };
            var response = Toby.UpdateUser(Util.ResourceId("user"), parameters);
        }

        [Test]
        [Ignore("delete endpoints doesn't work")]
        public void TestDeleteUser() {
            var response = Toby.DeleteUser(Util.ResourceId("user"));
            CheckKeys(new [] {"userId", "email"}, response);
        }

        [TestCase("name", "description")]
        [TestCase("", "description")]
        [TestCase("name", "")]
        [TestCase(null, null)]
        public void TestCreateWorkflow(string name, string description) {
            var spec = new Dictionary<string, object>{
                {"definition", new Dictionary<string, object>{
                    {"States", new Dictionary<string, string>()}
                }}
            };

            var environmentSecrets = new List<string>{ Util.ResourceId("secret") };
            var env = new Dictionary<string, string>{{"FOO", "BAR"}};
            var completedConfig = new Dictionary<string, object>{
                {"imageUrl", "my/docker:image"},
                {"secretId", Util.ResourceId("secret")},
                {"environment", env},
                {"environmentSecrets", environmentSecrets}
            };
            var errorConfig = new Dictionary<string, object>{
                {"email", "foo@lucid.com"},
                {"manualRetry", true}
            };
            var parameters = new Dictionary<string, string?>{
                {"name", name},
                {"description", description}
            };
            var response = Toby.CreateWorkflow(spec, errorConfig, completedConfig, parameters);
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

        [Test]
        public void TestGetWorkflow() {
            var response = Toby.GetWorkflow(Util.ResourceId("workflow"));
            CheckKeys(new [] {"workflowId", "name", "description"}, response);
        }

        [TestCase("name", "description")]
        [TestCase("", "description")]
        [TestCase("name", "")]
        [TestCase(null, null)]
        public void TestUpdateWorkflow(string name, string description) {
            var response = Toby.UpdateWorkflow(Util.ResourceId("workflow"), new Dictionary<string, string?>{
                {"name", name},
                {"description", description}
            });
            CheckKeys(new [] {"workflowId", "name", "description"}, response);
        }

        [Test]
        [Ignore("delete endpoints doesn't work")]
        public void TestDeleteWorkflow() {
            var response = Toby.DeleteWorkflow(Util.ResourceId("workflow"));
            CheckKeys(new [] {"workflowId", "name", "description"}, response);
        }

        [Test]
        public void TestExecuteWorkflow() {
            var content = new Dictionary<string, object>();
            var response = Toby.ExecuteWorkflow(Util.ResourceId("workflow"), content);
            var expectedKeys = new [] {
                "workflowId",
                "executionId",
                "startTime",
                "endTime",
                "transitionExecutions"
            };
            CheckKeys(expectedKeys, response);
        }

        [Ignore("multivalue query parameters don't work with prism")]
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
            var statuses = new List<string>{ "running", "succeeded" };
            var response = Toby.ListWorkflowExecutions(
                Util.ResourceId("workflow"),
                statuses,
                maxResults,
                nextToken,
                sortBy,
                order
            );
            CheckKeys(new [] {"workflowId", "executions"}, response);
        }

        [Test]
        public void TestGetWorkflowExecution() {
            var response = Toby.GetWorkflowExecution(Util.ResourceId("workflow"), Util.ResourceId("workflow-execution"));
            CheckKeys(new [] {"workflowId", "executionId"}, response);
        }

        [Test]
        public void TestUpdateWorkflowExecution() {
            var response = Toby.UpdateWorkflowExecution(Util.ResourceId("workflow"), Util.ResourceId("workflow-execution"), Util.ResourceId("transition"));
            var expectedKeys = new [] {
                "workflowId",
                "executionId",
                "startTime",
                "endTime",
                "transitionExecutions"
            };
            CheckKeys(expectedKeys, response);
        }

        [Test]
        [Ignore("delete endpoints doesn't work")]
        public void TestDeleteWorkflowExecution() {
            var response = Toby.DeleteWorkflowExecution(Util.ResourceId("workflow"), Util.ResourceId("workflow-execution"));
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
        public static string ConsentId() { return "las:consent:abc123def456abc123def456abc123de"; }
        public static string ContentType() { return "image/jpeg"; }
        public static string Description() { return "This is my new batch for receipts july 2020"; }
        public static string ModelId() { return "las:model:abc123def456abc123def456abc123de"; }
        public static string DocPath() { return Environment.ExpandEnvironmentVariables("Test/Files/example.jpeg"); }
        public static Credentials Creds()
        {
            return new Credentials("foo", "bar", "baz", "baaz", "http://127.0.0.1:4010");
        }
    }
}
