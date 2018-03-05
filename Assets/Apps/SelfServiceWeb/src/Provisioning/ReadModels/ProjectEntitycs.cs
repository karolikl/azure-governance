using Microsoft.WindowsAzure.Storage.Table;

namespace SelfServiceWeb.Provisioning.ReadModels
{
    public class ProjectEntity : TableEntity
    {
        public string Owner { get; set; }

        public int SpendLimit { get; set; }

        public string AddressPrefix { get; set; }
    }
}
