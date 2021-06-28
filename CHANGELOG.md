# Changelog 

## Version 1.4.0 - 2021-06-28

- Added CreateDataset
- Added GetDatasets
- Added UpdateDataset
- Added DeleteDataset
- Added datasetId argument to CreateDocument
- Added datasetId argument to ListDocuments
- Added datasetId argument to DeleteDocuments

## Version 1.3.0 - 2021-05-21

- Added CreateModel
- Added UpdateModel
- Added GetModel
- Added UpdateBatch
- Added UpdateAppClient

## Version 1.2.0 - 2021-04-30

- Updated startTime argument in UpdateTransitionExecution (PATCH /transitions/:id/executions/:id) to be a datetime object and not a string.
- Added UpdateWorkflowExecution (PATCH /workflows/:id/executions/:id)
- Added GetWorkflowExecution (GET /workflows/:id/executions/:id)
- Added DeleteAsset (DELETE /assets/:id)
- Added DeleteSecret (DELETE /secrets/:id)
- Updated CreateWorkflow (POST /workflows) to include completedConfig, and support manualRetry in errorConfig
- Updated UpdateTransition (PATCH /transitions/:id) to include assets, environment, environmentSecrets
- Updated UpdateWorkflow (PATCH /workflows/:id) to include completedConfig, and errorConfig
- Added CreateAppClient (POST /appClients)
- Added GetAppClient (GET /appClients)
- Added DeleteAppClient (DELETE /appClients/:id)
- Added ListLogs (GET /logs)
- Added DeleteBatches (DELETE /batches/:id)
- Updated DeleteDocuments (DELETE /documents:id) to support queryparameter batchId
