using System.Collections.Generic;

namespace SelfServiceWeb.Domain
{
    public class ProvisioningRequest
    {
        public bool ProvisionAzure { get; private set; }

        public bool ProvisionVSTS { get; private set; }

        public string ProjectName { get; private set; }

        public string Email { get; private set; }

        public string OwnerObjectId { get; private set; }

        public List<Environment> Environments { get; private set; }

        public ProvisioningRequest(bool provisionAzure, bool provisionVSTS, string projectName, string owner, string ownerObjectId)
        {
            ProvisionAzure = provisionAzure;
            ProvisionVSTS = provisionVSTS;
            ProjectName = projectName;
            Email = owner;
            OwnerObjectId = ownerObjectId;
            Environments = new List<Environment>();
        }
    }

    public class Environment
    {
        public string ResourceGroup { get; private set; }

        public string SubscriptionId { get; private set; }

        public string EnvironmentName { get; private set; }

        public Tags Tags { get; private set; }

        public bool CreateVNet { get; set; }

        public Environment(string resourceGroup, string subscriptionId, string projectName, string environment, string owner)
        {
            ResourceGroup = resourceGroup;
            SubscriptionId = subscriptionId;
            EnvironmentName = environment;
            Tags = new Tags(projectName, environment, owner);
        }
    }

    public class Tags
    {
        public string ProjectName { get; private set; }
        public string Environment { get; private set; }
        public string Owner { get; private set; }

        public Tags(string projectName, string environment, string owner)
        {
            ProjectName = projectName;
            Environment = environment;
            Owner = owner;
        }
    }
}
