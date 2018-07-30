namespace prbd_1718_presences_g03
{
    using System;
    using System.Collections.Generic;

    public partial class Presence
    {
        public int Student { get; set; } // 1 id d'étudiant
        public int CourseOccurence { get; set; } // 1 id de courseOccurence
        public short Present { get; set; }

        public virtual Courseoccurrence CourseOccurrence { get; set; }
        public virtual Student Student1 { get; set; }

        public Presence()
        {

        }

        public Presence(int st, int co, short p)
        {
            Student = st;
            CourseOccurence = co;
            Present = p;
        }

        public String PresentToString(short Present)
        {
            return Present == 1 ? "P" : "A";
        }

        public override String ToString()
        {
            return "" + this.Present;
        }
    }
}
