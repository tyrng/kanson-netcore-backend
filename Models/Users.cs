using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KansonNetCoreBackend.Models
{
    public partial class Users
    {
        public Users()
        {
            Boards = new HashSet<Boards>();
        }

        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public virtual ICollection<Boards> Boards { get; set; }
    }
}
