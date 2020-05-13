using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ToDoApi.Models
{
    public partial class UsersDTO
    {
        public UsersDTO() { }

        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
    }
}
