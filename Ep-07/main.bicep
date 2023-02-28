param location string = resourceGroup().location
param dataverseURL string
param application string = 'powerplatform'
param environment string = 'dev'
param type string = 'generic'
param customStorageName string = 'pp'

var functionAppName = 'func-${application}-${type}-${environment}'
var appServicePlanName = 'plan-${application}-${location}-${environment}'
var appInsightsName = 'app-insights-${application}-${environment}'
var storageAccountName = 'stpowertips${replace(customStorageName, '-', '')}${replace(environment, '-', '')}'
var functionRuntime = 'dotnet'
var logAnalyticsWorkspaceName = 'log-analytics-${application}-${environment}'

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

resource appServicePlan 'Microsoft.Web/serverfarms@2021-01-15' = {
  name: appServicePlanName
  location: location
  kind: 'functionapp'
  sku: {
    name: 'Y1'
  }
}

resource logAnalyticsWorkspace 'microsoft.operationalinsights/workspaces@2021-12-01-preview' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

var defaultAppSettings = [
  {
    name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
    value: reference(appInsights.id, '2020-02-02').InstrumentationKey
  }
  {
    name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
    value: 'InstrumentationKey=${reference(appInsights.id, '2020-02-02').InstrumentationKey}'
  }
  {
    name: 'AzureWebJobsStorage'
    value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'
  }
  {
    name: 'FUNCTIONS_EXTENSION_VERSION'
    value: '~3'
  }
  {
    name: 'FUNCTIONS_WORKER_RUNTIME'
    value: functionRuntime
  }
  {
    name: 'PowerApps:dataverseURL'
    value: dataverseURL
  }
  {
    name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
    value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=core.windows.net;AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'
  }
  {
    name: 'WEBSITE_CONTENTSHARE'
    value: functionAppName
  }
  {
    name: 'WEBSITE_RUN_FROM_PACKAGE'
    value: '1'
  }
]

resource azureFunction 'Microsoft.Web/sites@2022-03-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: defaultAppSettings
    }
  }
}

output functionAppHostName string = azureFunction.properties.defaultHostName
output functionName string = functionAppName
output applicationId string = reference(azureFunction.id, '2021-01-15', 'full').identity.principalId
