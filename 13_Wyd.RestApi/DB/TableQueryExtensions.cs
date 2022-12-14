using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;

//imported from: https://www.vivien-chevallier.com/Articles/executing-an-async-query-with-azure-table-storage-and-retrieve-all-the-results-in-a-single-operation
namespace AzureTableStorage.TableQueryAsync
{
    public static class TableQueryExtensions
    {
        /// <summary>
        /// Initiates an asynchronous operation to execute a query and return all the results.
        /// </summary>
        /// <param name="tableQuery">A Microsoft.WindowsAzure.Storage.Table.TableQuery representing the query to execute.</param>
        /// <param name="ct">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        public async static Task<IEnumerable<TElement>> ExecuteAsync<TElement>(this TableQuery<TElement> tableQuery, CancellationToken ct)
        {
            var nextQuery = tableQuery;
            var continuationToken = default(TableContinuationToken);
            var results = new List<TElement>();

            do
            {
                //Execute the next query segment async.
                var queryResult = await nextQuery.ExecuteSegmentedAsync(continuationToken, ct);
                
                if (queryResult == null || queryResult.Results == null)
                    return null;

                //Set exact results list capacity with result count.
                results.Capacity += queryResult.Results.Count;

                //Add segment results to results list.
                results.AddRange(queryResult.Results);

                continuationToken = queryResult.ContinuationToken;

                //Continuation token is not null, more records to load.
                if (continuationToken != null && tableQuery.TakeCount.HasValue)
                {
                    //Query has a take count, calculate the remaining number of items to load.
                    var itemsToLoad = tableQuery.TakeCount.Value - results.Count;

                    //If more items to load, update query take count, or else set next query to null.
                    nextQuery = itemsToLoad > 0
                        ? tableQuery.Take<TElement>(itemsToLoad).AsTableQuery()
                        : null;
                }

            } while (continuationToken != null && nextQuery != null && !ct.IsCancellationRequested);

            return results;
        }

        public async static Task<IEnumerable<TElement>> ExecuteAsync<TElement>(this TableQuery<TElement> tableQuery)
        {
            return await tableQuery.ExecuteAsync(CancellationToken.None);
        }

    }
}