using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using PRBD_Framework;
using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;


namespace prbd_1718_presences_g03
{
    /// <summary>
    /// Logique d'interaction pour courses.xaml
    /// </summary>
    public partial class CoursesView : UserControlBase
    {
        public ICommand ApplyFilter { get; set; }
        public ICommand ClearAll { get; set; }
        public ICommand DisplayNewCourse { get; set; }
        public ICommand DisplayCourseDetails { get; set; }

        public CoursesView()
        {
            App.CurrentUser.isAdmin();
            InitComp();
            App.Messenger.NotifyColleagues(App.MSG_SHOW_PLANNING_TAB);
        }

        private void InitComp()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
            DataContext = this;
            DefineCoursesBinding();
            DefineCommands();
        }

        private void DefineCommands()
        {
            DefineCommandDisplayCourseDetails();
            DefineCOmmandClearAll();
            DefineCommandDisplayNewCourse();
        }

        private void DefineCommandDisplayNewCourse()
        {
            DisplayNewCourse = new RelayCommand(() => {
                App.Messenger.NotifyColleagues(App.MSG_NEW_COURSE, new Course());
            });
        }

        private void DefineCoursesBinding()
        {
            IsHasInscriptionChecked = null; //met checkbox inscription en état indéterminé
            ApplyFilter = new RelayCommand(ApplyFilterAction);
            if (App.CurrentUser.isAdmin())
            {
                Courses = new ObservableCollection<Course>(App.Model.Course);
            }
            else
            {
                Courses = GetCourseByTeacher(App.CurrentUser);
            }
            DefineBindingTeacher(); // donne une valeur à Teacher selon qu'il soit Admin ou proff
            IsAdmin = (App.CurrentUser.Role).Trim().Equals("admin");
        }

        /**
         * Si l'utilisateur courant est un proff, il ne doit avoir accès qu'aux informations le concernant.
         **/
        private void DefineBindingTeacher()
        {
            if (App.CurrentUser.isAdmin())
            {
                Teachers = MainView.GetAllTeachersAction();
            }
            else // User est un Proffesseur
            {
                //on affiche que les infos concernant le proffesseur courant
                ObservableCollection<User> singleTeacher = new ObservableCollection<User>();
                singleTeacher.Add(App.CurrentUser);
                Teachers = singleTeacher;
                TeacherComboIndex = 0;
            }
        }

        private void DefineCommandDisplayCourseDetails()
        {
            DisplayCourseDetails = new RelayCommand(() => {
                App.Messenger.NotifyColleagues(App.MSG_COURSE_DETAILS, courseSelected);
            });
        } //MEMO Je cherche à quel course cela appartient.

        private void DefineCOmmandClearAll()
        {
            ClearAll = new RelayCommand(() => {
                Filter = "";
                DateDebutSelected = "";
                DateFinSelected = ""; //Ici il y a un bug, je ne parviens pas à clearAll() date de fin
                //TeacherSelected = null;
                teacherSelected = null;
                TeacherSelected = null;

                //to reset selected value
                IsHasInscriptionChecked = null;
            });
        }

        private void ApplyFilterAction()
        {
            List<Course> tempCourses = new List<Course>(MainQueryDataGrid());
            tempCourses = RetainFilter(tempCourses);
            tempCourses = RetainTeacherSelected(tempCourses);
            tempCourses = RetainDateDebutSelected(tempCourses);
            tempCourses = RetainDateFinSelected(tempCourses);
            tempCourses = RetainHasInscriptions(tempCourses);

            Courses = new ObservableCollection<Course>(tempCourses.ToList());
        }

        private List<Course> RetainHasInscriptions(List<Course> tempCourses)
        {
            if (isHasInscriptionChecked == true)
            {
                tempCourses = tempCourses.Where(c => ((c.Students).Count > 0)).ToList();
            }
            else if (isHasInscriptionChecked == false)
            {
                tempCourses = tempCourses.Where(c => ((c.Students).Count == 0)).ToList();
            }
            return tempCourses;
        }

        private List<Course> RetainDateFinSelected(List<Course> tempCourses)
        {
            if (dateFinSelected != null && dateFinSelected.Length > 0)
            {
                tempCourses = tempCourses.Where(c => (c.FinishDate <= this.DateFin)).ToList();
                Console.WriteLine(DateFin);
            }
            return tempCourses;
        }

