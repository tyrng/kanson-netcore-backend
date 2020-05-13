using System;
using System.Collections.Generic;

namespace ToDoApi.Models
{
    public partial class Cards
    {
        public string Id { get; set; }
        public string ListId { get; set; }
        public int Index { get; set; }
        public string Text { get; set; }

        public virtual Lists List { get; set; }
    }
}
