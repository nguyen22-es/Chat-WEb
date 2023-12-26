using AutoMapper;
using DATAA.Data.Entities;
using DataAccess.Entities;
using System.Security.Cryptography;
using WebchatSignalr.ViewModel;

namespace WebchatSignalr.Mappings
{
    public class ChatRoomProfile : Profile
    {
        public ChatRoomProfile()
        { // test
            CreateMap<ChatRoom, ChatRoomViewModel>()
                .ForMember(dst => dst.NameRoom, opt => opt.MapFrom(x => x.NameRoom))
                .ForMember(dst => dst.Admin, opt => opt.MapFrom(x => x.Admin))
                 .ForMember(dst => dst.RoomID, opt => opt.MapFrom(x => x.RoomID));              
            CreateMap<ChatRoomViewModel, ChatRoom>();
        }
    }
}
