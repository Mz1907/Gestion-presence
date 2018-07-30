using System;

namespace prbd_1718_presences_g03
{
    public partial class Course
    {
        public Double Pourcentage { get { return CalculPourcentage(); } }

        public double CalculPourcentage()
        {
            double comptageComplete = 0;
            foreach (var co in this.CourseOccurrence)
            {
                if (co.Presence.Count == this.Students.Count)
                    ++comptageComplete;
            }
            return Math.Round((comptageComplete / this.CourseOccurrence.Count) * 100, 2);
        }
    }
}
