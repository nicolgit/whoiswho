using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WhoIsWho.DataLoader.Sync.Services.Abstract
{
    public interface IWhoIsWhoDataSyncronizer
    {
        public Task ExecuteSynronizationAsync();
    }
}
