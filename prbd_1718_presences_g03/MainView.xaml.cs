using PRBD_Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace prbd_1718_presences_g03
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainView : WindowBase
    {
        public ICommand Save { get; set; }
        public ICommand Logout { get; set; }

        public bool IsAdmin
        {
            get { return App.CurrentUser.isAdmin(); }
        }


        public MainView()
        {
            InitializeComponent();
            DataContext = this;
            RegisterMessenger();

            Save = new RelayCommand(SaveAction);
            Logout = new RelayCommand(LogoutAction);
        }

        private void SaveAction()
        {
            //var isCourseModified = (from c in App.Model.ChangeTracker.Entries<Course>()
            //              where c.Entity.Code == App.currentCourse.Code
            //                        select c).FirstOrDefault();

            //var test = isCourseModified.State != EntityState.Unchanged;

            App.Model.SaveChanges();
            if (App.isNew)
            {
                App.Messenger.NotifyColleagues(App.MSG_SAVING_NEW_COURSE);
                App.isNew = false;
            }
        }

        private void LogoutAction()
        {
            CloseMainView();
        }

        private void RegisterMessenger()
        {
            App.Messenger.Register(App.MSG_COURSE_DETAILS, (Course course) => AddTabCourseSingle(course, false));
            App.Messenger.Register(App.MSG_NEW_COURSE, (Course course) => AddTabCourseSingle(course, true));
            App.Messenger.Register(App.MSG_SHOW_PRESENCE_FORM, () => CreateTabPresence(App.currentCourse, App.currentCo));
            App.Messenger.Register<TabItem>(App.MSG_CLOSE_TAB, tab => { tabControl.Items.Remove(tab); });

            if (!App.CurrentUser.isAdmin())
            {
                CreateTabPlanning();
            }

        }


        private void AddTabCourseSingle(Course course, bool isNew)
        {
            TabItem tab = CreateTabCourseSingle(course, isNew);
            tab.Header = "Course " + course.Code;
            AjouterOngletFocus(tab);
        }

        private void AjouterOngletFocus(TabItem tab)
        {
            // ajoute ce onglet à la liste des onglets existant du TabControl
            tabControl.Items.Add(tab);
            // exécute la méthode Focus() de l'onglet pour lui donner le focus (càd l'activer)
            Dispatcher.InvokeAsync(() => tab.Focus());
        }

        private TabItem CreateTabCourseSingle(Course course, bool isNew)
        {
            var tab = new TabItem()
            {
                Header = "<new member>",
                Content = new CourseSingle(this, course, isNew)
            };
            return tab;
        }

        /**
         * Création onglet présence
         */
        public void CreateTabPresence(Course course, Courseoccurrence courseoccurrence)
        {
            var tab = new TabItem()
            {
                Header = "Présences - " + course.Code + " - " + courseoccurrence.Date.ToString("dd/MM/yy").Trim(),
                Content = new PresenceView(course, courseoccurrence)
            };
            tabControl.Items.Add(tab);
            Dispatcher.InvokeAsync(() => tab.Focus());
        }

        /**
         * Création onglet Planning
         */
        public void CreateTabPlanning()
        {
            var tab = new TabItem()
            {
                Header = "Planning",
                Content = new PlanningView()
            };
            tabControl.Items.Add(tab);
        }



        public static ObservableCollection<User> GetAllTeachersAction()
        {
            var query = from u in App.Model.User
                        select u;

            return new ObservableCollection<User>(query);

        }

        public static ObservableCollection<Student> GetAllStudentsAction()
        {
            var query = from s in App.Model.Student
                        select s;

            return new ObservableCollection<Student>(query);
        }

        public static ObservableCollection<Student> GetAllStudentButCurrentCoursesAction(List<Student> alreadySubscribed)
        {
            var query = from s in App.Model.Student
                        select s;

            var res = query.ToList().Except(alreadySubscribed);
            return new ObservableCollection<Student>(res);
        }

        public static ObservableCollection<Presence> getPresencesByCours(Course course)
        {
            var query = from pr in App.Model.Presence
                        where pr.CourseOccurrence.Course.Code == course.Code
                        select pr;
            return new ObservableCollection<Presence>(query);
        }

        private void CloseMainView()
        {
            LoginView loginView = new LoginView();
            loginView.Show();
            this.Close();
        }

    }
}