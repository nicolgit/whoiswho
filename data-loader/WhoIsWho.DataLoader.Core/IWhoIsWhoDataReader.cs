using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WhoIsWho.DataLoader.Models;

namespace WhoIsWho.DataLoader.Core
{
    public interface IWhoIsWhoDataReader
    {
        IAsyncEnumerable<T> ReadDataAsync<T>(string tableName, TableQuery<T> query) where T : ITableEntity, new();
    }
}
