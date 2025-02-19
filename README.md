# DigiKey Bulk Import

## Running the export pipeline

The export pipeline will export product data out of DigiKey and into a local file Data/digikey-products.json

1. In ordercloud-bulk-import-console make sure _export.RunAsync is uncommented in BackgroundProcess.cs 
2. In ordercloud-bulk-import-console project set appsettings.json:
```json
{
  "DigiKey": {
    "BaseApiUrl": "https://api.digikey.com",
    "BaseAuthUrl": "https://api.digikey.com",
    "ClientId": "YOUR_DIGIKEY_CLIENT_ID",
    "ClientSecret": "YOUR_DIGIKEY_CLIENT_SECRET"
  }
}
``` 
3. Run the ordercloud-bulk-import-functions project

## Running the import pipeline

The import pipeline will import product data into OrderCloud, from the local file Data/digikey-products.json

1. In ordercloud-bulk-import-console make sure _import.RunAsync is uncommented in BackgroundProcess.cs
2. In ordercloud-bulk-import-console project set appsettings.json:
```json
{
  "OrderCloud": {
    "ApiUrl": "YOUR_ORDERCLOUD_API_URL",
    "MiddlewareClientID": "YOUR_MIDDLEWARE_CLIENT_ID",
    "MiddlewareClientSecret": "YOUR_MIDDLEWARE_CLIENT_SECRET",
    "CatalogId": "YOUR_ORDERCLOUD_CATALOG_ID"
  },
  "DigiKey": {
    "BaseApiUrl": "https://api.digikey.com",
    "BaseAuthUrl": "https://api.digikey.com",
    "ClientId": "YOUR_DIGIKEY_CLIENT_ID",
    "ClientSecret": "YOUR_DIGIKEY_CLIENT_SECRET"
  }
}
```
3. Run the ordercloud-bulk-import-console project. product data will be serialized to Data/digikey-products.json