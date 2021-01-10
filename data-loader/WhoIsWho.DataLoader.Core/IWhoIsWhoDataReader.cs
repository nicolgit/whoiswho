using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;

namespace WhoIsWho.DataLoader.Core
{
    public interface IWhoIsWhoDataReader
    {
        IAsyncEnumerable<T> ReadDataAsync<T>(string loaderIdentifier, string tableName, TableQuery<T> query) where T : ITableEntity, new();
    }
}
