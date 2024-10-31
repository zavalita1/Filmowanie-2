// @minLength(3)
// @maxLength(11)
// param storagePrefix string

// @allowed([
//   'Standard_LRS'
//   'Standard_GRS'
//   'Standard_RAGRS'
//   'Standard_ZRS'
//   'Premium_LRS'
//   'Premium_ZRS'
//   'Standard_GZRS'
//   'Standard_RAGZRS'
// ])
// param storageSKU string = 'Standard_LRS'

param location string = resourceGroup().location

// var uniqueStorageName = '${storagePrefix}${uniqueString(resourceGroup().id)}'

// resource stg 'Microsoft.Storage/storageAccounts@2023-04-01' = {
//   name: uniqueStorageName
//   location: location
//   sku: {
//     name: storageSKU
//   }
//   kind: 'StorageV2'
//   properties: {
//     supportsHttpsTrafficOnly: true
//   }
// }

// output storageEndpoint object = stg.properties.primaryEndpoints

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2021-06-01-preview' = {
  name: 'filmowanie-2'
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}


