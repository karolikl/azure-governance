# Introduction 
This Azure App Service Function app creates a new user in a VSTS account and assigns the user Project Administrator rights to a project.

# Triggering the function

This function is triggered using a [webhook](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook#trigger---webhook-example).
When triggering the function, the request body should contain the following json (note: example values included below): 

```
{
  "owner": "user@email.com",
  "projectId": "36f12af8-4a52-4a64-9945-275e56d09a91"
}
```

# Continuous integration and continuous delivery

Continous integration has been configured, so any commits to this repository will trigger a [build](https://corpdevevry.visualstudio.com/AzureGovernance/_build/index?context=mine&path=%5C&definitionId=4&_a=completed). 
Upon a successful build, a [release](https://corpdevevry.visualstudio.com/AzureGovernance/_release?definitionId=3&_a=definitionoverview) will be triggered. This will deploy the application to an Azure Function. 

## License:

This application is licensed under the MIT License. 