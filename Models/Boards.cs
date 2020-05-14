using System;
using System.Collections.Generic;

namespace KansonNetCoreBackend.Models
{
    public partial class Boards
    {
        public Boards()
        {
            Lists = new HashSet<Lists>();
        }

        public string Id { get; set; }
        public string UserId { get; set; }
        public int Index { get; set; }
        public string Title { get; set; }

        public virtual Users User { get; set; }
        public virtual ICollection<Lists> Lists { get; set; }
    }
}
