using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SelfServiceWeb.Provisioning
{
    public class Settings : ISettings
    {
        private readonly IConfiguration _configuration;

        public Settings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetSubscriptionId(string environment)
        {
            return _configuration.GetSection(ConfigurationPath.Combine("Provisioning", "Environments", environment, "SubscriptionId")).Value;
        }

        public string GetProvisioningUrl()
        {
            return _configuration.GetSection(ConfigurationPath.Combine("Provisioning", "Endpoint")).Value;
        }

        public string GetProjectDbConnectionString()
        {
            return _configuration.GetSection(ConfigurationPath.Combine("ConnectionStrings", "StorageConnectionString")).Value;
        }
    }
}
