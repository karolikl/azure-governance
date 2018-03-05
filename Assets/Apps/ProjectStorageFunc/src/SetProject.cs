using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProjectPersistenceFunc.TableEntities
{
    public static class SetProject
    {
        public class Project
        {
            public string ProjectName { get; set; }
            public string Owner { get; set; }
            public string Environment { get; set; }
            public bool CreateVNet { get; set; }
        }

        [FunctionName("SetProject")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log)
        {
            Project project = await req.Content.ReadAsAsync<Project>();
            log.Info(string.Format("Function triggered for project {0} with environment {1}.", project.ProjectName, project.Environment));

            // Retrieve the storage account from the connection string.
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            string prefix = string.Empty;

            if (project.CreateVNet)
                prefix = GetAddressPrefix(tableClient, log);

            if (prefix == null)
                return req.CreateResponse(HttpStatusCode.InternalServerError);

            int? spendLimit = GetSpendLimit(tableClient, project.Environment, log);
            if (spendLimit == null) spendLimit = 100;
            bool success = InsertProject(tableClient, project, spendLimit.Value, prefix, log);

            return success ? req.CreateResponse(HttpStatusCode.OK, prefix) : req.CreateResponse(HttpStatusCode.InternalServerError);
        }

        private static string GetAddressPrefix(CloudTableClient tableClient, TraceWriter log)
        {
            try
            {
                log.Info(string.Format("Retrieving available address prefix"));
                CloudTable table = tableClient.GetTableReference("tblAddressPrefix");

                TableQuery<AddressPrefixEntity> tableQuery = new TableQuery<AddressPrefixEntity>().Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "AddressPrefix"),
                    TableOperators.And,
                    TableQuery.GenerateFilterConditionForBool("Assigned", QueryComparisons.Equal, false))).Take(1);

                AddressPrefixEntity entity = table.ExecuteQuery(tableQuery).FirstOrDefault();
                if (entity != null && !entity.Assigned)
                {
                    entity.Assigned = true;
                    TableOperation updateOperation = TableOperation.Replace(entity);
                    table.Execute(updateOperation);

                    log.Info(string.Format("Assigned address prefix: {0}", entity.IPAddress));
                    return entity.IPAddress;
                }
                else
                {
                    log.Error(string.Format("No available address prefixes found"));
                    return null;
                }
            }
            catch (System.Exception e)
            {
                log.Error(string.Format("Could not retrieve a address prefix. Exception:{0}", e.Message));
                return null;
            }
        }

        private static int? GetSpendLimit(CloudTableClient tableClient, string environment, TraceWriter log)
        {
            try
            {
                log.Info(string.Format("Retrieving spend limit for environment:{0}", environment));
                CloudTable table = tableClient.GetTableReference("tblSpendLimit");

                TableOperation retrieveOperation = TableOperation.Retrieve<SpendLimitEntity>("SpendLimit", environment);
                TableResult retrievedResult = table.Execute(retrieveOperation);

                if (retrievedResult.Result != null)
                {
                    int limit = ((SpendLimitEntity)retrievedResult.Result).Limit;
                    log.Info(string.Format("Retireved spend limit {0} for environment {1}", limit, environment));
                    return limit;
                }
                else
                {
                    log.Error(string.Format("The spend limit for environment:{0} could not be retrieved.", environment));
                    return null;
                }
            }
            catch (System.Exception e)
            {
                log.Error(string.Format("The spend limit for environment:{0} could not be retrieved. Exception:{1}", environment, e.Message));
                return null;
            }
        }

        private static bool InsertProject(CloudTableClient tableClient, Project project, int spendLimit, string addressPrefix, TraceWriter log)
        {
            try
            {
                log.Info(string.Format("Inserting project {0} with environment {1} to table storage", project.ProjectName, project.Environment));
                CloudTable table = tableClient.GetTableReference("tblcorpdev");

                // Create the table if it doesn't exist.
                table.CreateIfNotExists();

                ProjectEntity projectEntity = new ProjectEntity(project.ProjectName, project.Environment, project.Owner);
                projectEntity.SpendLimit = spendLimit;
                projectEntity.AddressPrefix = addressPrefix;

                // Create the TableOperation object that inserts the customer entity.
                TableOperation insertOperation = TableOperation.Insert(projectEntity);

                // Execute the insert operation.
                table.Execute(insertOperation);
                log.Info(string.Format("Finished inserting project {0} with environment {1} to table storage", project.ProjectName, project.Environment));
                return true;
            }
            catch (System.Exception e)
            {
                log.Error("An error occured when inserting project to table storage: " + e.Message);
                return false;
            }
        }
    }
}
