using AutoMapper;
using DataAccess.Entities;
using SocialNetworking.Data.Entities;
using System.Security.Cryptography;
using WebchatSignalr.ViewModel;

namespace WebchatSignalr.Mappings
{
    public class UserProfile:Profile
    {
        public UserProfile()
        {

            CreateMap<User, UserViewModel>()
                .ForMember(dst => dst.Avartar, opt => opt.MapFrom(x => x.Avatar))
                .ForMember(dst => dst.DisplayName, opt => opt.MapFrom(x => x.Name))
                 .ForMember(dst => dst.ID, opt => opt.MapFrom(x => x.ID));
            CreateMap<UserViewModel, User>();


        }
    }
}
