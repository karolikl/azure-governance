using SelfServiceWeb.Domain;
using SelfServiceWeb.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SelfServiceWeb.Provisioning
{
    public class Provisioning : IProvisioning
    {
        private readonly IValidator _validator;
        private readonly IWorkflow _workflow;
        private readonly ISettings _settings;

        public Provisioning(IValidator validator, ISettings settings, IWorkflow workflow)
        {
            _settings = settings;
            _validator = validator;
            _workflow = workflow;
        }

        public async Task<bool> Run(ProvisioningViewModel provisioningViewModel, bool vnetCreatedBefore)
        {
            var request = CreateRequest(provisioningViewModel, vnetCreatedBefore);

            return await _workflow.Trigger(request);
        }

        private ProvisioningRequest CreateRequest(ProvisioningViewModel provisioningViewModel, bool vnetCreatedBefore)
        {
            ProvisioningRequest provisioningRequest = new ProvisioningRequest(provisioningViewModel.ProvisionAzure, provisioningViewModel.ProvisionVSTS, 
                provisioningViewModel.ProjectName, provisioningViewModel.Email, provisioningViewModel.OwnerObjectId);

            if (provisioningViewModel.ProvisionAzure)
            {
                if (provisioningViewModel.ProvisionDevEnvironment)
                    provisioningRequest.Environments.Add(CreateEnvironment(provisioningViewModel, Environments.Development));

                if (provisioningViewModel.ProvisionTestEnvironment)
                    provisioningRequest.Environments.Add(CreateEnvironment(provisioningViewModel, Environments.Test));

                if (provisioningViewModel.ProvisionStagingEnvironment)
                    provisioningRequest.Environments.Add(CreateEnvironment(provisioningViewModel, Environments.Staging));
            }

            if (!vnetCreatedBefore && provisioningRequest.Environments.Any())
                provisioningRequest.Environments.First().CreateVNet = true;

            return provisioningRequest;
        }

        private Environment CreateEnvironment(ProvisioningViewModel viewModel, string environment)
        {
            string subscriptionId = _settings.GetSubscriptionId(environment);
            string resourceGroupName = string.Concat(viewModel.ProjectName, "-", environment + "-rg");
            return new Environment(resourceGroupName, subscriptionId, viewModel.ProjectName, environment, viewModel.Email);
        }
    }
}
