| Language | Framework | Platform | Author |
| -------- | -------- |--------|--------|
| ASP.NET Core | .NET Core 2.0.1 | Azure Web App |


# SelfServiceWeb application

This is an ASP.NET Core MVC application built using Visual Studio 2017.
The application allows users to specify which development environments they want to provision for their project. Submitting a provisioning request will do the following: 
1. Validate whether the environments already exist in the "tblproject" storage table.  
2. Trigger the "Governance Workflow" Logic App to setup the environments.

## Setup

Before running this application, you will need to modify the values in the [appsettings.json](./src/appsettings.json) file:

```
  "Provisioning": {
    "Endpoint": "<Logic App endpoint>",
    "Environments": {
      "dev": {
        "SubscriptionId": "<Dev environment subscription ID>"
      },
      "test": {
        "SubscriptionId": "<Test environment subscription ID>"
      },
      "staging": {
        "SubscriptionId": "<Staging environment subscription ID>"
      }
    }
  },
  "ConnectionStrings": {
    "StorageConnectionString": "<Storage account connection string>"
  }
```



