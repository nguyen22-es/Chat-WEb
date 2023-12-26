using AutoMapper;
using DataAccess.Entities;
using DataAccess.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebchatSignalr.ViewModel;


namespace WebApplication1.Mappings
{
    public class MessageProfile:Profile
    {
        public MessageProfile()
        { // test
            CreateMap<Messages, MessageViewModel>()
                .ForMember(dst => dst.NameSend, opt => opt.MapFrom(x => x.UserSend.Name))
                .ForMember(dst => dst.Room, opt => opt.MapFrom(x => x.chatRoom.NameRoom))
                 .ForMember(dst => dst.RoomID, opt => opt.MapFrom(x => x.chatRoom.RoomID))
                .ForMember(dst => dst.UserID, opt => opt.MapFrom(x => x.UserSend.ID))
                 .ForMember(dst => dst.ChatUSerID, opt => opt.MapFrom(x => x.chatUser.ChatUserID))
                .ForMember(dst => dst.Avatar, opt => opt.MapFrom(x => x.UserSend.Avatar))
                .ForMember(dst => dst.Content, opt => opt.MapFrom(x => BasicEmojis.ParseEmojis(x.Content)))
                .ForMember(dst => dst.Timestamp, opt => opt.MapFrom(x => x.TimeSend));
            CreateMap<MessageViewModel, Messages>();
        }
    }
}
