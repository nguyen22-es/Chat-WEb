using API.SignalrHub;
using AutoMapper;
using DATAA.Data.Entities;
using DataAccess;
using DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebchatSignalr.ViewModel;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ClientChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatRoomController : ControllerBase
    {
        public readonly ManageAppDbContext manageAppDbContext;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ChatHub chatHub;
        private readonly IMapper _mapper;

        public ChatRoomController(ChatHub chatHub, ManageAppDbContext manageAppDbContext, IHubContext<ChatHub> hubContext, IMapper mapper)
        {
            this.manageAppDbContext = manageAppDbContext;
            _hubContext = hubContext;
            this.chatHub = chatHub;
            _mapper= mapper;
        }
        // GET: api/<ChatRoomController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChatRoomViewModel>>> Get(string UserID)  // thông tin những phòng mình tham gia
        {
            var user =  manageAppDbContext.chatRoomUsers.Where(n => n.UserID == UserID).ToList();
            var Room = await manageAppDbContext.chatRooms.Where(i => i.Admin == UserID).ToListAsync();

            var ListRoom = new List<ChatRoom>();

            foreach (var item in user)
            {
               var room = manageAppDbContext.chatRooms.FirstOrDefault(n => n.RoomID == item.RoomID);
                ListRoom.Add(room);
            }

            var ListRoomAdminViewModel = _mapper.Map<List<ChatRoom>, List<ChatRoomViewModel>>(Room);

            var ListRoomViewModel = _mapper.Map<List<ChatRoom>, List<ChatRoomViewModel>>(ListRoom);
            ListRoomViewModel.AddRange(ListRoomAdminViewModel);

            return Ok(ListRoomViewModel);
        }

        // GET api/<ChatRoomController>/5
        [HttpGet("RoomAdmin")]
        public async Task<ActionResult<ChatRoom>> GetRoom(string id) // thông tin phòng mình làm admin
        {
            var Room = await manageAppDbContext.chatRooms.FirstOrDefaultAsync(i => i.Admin == id);
            if (Room == null)
                return NotFound();


            var Model = _mapper.Map<ChatRoom,ChatRoomViewModel>(Room);
            return Ok(Model);
        }

        // POST api/<ChatRoomController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] ChatRoomViewModel model) // tạo phòng chat mới
        {
            var id = Guid.NewGuid().ToString();
            model.RoomID = id;
            
            var Room = _mapper.Map<ChatRoomViewModel,ChatRoom>(model);
            manageAppDbContext.chatRooms.Add(Room);

          var result =   await manageAppDbContext.SaveChangesAsync();

            if (result > 0)
            {
                var newRoom = manageAppDbContext.chatRooms.FirstOrDefault(n => n.RoomID == id);

                ChatRoomUser chatRoomUser = new ChatRoomUser 
                {
                RoomID = id,
                UserID = model.Admin
                };

                var newRoomViewModel = _mapper.Map<ChatRoom,ChatRoomViewModel>(newRoom);

                return Ok(newRoomViewModel);
            }
            else
            {
                return StatusCode(500, "Lỗi tao phòng chat.");
            }

        }
        [HttpPost("AddUser")]
        public async Task<ActionResult> AddUser([FromBody] RequestAddUser requestAddUser)  // thêm người vào phòng chat
        {
            var AddRoom = new ChatRoomUser
            {
                RoomID = requestAddUser.RoomID,
                UserID = requestAddUser.userID,

            };

            manageAppDbContext.chatRoomUsers.Add(AddRoom);
           
            var result = await manageAppDbContext.SaveChangesAsync();

            if (result > 0)
            {          
                
                var user = await manageAppDbContext.Users.FirstOrDefaultAsync(r => r.ID == requestAddUser.userID);
                await _hubContext.Clients.Group(requestAddUser.RoomID).SendAsync("AddUser");
                return Ok();
            }
            else
            {
                return StatusCode(500, "Lỗi Add User.");
            }

        }

        [HttpDelete]
        public async Task<ActionResult> DeleteUserToRoom(string UserID,string RoomID)
        {
            var Room = await manageAppDbContext.chatRoomUsers.Include(u => u.user).FirstOrDefaultAsync(i => i.UserID == UserID && i.RoomID == RoomID);
            if (Room == null)
            {
                return NotFound();
            }

            manageAppDbContext.Remove(Room);

            try
            {

                var result = await manageAppDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    var RoomName = await manageAppDbContext.chatRooms.FirstOrDefaultAsync(i => i.RoomID == RoomID);
                    await _hubContext.Clients.Groups(RoomName.NameRoom).SendAsync("addUser", Room.user);
                    return Ok();
                }
                else
                {
                    return StatusCode(500, "Lỗi xóa User khỏi chat.");
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Lỗi xóa User khỏi chat: {ex.Message}");
            }


        }

        // DELETE api/<ChatRoomController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var Room = await manageAppDbContext.chatRooms.FirstOrDefaultAsync(i => i.Admin == id);
            if (Room == null)
            {
                return NotFound();
            }

            manageAppDbContext.Remove(Room);

            try
            {
             
                var result = await manageAppDbContext.SaveChangesAsync();

                if (result > 0)
                {
                  
                    return Ok(); 
                }
                else
                {                   
                    return StatusCode(500, "Lỗi xóa phòng chat."); 
                }
            }
            catch (Exception ex)
            {
             
                return StatusCode(500, $"Lỗi xóa phòng chat: {ex.Message}"); 
            }


        }



    }
}
