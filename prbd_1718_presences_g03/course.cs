namespace prbd_1718_presences_g03
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Documents;

    public partial class Course
    {
        public Course()
        {
            this.CourseOccurrence = new HashSet<Courseoccurrence>();
            this.Students = new HashSet<Student>();
        }

        public int Code { get; set; }
        public string Title { get; set; }
        public System.TimeSpan StartTime { get; set; }
        public System.TimeSpan EndTime { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime FinishDate { get; set; }
        public int DayOfWeek { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<Courseoccurrence> CourseOccurrence { get; set; }
        public virtual ICollection<Student> Students { get; set; }

        public override String ToString()
        {
            String res = "";
            return res;
        }




        public ObservableCollection<Courseoccurrence> computeCourseOccurences()
        {
            ObservableCollection<Courseoccurrence> courseOccurenceColl = new ObservableCollection<Courseoccurrence>();
            DateTime startDate = this.StartDate;
            DateTime endDate = this.FinishDate;

            TimeSpan diff = endDate - startDate;
            int days = diff.Days;
            for (var i = 0; i <= days; ++i)
            {
                DateTime testDate = startDate.AddDays(i);
                if ((int)testDate.DayOfWeek == this.DayOfWeek)
                {
                    //on va ajouter l'occurence du cours à la collection
                    Courseoccurrence currentCO = new Courseoccurrence();
                    currentCO.Course = this;
                    currentCO.Date = testDate;
                    currentCO.Presence = null;
                    courseOccurenceColl.Add(currentCO);
                }

            }

            Console.WriteLine("oui la méthode compute a été executée");
            return courseOccurenceColl;
        }
    }
}