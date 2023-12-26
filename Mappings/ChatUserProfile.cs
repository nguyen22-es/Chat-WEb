using AutoMapper;
using DATAA.Data.Entities;
using SocialNetworking.Data.Entities;
using WebchatSignalr.ViewModel;

namespace WebchatSignalr.Mappings
{
    public class ChatUserProfile : Profile
    {

        public ChatUserProfile()
        { 
           
            CreateMap<ChatUser, ChatUserViewModel>()
                .ForMember(dst => dst.ChatUserID, opt => opt.MapFrom(x => x.ChatUserID))
                .ForMember(dst => dst.FriendChatID, opt => opt.MapFrom(x => x.User2))
                 .ForMember(dst => dst.FriendChatName, opt => opt.MapFrom(x => x.User2.Name));
            CreateMap<ChatRoomViewModel, ChatUser>();


        }


    }
}
