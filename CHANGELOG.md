# Changelog 

## v1.16 -> v1.2.0:

- startTime argument in PATCH /transitions/:id/executions/:id can be supplied as datetime object.
- PATCH /workflows/:id/executions/:id
- GET /workflows/:id/executions/:id
- DELETE /assets/:id
- DELETE /secrets/:id
- Update POST /workflows to include completedConfig, and support manualRetry in errorConfig
- Update PATCH /transitions/:id to include assets, environment, environmentSecrets
- Update PATCH /workflows/:id to include completedConfig, and errorConfig
- POST /appClients
- GET /appClients
- DELETE /appClients/:id
- GET /logs
- DELETE /batches/:id
- DELETE /documents:id?batchId=...
