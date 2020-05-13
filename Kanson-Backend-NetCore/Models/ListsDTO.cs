using System;
using System.Collections.Generic;

namespace KansonBackendNetCore.Models
{
    public partial class ListsDTO
    {
        public ListsDTO() { }

        public string Id { get; set; }
        public string BoardId { get; set; }
        public int Index { get; set; }
        public string Title { get; set; }
    }
}
