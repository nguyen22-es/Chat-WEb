using AutoMapper;
using DATAA.Data.Entities;
using DataAccess;
using DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebchatSignalr.ViewModel;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebchatSignalr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public readonly ManageAppDbContext manageAppDbContext;
        private readonly IMapper _mapper;
        public UserController(ManageAppDbContext manageAppDbContext, IMapper mapper) {
            this.manageAppDbContext = manageAppDbContext;
            this._mapper = mapper;
        }
        // GET: api/<UserController>
        [HttpGet]
        public async Task<ActionResult> Get(string id) // thông tin phòng mình làm admin
        {
            var user = await manageAppDbContext.Users.FirstOrDefaultAsync(i => i.ID == id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // POST api/<ChatRoomController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] UserViewModel model) // tạo phòng chat mới
        {
            var user = new User
            {
                Avatar = model.Avartar,
                Name = model.DisplayName,
                ID = model.ID,

            };

            manageAppDbContext.Users.Add(user);

             await manageAppDbContext.SaveChangesAsync();

         

                return Ok();
           

        }


        [HttpGet("All")]
        public async Task<ActionResult<List<UserViewModel>>> GetUser(string roomID) 
        {
            var user = await manageAppDbContext.Users.Include(u => u.chatRoomUsers).Where(u => u.chatRoomUsers.All(cr => cr.RoomID != roomID)).ToListAsync();
            if (user == null)
                return NotFound();


            var ListUser = _mapper.Map<List<User>, List<UserViewModel>>(user);

            return Ok(ListUser);
        }

        [HttpGet("AllUser")]
        public async Task<ActionResult<List<UserViewModel>>> GetAllUser() // thông tin phòng mình làm admin
        {
            var user = await manageAppDbContext.Users.ToListAsync();
            if (user == null)
                return NotFound();


            var ListUser = _mapper.Map<List<User>, List<UserViewModel>>(user);

            return Ok(ListUser);
        }




    }
}
