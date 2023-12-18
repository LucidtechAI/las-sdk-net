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
            var mockCreds = new Mock<Credentials>("test", "test", "test", "http://localhost:4010");
            mockCreds
                .Protected()
                .Setup<(string, DateTime)>("GetClientCredentials")
                .Returns(("foobar", DateTime.Now));

            if (Environment.GetEnvironmentVariable("CREDENTIALS") == "FROM_FILE") {
                Toby = new Client();
            }
            else {
                Toby = new Client(mockCreds.Object);
            }
        }

        [SetUp]
        public void Setup()
        {
            byte[] body = File.ReadAllBytes(Example.DocPath());
            var groundTruth = new List<Dictionary<string, string>> {
                new Dictionary<string, string>{{"label", "total_amount"},{"value", "54.50"}},
                new Dictionary<string, string>{{"label", "purchase_date"},{"value", "2007-07-30"}}
            };
            var response = Toby.CreateDocument(
                body,
                Example.ContentType(),
                Example.ConsentId(),
                groundTruth: groundTruth
            );
            CreateDocResponse = JsonSerialPublisher.ObjectToDict<Dictionary<string, object>>(response);
        }

        [TestCase("name", "description", true)]
        [TestCase("", "", false)]
        [TestCase(null, null, false)]
        public void TestCreateAppClient(string? name, string? description, bool generateSecret ) {
            var parameters = new Dictionary<string, string?> {
                {"name", name},
                {"description", description}
            };
            var response = Toby.CreateAppClient(
                attributes: parameters,
                generateSecret: generateSecret,
                logoutUrls: new List<string>{"https://localhost/logout:3030"},
                loginUrls: new List<string>{"https://localhost/login:3030"},
                callbackUrls: new List<string>{"https://localhost/callback:3030"},
                defaultLoginUrl: "https://localhost/login:3030"
            );
            CheckKeys(Util.ExpectedKeys("appClient"), response);
        }

        [TestCase("foo", 3)]
        [TestCase(null, null)]
        public void TestListAppClients(string nextToken, int maxResults) {
            var response = Toby.ListAppClients(nextToken: nextToken, maxResults: maxResults);
            CheckKeys(Util.ExpectedKeys("appClients"), response);
        }

        [TestCase("name", "description")]
        [TestCase("", null)]
        [TestCase(null, null)]
        public void TestUpdateAppClient(string? name, string? description) {
            var parameters = new Dictionary<string, string?> {
                {"name", name},
                {"description", description}
            };
            var response = Toby.UpdateAppClient(
                appClientId: Util.ResourceId("app-client"),
                attributes: parameters
            );
            CheckKeys(Util.ExpectedKeys("appClient"), response);
        }

        [Ignore("delete endpoints doesn't work")]
        [Test]
        public void TestDeleteAppClient() {
            var response = Toby.DeleteAppClient(Util.ResourceId("app-client"));
            CheckKeys(Util.ExpectedKeys("appClient"), response);
        }

        [TestCase("name", "description")]
        [TestCase("", "")]
        [TestCase(null, null)]
        public void TestCreateAsset(string? name, string? description) {
            var bytes = BitConverter.GetBytes(12345);
            var parameters = new Dictionary<string, string?> {
                {"name", name},
                {"description", description}
            };
            var response = Toby.CreateAsset(bytes, parameters);
            CheckKeys(Util.ExpectedKeys("asset"), response);
        }

        [TestCase("foo", 3)]
        [TestCase(null, null)]
        public void TestListAssets(string nextToken, int maxResults) {
            var response = Toby.ListAssets(nextToken: nextToken, maxResults: maxResults);
            CheckKeys(Util.ExpectedKeys("assets"), response);
        }


        [Test]
        public void TestGetAssetById() {
            var response = Toby.GetAsset(Util.ResourceId("asset"));
            CheckKeys(Util.ExpectedKeys("asset"), response);
        }

        [TestCase("name", "description")]
        [TestCase("", "")]
        public void TestUpdateAsset(string? name, string? description) {
            var content = BitConverter.GetBytes(123456);
            var response = Toby.UpdateAsset(Util.ResourceId("asset"), content, new Dictionary<string?, string?> {
                {"name", name},
                {"description", description}
            });
            CheckKeys(Util.ExpectedKeys("asset"), response);
        }

        [Ignore("delete endpoints doesn't work")]
        [Test]
        public void TestDeleteAsset() {
            var response = Toby.DeleteAsset(Util.ResourceId("asset"));
            CheckKeys(Util.ExpectedKeys("asset"), response);
        }

        [Test]
        public void TestCreateDocument()
        {
            CheckKeys(Util.ExpectedKeys("document"), CreateDocResponse);
        }

        [TestCase(null, null, null, null)]
        public void TestListDocuments(string? nextToken, int? maxResults, string? consentId, string? datasetId) {
            var response = Toby.ListDocuments(
                nextToken: nextToken,
                maxResults: maxResults,
                consentId: consentId,
                datasetId: datasetId
            );
            CheckKeys(Util.ExpectedKeys("documents"), response);
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
            CheckKeys(Util.ExpectedKeys("prediction"), response);
        }

        [Test]
        public void TestCreatePredictionMaxPages()
        {
            var response = Toby.CreatePrediction(
                (string)CreateDocResponse["documentId"],
                Example.ModelId(),
                maxPages: 2
            );
            CheckKeys(Util.ExpectedKeys("prediction"), response);
        }

        [Test]
        public void TestCreatePredictionAutoRotate()
        {
            var response = Toby.CreatePrediction(
                (string)CreateDocResponse["documentId"],
                Example.ModelId(),
                autoRotate: true
            );
            CheckKeys(Util.ExpectedKeys("prediction"), response);
        }

        [Test]
        public void TestCreatePredictionPostProcess()
        {
            var response = Toby.CreatePrediction(
                (string)CreateDocResponse["documentId"],
                Example.ModelId(),
                postprocessConfig: Util.PostprocessConfigBestFirst()
            );
            CheckKeys(Util.ExpectedKeys("predictions"), response);

            var response2 = Toby.CreatePrediction(
                (string)CreateDocResponse["documentId"],
                Example.ModelId(),
                postprocessConfig: Util.PostprocessConfigBestNPages()
            );
            CheckKeys(Util.ExpectedKeys("predictions"), response2);
        }

        [Test]
        public void TestGetDocument()
        {
            var response = Toby.GetDocument((string)CreateDocResponse["documentId"]);
            CheckKeys(Util.ExpectedKeys("document"), response);
        }

        [TestCase("54.50", "2007-07-30")]
        public void TestUpdateDocument(string total_amount, string purchase_date)
        {
            var ground_truth = new List<Dictionary<string, string>> {
                new Dictionary<string, string>{{"label", "total_amount"},{"value", total_amount}},
                new Dictionary<string, string>{{"label", "purchase_date"},{"value", purchase_date}}
            };
            var response = Toby.UpdateDocument(
                (string)CreateDocResponse["documentId"],
                ground_truth,
                Util.ResourceId("dataset")
            );
            CheckKeys(Util.ExpectedKeys("document"), response);
        }

        [Ignore("delete endpoints doesn't work")]
        [TestCase(2, "foo", "las:consent:3ac6c39a3f9948a3b1aeb23ae7c73291")]
        public void TestDeleteDocuments(int maxResults, string nextToken, string consentId) {
            var response = Toby.DeleteDocuments(
                maxResults: maxResults,
                nextToken: nextToken,
                consentId: consentId,
                datasetId: Util.ResourceId("dataset")
            );
            CheckKeys(Util.ExpectedKeys("documents"), response);
        }

        [Ignore("delete endpoints doesn't work")]
        public void TestDeleteDocument() {
            var response = Toby.DeleteDocument(Util.ResourceId("document"));
            CheckKeys(Util.ExpectedKeys("document"), response);
        }

        [TestCase(null, null)]
        [TestCase("name", "description")]
        public void TestCreateDataset(string? name, string? description)
        {
            var response = Toby.CreateDataset(Example.Description());
            CheckKeys(Util.ExpectedKeys("dataset"), response);
        }

        [Test]
        public void TestListDatasets() {
            var response = Toby.ListDatasets(
                maxResults: 100,
                nextToken: "foo"
            );
            CheckKeys(Util.ExpectedKeys("datasets"), response);
        }

        [TestCase("name", "description")]
        [TestCase("", null)]
        [TestCase(null, null)]
        public void TestUpdateDataset(string? name, string? description) {
            var parameters = new Dictionary<string, string?> {
                {"name", name},
                {"description", description}
            };
            var response = Toby.UpdateDataset(
                datasetId: Util.ResourceId("dataset"),
                attributes: parameters
            );
            CheckKeys(Util.ExpectedKeys("dataset"), response);
        }

        [Ignore("delete endpoints doesn't work")]
        [TestCase(true)]
        [TestCase(false)]
        public void TestDeleteDataset(bool deleteDocuments) {
            var response = Toby.DeleteDataset(Util.ResourceId("dataset"), deleteDocuments);
            CheckKeys(Util.ExpectedKeys("dataset"), response);
        }

        [Test]
        public void TestListLogs() {
            var response = Toby.ListLogs(
                transitionId: Util.ResourceId("transition"),
                transitionExecutionId: Util.ResourceId("transition-execution"),
                workflowId: Util.ResourceId("workflow"),
                workflowExecutionId: Util.ResourceId("workflow-execution"),
                maxResults: 100,
                nextToken: "foo"
            );
            CheckKeys(Util.ExpectedKeys("logs"), response);
        }

        [TestCase("foo", 3)]
        [TestCase(null, null)]
        public void TestListModels(string nextToken, int maxResults) {
            var response = Toby.ListModels(nextToken: nextToken, maxResults: maxResults);
            CheckKeys(Util.ExpectedKeys("models"), response);
        }

        [Test]
        public void TestGetModel() {
            var response = Toby.GetModel(Util.ResourceId("model"));
            CheckKeys(Util.ExpectedKeys("model"), response);
        }

        [TestCase("name", "description")]
        [TestCase(null, "description")]
        public void TestCreateModel(string name, string description) {
            var response = Toby.CreateModel(
                fieldConfig: Util.FieldConfig(),
                preprocessConfig: Util.PreprocessConfig(),
                name: name,
                description: description
            );
            CheckKeys(Util.ExpectedKeys("model"), response);
        }

        [Test]
        public void TestCreateModelSimple() {
            var response = Toby.CreateModel(
                fieldConfig: Util.FieldConfig()
            );
            CheckKeys(Util.ExpectedKeys("model"), response);
        }


        [TestCase("name", "description")]
        [TestCase(null, "description")]
        [Ignore("TODO: Fixme")]
        public void TestUpdateModel(string? name, string? description) {
            var response = Toby.UpdateModel(
                modelId: Util.ResourceId("model"),
                fieldConfig: Util.FieldConfig(),
                preprocessConfig: Util.PreprocessConfig(),
                name: name,
                description: description,
                status: "training"
            );
            CheckKeys(Util.ExpectedKeys("model"), response);
        }

        public void TestUpdateModelSimple() {
            var response = Toby.UpdateModel(modelId: Util.ResourceId("model"));
            CheckKeys(Util.ExpectedKeys("model"), response);
        }

        [TestCase(null, null)]
        [TestCase("name", "description")]
        public void TestCreateDataBundle(string? name, string? description)
        {
            var datasetIds= new List<string>{ Util.ResourceId("dataset"), Util.ResourceId("dataset")};
            var response = Toby.CreateDataBundle(
                modelId: Util.ResourceId("model"),
                datasetIds: datasetIds,
                name: name,
                description: description
            );
            CheckKeys(Util.ExpectedKeys("dataBundle"), response);
        }

        [Test]
        public void TestListDataBundles() {
            var response = Toby.ListDataBundles(
                modelId: Util.ResourceId("model"),
                maxResults: 100,
                nextToken: "foo"
            );
            CheckKeys(Util.ExpectedKeys("dataBundles"), response);
        }

        [TestCase("name", "description")]
        [TestCase("", null)]
        [TestCase(null, null)]
        public void TestUpdateDataBundle(string? name, string? description) {
            var parameters = new Dictionary<string, string?> {
                {"name", name},
                {"description", description}
            };
            var response = Toby.UpdateDataBundle(
                modelId: Util.ResourceId("model"),
                dataBundleId: Util.ResourceId("model-data-bundle"),
                attributes: parameters
            );
            CheckKeys(Util.ExpectedKeys("dataBundle"), response);
        }

        [Ignore("delete endpoints doesn't work")]
        [TestCase(true)]
        [TestCase(false)]
        public void TestDeleteDataBundle(bool deleteDocuments) {
            var response = Toby.DeleteDataBundle(Util.ResourceId("model"), Util.ResourceId("model-data-bundle"));
            CheckKeys(Util.ExpectedKeys("dataBundle"), response);
        }

        [TestCase("foo", 3)]
        [TestCase(null, null)]
        public void TestListPredictions(string nextToken, int maxResults) {
            var response = Toby.ListPredictions(nextToken: nextToken, maxResults: maxResults);
            CheckKeys(Util.ExpectedKeys("predictions"), response);
        }

        [TestCase("foo", "bar")]
        public void TestCreateSecret(string username, string password) {
            var data = new Dictionary<string, string>{
                {"username", username},
                {"password", password}
            };
            var response = Toby.CreateSecret(data);
            CheckKeys(Util.ExpectedKeys("secret"), response);
        }

        [TestCase("foo", 3)]
        [TestCase(null, null)]
        public void TestListSecrets(string nextToken, int maxResults) {
            var response = Toby.ListSecrets(nextToken: nextToken, maxResults: maxResults);
            CheckKeys(Util.ExpectedKeys("secrets"), response);
        }

        [TestCase("foo", "bar", "name", "description")]
        [TestCase("foo", "bar", "name", "")]
        public void TestUpdateSecret(string username, string password, string? name = null, string? description = null) {
            var data = new Dictionary<string, string> {
                {"username", username},
                {"password", password}
            };
            var response = Toby.UpdateSecret(Util.ResourceId("secret"), data, new Dictionary<string, string?>
            {
                {"name", name},
                {"description", description}
            });
            CheckKeys(Util.ExpectedKeys("secret"), response);
        }

        [Ignore("delete endpoints doesn't work")]
        [Test]
        public void TestDeleteSecret() {
            var response = Toby.DeleteSecret(Util.ResourceId("secret"));
            CheckKeys(Util.ExpectedKeys("secret"), response);
        }

        [TestCase("docker", "name", "description")]
        [TestCase("manual", "name", "description")]
        [TestCase("docker", null, null)]
        public void TestCreateTransition(string transitionType, string name, string description) {
            var schema = new Dictionary<string, string> {
                {"schema", "https://json-schema.org/draft-04/schema#"},
                {"title", "response"}
            };

            var inputSchema = schema;
            var outputSchema = schema;
            var attributes = new Dictionary<string, string> {
                {"name", name},
                {"description", description}
            };

            Dictionary<string, object>? parameters = null;

            if (transitionType == "docker") {
                parameters = new Dictionary<string, object> {
                    {"cpu", 256},
                    {"imageUrl", "image_url"}
                };
            }

            var response = Toby.CreateTransition(transitionType, inputSchema, outputSchema, parameters, attributes);
            CheckKeys(Util.ExpectedKeys("transition"), response);
        }

        [TestCase("docker")]
        [TestCase("manual")]
        [TestCase(null)]
        public void TestListTransitions(string? transitionType) {
            var response = Toby.ListTransitions(transitionType);
            CheckKeys(Util.ExpectedKeys("transitions"), response);
        }

        [Test]
        public void TestGetTransition() {
            var response = Toby.GetTransition(Util.ResourceId("transition"));
            CheckKeys(Util.ExpectedKeys("transition"), response);
        }

        [Ignore("delete endpoints doesn't work")]
        [Test]
        public void TestDeleteTransition() {
            var response = Toby.DeleteTransition(Util.ResourceId("transition"));
            CheckKeys(Util.ExpectedKeys("transition"), response);
        }

        [TestCase("foo", "bar")]
        [TestCase(null, null)]
        public void TestUpdateTransition(string? name, string? description) {

            var schema = new Dictionary<string, string> {
                {"schema", "https://json-schema.org/draft-04/schema#"},
                {"title", "response"}
            };
            var inputSchema = schema;
            var outputSchema = schema;
            var assets = new Dictionary<string, string?> {
                {"foo", Util.ResourceId("asset")},
                {"bar", Util.ResourceId("asset")}
            };
            var environment = new Dictionary<string, string?> {
                {"FOO", "FOO"},
                {"BAR", "BAR"}
            };
            var environmentSecrets = new List<string>{ Util.ResourceId("secret")};
            var parameters = new Dictionary<string, string?>
            {
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
            CheckKeys(Util.ExpectedKeys("transition"), response);
        }

        public void TestGetTransitionExecution() {
            var response = Toby.GetTransitionExecution(Util.ResourceId("transition"), Util.ResourceId("transition-execution"));
            CheckKeys(Util.ExpectedKeys("transition-execution"), response);
        }

        [Test]
        public void TestExecuteTransition() {
            var response = Toby.ExecuteTransition(Util.ResourceId("transition"));
            CheckKeys(Util.ExpectedKeys("transition-execution"), response);
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
            CheckKeys(Util.ExpectedKeys("transition-executions"), response);
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
            var executionIds = new List<string> {
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
            CheckKeys(Util.ExpectedKeys("transition-executions"), response);
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
            CheckKeys(Util.ExpectedKeys("transition-execution"), response);
        }

        [Test]
        public void TestSendHeartbeat() {
            var response = Toby.SendHeartbeat(Util.ResourceId("transition"), Util.ResourceId("transition-execution"));
            CheckKeys(Util.ExpectedKeys("heartbeats"), response);
        }

        [Ignore("")]
        [TestCase("foo@bar.com")]
        public void TestCreateUser(string email) {
            var response = Toby.CreateUser(email);
            CheckKeys(Util.ExpectedKeys("user"), response);
        }

        [TestCase("foo", 3)]
        [TestCase(null, null)]
        public void TestListUsers(string nextToken, int maxResults) {
            var response = Toby.ListUsers(nextToken: nextToken, maxResults: maxResults);
            CheckKeys(Util.ExpectedKeys("users"), response);
        }

        [Test]
        public void TestGetUser() {
            var response = Toby.GetUser(Util.ResourceId("user"));
            CheckKeys(Util.ExpectedKeys("user"), response);
        }

        [TestCase("las:role:08b49ae64cd746f384f05880ef5de72f")]
        public void TestUpdateUser(string? role_id) {
            var parameters = new Dictionary<string, object?> {};

            if (role_id != null) {
                parameters.Add("roleIds", new List<string>{role_id});
            }

            var response = Toby.UpdateUser(Util.ResourceId("user"), parameters);
        }

        [Test]
        [Ignore("delete endpoints doesn't work")]
        public void TestDeleteUser() {
            var response = Toby.DeleteUser(Util.ResourceId("user"));
            CheckKeys(Util.ExpectedKeys("user"), response);
        }

        [TestCase("name", "description")]
        [TestCase("", "description")]
        [TestCase("name", "")]
        [TestCase(null, null)]
        public void TestCreateWorkflow(string name, string description) {
            var spec = new Dictionary<string, object> {
                {"definition", new Dictionary<string, object> {
                        {"States", new Dictionary<string, string>()}
                    }
                }
            };
            var parameters = new Dictionary<string, string?> {
                {"name", name},
                {"description", description}
            };
            var response = Toby.CreateWorkflow(spec, Util.ErrorConfig(), Util.CompletedConfig(), parameters);
            CheckKeys(Util.ExpectedKeys("workflow"), response);
        }

        [TestCase(100, "foo")]
        [TestCase(null, "foo")]
        [TestCase(100, null)]
        public void TestListWorkflows(
            int? maxResults = null,
            string? nextToken = null
        ) {
            var response = Toby.ListWorkflows(maxResults, nextToken);
            CheckKeys(Util.ExpectedKeys("workflows"), response);
        }

        [Test]
        public void TestGetWorkflow() {
            var response = Toby.GetWorkflow(Util.ResourceId("workflow"));
            CheckKeys(Util.ExpectedKeys("workflow"), response);
        }

        [TestCase("name", "description")]
        [TestCase("", "description")]
        [TestCase("name", "")]
        [TestCase(null, null)]
        public void TestUpdateWorkflow(string name, string description) {
            var response = Toby.UpdateWorkflow(
                Util.ResourceId("workflow"),
                errorConfig: Util.ErrorConfig(),
                completedConfig: Util.CompletedConfig(),
                attributes: Util.NameAndDescription(name, description)
            );
            CheckKeys(Util.ExpectedKeys("workflow"), response);
        }

        [Test]
        [Ignore("delete endpoints doesn't work")]
        public void TestDeleteWorkflow() {
            var response = Toby.DeleteWorkflow(Util.ResourceId("workflow"));
            CheckKeys(Util.ExpectedKeys("workflow"), response);
        }

        [Test]
        public void TestExecuteWorkflow() {
            var content = new Dictionary<string, object>();
            var response = Toby.ExecuteWorkflow(Util.ResourceId("workflow"), content);
            CheckKeys(Util.ExpectedKeys("workflow-execution"), response);
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
            CheckKeys(Util.ExpectedKeys("workflow-executions"), response);
        }

        [Test]
        public void TestGetWorkflowExecution() {
            var response = Toby.GetWorkflowExecution(Util.ResourceId("workflow"), Util.ResourceId("workflow-execution"));
            CheckKeys(Util.ExpectedKeys("workflow-execution"), response);
        }

        [Test]
        public void TestUpdateWorkflowExecution() {
            var response = Toby.UpdateWorkflowExecution(Util.ResourceId("workflow"), Util.ResourceId("workflow-execution"), Util.ResourceId("transition"));
            CheckKeys(Util.ExpectedKeys("workflow-execution"), response);
        }

        [Test]
        public void TestUpdateWorkflowExecutionWithStatus() {
            var response = Toby.UpdateWorkflowExecution(Util.ResourceId("workflow"), Util.ResourceId("workflow-execution"), status: "completed");
            CheckKeys(Util.ExpectedKeys("workflow-execution"), response);
        }

        [Test]
        [Ignore("delete endpoints doesn't work")]
        public void TestDeleteWorkflowExecution() {
            var response = Toby.DeleteWorkflowExecution(Util.ResourceId("workflow"), Util.ResourceId("workflow-execution"));
            CheckKeys(Util.ExpectedKeys("workflow-execution"), response);
        }
    }

    public static class Example
    {
        public static string ConsentId() { return "las:consent:abc123def456abc123def456abc123de"; }
        public static string ContentType() { return "image/jpeg"; }
        public static string Description() { return "This is my new dataset for receipts july 2020"; }
        public static string ModelId() { return "las:model:abc123def456abc123def456abc123de"; }
        public static string DocPath() { return Environment.ExpandEnvironmentVariables("Test/Files/example.jpeg"); }
        public static Credentials Creds()
        {
            return new Credentials("foo", "bar", "baaz", "http://127.0.0.1:4010");
        }
    }
}
