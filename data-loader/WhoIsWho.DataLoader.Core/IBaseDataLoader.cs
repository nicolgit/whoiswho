using System.Threading.Tasks;
using WhoIsWho.DataLoader.Models;

namespace WhoIsWho.DataLoader.Core
{
    public interface IBaseDataLoader
    {
        string LoaderIdentifier { get; }
        Task LoadData();
        Task<WhoIsWhoEntity> InsertOrMergeEntityAsync(WhoIsWhoEntity entity);
    }
}