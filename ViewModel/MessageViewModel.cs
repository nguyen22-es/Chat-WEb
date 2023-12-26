using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebchatSignalr.ViewModel
{
    public class MessageViewModel
    {
      
        public string Content { get; set; }
        public string Timestamp { get; set; }
        public string NameSend { get; set; }
        public string UserID { get; set; }
        public string ChatUSerID { get; set; }
        public string RoomID { get; set; }      
        public string Room { get; set; }
        public string Avatar { get; set; }
    }
}
