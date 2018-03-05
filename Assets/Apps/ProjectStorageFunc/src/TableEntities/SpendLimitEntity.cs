using Microsoft.WindowsAzure.Storage.Table;

namespace ProjectPersistenceFunc
{
    public class SpendLimitEntity : TableEntity
    {
        public int Limit { get; set; }
    }
}
