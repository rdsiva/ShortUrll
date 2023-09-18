using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Functions.ShortUrlGenerator.Domain.Models;

namespace Functions.ShortUrlGenerator.Domain
{
    public class StorageTableHelper
    {
        private string StorageConnectionString { get; set; }

        public StorageTableHelper() { }

        public StorageTableHelper(IConfiguration config)
        {
            StorageConnectionString = config.GetSection("DataStorage").Value?? "SomeStorageAccount";
        }

        public StorageTableHelper(string connectionstring)
        {
            StorageConnectionString = connectionstring;
        }
        public CloudStorageAccount CreateStorageAccountFromConnectionString()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            return storageAccount;
        }

        private CloudTable GetTable(string tableName)
        {
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString();
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(tableName);
            table.CreateIfNotExists();

            return table;
        }

        private CloudTable GetUrlsTable()
        {
            CloudTable table = GetTable("UrlsDetails");
            return table;
        }

        private CloudTable GetStatsTable()
        {
            CloudTable table = GetTable("ClickStats");
            return table;
        }

        public async Task<ShortUrlEntity?> SaveShortUrlEntity(ShortUrlEntity newShortUrl)
        {

            // serializing the collection easier on json shares
            //newShortUrl.SchedulesPropertyRaw = JsonSerializer.Serialize<List<Schedule>>(newShortUrl.Schedules);

            TableOperation insOperation = TableOperation.InsertOrMerge(newShortUrl);
            TableResult result = await GetUrlsTable().ExecuteAsync(insOperation);
            ShortUrlEntity? eShortUrl = result.Result as ShortUrlEntity;
            return eShortUrl;
        }

        public async Task<ShortUrlEntity?> GetShortUrlEntity(ShortUrlEntity row)
        {
            TableOperation selOperation = TableOperation.Retrieve<ShortUrlEntity>(row.PartitionKey, row.RowKey);
            TableResult result = await GetUrlsTable().ExecuteAsync(selOperation);
            ShortUrlEntity? eShortUrl = result.Result as ShortUrlEntity;
            return eShortUrl;
        }

        public async Task<List<ShortUrlEntity>> GetAllShortUrlEntities()
        {
            var tblUrls = GetUrlsTable();
            TableContinuationToken token = null;
            var lstShortUrl = new List<ShortUrlEntity>();
            do
            {
                // Retreiving all entities that are NOT the NextId entity 
                // (it's the only one in the partion "KEY")
                TableQuery<ShortUrlEntity> rangeQuery = new TableQuery<ShortUrlEntity>().Where(
                    filter: TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, "KEY"));

                var queryResult = await tblUrls.ExecuteQuerySegmentedAsync(rangeQuery, token);
                lstShortUrl.AddRange(queryResult.Results as List<ShortUrlEntity>);
                token = queryResult.ContinuationToken;
            } while (token != null);
            return lstShortUrl;
        }

        /// <summary>
        /// Returns the ShortUrlEntity of the <paramref name="vanity"/>
        /// </summary>
        /// <param name="vanity"></param>
        /// <returns>ShortUrlEntity</returns>
        public async Task<ShortUrlEntity> GetShortUrlEntityByKey(string Key)
        {
            var tblUrls = GetUrlsTable();
            TableContinuationToken token = null;
            ShortUrlEntity shortUrlEntity = null;
            do
            {
                TableQuery<ShortUrlEntity> query = new TableQuery<ShortUrlEntity>().Where(
                    filter: TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, Key));
                var queryResult = await tblUrls.ExecuteQuerySegmentedAsync(query, token);
                shortUrlEntity = queryResult.Results.FirstOrDefault();
            } while (token != null);

            return shortUrlEntity;
        }

        public async Task SaveClickStatsEntity(ClickStatsEntity newStats)
        {
            TableOperation insOperation = TableOperation.InsertOrMerge(newStats);
            TableResult result = await GetStatsTable().ExecuteAsync(insOperation);
        }

        public async Task<bool> IfShortUrlEntityExist(ShortUrlEntity row)
        {
            ShortUrlEntity eShortUrl = await GetShortUrlEntity(row);
            return (eShortUrl != null);
        }

        public async Task<bool> IfShortUrlEntityExistByKey(string vanity)
        {
            ShortUrlEntity shortUrlEntity = await GetShortUrlEntityByKey(vanity);
            return (shortUrlEntity != null);
        }

        public async Task<List<ClickStatsEntity>> GetAllStatsByKey(string endUtlString)
        {
            var tblUrls = GetStatsTable();
            TableContinuationToken token = null;
            var lstShortUrl = new List<ClickStatsEntity>();
            do
            {
                TableQuery<ClickStatsEntity> rangeQuery;

                if (string.IsNullOrEmpty(endUtlString))
                {
                    rangeQuery = new TableQuery<ClickStatsEntity>();
                }
                else
                {
                    rangeQuery = new TableQuery<ClickStatsEntity>().Where(
                    filter: TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, endUtlString));
                }

                var queryResult = await tblUrls.ExecuteQuerySegmentedAsync(rangeQuery, token);
                lstShortUrl.AddRange(queryResult.Results as List<ClickStatsEntity>);
                token = queryResult.ContinuationToken;
            } while (token != null);
            return lstShortUrl;
        }

        public async Task<int> GetNextTableId()
        {
            //Get current ID
            TableOperation selOperation = TableOperation.Retrieve<NextId>("1", "KEY");
            TableResult result = await GetUrlsTable().ExecuteAsync(selOperation);
            NextId entity = result.Result as NextId;

            if (entity == null)
            {
                entity = new NextId
                {
                    PartitionKey = "1",
                    RowKey = "KEY",
                    Id = 1
                };
            }
            entity.Id++;

            //Update
            TableOperation updOperation = TableOperation.InsertOrMerge(entity);

            // Execute the operation.
            await GetUrlsTable().ExecuteAsync(updOperation);

            return entity.Id;
        }
    }
}