        private List<Course> RetainDateDebutSelected(List<Course> tempCourses)
        {
            if (dateDebutSelected != null && dateDebutSelected.Length > 0)
            {
                tempCourses = tempCourses.Where(c => (c.StartDate >= this.DateDebut)).ToList();
            }
            return tempCourses;
        }

        private List<Course> RetainFilter(List<Course> tempCourses)
        {
            if (filter != null && filter.Length > 0)
            {
                tempCourses = tempCourses.Where(c => (c.Title).ToLower().Contains(filter.Trim().ToLower())).ToList();

            }
            return tempCourses;
        }

        private List<Course> RetainTeacherSelected(List<Course> tempCourses)
        {
            if (teacherSelected != null && teacherSelected.FullName.Length > 0)
            {
                tempCourses = tempCourses.Where(c => (c.User.FullName).ToLower().Contains(TeacherSelected.FullName.Trim().ToLower())).ToList();

            }
            return tempCourses;

        }

        private List<Course> MainQueryDataGrid()
        {
            var query = from c in App.Model.Course
                        select c;

            return new List<Course>(query);
        }

        private ObservableCollection<User> GetAllTeachersAction()
        {
            var query = from u in App.Model.User
                        select u;

            return new ObservableCollection<User>(query);

        }

        private User teacherSelected;
        public User TeacherSelected
        {
            get { return teacherSelected; }
            set
            {
                if (value != null)
                {
                    teacherSelected = value;
                    ApplyFilterAction();
                    RaisePropertyChanged(nameof(TeacherSelected));
                }
            }
        }

        private Course courseSelected;
        public Course CourseSelected
        {
            get { return courseSelected; }
            set
            {
                if (value != null)
                {
                    courseSelected = value;
                    RaisePropertyChanged(nameof(CourseSelected));
                }
            }
        }


        private Boolean? isHasInscriptionChecked;
        public Boolean? IsHasInscriptionChecked
        {
            get { return isHasInscriptionChecked; }
            set
            {
                isHasInscriptionChecked = value;
                ApplyFilterAction();
                RaisePropertyChanged(nameof(IsHasInscriptionChecked));
            }
        }


        private DateTime? DateDebut { get; set; }
        private string dateDebutSelected;
        public string DateDebutSelected
        {
            get { return dateDebutSelected; }
            set
            {
                dateDebutSelected = value; // assigner ici permet de faire un test dans retainDateDebutSelected()
                DateDebut = DatePickerToDatetime(dateDebutSelected);
                ApplyFilterAction();
                RaisePropertyChanged(nameof(DateDebutSelected)); //avertit la vue (dans l'autre sens) lorsque cette propriété est modifiée
            }
        }

        private DateTime? DateFin { get; set; }
        private string dateFinSelected;
        public string DateFinSelected
        {
            get { return dateFinSelected; }
            set
            {
                dateFinSelected = value;
                DateFin = DatePickerToDatetime(dateFinSelected);
                ApplyFilterAction();
                RaisePropertyChanged(nameof(DateFinSelected));
            }
        }

        private DateTime DatePickerToDatetime(string datePickerValue)
        {
            DateTime d = new DateTime();
            try
            {
                d = DateTime.ParseExact(datePickerValue, "M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                Console.WriteLine("erreur lors de la conversion du datetime");

            }
            return d;
        }

        private string filter;
        public string Filter
        {
            get { return filter; }
            set
            {
                filter = value;
                ApplyFilterAction();
                RaisePropertyChanged(nameof(Filter));
            }
        }

        private ObservableCollection<Course> courses;
        public ObservableCollection<Course> Courses
        {
            get
            {
                return courses;
            }
            set
            {
                courses = value;
                RaisePropertyChanged(nameof(Courses));
            }
        }

        public int TeacherComboIndex { get; set; }

        private ObservableCollection<User> teachers;
        public ObservableCollection<User> Teachers
        {
            get { return teachers; }
            set
            {
                teachers = value;
                //GetAllTeachersAction();
            }
        }


        public bool IsAdmin { get; set; }

        private ObservableCollection<Course> GetCourseByTeacher(User teacher)
        {
            var query = from c in App.Model.Course
                        where c.User.Id == teacher.Id
                        select c;

            return new ObservableCollection<Course>(query);
        }

    }
}