using SelfServiceWeb.Domain;
using SelfServiceWeb.Provisioning.ReadModels;
using SelfServiceWeb.ViewModels;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SelfServiceWeb.Provisioning
{
    public class EnvironmentValidator : IValidator
    {
        private readonly ISettings _settings;

        public EnvironmentValidator(ISettings settings)
        {
            _settings = settings;
        }

        public List<ExistingEnvironment> IsValid(ProvisioningViewModel model)
        {
            List<ExistingEnvironment> existingEnvironments = new List<ExistingEnvironment>();
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_settings.GetProjectDbConnectionString());

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("tblproject");

            bool devExists = Exists(table, model.ProjectName, Environments.Development).Result;
            if (devExists)
            {
                var dev = new ExistingEnvironment { Name = Environments.Development };
                if (model.ProvisionDevEnvironment)
                    dev.ErrorMessage = $"Project with name {model.ProjectName} already has an existing development environment";
                existingEnvironments.Add(dev);
            }

            bool testExists = Exists(table, model.ProjectName, Environments.Test).Result;
            if (testExists)
            {
                var test = new ExistingEnvironment { Name = Environments.Test };
                if (model.ProvisionTestEnvironment)
                    test.ErrorMessage = $"Project with name {model.ProjectName} already has an existing test environment";
                existingEnvironments.Add(test);
            }

            bool stagingExists = Exists(table, model.ProjectName, Environments.Staging).Result;
            if (stagingExists)
            {
                var staging = new ExistingEnvironment { Name = Environments.Staging };
                if (model.ProvisionStagingEnvironment)
                    staging.ErrorMessage = $"Project with name {model.ProjectName} already has an existing staging environment";
                existingEnvironments.Add(staging);
            }
            return existingEnvironments;
        }

        private async Task<bool> Exists(CloudTable table, string projectName, string environment)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<ProjectEntity>(projectName, environment);
            TableResult tableResult = await table.ExecuteAsync(retrieveOperation);
            return tableResult.Result != null;
        }
    }
}
