using System;
using System.Collections.Generic;

namespace KansonBackendNetCore.Models
{
    public partial class Lists
    {
        public Lists()
        {
            Cards = new HashSet<Cards>();
        }

        public string Id { get; set; }
        public string BoardId { get; set; }
        public int Index { get; set; }
        public string Title { get; set; }

        public virtual Boards Board { get; set; }
        public virtual ICollection<Cards> Cards { get; set; }
    }
}
