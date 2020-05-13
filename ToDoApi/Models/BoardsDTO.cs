using System;
using System.Collections.Generic;

namespace ToDoApi.Models
{
    public partial class BoardsDTO
    {
        public BoardsDTO() { }

        public string Id { get; set; }
        public string UserId { get; set; }
        public int Index { get; set; }
        public string Title { get; set; }
    }
}
