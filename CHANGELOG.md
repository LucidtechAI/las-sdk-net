# Changelog 

## Version 5.0.0 - 2023-12-19

- Added status UpdateWorkflowExecution
- Remove width and height from CreateModel
- Remove width and height from UpdateModel
- Remove status from UpdateModel
- Remove contentType from CreateDocument

## Version 4.1.0 - 2023-06-27

- Added support for uploading document to file server

## Version 3.1.0 - 2022-03-28

- Added postprocessConfig to CreatePrediction

## Version 3.0.0 - 2021-10-22

- Removed apiKey from Credentials. 

## Version 2.0.0 - 2021-10-13

- Removed all support for batches. Use datasets instead.

## Version 1.5.0 - 2021-07-01

- Added DeleteDocument
- Added optional parameter datasetId to UpdateDocument
- Added parameter deleteAll to DeleteDocuments
- Added CreateDataBundle
- Added ListDataBundles 
- Added UpdateDataBundle
- Added DeleteDataBundle

## Version 1.4.1 - 2021-07-01

- Revert argument order for CreateDocuments, ListDocuments, and DeleteDocuments to avoid breaking changes.

## Version 1.4.0 - 2021-06-28

- Added CreateDataset
- Added ListDatasets
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
