namespace prbd_1718_presences_g03
{
    using System;
    using System.Collections.Generic;

    public partial class Courseoccurrence
    {
        public Courseoccurrence()
        {
            this.Presence = new HashSet<Presence>();
        }

        public int Id { get; set; }
        public System.DateTime Date { get; set; }

        public virtual Course Course { get; set; }
        public virtual ICollection<Presence> Presence { get; set; }

        public override String ToString()
        {
            return Date.ToString("dd-MM-yy").Trim();
        }
    }
}