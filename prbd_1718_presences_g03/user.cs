namespace prbd_1718_presences_g03
{
    using System;
    using System.Collections.Generic;

    public partial class User
    {
        public User()
        {
            this.Course = new HashSet<Course>();
        }

        public int Id { get; set; }
        public string Pseudo { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }

        public virtual ICollection<Course> Course { get; set; }

        public override String ToString()
        {
            return FullName;
        }

        public bool isAdmin()
        {
            return Role.Equals("admin");
        }
    }
}
