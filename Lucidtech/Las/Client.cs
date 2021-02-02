using System;
using System.Collections.Generic;
using System.Net;
using Polly;

using RestSharp;
using Newtonsoft.Json;

using Lucidtech.Las.Core;
using Lucidtech.Las.Utils;

namespace Lucidtech.Las
{
    /// <summary>
    /// A low level client to invoke api methods from Lucidtech AI Services.
    /// </summary>
    public class Client 
    {
        private RestClient RestSharpClient { get; }
        private Credentials LasCredentials { get; }

        /// <summary>
        /// Client constructor.
        /// </summary>
        /// <param name="credentials"> Keys, endpoints and credentials needed for authorization </param>
        public Client(Credentials credentials)
        {
            LasCredentials = credentials;
            var uri = new Uri(LasCredentials.ApiEndpoint);
            RestSharpClient = new RestClient(uri.GetLeftPart(UriPartial.Authority));
        }
        
        /// <summary>
        /// Client constructor with credentials read from local file.
        /// </summary>
        public Client() : this(new Credentials()) {}

        public object CreateAsset(byte[] content, Dictionary<string, string?>? optionalParams) {
            string base64Content = System.Convert.ToBase64String(content);
            var body = new Dictionary<string, string?>(){
                {"content", base64Content}
            };

