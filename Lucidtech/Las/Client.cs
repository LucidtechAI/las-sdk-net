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
    /// Client to invoke api methods from Lucidtech AI Services.
    /// </summary>
    public class Client : RestClient
    {
        public Credentials LasCredentials { get; }

        /// <summary>
        /// Client constructor.
        /// </summary>
        /// <param name="credentials"> Keys, endpoints and credentials needed for authorization </param>
        public Client(Credentials credentials)
        : base(new Uri(credentials.ApiEndpoint).GetLeftPart(UriPartial.Authority))
        {
            LasCredentials = credentials;
        }

        /// <summary>
        /// Client constructor with credentials read from local file.
        /// </summary>
        public Client() : this(new Credentials()) {}

        /// <summary>Creates an appClient, calls the POST /appClients endpoint.</summary>
        /// <param name="generateSecret">Set to false to ceate a Public app client, default: true</param>
        /// <param name="logoutUrls">List of logout urls</param>
        /// <param name="callbackUrls">List of callback urls</param>
        /// <param name="loginUrls">List of login urls</param>
        /// <param name="defaultLoginUrl">default login url</param>
        /// <param name="attributes">Additional attributes</param>
        /// <returns>AppClient response from REST API</returns>
        public object CreateAppClient(
            bool generateSecret = true,
            List<string>? logoutUrls = null,
            List<string>? loginUrls = null,
            List<string>? callbackUrls = null,
            string? defaultLoginUrl = null,
            Dictionary<string, string?>? attributes = null
        ) {
            var body = new Dictionary<string, object> {
                {"generateSecret", generateSecret}
            };

            if (loginUrls != null) {
                body.Add("loginUrls", loginUrls);
            }

            if (logoutUrls != null) {
                body.Add("logoutUrls", logoutUrls);
            }

            if (callbackUrls != null) {
                body.Add("callbackUrls", callbackUrls);
            }

            if (defaultLoginUrl != null) {
                body.Add("defaultLoginUrl", defaultLoginUrl);
            }

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            RestRequest request = ClientRestRequest(Method.POST, "/appClients", body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary> List available appClients, calls the GET /appClients endpoint. </summary>
        /// <example>
        /// <code>
        /// var response = client.ListAppClients();
        /// </code>
        /// </example>
        /// <param name="maxResults">Number of items to show on a single page</param>
        /// <param name="nextToken">Token to retrieve the next page</param>
        /// <returns>
        /// JSON object with two keys:
        /// - "appClients" AppClients response from REST API without the content of each appClient
        /// - "nextToken" allowing for retrieving the next portion of data
        /// </returns>
        public object ListAppClients(int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object?>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/appClients", null, queryParams);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Updates an existing appClient, calls the PATCH /appClients/{appClientId} endpoint.</summary>
        /// <param name="appClientId">Id of the appClient</param>
        /// <param name="attributes">Additional attributes</param>
        /// <returns>AppClient response from REST API</returns>
        public object UpdateAppClient(
            string appClientId,
            Dictionary<string, string?>? attributes
        ) {
            var body = new Dictionary<string, object?>();

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            RestRequest request = ClientRestRequest(Method.PATCH, $"/appClients/{appClientId}", body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Delete an appClient, calls the DELETE /appClients/{appClientId} endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.DeleteAppClient("&lt;appClientId&gt;");
        /// </code>
        /// </example>
        /// <param name="appClientId">Id of the appClient</param>
        /// <returns>AppClient response from REST API</returns>
        public object DeleteAppClient(string appClientId) {
            var request = ClientRestRequest(Method.DELETE, $"/appClients/{appClientId}");
            return ExecuteRequestResilient(this, request);
        }


        /// <summary>Creates an asset, calls the POST /assets endpoint.</summary>
        /// <example>
        /// <code>
        /// byte[] content = File.ReadAllBytes("myScript.js");
        /// client.CreateAsset(content);
        /// </code>
        /// </example>
        /// <param name="content">Asset content</param>
        /// <param name="attributes">Additional attributes</param>
        /// <returns>Asset response from REST API</returns>
        public object CreateAsset(byte[] content, Dictionary<string, string?>? attributes) {
            string base64Content = System.Convert.ToBase64String(content);
            var body = new Dictionary<string, string?>{
                {"content", base64Content}
            };

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            RestRequest request = ClientRestRequest(Method.POST, "/assets", body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary> List available assets, calls the GET /assets endpoint. </summary>
        /// <example>
        /// <code>
        /// var response = client.ListAssets();
        /// </code>
        /// </example>
        /// <param name="maxResults">Number of items to show on a single page</param>
        /// <param name="nextToken">Token to retrieve the next page</param>
        /// <returns>
        /// JSON object with two keys:
        /// - "assets" Assets response from REST API without the content of each asset
        /// - "nextToken" allowing for retrieving the next portion of data
        /// </returns>
        public object ListAssets(int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object?>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/assets", null, queryParams);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Get asset from the REST API, calls the GET /assets/{assetId} endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.GetAsset("&lt;asset_id&gt;");
        /// </code>
        /// </example>
        /// <param name="assetId">Asset ID</param>
        /// <returns>Asset object</returns>
        public object GetAsset(string assetId) {
            RestRequest request = ClientRestRequest(Method.GET, $"/assets/{assetId}");
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Updates an asset, calls the PATCH /assets/{assetId} endpoint.</summary>
        /// <example>
        /// <code>
        /// byte[] newContent = File.ReadAllBytes("MyScript.js");
        /// var response = client.UpdateAsset("&lt;asset_id&gt;", newContent);
        /// </code>
        /// </example>
        /// <param name="assetId">Asset ID</param>
        /// <param name="content">New content</param>
        /// <param name="attributes">Additional attributes</param>
        /// <returns>Asset object</returns>
        public object UpdateAsset(string assetId, byte[]? content = null, Dictionary<string, string?>? attributes = null) {
            var body = new Dictionary<string, string?>();

            if (content != null) {
                string base64Content = System.Convert.ToBase64String(content);
                body.Add("content", base64Content);
            }

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            RestRequest request = ClientRestRequest(Method.PATCH, $"/assets/{assetId}", body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Delete an asset, calls the DELETE /assets/{assetId} endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.DeleteAsset("&lt;assetId&gt;");
        /// </code>
        /// </example>
        /// <param name="assetId">Id of the asset</param>
        /// <returns>Asset response from REST API</returns>
        public object DeleteAsset(string assetId) {
            var request = ClientRestRequest(Method.DELETE, $"/assets/{assetId}");
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>
        /// Creates a document handle, calls the POST /documents endpoint
        /// </summary>
        /// <param name="content"> Content to POST </param>
        /// <param name="consentId"> An identifier to mark the owner of the document handle </param>
        /// <param name="datasetId"> Specifies the dataset to which the document will be associated with </param>
        /// <param name="groundTruth"> A list of items {label: value},
        /// representing the ground truth values for the document </param>
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields
        /// with documentId, contentType and consentId
        /// </returns>
        public object CreateDocument(
            byte[] content,
            string? consentId = null,
            List<Dictionary<string, string>>? groundTruth = null,
            string? datasetId = null
        ) {
            var body = new Dictionary<string, object>();

            if (consentId != null) {
                body.Add("consentId", consentId);
            }

            if (datasetId != null) {
                body.Add("datasetId", datasetId);
            }

            if (groundTruth != null) {
                body.Add("groundTruth", groundTruth);
            }
            
            string bodyString = JsonConvert.SerializeObject(body);
            object bodyObject = JsonConvert.DeserializeObject(bodyString);
            
            RestRequest createDocument = ClientRestRequest(Method.POST, "/documents", body);
            dynamic response = ExecuteRequestResilient(this, createDocument);
            string fileUrl = response.fileUrl;

            var putRequest = PutFileRequest(fileUrl, content);

            var fileServerClient = new RestClient(new Uri(fileUrl));
            fileServerClient.Proxy = this.Proxy;
            ExecuteRequestResilient(fileServerClient, putRequest);
            
            return response;
        }

        /// <summary>
        /// Creates a document handle, calls the POST /documents endpoint
        /// </summary>
        /// <param name="content"> Content to POST </param>
        /// <param name="contentType"> A mime type for the document handle </param>
        /// <param name="consentId"> An identifier to mark the owner of the document handle </param>
        /// <param name="datasetId"> Specifies the dataset to which the document will be associated with </param>
        /// <param name="groundTruth"> A list of items {label: value},
        /// representing the ground truth values for the document </param>
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields
        /// with documentId, contentType and consentId
        /// </returns>
        public object CreateDocument(
            byte[] content,
            string contentType,
            string? consentId = null,
            List<Dictionary<string, string>>? groundTruth = null,
            string? datasetId = null
        ) => CreateDocument(content, consentId, groundTruth, datasetId);

        /// <summary>
        /// Get documents from the REST API, calls the GET /documents endpoint.
        /// </summary>
        /// <example>
        /// Create a document handle for a jpeg image
        /// <code>
        /// var response = client.ListDocuments('&lt;datasetId&gt;');
        /// </code>
        /// </example>
        /// <param name="consentId"> An identifier to mark the owner of the document handle </param>
        /// <param name="datasetId"> The dataset id that contains the documents of interest </param>
        /// <param name="maxResults">Number of items to show on a single page</param>
        /// <param name="nextToken">Token to retrieve the next page</param>
        /// <returns> Documents from REST API </returns>
        public object ListDocuments(
            string? consentId = null,
            int? maxResults = null,
            string? nextToken = null,
            string? datasetId = null
        ) {
            var queryParams = new Dictionary<string, object?>();

            if (consentId != null) {
                queryParams.Add("consentId", consentId);
            }

            if (datasetId != null) {
                queryParams.Add("datasetId", datasetId);
            }

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/documents", null, queryParams);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>
        /// Get document from the REST API, calls the GET /documents/{documentId} endpoint.
        /// </summary>
        /// <example>
        /// Get information of document specified by documentId
        /// <code>
        /// var response = client.GetDocument('&lt;documentId&gt;');
        /// </code>
        /// </example>
        /// <param name="documentId"> The document id to run inference and create a prediction on </param>
        /// <returns> Document information from REST API </returns>
        public object GetDocument(string documentId) {
            RestRequest request = ClientRestRequest(Method.GET, $"/documents/{documentId}");
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>
        /// Update ground truth of the document, calls the POST /documents/{documentId} endpoint.
        /// This enables the API to learn from past mistakes.
        /// </summary>
        /// <param name="documentId"> Path to document to upload,
        /// Same as provided to <see cref="CreateDocument"/></param>
        /// <param name="groundTruth"> A list of ground truth items </param>
        /// <param name="datasetId"> change or add the documents datasetId </param>
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields
        /// documentId, consentId, uploadUrl, contentType and ground truth.
        /// </returns>
        ///
        public object UpdateDocument(
            string documentId,
            List<Dictionary<string, string>>? groundTruth = null,
            string? datasetId = null
        ) {
            var body = new Dictionary<string, object>();

            if (groundTruth != null) {
                string groundTruthString = JsonConvert.SerializeObject(groundTruth);
                body.Add("groundTruth", JsonConvert.DeserializeObject(groundTruthString));
            }

            if (datasetId != null) {
                body.Add("datasetId", datasetId);
            }

            RestRequest request = ClientRestRequest(Method.PATCH, $"/documents/{documentId}", body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>
        /// Delete documents with specified consentId, calls DELETE /documents endpoint.
        /// </summary>
        /// <example><code>
        /// var response = client.DeleteConsent('&lt;consentId&gt;');
        /// </code></example>
        /// <param name="consentId"> Delete documents with provided consentId </param>
        /// <param name="datasetId"> Delete documents with provided datasetId </param>
        /// <param name="maxResults">Maximum number of items to delete</param>
        /// <param name="nextToken">Token to retrieve the next page</param>
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields
        /// consentId, nextToken and documents
        /// </returns>
        public object DeleteDocuments(
            string? consentId = null,
            int? maxResults = null,
            string? nextToken = null,
            string? datasetId = null,
            bool deleteAll = false
        ) {
            if (maxResults != null && deleteAll) {
                throw new ArgumentException("Cannot specify maxResults when deleteAll=True");
            }

            var queryParams = new Dictionary<string, object?>();

            if (consentId != null) {
                queryParams.Add("consentId", consentId);
            }

            if (datasetId != null) {
                queryParams.Add("datasetId", datasetId);
            }

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.DELETE, "/documents", null, queryParams);
            var objectResponse = ExecuteRequestResilient(this, request);
            var response = JsonSerialPublisher.ObjectToDict<Dictionary<string, object>>(objectResponse);

            if (deleteAll) {
                var documentsDeleted = JsonSerialPublisher.ObjectToDict<List<object>>(response["documents"]);

                while (response["nextToken"] != null) {
                    queryParams["nextToken"] = response["nextToken"].ToString();
                    request = ClientRestRequest(Method.DELETE, "/documents", null, queryParams);
                    var intermediateObjectResponse = ExecuteRequestResilient(this, request);
                    response = JsonSerialPublisher.ObjectToDict<Dictionary<string, object>>(intermediateObjectResponse);
                    documentsDeleted.AddRange(JsonSerialPublisher.ObjectToDict<List<object>>(response["documents"]));
                    Console.WriteLine($"Deleted {documentsDeleted.Count} documents so far");
                }

                response["documents"] = documentsDeleted;
            }

            string responseString = JsonConvert.SerializeObject(response);
            return JsonConvert.DeserializeObject(responseString);
        }

        /// <summary>Delete a document, calls the DELETE /documents/{documentId} endpoint.</summary>
        /// <param name="documentId">Id of the document</param>
        /// <returns>Document response from REST API</returns>
        public object DeleteDocument(string documentId) {
            var request = ClientRestRequest(Method.DELETE, $"/documents/{documentId}");
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>
        /// Create a dataset handle, calls the POST /datasets endpoint.
        /// </summary>
        /// <example>
        /// Create a new dataset with the provided description.
        /// on the document specified by datasetId
        /// <code>
        /// var response = client.CreateDataset("Data gathered from the Mars Rover Invoice Scan Mission");
        /// </code>
        /// </example>
        /// <param name="name">Name of the dataset</param>
        /// <param name="description"> A brief description of the dataset </param>
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields datasetId and description.
        /// datasetId can be used as an input when posting documents to make them a part of this dataset.
        /// </returns>
        public object CreateDataset(string? name = null, string? description = null)
        {
            var body = new Dictionary<string, string?>();

            if (name != null) {
                body.Add("name", name);
            }

            if (description != null) {
                body.Add("description", description);
            }

            RestRequest request = ClientRestRequest(Method.POST, "/datasets", body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>List datasets available, calls the GET /datasets endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.ListDatasets();
        /// </code>
        /// </example>
        /// <param name="maxResults">Number of items to show on a single page</param>
        /// <param name="nextToken">Token to retrieve the next page</param>
        /// <returns>
        /// JSON object with two keys:
        /// - "datasets" which contains a list of Dataset objects
        /// - "nextToken" allowing for retrieving the next portion of data
        /// </returns>
        public object ListDatasets(int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object?>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/datasets", null, queryParams);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Updates an existing dataset, calls the PATCH /datasets/{datasetId} endpoint.</summary>
        /// <param name="datasetId">Id of the dataset</param>
        /// <param name="attributes">Additional attributes</param>
        /// <returns>Dataset response from REST API</returns>
        public object UpdateDataset(
            string datasetId,
            Dictionary<string, string?>? attributes
        ) {
            var body = new Dictionary<string, object?>();

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            RestRequest request = ClientRestRequest(Method.PATCH, $"/datasets/{datasetId}", body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Delete a dataset, calls the DELETE /datasets/{datasetId} endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.DeleteDataset("&lt;datasetId&gt;");
        /// </code>
        /// </example>
        /// <param name="datasetId">Id of the dataset</param>
        /// <param name="deleteDocuments">Set to true to delete documents in dataset before deleting dataset</param>
        /// <returns>Dataset response from REST API</returns>
        public object DeleteDataset(string datasetId, bool deleteDocuments = false) {

            if (deleteDocuments) {
                this.DeleteDocuments(datasetId: datasetId, deleteAll: true);
            }

            var request = ClientRestRequest(Method.DELETE, $"/datasets/{datasetId}");
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>
        /// Run inference and create a prediction, calls the POST /predictions endpoint.
        /// </summary>
        /// <example>
        /// Run inference and create a prediction using the invoice model
        /// on the document specified by documentId
        /// <code>
        /// var response = client.CreatePrediction('&lt;documentId&gt;',"las:model:99cac468f7cf47ddad12e5e017540389");
        /// </code>
        /// </example>
        /// <param name="documentId"> Path to document to
        /// upload Same as provided to <see cref="CreateDocument"/></param>
        /// <param name="modelId"> Id of the model to use for inference </param>
        /// <param name="maxPages"> Maximum number of pages to run predictions on </param>
        /// <param name="autoRotate"> Whether or not to let the API try different
        /// rotations on the document when running </param>
        /// <param name="imageQuality">Image quality used for conversion. Must be either LOW (default) or HIGH.</param>
        /// <param name="postprocessConfig">Postprocessing-related options.</param>
        /// <returns>
        /// A deserialized object that can be interpreted as a Dictionary with the fields documentId and predictions,
        /// the value of predictions is the output from the model.
        /// </returns>
        public object CreatePrediction(
            string documentId,
            string modelId,
            int? maxPages = null,
            bool? autoRotate = null,
            string? imageQuality = null,
            Dictionary<string, object>? postprocessConfig = null
        )
        {
            var body = new Dictionary<string, object> { {"documentId", documentId}, {"modelId", modelId}};

            if (maxPages != null) { body.Add("maxPages", maxPages);}
            if (autoRotate != null) { body.Add("autoRotate", autoRotate);}
            if (imageQuality != null) { body.Add("imageQuality", imageQuality);}
            if (postprocessConfig != null) { body.Add("postprocessConfig", postprocessConfig);}

            RestRequest request = ClientRestRequest(Method.POST, "/predictions", body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>List predictions available, calls the GET /predictions endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.ListPredictions();
        /// </code>
        /// </example>
        /// <param name="maxResults">Number of items to show on a single page</param>
        /// <param name="nextToken">Token to retrieve the next page</param>
        /// <returns>
        /// JSON object with two keys:
        /// - "predictions" which contains a list of Prediction objects
        /// - "nextToken" allowing for retrieving the next portion of data
        /// </returns>
        public object ListPredictions(int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object?>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/predictions", null, queryParams);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>List logs, calls the GET /logs endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.ListLogs();
        /// </code>
        /// </example>
        /// <param name="transitionId">Only show logs from this transition</param>
        /// <param name="transitionExecutionId">Only show logs from this transition execution</param>
        /// <param name="workflowId">Only show logs from this workflow</param>
        /// <param name="workflowExecutionId">Only show logs from this workflow execution</param>
        /// <param name="maxResults">Number of items to show on a single page</param>
        /// <param name="nextToken">Token to retrieve the next page</param>
        /// <returns>Logs response from REST API</returns>
        public object ListLogs(
            string? transitionId = null,
            string? transitionExecutionId = null,
            string? workflowId = null,
            string? workflowExecutionId = null,
            int? maxResults = null,
            string? nextToken = null
        ) {
            var queryParams = new Dictionary<string, object?>();

            if (transitionId != null) {
                queryParams.Add("transitionId", transitionId);
            }

            if (transitionExecutionId != null) {
                queryParams.Add("transitionExecutionId", transitionExecutionId);
            }

            if (workflowId != null) {
                queryParams.Add("workflowId", workflowId);
            }

            if (workflowExecutionId != null) {
                queryParams.Add("workflowExecutionId", workflowExecutionId);
            }

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/logs", null, queryParams);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Creates a model, calls the POST /models endpoint.</summary>
        /// <param name="fieldConfig">Specification of the fields that the model is going to predict</param>
        /// <param name="preprocessConfig">Specification of the processing steps prior to the prediction of an image</param>
        /// <param name="name">Name of the model</param>
        /// <param name="description">Description of the model</param>
        /// <param name="attributes">Additional attributes</param>
        /// <returns>Model response from REST API</returns>
        public object CreateModel(
            Dictionary<string, object> fieldConfig,
            Dictionary<string, object>? preprocessConfig = null,
            string? name = null,
            string? description = null,
            Dictionary<string, string?>? attributes = null
        ) {
            var body = new Dictionary<string, object?> {
                {"fieldConfig", fieldConfig}
            };

            if (preprocessConfig != null) {
                body.Add("preprocessConfig", preprocessConfig);
            }

            if (name != null) {
                body.Add("name", name);
            }

            if (description != null) {
                body.Add("description", description);
            }

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            RestRequest request = ClientRestRequest(Method.POST, "/models", body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>List models available, calls the GET /models endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.ListModels();
        /// </code>
        /// </example>
        /// <param name="maxResults">Number of items to show on a single page</param>
        /// <param name="nextToken">Token to retrieve the next page</param>
        /// <returns>
        /// JSON object with two keys:
        /// - "models" which contains a list of Prediction objects
        /// - "nextToken" allowing for retrieving the next portion of data
        /// </returns>
        public object ListModels(int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object?>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/models", null, queryParams);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Get information about a specific model, calls the GET /models/{modelId} endpoint.</summary>
        /// <param name="modelId">Id of the model</param>
        /// <returns>Model response from REST API</returns>
        public object GetModel(string modelId) {
            var request = ClientRestRequest(Method.GET, $"/models/{modelId}");
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Updates a model, calls the PATCH /models/{modelId} endpoint.</summary>
        /// <param name="modelId">Id of the model</param>
        /// <param name="fieldConfig">Specification of the fields that the model is going to predict</param>
        /// <param name="preprocessConfig">Specification of the processing steps prior to the prediction of an image</param>
        /// <param name="name">Name of the model</param>
        /// <param name="description">Description of the model</param>
        /// <param name="status">New status for the model</param>
        /// <param name="attributes">Additional attributes</param>
        /// <returns>Model response from REST API</returns>
        public object UpdateModel(
            string modelId,
            Dictionary<string, object>? fieldConfig = null,
            Dictionary<string, object>? preprocessConfig = null,
            string? name = null,
            string? description = null,
            string? status = null,
            Dictionary<string, string?>? attributes = null
        ) {
            var body = new Dictionary<string, object?>();

            if (fieldConfig != null) {
                body.Add("fieldConfig", fieldConfig);
            }

            if (preprocessConfig != null) {
                body.Add("preprocessConfig", preprocessConfig);
            }

            if (name != null) {
                body.Add("name", name);
            }

            if (description != null) {
                body.Add("description", description);
            }

            if (status != null) {
                body.Add("status", status);
            }

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            RestRequest request = ClientRestRequest(Method.PATCH, $"/models/{modelId}", body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>
        /// Create a data bundle handle, calls the POST /models/{modelId}/dataBundles endpoint.
        /// </summary>
        /// <param name="modelId">Id of the model </param>
        /// <param name="datasetIds">List of Dataset Ids that will be included in the data bundle</param>
        /// <param name="name">Name of the data bundle</param>
        /// <param name="description">A brief description of the data bundle </param>
        /// <returns>Data Bundle response from REST API</returns>
        public object CreateDataBundle(
            string modelId,
            List<string> datasetIds,
            string? name = null,
            string? description = null
        ) {
            var body = new Dictionary<string, object> {
                {"datasetIds", datasetIds},
            };

            if (name != null) {
                body.Add("name", name);
            }

            if (description != null) {
                body.Add("description", description);
            }

            RestRequest request = ClientRestRequest(Method.POST, $"/models/{modelId}/dataBundles", body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>List data bundles available, calls the GET /models/{modelId}/dataBundles endpoint.</summary>
        /// <param name="modelId">Id of the model</param>
        /// <param name="maxResults">Number of items to show on a single page</param>
        /// <param name="nextToken">Token to retrieve the next page</param>
        /// <returns>
        /// JSON object with two keys:
        /// - "dataBundles" which contains a list of data bundle objects
        /// - "nextToken" allowing for retrieving the next portion of data
        /// </returns>
        public object ListDataBundles(string modelId, int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object?>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, $"/models/{modelId}/dataBundles", null, queryParams);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Updates an existing data bundle, calls the PATCH /models/{modelId}/dataBundles/{dataBundleId} endpoint.</summary>
        /// <param name="modelId">Id of the model</param>
        /// <param name="dataBundleId">Id of the data bundle</param>
        /// <param name="attributes">Additional attributes</param>
        /// <returns>Data Bundle response from REST API</returns>
        public object UpdateDataBundle(
            string modelId,
            string dataBundleId,
            Dictionary<string, string?>? attributes
        ) {
            var body = new Dictionary<string, object?>();

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }
            string url = $"/models/{modelId}/dataBundles/{dataBundleId}";
            RestRequest request = ClientRestRequest(Method.PATCH, url, body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>
        /// Delete a data bundle, calls the DELETE /models/{modelId}/dataBundles/{dataBundleId} endpoint.
        /// </summary>
        /// <param name="modelId">Id of the model</param>
        /// <param name="dataBundleId">Id of the data bundle</param>
        /// <returns>Data Bundle response from REST API</returns>
        public object DeleteDataBundle(string modelId, string dataBundleId) {
            var request = ClientRestRequest(Method.DELETE, $"/models/{modelId}/dataBundles/{dataBundleId}");
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Creates an secret, calls the POST /secrets endpoint.</summary>
        /// <example>
        /// <code>
        /// var data = new Dictionary&lt;string, string&gt;{
        ///     {"key", "my_secret_value"}
        /// }
        /// var response = client.CreateSecret(data);
        /// </code>
        /// </example>
        /// <param name="data">A dictionary containing values to be hidden</param>
        /// <param name="attributes">Additional attributes</param>
        /// <returns>A Secret object</returns>
        public object CreateSecret(Dictionary<string, string> data, Dictionary<string, string?>? attributes = null) {
            var body = new Dictionary<string, object?>() {
                {"data", data}
            };

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            RestRequest request = ClientRestRequest(Method.POST, "/secrets", body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>List secrets available, calls the GET /secrets endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.ListSecrets();
        /// </code>
        /// </example>
        /// <param name="maxResults">Number of items to show on a single page</param>
        /// <param name="nextToken">Token to retrieve the next page</param>
        /// <returns>
        /// JSON object with two keys:
        /// - "secrets" which contains a list of Prediction objects
        /// - "nextToken" allowing for retrieving the next portion of data
        /// </returns>
        public object ListSecrets(int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object?>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/secrets", null, queryParams);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Updates a secret, calls the PATCH /secrets/secretId endpoint.</summary>
        /// <example>
        /// <code>
        /// var data = new Dictionary&lt;string, string&gt;{
        ///     {"key", "my_new_secret_value"}
        /// }
        /// var response = client.UpdateSecret("&lt;secretId&gt;", data);
        /// </code>
        /// </example>
        /// <param name="secretId">Secret ID</param>
        /// <param name="data">New data</param>
        /// <param name="attributes">Additional attributes</param>
        public object UpdateSecret(
            string secretId,
            Dictionary<string, string>? data,
            Dictionary<string, string?>? attributes = null
        ) {
            var body = new Dictionary<string, object?>();

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            if (data != null) {
                body.Add("data", data);
            }

            RestRequest request = ClientRestRequest(Method.PATCH, $"/secrets/{secretId}", body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Delete a secret, calls the DELETE /secrets/{secretId} endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.DeleteSecret("&lt;secretId&gt;");
        /// </code>
        /// </example>
        /// <param name="secretId">Id of the secret</param>
        /// <returns>Secret response from REST API</returns>
        public object DeleteSecret(string secretId) {
            var request = ClientRestRequest(Method.DELETE, $"/secrets/{secretId}");
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Creates a transition, calls the POST /transitions endpoint.</summary>
        /// <example>
        /// <code>
        /// var inputSchema = new Dictionary&lt;string, string&gt;{
        ///     {"$schema", "https://json-schema.org/draft-04/schema#"},
        ///     {"title", "input"}
        /// };
        /// var outputSchema = new Dictionary&lt;string, string&gt;{
        ///     {"$schema", "https://json-schema/draft-04/schema#"},
        ///     {"title", "output"}
        /// };
        /// var params = new Dictionary&lt;string, object&gt;{
        ///     {"imageUrl", "&lt;image_url&gt;"},
        ///     {"credentials", new Dictionary&lt;string, string&gt;{
        ///         {"username", "&lt;username&gt;"},
        ///         {"password", "&lt;password&gt;"}
        ///     }
        /// };
        /// var response = client.CreateTransition("&lt;transition_type&gt;", inputSchema, outputSchema, parameters: params);
        /// </code>
        /// </example>
        /// <param name="transitionType">Type of transition: "docker"|"manual"</param>
        /// <param name="inputJsonSchema">Json-schema that defines the input to the transition</param>
        /// <param name="outputJsonSchema">Json-schema that defines the output of the transition</param>
        /// <param name="parameters">Parameters to the corresponding transition type</param>
        /// <param name="attributes">Additional attributes</param>
        /// <returns>Transition response from REST API</returns>
        public object CreateTransition(
            string transitionType,
            Dictionary<string, string>? inputJsonSchema = null,
            Dictionary<string, string>? outputJsonSchema = null,
            Dictionary<string, object?>? parameters = null,
            Dictionary<string, string?>? attributes = null
        ) {
            var body = new Dictionary<string, object?> {
                {"transitionType", transitionType},
            };

            if (inputJsonSchema != null) {
                body.Add("inputJsonSchema", inputJsonSchema);
            }

            if (outputJsonSchema != null) {
                body.Add("outputJsonSchema", outputJsonSchema);
            }

            if (parameters != null) {
                body.Add("parameters", parameters);
            }

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            RestRequest request = ClientRestRequest(Method.POST, "/transitions", body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>List transitions, calls the GET /transitions endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.ListTransitions();
        /// </code>
        /// </example>
        /// <param name="transitionType">Type of transitions</param>
        /// <param name="maxResults">Number of items to show on a single page</param>
        /// <param name="nextToken">Token to retrieve the next page</param>
        /// <returns>Transitions response from REST API</returns>
        public object ListTransitions(string? transitionType = null, int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object?>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            RestRequest request = ClientRestRequest(Method.GET, "/transitions", null, queryParams);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Get information about a specific transition,
        /// calls the GET /transitions/{transition_id} endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.GetTransition("&lt;transition_id&gt;");
        /// </code>
        /// </example>
        /// <param name="transitionId">Id of the transition</param>
        /// <returns>Transition response from REST API</returns>
        public object GetTransition(string transitionId) {
            var request = ClientRestRequest(Method.GET, $"/transitions/{transitionId}");
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Delete a transition, calls the DELETE /transitions/{transition_id} endpoint.
        /// Will fail if transition is in use by one or more workflows.</summary>
        /// <example>
        /// <code>
        /// var response = client.DeleteTransition("&lt;transition_id&gt;");
        /// </code>
        /// </example>
        /// <param name="transitionId">Id of the transition</param>
        /// <returns>Transition response from REST API</returns>
        public object DeleteTransition(string transitionId) {
            var request = ClientRestRequest(Method.DELETE, $"/transitions/{transitionId}");
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Get an execution of a transition, calls the GET /transitions/{transitionId}/executions/{executionId} endpoint</summary>
        /// <example>
        /// <code>
        /// var response = client.GetTransitionExecution("&lt;transition_id&gt;", "&lt;execution_id&gt;");
        /// </code>
        /// </example>
        /// <param name="transitionId">Id of the transition</param>
        /// <param name="executionId">Id of the execution</param>
        /// <returns>Transition execution response from REST API</returns>
        public object GetTransitionExecution(string transitionId, string executionId) {
            RestRequest request = ClientRestRequest(Method.GET, $"/transitions/{transitionId}/executions/{executionId}");
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Updates an existing transition, calls the PATCH /transitions/{transitionId} endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.UpdateTransition("&lt;transitionId&gt;");
        /// </code>
        /// </example>
        /// <param name="transitionId">Id of the transition</param>
        /// <param name="inputJsonSchema">Json-schema that defines the input to the transition</param>
        /// <param name="outputJsonSchema">Json-schema that defines the output of the transition</param>
        /// <param name="attributes">Additional attributes</param>
        /// <returns>Transition response from REST API</returns>
        public object UpdateTransition(
            string transitionId,
            Dictionary<string, string>? inputJsonSchema,
            Dictionary<string, string>? outputJsonSchema,
            Dictionary<string, string>? assets,
            Dictionary<string, string>? environment,
            List<string>? environmentSecrets,
            Dictionary<string, string?> attributes
        ) {
            var body = new Dictionary<string, object?>();

            if (inputJsonSchema != null) {
                body.Add("inputJsonSchema", inputJsonSchema);
            }

            if (outputJsonSchema != null) {
                body.Add("outputJsonSchema", outputJsonSchema);
            }

            if (assets != null) {
                body.Add("assets", assets);
            }

            if (environment != null) {
                body.Add("environment", environment);
            }

            if (environmentSecrets != null) {
                body.Add("environmentSecrets", environmentSecrets);
            }

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            RestRequest request = ClientRestRequest(Method.PATCH, $"/transitions/{transitionId}", body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Start executing a manual transition, calls the POST /transitions/{transitionId}/executions endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.ExecuteTransition("&lt;transitionId&gt;");
        /// </code>
        /// </example>
        /// <param name="transitionId">Id of the transition</param>
        /// <returns>Transition exexution response from REST API</returns>
        public object ExecuteTransition(string transitionId) {
            var request = ClientRestRequest(Method.POST, $"/transitions/{transitionId}/executions");
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>List executions in a transition, calls the GET /transitions/{transitionId}/executions endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.ListTransitionExecutions("&lt;transitionId&gt;", new [] {"succeeded", "failed"});
        /// </code>
        /// </example>
        /// <param name="transitionId">Id of the transition</param>
        /// <param name="status">Status to filter by</param>
        /// <param name="executionIds">List of execution ids to filter by</param>
        /// <param name="maxResults">Maximum number of results to be returned</param>
        /// <param name="nextToken">A unique token used to retrieve the next page</param>
        /// <param name="sortBy">The sorting variable of the execution: "endTime" | "startTime"</param>
        /// <param name="order">Order of the executions: "ascending" | "descending"</param>
        /// <returns>Transition executions response from the REST API</returns>
        public object ListTransitionExecutions(
            string transitionId,
            string? status = null,
            List<string>? executionIds = null,
            int? maxResults = null,
            string? nextToken = null,
            string? sortBy = null,
            string? order = null
        ) {
            List<string>? statuses = null;
            if (status != null) {
                statuses = new List<string> {status};
            }
            return ListTransitionExecutions(transitionId, statuses, executionIds, maxResults, nextToken, sortBy, order);
         }

        /// <summary>List executions in a transition, calls the GET /transitions/{transitionId}/executions endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.ListTransitionExecutions("&lt;transitionId&gt;", new [] {"succeeded", "failed"});
        /// </code>
        /// </example>
        /// <param name="transitionId">Id of the transition</param>
        /// <param name="statuses">List of execution statuses to filter by</param>
        /// <param name="executionIds">List of execution ids to filter by</param>
        /// <param name="maxResults">Maximum number of results to be returned</param>
        /// <param name="nextToken">A unique token used to retrieve the next page</param>
        /// <param name="sortBy">The sorting variable of the execution: "endTime" | "startTime"</param>
        /// <param name="order">Order of the executions: "ascending" | "descending"</param>
        /// <returns>Transition executions response from the REST API</returns>
        public object ListTransitionExecutions(
            string transitionId,
            List<string>? statuses = null,
            List<string>? executionIds = null,
            int? maxResults = null,
            string? nextToken = null,
            string? sortBy = null,
            string? order = null
        ) {
            var queryParams = new Dictionary<string, object?>();

            if (statuses != null) {
                queryParams.Add("status", statuses);
            }

            if (executionIds != null) {
                queryParams.Add("executionId", executionIds);
            }

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

            var request = ClientRestRequest(Method.GET, $"/transitions/{transitionId}/executions", null, queryParams);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Ends the processing of the transition execution,
        /// calls the PATCH /transitions/{transitionId}/executions/{executionId} endpoint.</summary>
        /// <example>
        /// <code>
        /// var output = new Dictionary&lt;string, string&gt;();
        /// client.UpdateTransitionExecution("&lt;transitionId&gt;", "&lt;executionId&gt;, "succeeded", output: output);
        /// </code>
        /// </example>
        /// <param name="transitionId">Id of the transition</param>
        /// <param name="executionId">Id of the execution</param>
        /// <param name="status">Status of the execution: "succeeded" | "failed"</param>
        /// <param name="output">Output from the execution, required when status is "succeeded"</param>
        /// <param name="error">Error from the execution, required when status is "failed"</param>
        /// <param name="startTime"> Utc start time that will replace the original start time of the execution</param>
        /// <returns>Transition execution response from REST API</returns>
        public object UpdateTransitionExecution(
            string transitionId,
            string executionId,
            string status,
            Dictionary<string, string>? output = null,
            Dictionary<string, string>? error = null,
            DateTime? startTime = null
        ) {
            var url = $"/transitions/{transitionId}/executions/{executionId}";
            var body = new Dictionary<string, object> {
                {"status", status},
            };

            if (output != null) {
                body.Add("output", output);
            }

            if (error != null) {
                body.Add("error", error);
            }

            if (startTime != null) {
                body.Add("startTime", startTime.Value.ToString("yyyy-MM-ddTHH:mm:ss.ffffzzz"));
            }

            var request = ClientRestRequest(Method.PATCH, url, body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Send heartbeat for a manual execution,
        /// calls the POST /transitions/{transitionId}/executions/{executionId}/heartbeats endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.sendHeartbeat("&lt;transitionId&gt;", "&lt;executionId&gt;");
        /// </code>
        /// </example>
        /// <param name="transitionId">Id of the transition</param>
        /// <param name="executionId">Id of the execution</param>
        /// <returns>Transition exexution response from REST API</returns>
        public object SendHeartbeat(string transitionId, string executionId) {
            var url = $"/transitions/{transitionId}/executions/{executionId}/heartbeats";
            var request = ClientRestRequest(Method.POST, url);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Creates a new user, calls the POST /users endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.CreateUser("foo@bar.com");
        /// </code>
        /// </example>
        /// <param name="email">New user's email</param>
        /// <param name="attributes">Additional attributes. Currently supported are: name, avatar</param>
        /// <returns>User response from REST API</returns>
        public object CreateUser(string email, Dictionary<string, string?>? attributes = null) {
            var body = new Dictionary<string, string> {
                {"email", email}
            };

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            var request = ClientRestRequest(Method.POST, "/users", body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>List users, calls the GET /users endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.ListUsers();
        /// </code>
        /// </example>
        /// <param name="maxResults">Maximum number of results to be returned</param>
        /// <param name="nextToken">A unique token used to retrieve the next page</param>
        /// <returns>Users response from REST API</returns>
        public object ListUsers(int? maxResults = null, string? nextToken = null) {
            var queryParams = new Dictionary<string, object?>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            var request = ClientRestRequest(Method.GET, "/users", null, queryParams);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Get information about a specific user, calls the GET /users/{user_id} endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.GetUser("&lt;user_id&gt;");
        /// </code>
        /// </example>
        /// <param name="userId">Id of the user</param>
        /// <returns>User response from REST API</returns>
        public object GetUser(string userId) {
            var request = ClientRestRequest(Method.GET, $"/users/{userId}");
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Delete the user with the provided user_id, calls the DELETE /users/{userId} endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.DeleteUser("&lt;user_id&gt;");
        /// </code>
        /// </example>
        /// <param name="userId">Id of the user</param>
        /// <returns>User response from REST API</returns>
        public object DeleteUser(string userId) {
            var request = ClientRestRequest(Method.DELETE, $"/users/{userId}");
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Updates a user, calls the PATCH /users/{userId} endpoint.</summary>
        /// <example>
        /// <code>
        /// var parameters = new Dictionary&lt;string, string&gt;{
        ///     {"name", "User"}
        /// };
        /// var response = client.UpdateUser("&lt;user_id&gt;", parameters);
        /// </code>
        /// </example>
        /// <param name="userId">Id of the user</param>
        /// <param name="attributes">
        /// Attributes to update.
        /// Allowed attributes:
        ///     name (string),
        ///     avatar (base64-encoded image)
        /// </param>
        /// <returns>User response from REST API</returns>
        public object UpdateUser(string userId, Dictionary<string, object?> attributes) {
            var body = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object?> entry in attributes) {
                body.Add(entry.Key, entry.Value);
            }

            var request = ClientRestRequest(Method.PATCH, $"/users/{userId}", body: body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>
        /// Creates a new workflow, calls the POST /workflows endpoint.
        /// Check out Lucidtech's tutorials for more info on how to create a workflow.
        /// </summary>
        /// <example>
        /// <code>
        /// var specification = new Dictionary&lt;string, object&gt;{
        ///     {"language", "ASL"},
        ///     {"version", "1.0.0"},
        ///     {"definition", {...}}
        /// };
        /// var environmentSecrets = new List&lt;string&gt;{ "las:secret:&lt;hex-uuid&gt;" };
        /// var env = new Dictionary&lt;string, string&gt;{{"FOO", "BAR"}};
        /// var completedConfig = new Dictionary&lt;string, object&gt;{
        ///     {"imageUrl", "my/docker:image"},
        ///     {"secretId", secretId},
        ///     {"environment", env},
        ///     {"environmentSecrets", environmentSecrets}
        /// };
        /// var errorConfig = new Dictionary&lt;string, object&gt;{
        ///     {"email", "foo@example.com"},
        ///     {"manualRetry", true}
        /// };
        /// var parameters = new Dictionary&lt;string, string?&gt;{
        ///     {"name", name},
        ///     {"description", description}
        /// };
        /// var response = Toby.CreateWorkflow(spec, errorConfig, completedConfig, parameters);
        /// </code>
        /// </example>
        /// <param name="specification">Workflow specification. Currently only ASL is supported: https://states-language.net/spec.html</param>
        /// <param name="errorConfig">Error handler configuration</param>
        /// <param name="completedConfig">Configuration of a job to run whenever a workflow execution ends</param>
        /// <param name="attributes">Additional attributes. Currently supported are: name, description.</param>
        /// <returns>Workflow response from REST API</returns>
        public object CreateWorkflow(
            Dictionary<string, object> specification,
            Dictionary<string, object>? errorConfig = null,
            Dictionary<string, object>? completedConfig = null,
            Dictionary<string, string?>? attributes = null
        ) {
            var body = new Dictionary<string, object?> {
                {"specification", specification}
            };

            if (errorConfig != null) {
                body.Add("errorConfig", errorConfig);
            }

            if (completedConfig != null) {
                body.Add("completedConfig", completedConfig);
            }

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            var request = ClientRestRequest(Method.POST, "/workflows", body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>List workflows, calls the GET /workflows endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.ListWorkflows();
        /// </code>
        /// </example>
        /// <param name="maxResults">Maximum number of results to be returned</param>
        /// <param name="nextToken">A unique token used to retrieve the next page</param>
        /// <returns>Workflows response from REST API</returns>
        public object ListWorkflows(int? maxResults, string nextToken) {
            var queryParams = new Dictionary<string, object?>();

            if (maxResults != null) {
                queryParams.Add("maxResults", maxResults.ToString());
            }

            if (nextToken != null) {
                queryParams.Add("nextToken", nextToken);
            }

            var request = ClientRestRequest(Method.GET, "/workflows", null, queryParams);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Creates a workflow handle, calls the PATCH /workflows/{workflowId} endpoint.</summary>
        /// <example>
        /// <code>
        /// var newParameters = new Dictionary&lt;string, string&gt;{
        ///     {"name", "New Name"},
        ///     {"description", "My updated awesome workflow"}
        /// };
        /// var response = client.UpdateWorkflow("&lt;workflow_id&gt;, newParameters);
        /// </code>
        /// </example>
        /// <param name="workflowId">Id of the workflow</param>
        /// <param name="attributes">Attributes to update. Currently supported are: name, description</param>
        /// <returns>Workflow response from REST API</returns>
        public object UpdateWorkflow(
            string workflowId,
            Dictionary<string, object>? errorConfig,
            Dictionary<string, object>? completedConfig,
            Dictionary<string, string?> attributes
        ) {
            var body = new Dictionary<string, object?> {};

            if (errorConfig != null) {
                body.Add("errorConfig", errorConfig);
            }

            if (completedConfig != null) {
                body.Add("completedConfig", completedConfig);
            }

            if (attributes != null) {
                foreach (KeyValuePair<string, string?> entry in attributes) {
                    body.Add(entry.Key, entry.Value);
                }
            }

            var request = ClientRestRequest(Method.PATCH, $"/workflows/{workflowId}", body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Get information about a specific workflow,
        /// calls the GET /workflows/{workflow_id} endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.GetWorkflow("&lt;workflow_id&gt;");
        /// </code>
        /// </example>
        /// <param name="workflowId">Id of the workflow</param>
        /// <returns>Workflow response from REST API</returns>
        public object GetWorkflow(string workflowId) {
            var request = ClientRestRequest(Method.GET, $"/workflows/{workflowId}");
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Delete the workflow with the provided workflow_id,
        /// calls the DELETE /workflows/{workflowId} endpoint.</summary>
        /// <example>
        /// <code>
        /// var response = client.DeleteWorkflow("&lt;workflow_id&gt;");
        /// </code>
        /// </example>
        /// <param name="workflowId">Id of the workflow</param>
        /// <returns>Workflow response from REST API</returns>
        public object DeleteWorkflow(string workflowId) {
            var request = ClientRestRequest(Method.DELETE, $"/workflows/{workflowId}");
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Start a workflow execution, calls the POST /workflows/{workflowId}/executions endpoint.</summary>
        /// <example>
        /// <code>
        /// var content = new Dictionary&lt;string, object&gt;();
        /// var response = client.ExecuteWorkflow("&lt;workflowId&gt;, content);
        /// </code>
        /// </example>
        /// <param name="workflowId">Id of the workflow</param>
        /// <param name="content">Input to the first step of the workflow</param>
        /// <returns>Workflow execution response from REST API</returns>
        public object ExecuteWorkflow(string workflowId, Dictionary<string, object> content) {
            var body = new Dictionary<string, object> {
                {"input", content}
            };
            var request = ClientRestRequest(Method.POST, $"/workflows/{workflowId}/executions", body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>List executions in a workflow, calls the GET /workflows/{workflowId}/executions endpoint.</summary>
        /// <example>
        /// <code>
        /// var statuses = new [] {"running", "succeeded"};
        /// var response = client.ListWorkflowExecutions("&lt;workflow_id&gt;", statuses);
        /// </code>
        /// </example>
        /// <param name="workflowId">Id of the workflow</param>
        /// <param name="status">Workflow execution status to filter by</param>
        /// <param name="maxResults">Maximum number of results to be returned</param>
        /// <param name="nextToken">A unique token used to retrieve the next page</param>
        /// <param name="sortBy">The sorting variable of the execution: "endTime" | "startTime"</param>
        /// <param name="order">Order of the executions: "ascending" | "descending"</param>
        /// <returns>WorkflowExecutions response from REST API</returns>
        public object ListWorkflowExecutions(
            string workflowId,
            string? status = null,
            int? maxResults = null,
            string? nextToken = null,
            string? sortBy = null,
            string? order = null
        ) => ListWorkflowExecutions(workflowId, new List<string> {status}, maxResults, nextToken, sortBy, order);

        /// <summary>List executions in a workflow, calls the GET /workflows/{workflowId}/executions endpoint.</summary>
        /// <example>
        /// <code>
        /// var statuses = new [] {"running", "succeeded"};
        /// var response = client.ListWorkflowExecutions("&lt;workflow_id&gt;", statuses);
        /// </code>
        /// </example>
        /// <param name="workflowId">Id of the workflow</param>
        /// <param name="statuses">Workflow execution statuses to filter by</param>
        /// <param name="maxResults">Maximum number of results to be returned</param>
        /// <param name="nextToken">A unique token used to retrieve the next page</param>
        /// <param name="sortBy">The sorting variable of the execution: "endTime" | "startTime"</param>
        /// <param name="order">Order of the executions: "ascending" | "descending"</param>
        /// <returns>WorkflowExecutions response from REST API</returns>
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
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>Get an execution of a workflow, calls the GET /workflows/{workflowId}/executions/{executionId} endpoint</summary>
        /// <example>
        /// <code>
        /// var response = client.GetWorkflowExecution("&lt;workflow_id&gt;", "&lt;execution_id&gt;");
        /// </code>
        /// </example>
        /// <param name="workflowId">Id of the workflow</param>
        /// <param name="executionId">Id of the execution</param>
        /// <returns>Workflow execution response from REST API</returns>
        public object GetWorkflowExecution(string workflowId, string executionId) {
            RestRequest request = ClientRestRequest(Method.GET, $"/workflows/{workflowId}/executions/{executionId}");
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>
        /// Change the processing of a workflow execution. Retry, end or mark as completed. 
        /// calls the PATCH /workflows/{workflowId}/executions/{executionId} endpoint.
        /// </summary>
        /// <example>
        /// <code>
        /// var response = client.UpdateWorkflowExecution("&lt;workflow_id&gt;", "&lt;execution_id&gt;", nextTransitionId: "&lt;next_transition_id&gt;");
        /// </code>
        /// </example>
        /// <param name="workflowId">Id of the workflow</param>
        /// <param name="executionId">Id of the execution</param>
        /// <param name="nextTransitionId">The next transition to transition into, to end the workflow-execution,
        /// use: las:transition:commons-failed</param>
        /// <param name="status">Update the execution with this status, can only update from succeeded to completed and vice versa.
        /// <returns>WorkflowExecution response from REST API</returns>
        public object UpdateWorkflowExecution(string workflowId, string executionId, string? nextTransitionId = null, string? status = null) {
            var body = new Dictionary<string, string> {};

            if (nextTransitionId != null) {
                body.Add("nextTransitionId", nextTransitionId);
            }

            if (status != null) {
                body.Add("status", status);
            }

            var request = ClientRestRequest(Method.PATCH, $"/workflows/{workflowId}/executions/{executionId}", body);
            return ExecuteRequestResilient(this, request);
        }

        /// <summary>
        /// Deletes the execution with the provided execution_id from workflow_id,
        /// calls the DELETE /workflows/{workflowId}/executions/{executionId} endpoint.
        /// </summary>
        /// <example>
        /// <code>
        /// var response = client.DeleteWorkflowExecution("&lt;workflow_id&gt;", "&lt;execution_id&gt;");
        /// </code>
        /// </example>
        /// <param name="workflowId">Id of the workflow</param>
        /// <param name="executionId">Id of the execution</param>
        /// <returns>WorkflowExecution response from REST API</returns>
        public object DeleteWorkflowExecution(string workflowId, string executionId) {
            var request = ClientRestRequest(Method.DELETE, $"/workflows/{workflowId}/executions/{executionId}");
            return ExecuteRequestResilient(this, request);
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
            RestRequest request = new RestRequest(endpoint, method, DataFormat.Json);
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

            foreach (var entry in queryParams) {

                if (entry.Value == null) {
                    continue;
                }
                else if (entry.Value is List<string?>) {
                    foreach (var item in entry.Value as List<string>) {
                        request.AddQueryParameter(entry.Key, item);
                    }
                }
                else {
                    request.AddQueryParameter(entry.Key, entry.Value.ToString());
                }
            }

            var headers = CreateSigningHeaders();

            foreach (var entry in headers) {
                request.AddHeader(entry.Key, entry.Value);
            }

            return request;
        }

        private RestRequest PutFileRequest(string path, byte[] content)
        {
            string contentType = "application/octet-stream";
            RestRequest request = new RestRequest(new Uri(path), Method.PUT);
            request.AddParameter(contentType, content, contentType, ParameterType.RequestBody);

            var headers = CreateSigningHeaders();
            foreach (var entry in headers) {
                request.AddHeader(entry.Key, entry.Value);
            }

            return request;
        }

        private Dictionary<string, string> CreateSigningHeaders()
        {
            var headers = new Dictionary<string, string> {
                {"Authorization", $"Bearer {LasCredentials.GetAccessToken()}"}
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
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return new Dictionary<string, string>{  {"Your request executed successfully", "204"} };
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new InvalidCredentialsException("Credentials provided is not valid.");
            }
            else if ( (int)response.StatusCode == 429 && response.Content.Contains("Too Many Requests"))
            {
                throw new TooManyRequestsException("You have reached the limit of requests per second.");
            }
            else if ( (int)response.StatusCode == 429 && response.Content.Contains("Limit Exceeded"))
            {
                throw new LimitExceededException("You have reached the limit of total requests per month.");
            }
            else if (response.ResponseStatus == ResponseStatus.Error || response.StatusCode != HttpStatusCode.OK)
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
