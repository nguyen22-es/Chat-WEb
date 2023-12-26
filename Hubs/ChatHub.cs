
using AutoMapper;
using DATAA.Data.Entities;
using DataAccess;
using DataAccess.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;
using WebchatSignalr.ViewModel;


namespace API.SignalrHub
{

    [Authorize]
    public class ChatHub : Hub
    {
        public readonly ManageAppDbContext manageAppDbContext;
        private readonly IMapper _mapper;
        public readonly static List<User> _Connections = new List<User>();
        private readonly static Dictionary<string, string> _ConnectionsMap = new Dictionary<string, string>();

        public ChatHub(ManageAppDbContext manageAppDbContext, IMapper mapper)
        {
            _mapper = mapper;
            this.manageAppDbContext = manageAppDbContext;
        }


        public async Task Join(string Room ,string UserID)
        {


            try
            {
                var user = _Connections.Where(u => u.ID == UserID).FirstOrDefault();
               
                    await Groups.AddToGroupAsync(Context.ConnectionId, Room);
                   
               
                await Clients.OthersInGroup(Room).SendAsync("AddUser", user);
                
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("onError", "You failed to join the chat room!" + ex.Message);
            }
        }


        public async Task CreateMessageUser(string receiverName, string message,string ChatUserID)
        {
            if (_ConnectionsMap.TryGetValue(receiverName, out string userId))
            {


                var user = manageAppDbContext.Users.FirstOrDefault(u => u.ID == IdentityName());
                var room = manageAppDbContext.ChatUsers.FirstOrDefault(r => r.ChatUserID == ChatUserID);

                var msg = new Messages()
                {
                    Content = Regex.Replace(message, @"<.*?>", string.Empty),
                    chatUserID = ChatUserID,
                    SenderUserID = user.ID,
                    TimeSend = DateTime.Now
                };

                manageAppDbContext.Messages.Add(msg);
                await manageAppDbContext.SaveChangesAsync();

                // Broadcast the message
                var createdMessage = _mapper.Map<Messages, MessageViewModel>(msg);



                await Clients.Client(userId).SendAsync("newMessage", createdMessage);
                await Clients.Caller.SendAsync("newMessage", createdMessage);



            }
        }




        public async Task Leave(ChatRoom Room)
        {
            var user = _Connections.Where(u => u.ID == IdentityName()).FirstOrDefault();
            await Clients.OthersInGroup(Room.NameRoom).SendAsync("addUser", user);

        }



        public override Task OnConnectedAsync()
        {
                    
            try
            {
               
                var user = manageAppDbContext.Users.Where(u => u.ID == IdentityName()).FirstOrDefault();
                var ListRoom = manageAppDbContext.chatRoomUsers.Include(n => n.room).Where(u => u.UserID == user.ID).ToList();
                

                if (!_Connections.Any(u => u.Name == IdentityName()))
                {
                    _Connections.Add(user);
                    _ConnectionsMap.Add(user.ID, Context.ConnectionId);
                }

                foreach (var item in ListRoom)
                {
                     Groups.AddToGroupAsync(Context.ConnectionId, item.RoomID);
                }


                Clients.Caller.SendAsync("getProfileInfo", user.Name, user.Avatar, user.ID);
            }
            catch (Exception ex)
            {
                Clients.Caller.SendAsync("onError", "OnConnected:" + ex.Message);
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                
                var user = _Connections.Where(u => u.ID == IdentityName()).First();
                _Connections.Remove(user);

                _ConnectionsMap.Remove(user.ID);
            }
            catch (Exception ex)
            {
                Clients.Caller.SendAsync("onError", "OnDisconnected: " + ex.Message);
            }

            return base.OnDisconnectedAsync(exception);
        }

        private string IdentityName()
        {
            var claim = Context.User.Claims.ToList();
                var ID = claim.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier);


                 return ID.Value; 
        }


     }
}
