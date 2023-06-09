﻿using System.Data;

namespace messages_backend.Models.Entities
{
    public class Account : BaseModel
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string MainRSAKey { get; set; }
        public string ComainRSAKey { get; set; }
        public Role Role { get; set; } 
        public List<Message> MessagesSent { get; set; } = new List<Message>();
        public List<Message> MessagesReceived { get;set; } = new List<Message>();
      
    }
}
