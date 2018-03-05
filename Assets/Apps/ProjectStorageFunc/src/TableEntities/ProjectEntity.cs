using Microsoft.WindowsAzure.Storage.Table;

namespace ProjectPersistenceFunc.TableEntities
{
    public class ProjectEntity : TableEntity
    {
        public ProjectEntity(string projectName, string environment, string owner)
        {
            PartitionKey = projectName;
            RowKey = environment;
            Owner = owner;
        }

        public string Owner { get; set; }

        public int SpendLimit { get; set; }

        public string AddressPrefix { get; set; }
    }
}
