using System;
using System.Collections.Generic;

namespace KansonNetCoreBackend.Models
{
    public partial class CardsDTO
    {
        public string Id { get; set; }
        public string ListId { get; set; }
        public int Index { get; set; }
        public string Text { get; set; }
    }
}
