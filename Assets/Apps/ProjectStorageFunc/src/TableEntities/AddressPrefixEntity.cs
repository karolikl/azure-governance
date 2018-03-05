using Microsoft.WindowsAzure.Storage.Table;

namespace ProjectPersistenceFunc.TableEntities
{
    public class AddressPrefixEntity : TableEntity
    {
        public string IPAddress { get; set; }

        public bool Assigned { get; set; }
    }
}
