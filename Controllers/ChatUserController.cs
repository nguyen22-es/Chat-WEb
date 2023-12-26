using API.SignalrHub;
using AutoMapper;
using DATAA.Data.Entities;
using DataAccess;
using DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialNetworking.Data.Entities;
using WebchatSignalr.ViewModel;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebchatSignalr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatUserController : ControllerBase
    {
        public readonly ManageAppDbContext manageAppDbContext;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ChatHub chatHub;
        private readonly IMapper _mapper;

        public ChatUserController(ChatHub chatHub, ManageAppDbContext manageAppDbContext, IHubContext<ChatHub> hubContext, IMapper mapper)
        {
            this.manageAppDbContext = manageAppDbContext;
            _hubContext = hubContext;
            this.chatHub = chatHub;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChatUserViewModel>>> Get(string UserID)  // thông tin những phòng mình tham gia
        {
            var user = manageAppDbContext.ChatUsers.Include(n => n.User2).Include(n =>n.User1).Where(n => n.IDuser1 == UserID || n.IDuser2 == UserID).ToList();

            var ListRoom = new List<ChatUserViewModel>();

            foreach(var item in user)
            {
                if(item.IDuser1 == UserID)
                {
                    ListRoom.Add(new ChatUserViewModel {ChatUserID = item.ChatUserID, FriendChatID = item.IDuser2, FriendChatName = item.User2.Name });
                }
                if(item.IDuser2 == UserID)
                {
                    ListRoom.Add(new ChatUserViewModel { ChatUserID = item.ChatUserID, FriendChatID = item.IDuser1, FriendChatName = item.User1.Name });
                }
            }


        

          

            return Ok(ListRoom);
        }
        // GET api/<ChatUserController>/5
        [HttpPost]
        public async Task<ActionResult> Post(string ID,[FromBody] string FriendChatID) // tạo phòng chat mới
        {
            var chatUser = messageViewModelsAsync(ID, FriendChatID).Result;
            if (chatUser != null)
                return Ok(chatUser);



            var id = Guid.NewGuid().ToString();          
            var Room = new ChatUser { 
            IDuser1 = ID,
            IDuser2 = FriendChatID,
            ChatUserID = id

            };

         

            manageAppDbContext.ChatUsers.Add(Room);

            var result = await manageAppDbContext.SaveChangesAsync();

            if (result > 0)
            {
                var chat = messageViewModelsAsync(ID, FriendChatID).Result;
                return Ok(chat);
            }
            else
            {
                return StatusCode(500, "Lỗi tao phòng chat.");
            }

        }


        private async Task<List<MessageViewModel>> messageViewModelsAsync(string ID,string FriendChatID)
        {

            var chatUser = await manageAppDbContext.ChatUsers.Include(m => m.Messages.Take(20)).ThenInclude(b => b.UserSend).FirstOrDefaultAsync(u => u.IDuser1 == ID && u.IDuser2 == FriendChatID || u.IDuser2 == ID && u.IDuser1 == FriendChatID);

            if (chatUser != null)
            {

                var message = chatUser.Messages.OrderByDescending(n => n.TimeSend).ToList();


                var messageViewModel = _mapper.Map<List<Messages>, List<MessageViewModel>>(message);
                return messageViewModel;


            }
            else { return null; }

        }

    }
}
