namespace prbd_1718_presences_g03
{
    using System;
    using System.Collections.Generic;

    public partial class Student : IComparable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Student()
        {
            this.Certificate = new HashSet<Certificate>();
            this.Presence = new HashSet<Presence>();
            this.Course = new HashSet<Course>();
        }

        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Sex { get; set; }

        public virtual ICollection<Certificate> Certificate { get; set; }
        public virtual ICollection<Presence> Presence { get; set; }
        public virtual ICollection<Course> Course { get; set; }

        public override String ToString()
        {
            return LastName + ", " + FirstName;
        }


        public int CompareTo(object obj)
        {
            if (obj is Student)
            {
                Student other = (Student)obj;
                if (LastName.CompareTo(other.LastName) == 0) // ils ont le même nom de famille
                {
                    return FirstName.CompareTo(other.FirstName);
                }
                else
                {
                    return LastName.CompareTo(other.LastName);
                }

            }
            else
                throw new ArgumentException("Object is not a Student.");
        }
    }
}
