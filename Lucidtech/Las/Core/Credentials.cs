using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using IniParser;
using IniParser.Model;

using RestSharp;
using RestSharp.Authenticators;

using Lucidtech.Las.Utils;

namespace Lucidtech.Las.Core
{
    /// <summary>
    /// Used to fetch and store credentials. One of 3 conditions must be met to successfully create credentials.
    /// 1. ClientId, ClientSecret, AuthEndpoint and ApiEndpoint are provided
    /// 2. The path to the file where the credentials are stored is provided
    /// 3. Credentials are located in default path ~/.lucidtech/credentials.cfg
    ///
    /// Get credentials by contacting hello@lucidtech.ai
    ///
    /// </summary>
    public class Credentials
    {

        /// <summary>
        /// Client ID. Provided by Lucidtech.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// Client Secret. Provided by Lucidtech.
        /// </summary>
        public string ClientSecret { get; }

        /// <summary>
        /// AWS Authorization endpoint. Provided by Lucidtech.
        /// </summary>
        public string AuthEndpoint { get; }

        /// <summary>
        /// AWS API Gateway API endpoint. Provided by Lucidtech.
        /// </summary>
        public string ApiEndpoint{ get; }

        /// <summary>
        /// Access token and timestamp to API endpoint. Provided and updated by calling AccessToken.
        /// </summary>
        private string? AccessToken;

        /// <summary>
        /// Timestamp for access token to API endpoint. Provided and updated by calling AccessToken.
        /// </summary>
        private DateTime ExpirationTime;

        /// <summary>
        /// RestClient for making request to the authorization endpoint.
        /// </summary>
        private RestClient? RestSharpClient { get; set; }

        protected virtual (string, DateTime) GetClientCredentials()
        {
            string url = "oauth2/token?grant_type=client_credentials";
            var headers = new Dictionary<string, string>{ {"Content-Type", "application/x-www-form-urlencoded"} };

            var request = new RestRequest(url, Method.POST, DataFormat.Json);
            request.JsonSerializer = JsonSerialPublisher.Default;
            foreach (var entry in headers) { request.AddHeader(entry.Key, entry.Value); }

            var response = RestSharpClient.Execute(request);
            var responseData = JsonDecode(response);
            var responseDict = JsonSerialPublisher.ObjectToDict<Dictionary<string, object>>(responseData);

            string accessToken = (string)responseDict["access_token"];
            double expiresIn = Convert.ToDouble(responseDict["expires_in"]);
            DateTime exp = DateTime.UtcNow.AddSeconds(expiresIn);

            return (accessToken, exp);
        }

        /// <summary>
        /// Get Access token to API endpoint.
        /// </summary>
        public string GetAccessToken()
        {
            if (string.IsNullOrEmpty(AccessToken) || (DateTime.UtcNow > ExpirationTime))
            {
                (AccessToken, ExpirationTime) = GetClientCredentials();
            }
            return AccessToken;
        }

        private object JsonDecode(IRestResponse response)
        {
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

        /// <summary>
        /// Credentials constructor where ClientId, ClientSecret, AuthEndpoint and ApiEndpoint are provided by
        /// Lucidtech.
        /// </summary>
        /// <param name="clientId"> client id </param>
        /// <param name="clientSecret"> client secret </param>
        /// <param name="authEndpoint"> Authorization endpoint </param>
        /// <param name="apiEndpoint"> API endpoint </param>
        /// <exception cref="ArgumentException"></exception>
        public Credentials(
            string clientId,
            string clientSecret,
            string authEndpoint,
            string apiEndpoint
        ) : this()
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            AuthEndpoint = authEndpoint;
            ApiEndpoint = apiEndpoint;
        }

        /// <summary>
        /// Credentials constructor where the path to the credentials config is provided.
        /// </summary>
        /// <param name="credentialsPath">Path to the file where the credentials are stored </param>
        public Credentials(string? credentialsPath)
        {
            var envCred = GetCredentialsFromEnv();
            
            ClientId = envCred["ClientId"];
            ClientSecret = envCred["ClientSecret"];
            AuthEndpoint = envCred["AuthEndpoint"];
            ApiEndpoint = envCred["ApiEndpoint"];
            
            if (credentialsPath != null && File.Exists(credentialsPath))
            {
                var pathCred = ReadCredentials(credentialsPath);
                ClientId = envCred["ClientId"] != null ? envCred["ClientId"] : pathCred["ClientId"];
                ClientSecret = envCred["ClientSecret"] != null ? envCred["ClientSecret"] : pathCred["ClientSecret"];
                AuthEndpoint = envCred["AuthEndpoint"] != null ? envCred["AuthEndpoint"] : pathCred["AuthEndpoint"];
                ApiEndpoint = envCred["ApiEndpoint"] != null ? envCred["ApiEndpoint"] : pathCred["ApiEndpoint"];
            }
           
            RestSharpClient = new RestClient($"https://{AuthEndpoint}");
            RestSharpClient.Authenticator = new HttpBasicAuthenticator(ClientId, ClientSecret);
            ExpirationTime = DateTime.UtcNow;
            AccessToken = null;
        }

        /// <summary>
        /// Credentials constructor where the credentials are located at the default path.
        /// ~/.lucidtech/credentials.cfg for linux and %USERPROFILE%\.lucidtech\credentials.cfg for Windows.
        /// </summary>
        public Credentials() : this(GetCredentialsPath()) {}

        private Dictionary<string, string> GetCredentialsFromEnv()
        {
            var envVars = new Dictionary<string, string>()
            {
                {"ClientId", Environment.GetEnvironmentVariable("LAS_CLIENT_ID")},
                {"ClientSecret", Environment.GetEnvironmentVariable("LAS_CLIENT_SECRET")},
                {"AuthEndpoint", Environment.GetEnvironmentVariable("LAS_AUTH_ENDPOINT")},
                {"ApiEndpoint", Environment.GetEnvironmentVariable("LAS_API_ENDPOINT")}
            };

            return envVars;
        }


        private static string GetCredentialsPath()
        {
            string path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                "%USERPROFILE%\\.lucidtech\\credentials.cfg" : "%HOME%/.lucidtech/credentials.cfg";
            return Environment.ExpandEnvironmentVariables(path);
        }

        private static Dictionary<string, string> ReadCredentials(string credentialPath)
        {
            var parser = new FileIniDataParser();
            const string section = "default";
            IniData config = parser.ReadFile(credentialPath);
            var ret = new Dictionary<string, string>()
            {
                {"ClientId", config[section]["client_id"]},
                {"ClientSecret", config[section]["client_secret"]},
                {"AuthEndpoint", config[section]["auth_endpoint"]},
                {"ApiEndpoint", config[section]["api_endpoint"]}
            };
            return ret;
        }
    }
}
