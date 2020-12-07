using System.Threading.Tasks;

namespace WhoIsWho.DataLoader.Sync.Services
{
    public interface IWhoIsWhoDataSyncronizer
    {
        Task ExecuteDataSyncronizationAsync(string loaderIdentifier);
    }
}