            if (optionalParams != null) {
                foreach (KeyValuePair<string, string?> entry in optionalParams) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            RestRequest request = ClientRestRequest(Method.POST, "/assets", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object ListAssets(int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/assets", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object GetAsset(string assetId) {
            RestRequest request = ClientRestRequest(Method.GET, $"/assets/{assetId}");
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object UpdateAsset(string assetId, byte[]? content = null, Dictionary<string, string?>? optionalParams = null) {
            var body = new Dictionary<string, string?>();

            if (content != null) {
                string base64Content = System.Convert.ToBase64String(content);
                body.Add("content", base64Content);
            }

            if (optionalParams != null) {
                foreach (KeyValuePair<string, string?> entry in optionalParams) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            RestRequest request = ClientRestRequest(Method.PATCH, $"/assets/{assetId}", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>
        /// Creates a document handle, calls the POST /documents endpoint
        /// </summary>
        /// <example>
        /// Create a document handle for a jpeg image
        /// <code>
        /// Client client = new Client();
        /// byte[] content = File.ReadAllBytes("MyReceipt.jpeg");
        /// var response = client.CreateDocument(content, "image/jpeg", "bar");
        /// </code>
        /// </example>
        /// <param name="content"> Content to POST </param>
        /// <param name="contentType"> A mime type for the document handle </param>
        /// <param name="consentId"> An identifier to mark the owner of the document handle </param>
        /// <param name="batchId"> Specifies the batch to which the document will be associated with </param>
        /// <param name="feedback"> A list of feedback items {label: value},
        /// representing the ground truth values for the document </param>
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields
        /// with batchId, documentId, contentType and consentId
        /// </returns>
        public object CreateDocument(
            byte[] content,
            string contentType,
            string? consentId = null,
            string? batchId = null,
            List<Dictionary<string, string>>? groundTruth = null)
        {
            string base64Content = System.Convert.ToBase64String(content);
            var body = new Dictionary<string, string?>()
            {
                {"content", base64Content},
                {"contentType", contentType}, 
            };

            if(consentId != null) {
                body.Add("consentId", consentId);
            }
            
            if (!string.IsNullOrEmpty(batchId)) {
                body.Add("batchId", batchId);
            }

            if (groundTruth != null) { 
                string fb = JsonConvert.SerializeObject(groundTruth);
                body.Add("groundTruth", fb);
            }
            
            RestRequest request = ClientRestRequest(Method.POST, "/documents", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>
        /// Get documents from the REST API, calls the GET /documents endpoint.
        /// </summary>
        /// <example>
        /// Create a document handle for a jpeg image
        /// <code>
        /// Client client = new Client();
        /// var response = client.ListDocuments('&lt;batchId&gt;');
        /// </code>
        /// </example>
        /// <param name="batchId"> The batch id that contains the documents of interest </param>
        /// <param name="consentId"> An identifier to mark the owner of the document handle </param>
        /// <returns> Documents from REST API contained in batch </returns>
        public object ListDocuments(
            string? batchId = null, 
            string? consentId = null, 
            int? maxResults = null, 
            string? nextToken = null
        )
        {
            var queryParams = new Dictionary<string, object?>();

            if (!string.IsNullOrEmpty(batchId)) {
                queryParams.Add("batchId", batchId);
            }

            if (!string.IsNullOrEmpty(consentId)) {
                queryParams.Add("consentId", consentId);
            }

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/documents", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        } 
        
        /// <summary>
        /// Get document from the REST API, calls the GET /documents/{documentId} endpoint.
        /// </summary>
        /// <example>
        /// Get information of document specified by &lt;documentId&gt;
        /// <code>
        /// Client client = new Client();
        /// var response = client.GetDocument('&lt;documentId&gt;');
        /// </code>
        /// </example>
        /// <param name="documentId"> The document id to run inference and create a prediction on </param>
        /// <returns> Document information from REST API </returns>
        public object GetDocument(string documentId)
        {
            RestRequest request = ClientRestRequest(Method.GET, $"/documents/{documentId}");
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>
        /// Post feedback to the REST API, calls the POST /documents/{documentId} endpoint.
        /// Posting feedback means posting the ground truth data for the particular document.
        /// This enables the API to learn from past mistakes. 
        /// </summary> 
        /// <example>
        /// <code>
        /// Client client = new Client();
        /// var feedback = new List&lt;Dictionary&lt;string, string&gt;&gt;()
        /// {
        ///     new Dictionary&lt;string, string&gt;(){{"label", "total_amount"},{"value", "54.50"}},
        ///     new Dictionary&lt;string, string&gt;(){{"label", "purchase_date"},{"value", "2007-07-30"}}
        /// };
        /// var response = client.UpdateDocument('&lt;documentId&gt;', feedback);
        /// </code>
        /// </example>
        /// <param name="documentId"> Path to document to upload,
        /// Same as provided to <see cref="CreateDocument"/></param>
        /// <param name="groundTruth"> A list of ground truth items </param>
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields
        /// documentId, consentId, uploadUrl, contentType and feedback.
        /// </returns>
        ///
        public object UpdateDocument(string documentId, List<Dictionary<string, string>> groundTruth)
        {
            var bodyDict = new Dictionary<string, List<Dictionary<string,string>>>() {{"groundTruth", groundTruth}};
            
            // Doing a manual cast from Dictionary to object to help out the serialization process
            string bodyString = JsonConvert.SerializeObject(bodyDict);
            object body = JsonConvert.DeserializeObject(bodyString);
            
            RestRequest request = ClientRestRequest(Method.PATCH, $"/documents/{documentId}", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object DeleteDocuments(string? consentId = null) {
            var queryParams = new Dictionary<string, object?>();

            if (consentId != null) {
                queryParams.Add("consentId", consentId);
            }

            RestRequest request = ClientRestRequest(Method.DELETE, "/documents", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>
        /// Delete documents with this consentId, calls DELETE /consent/{consentId} endpoint.
        /// </summary>
        /// <example><code>
        /// Client client = new Client();
        /// var response = client.DeleteConsent('&lt;consentId&gt;');
        /// </code></example>
        /// <param name="consentId"> Delete documents with provided consentId </param>
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields
        /// consentId and documentIds 
        /// </returns>
        public object DeleteConsent(string consentId)
        {
            RestRequest request = ClientRestRequest(Method.DELETE, $"/consents/{consentId}");
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>
        /// Create a batch handle, calls the POST /batches endpoint.
        /// </summary>
        /// <example>
        /// Create a new batch with the provided description.
        /// on the document specified by '&lt;batchId&gt;'
        /// <code>
        /// Client client = new Client();
        /// var response = client.CreateBatch("Data gathered from the Mars Rover Invoice Scan Mission");
        /// </code>
        /// </example>
        /// <param name="description"> A brief description of the purpose of the batch </param>
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields batchId and description.
        /// batchId can be used as an input when posting documents to make them a part of this batch.
        /// </returns>
        public object CreateBatch(string description)
        {
            var body = new Dictionary<string, string>() { {"description", description} };
            RestRequest request = ClientRestRequest(Method.POST, "/batches", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>
        /// Run inference and create a prediction, calls the POST /predictions endpoint.
        /// </summary>
        /// <example>
        /// Run inference and create a prediction using the invoice model
        /// on the document specified by '&lt;documentId&gt;'
        /// <code>
        /// Client client = new Client();
        /// var response = client.CreatePrediction('&lt;documentId&gt;',"las:model:99cac468f7cf47ddad12e5e017540389");
        /// </code>
        /// </example>
        /// <param name="documentId"> Path to document to
        /// upload Same as provided to <see cref="CreateDocument"/></param>
        /// <param name="modelId"> Id of the model to use for inference </param>
        /// <param name="maxPages"> Maximum number of pages to run predictions on </param>
        /// <param name="autoRotate"> Whether or not to let the API try different 
        /// rotations on the document when running </param>
        /// <param name="extras"> Extra information to add to json body </param>
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields documentId and predictions,
        /// the value of predictions is the output from the model.
        /// </returns>
        public object CreatePrediction(
            string documentId,
            string modelId,
            int? maxPages = null,
            bool? autoRotate = null
        )
        {
            var body = new Dictionary<string, object>() { {"documentId", documentId}, {"modelId", modelId}};
            if (maxPages != null) { body.Add("maxPages", maxPages);}
            if (autoRotate != null) { body.Add("autoRotate", autoRotate);}

            RestRequest request = ClientRestRequest(Method.POST, "/predictions", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object ListPredictions(int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/predictions", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object ListModels(int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/models", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object CreateSecret(Dictionary<string, string> data, Dictionary<string, string?>? optionalParams = null) {
            var body = new Dictionary<string, object?>() {
                {"data", data}
            };

            if (optionalParams != null) {
                foreach (KeyValuePair<string, string?> entry in optionalParams) {
                    body.Add(entry.Key, entry.Value);
                }
            }
            RestRequest request = ClientRestRequest(Method.POST, "/secrets", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object ListSecrets(int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/secrets", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object UpdateSecret(
            string secretId,
            Dictionary<string, string>? data,
            Dictionary<string, string?>? optionalParams = null
        ) {
            var body = new Dictionary<string, object?>();

            if (optionalParams != null) {
                foreach (KeyValuePair<string, string?> entry in optionalParams) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            if (data != null) {
                body.Add("data", data);
            }

            RestRequest request = ClientRestRequest(Method.PATCH, $"/secrets/{secretId}", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object CreateTransition(
            string transitionType,
            Dictionary<string, string> inputJsonSchema,
            Dictionary<string, string> outputJsonSchema,
            Dictionary<string, object?>? parameters = null,
            Dictionary<string, string?>? optionalParams = null
        ) {
            var body = new Dictionary<string, object?>() {
                {"inputJsonSchema", inputJsonSchema},
                {"outputJsonSchema", outputJsonSchema},
                {"transitionType", transitionType},
            };

            if (parameters != null) {
                body.Add("parameters", parameters);
            }

            if (optionalParams != null) {
                foreach (KeyValuePair<string, string?> entry in optionalParams) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            RestRequest request = ClientRestRequest(Method.POST, "/transitions", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object ListTransitions(string? transitionType = null, int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/transitions", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }
        public object ListTransitions(List<string> transitionType, int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/transitions", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object GetTransitionExecution(string transitionId, string executionId) {
            RestRequest request = ClientRestRequest(Method.GET, $"/transitions/{transitionId}/executions/{executionId}");
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object UpdateTransition(
            string transitionId,
            Dictionary<string, string> inputJsonSchema,
            Dictionary<string, string> outputJsonSchema,
            Dictionary<string, string?> optionalParams
        ) {
            var body = new Dictionary<string, object?>() {
                {"inputJsonSchema", inputJsonSchema},
                {"outputJsonSchema", outputJsonSchema}
            };

            if (optionalParams != null) {
                foreach (KeyValuePair<string, string?> entry in optionalParams) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            RestRequest request = ClientRestRequest(Method.PATCH, $"/transitions/{transitionId}", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object ExecuteTransition(string transitionId) {
            var request = ClientRestRequest(Method.POST, $"/transitions/{transitionId}/executions");
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object ListTransitionExecutions(
            string transitionId,
            List<string>? statuses = null,
            List<string>? executionIds = null,
            int? maxResults = null,
            string? nextToken = null,
            string? sortBy = null,
            string? order = null
        ) {
            var queryParams = new Dictionary<string, object?> {
                {"status", statuses},
                {"executionId", executionIds},
            };

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            if (sortBy != null) {
                queryParams.Add("sortBy", sortBy);
            }

            if (order != null) {
                queryParams.Add("order", order);
            }

            var request = ClientRestRequest(Method.GET, "/transitions/{transitionId}/executions", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object UpdateTransitionExecution(
            string transitionId,
            string executionId,
            string status,
            Dictionary<string, string>? output = null,
            Dictionary<string, string>? error = null
        ) {
            var url = $"/transitions/{transitionId}/executions/{executionId}";
            var body = new Dictionary<string, object>{
                {"status", status},
            };

            if (output != null) {
                body.Add("output", output);
            }

            if (error != null) {
                body.Add("error", error);
            }
            var request = ClientRestRequest(Method.PATCH, url, body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object CreateUser(string email) {
            var body = new Dictionary<string, string>{
                {"email", email}
            };
            var request = ClientRestRequest(Method.POST, "/users", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object ListUsers(int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            var request = ClientRestRequest(Method.GET, "/users", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object GetUser(string userId) {
            var request = ClientRestRequest(Method.GET, $"/users/{userId}");
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object DeleteUser(string userId) {
            var request = ClientRestRequest(Method.DELETE, $"/users/{userId}");
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object CreateWorkflow(
            Dictionary<string, object> spec,
            Dictionary<string, string>? errorConfig = null,
            Dictionary<string, string?>? optionalParams = null
        ) {
            var body = new Dictionary<string, object?>{
                {"specification", spec}
            };

            if (errorConfig != null) {
                body.Add("errorConfig", errorConfig);
            }

            if (optionalParams != null) {
                foreach (KeyValuePair<string, string?> entry in optionalParams) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            var request = ClientRestRequest(Method.POST, "/workflows", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object ListWorkflows(int? maxResults, string nextToken) {
            var queryParams = new Dictionary<string, object>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            var request = ClientRestRequest(Method.GET, "/workflows", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object UpdateWorkflow(string workflowId, Dictionary<string, string?>? optionalParams = null) {
            var request = ClientRestRequest(Method.PATCH, $"/workflows/{workflowId}", optionalParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object DeleteWorkflow(string workflowId) {
            var request = ClientRestRequest(Method.DELETE, $"/workflows/{workflowId}");
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object ExecuteWorkflow(string workflowId, Dictionary<string, object> content) {
            var body = new Dictionary<string, object> {
                {"input", content}
            };
            var request = ClientRestRequest(Method.POST, $"/workflows/{workflowId}/executions", body);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object ListWorkflowExecutions(
            string workflowId,
            List<string>? statuses = null,
            int? maxResults = null,
            string? nextToken = null,
            string? sortBy = null,
            string? order = null
        ) {
            var queryParams = new Dictionary<string, object?> {
                {"status", statuses},
            };

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            if (sortBy != null) {
                queryParams.Add("sortBy", sortBy);
            }

            if (order != null) {
                queryParams.Add("order", order);
            }

            var request = ClientRestRequest(Method.GET, $"/workflows/{workflowId}/executions", null, queryParams);
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        public object DeleteWorkflowExecution(string workflowId, string executionId) {
            var request = ClientRestRequest(Method.DELETE, $"/workflows/{workflowId}/executions/{executionId}");
            return ExecuteRequestResilient(RestSharpClient, request);
        }

        /// <summary>
        /// Create a HTTP web request for the REST API. 
        /// </summary>
        /// <param name="method"> The request method, e.g. POST, PUT, GET, DELETE </param>
        /// <param name="path"> The path to the domain upon which to apply the request,
        /// the total path will be <see href="Credentials.ApiEndpoint"/>path</param>
        /// <param name="body"> The content of the request </param>
        /// <param name="queryParams">Query parameters</param>
        /// <returns>
        /// An object of type <see cref="RestRequest"/> defined by the input
        /// </returns>
        private RestRequest ClientRestRequest(
            Method method,
            string path,
            object? body = null,
            Dictionary<string, object?>? queryParams = null)
        {
            Uri endpoint = new Uri(string.Concat(LasCredentials.ApiEndpoint, path));

            var request = new RestRequest(endpoint, method, DataFormat.Json);
            request.JsonSerializer = JsonSerialPublisher.Default;

            if (body == null) {
                body = new Dictionary<string, string>();
            }
            
            if (method == Method.POST || method == Method.PATCH) {
                request.AddJsonBody(body); 
            }

            if (queryParams == null) {
                queryParams = new Dictionary<string, object?>();
            }

            foreach (var entry in queryParams)
            {
                if (entry.Value == null) {
                    continue;
                }
                request.AddQueryParameter(entry.Key, entry.Value.ToString());
            }

            var headers = CreateSigningHeaders();

            foreach (var entry in headers) {
                request.AddHeader(entry.Key, entry.Value);
            }

            return request;
        }
        

        private Dictionary<string, string> CreateSigningHeaders()
        {
            var headers = new Dictionary<string, string>()
            {
                {"Authorization", $"Bearer {LasCredentials.GetAccessToken()}"},
                {"X-Api-Key", LasCredentials.ApiKey}
            };
            headers.Add("Content-Type", "application/json");
            
            return headers;
        }

        private object ExecuteRequestResilient(RestClient client, RestRequest request)
        {
            var clogged = Policy
                .Handle<TooManyRequestsException>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(0.5),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(4)
                });
            var bad = Policy
                .Handle<RequestException>(e => !FatalCode(e.Response.StatusCode))
                .Retry();

            var policy = Policy.Wrap(clogged, bad);
            var result = policy.Execute(() => ExecuteRequest(client, request));
            return result;
        }
        
        private object ExecuteRequest(RestClient client, RestRequest request)
        {
            IRestResponse response = client.Execute(request);
            return JsonDecode(response);
        }

        private static bool FatalCode(HttpStatusCode code)
        {
            return 400 <= (int) code && (int) code < 500;
        }
        
        private object JsonDecode(IRestResponse response)
        {
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new InvalidCredentialsException("Credentials provided is not valid.");
            }
            if ( (int)response.StatusCode == 429 && response.Content.Contains("Too Many Requests"))
            {
                throw new TooManyRequestsException("You have reached the limit of requests per second.");
            }
            if ( (int)response.StatusCode == 429 && response.Content.Contains("Limit Exceeded"))
            {
                throw new LimitExceededException("You have reached the limit of total requests per month.");
            }
            if (response.ResponseStatus == ResponseStatus.Error || response.StatusCode != HttpStatusCode.OK)
            {
                throw new RequestException(response);
            }
            try
            {
                var jsonResponse = JsonSerialPublisher.DeserializeObject(response.Content);
                return jsonResponse;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in response. Returned {e}");
                throw new Exception(response.ToString());
            }
        }
    } 
}