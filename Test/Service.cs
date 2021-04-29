using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace Test.Service
{

    public static class Util {
    
        public static string ResourceId(string resourceName){
            return $"las:{resourceName}:{Guid.NewGuid().ToString().Replace("-", "")}";
        }
        
        public static string[] ExpectedKeys(string resourceName){
            switch (resourceName) {
                case "appClient":
                    return new [] {"appClientId", "name", "description"};
                case "appClients": 
                    return new [] {"nextToken", "appClients"};
                case "asset":
                    return new [] {"assetId", "name", "description"};
                case "assets": 
                    return new [] {"nextToken", "assets"};
                case "document":
                    return new [] {"documentId", "contentType", "consentId", "groundTruth"};
                case "documents": 
                    return new [] {"nextToken", "documents"};
                case "prediction":
                    return new [] {"documentId", "predictions"};
                case "predictions":
                    return new [] {"predictions"};
                case "batch":
                    return new[] {"name", "description", "batchId"};
                case "batches":
                    return new[] {"batches", "nextToken"};
                case "models":
                    return new[] {"models", "nextToken"};
                case "secret":
                    return new [] {"secretId", "name", "description"};
                case "secrets": 
                    return new [] {"nextToken", "secrets"};
                case "transition":
                    return new [] {"transitionId", "name", "description", "transitionType"};
                case "transitions": 
                    return new [] {"nextToken", "transitions"};
                case "transition-execution":
                    return new [] {"transitionId", "executionId", "status", "completedBy", "endTime", "input", "logId", "startTime"};
                case "transition-executions":
                    return new [] {"nextToken", "executions"};
                case "heartbeats":
                    return new [] {"Your request executed successfully"};
                case "user":
                    return new [] {"userId", "name", "avatar", "email"};
                case "users": 
                    return new [] {"nextToken", "users"};
                case "workflow":
                    return new [] {"workflowId", "name", "description"};
                case "workflows": 
                    return new [] {"nextToken", "workflows"};
                case "workflow-execution":
                    return new [] {"workflowId", "executionId", "status", "completedBy", "endTime", "logId", "startTime", "transitionExecutions"};
                case "workflow-executions":
                    return new [] {"nextToken", "executions", "workflowType"};
                default:
                    throw new Exception($"{resourceName} is not a valid resource name");
            }
        }
        
        public static Dictionary<string, object> CompletedConfig(){
            var environment = new Dictionary<string, string?>{
                {"FOO", "FOO"},
                {"BAR", "BAR"}
            };
            return new Dictionary<string, object>{
                {"imageUrl", "my/docker:image"},
                {"secretId", Util.ResourceId("secret")},
                {"environment", environment},
                {"environmentSecrets", new List<string>{Util.ResourceId("secret")}}
            };
        }
            
        public static Dictionary<string, object> ErrorConfig(){
            return new Dictionary<string, object>{
                {"email", "foo@lucidtech.io"},
                {"manualRetry", true}
            };
        }
            
        public static Dictionary<string, string?> NameAndDescription(string? name, string? description){
            return new Dictionary<string, string?>{
                {"name", name},
                {"description", description}
            };
        }
    }
    
}
