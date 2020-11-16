using AutoMapper;

namespace WhoIsWho.DataLoader.Models.AutoMapperProfiles
{
    public class WhoIsWhoEntityProfile : Profile
    {
        public WhoIsWhoEntityProfile()
        {
            CreateMap<WhoIsWhoEntity, WhoIsWhoSyncedEntity>();
        }
    }
}
