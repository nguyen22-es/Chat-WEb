using API.SignalrHub;
using AutoMapper;
using DATAA.Data.Entities;
using DataAccess;
using DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialNetworking.Data.Entities;
using System.Text.RegularExpressions;
using WebchatSignalr.ViewModel;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebchatSignalr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly ManageAppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;
        public MessagesController(ManageAppDbContext context, IHubContext<ChatHub> hubContext, IMapper mapper)
        {
            _context = context;
            _hubContext = hubContext;
            _mapper = mapper;
        }

        [HttpGet("Room")]
        public async Task<ActionResult<List<MessageViewModel>>> Get(string id)  // tin nhắn trong phòng
        {
            var message =  _context.Messages.Include(n => n.UserSend).Include(n => n.chatRoom).Where(m => m.chatRoomID==id).OrderByDescending(m => m.TimeSend).ToList();
            if (message == null)
                return NotFound();

            var messageViewModel = _mapper.Map<List<Messages>, List<MessageViewModel>>(message);
            return Ok(messageViewModel);
        }

        // GET api/<MessagesController>/5
        [HttpGet("id")]
        public async Task<ActionResult<List<MessageViewModel>>> GetChatUser(string id)  // tin nhắn riêng
        {
            /*var room = await _context.ChatUsers.Include(m => m.Messages).ThenInclude(u => u.chatUser).FirstOrDefaultAsync(r => r.ChatUserID == id);
            if (room == null)
                return BadRequest();*/

            var message = _context.Messages.Include(c => c.UserSend).Include(n => n.chatUser).Where(m => m.chatUserID == id).OrderByDescending(m => m.TimeSend).ToList();
            if (message == null)
                return NotFound();

            var messageViewModel = _mapper.Map<List<Messages>, List<MessageViewModel>>(message);
            return Ok(messageViewModel);
        }
        // POST api/<MessagesController>
        [HttpPost]
        public async Task<ActionResult> CreateMessageUser([FromBody] MessageViewModel messageViewModel)
        {
           
            

            var msg = new Messages()
                {
                    Content = Regex.Replace(messageViewModel.Content, @"<.*?>", string.Empty),
                    chatUserID = messageViewModel.ChatUSerID,
                    SenderUserID = messageViewModel.UserID,
                    TimeSend = DateTime.Now
                };
               
            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            // Broadcast the message
            var chat = _context.ChatUsers.Include(n=>n.User2).Include(n=>n.User1).FirstOrDefault(u => u.ChatUserID == messageViewModel.ChatUSerID);

            
            var createdMessage = _mapper.Map<Messages, MessageViewModel>(msg);

            createdMessage.UserID = messageViewModel.UserID;
            if (createdMessage.UserID == chat.IDuser1)
            {
                createdMessage.NameSend = chat.User1.Name;
            }
            else
            {
                createdMessage.NameSend = chat.User2.Name;
            }
            
            await _hubContext.Clients.User(chat.IDuser1).SendAsync("newMessage", createdMessage);
            await _hubContext.Clients.User(chat.IDuser2).SendAsync("newMessage", createdMessage);

            return Ok();
        }
        [HttpPost("chatRoom")]
        public async Task<ActionResult> CreateMessageRoom( [FromBody] MessageViewModel messageViewModel) // gửi tin nhắn vào trong nhóm
        {
            
            var room = _context.chatRoomUsers.FirstOrDefault(r => r.RoomID == messageViewModel.RoomID);
            var user = _context.Users.FirstOrDefault(r => r.ID == messageViewModel.UserID);

            var msg = new Messages()
            {
                Content = Regex.Replace(messageViewModel.Content, @"<.*?>", string.Empty),
                chatRoomID = messageViewModel.RoomID,               
                SenderUserID = messageViewModel.UserID,
                TimeSend = DateTime.Now
            };

            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            // Broadcast the message



            var createdMessage = _mapper.Map<Messages, MessageViewModel>(msg);

            createdMessage.UserID = messageViewModel.UserID;
            createdMessage.NameSend = user.Name;
            createdMessage.RoomID = messageViewModel.RoomID;
            await _hubContext.Clients.Group(messageViewModel.RoomID).SendAsync("newMessage", createdMessage);
        //    await _hubContext.Clients.User(messageViewModel.UserID).SendAsync("newMessage", createdMessage);

            return Ok();
        }


    }
}
