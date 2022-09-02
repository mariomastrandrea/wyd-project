using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _13_Wyd.RestApi.DB.Entities.Log;
using AzureTableStorage.TableQueryAsync;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Extensions.Configuration;

namespace _13_Wyd.RestApi.DB.Tables.Log
{
    public class LogTS
    {
        private readonly CloudTableClient TableStorageClient;
        private readonly CloudTable Table;
        private readonly string TABLE_NAME;


        public LogTS(CloudTableClient tableStorageClient, IConfiguration configuration)
        {
            this.TableStorageClient = tableStorageClient;
            this.TABLE_NAME = configuration.GetSection("TableStorage")
                                           .GetSection("Tables")
                                           .GetValue<string>("Log");

            this.Table = this.TableStorageClient.GetTableReference(TABLE_NAME);
            this.Table.CreateIfNotExists();
        }

        public async Task<ModelClasses.Log> CreateLog(ModelClasses.Log newLog)
        {
            if (newLog == null || string.IsNullOrWhiteSpace(newLog.Id)
                || string.IsNullOrWhiteSpace(newLog.Message))
                return null;

            LogEntity newEntity = new LogEntity(newLog);
            TableOperation insertOperation = TableOperation.Insert(newEntity);
            TableResult result = await this.Table.ExecuteAsync(insertOperation);

            if (result == null || result.Result == null || result.HttpStatusCode >= 400)
                return null;    //some error occured

            LogEntity insertedEntity = result.Result as LogEntity;
            return insertedEntity.ToLog();
        }

        public async Task<IEnumerable<ModelClasses.Log>> GetTopLogs(int num)
        {
            if (num <= 0) return null;

            var query = this.Table.CreateQuery<LogEntity>()
                                    .Take<LogEntity>(num)
                                    .AsTableQuery();

            IEnumerable<LogEntity> topRows = await query.ExecuteAsync();

            if (topRows == null) return null;

            IEnumerable<ModelClasses.Log> topLogs = topRows.Select(entity => entity.ToLog());
            return topLogs;
        }
    }
}
