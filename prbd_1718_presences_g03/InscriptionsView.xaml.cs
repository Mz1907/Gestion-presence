using PRBD_Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace prbd_1718_presences_g03
{
    public partial class InscriptionsView : UserControlBase
    {
        private Course courseNew;
        private Course courseUpdate;

        public InscriptionsView(Course course, bool isNew)
        {
            IsAdmin = App.CurrentUser.isAdmin();
            if (isNew)
            {
                courseNew = course;
                Students = MainView.GetAllStudentsAction();
            }
            else
            {
                courseUpdate = GetCourseByCode(course); //reference vers le cours dans App.model.course 
                DefineStudentsBindings(course);
            }

            InitializeComponent();
            DefineCommands(course);
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
            DataContext = this;
        }

        /**
         * Si l'utilisateur courant n'est pas admin il ne peut utiliser les buttons add / delete du datagrid inscription
         */
        public bool IsAdmin { get; set; }

        ////////////////// DEBUT commandes Datagrid inscriptions ////////////
        public ICommand AddOneStudent { get; set; }
        public ICommand AddAllStudents { get; set; }
        public ICommand DeleteOneStudent { get; set; }
        public ICommand DeleteAllStudents { get; set; }

        private void DefineCommandAddOneStudent()
        {
            AddOneStudent = new RelayCommand(() =>
            {
                if (studentSelected != null)
                {
                    StudentsSubscribed.Add(studentSelected);
                    Students.Remove(studentSelected);
                }
            });
        }

        private void DefineCommandAddAllStudents()
        {
            AddAllStudents = new RelayCommand(() =>
            {
                Students.ToList().ForEach(this.StudentsSubscribed.Add);
                Students.Clear();
            });
        }

        private void DefineCommandDeleteOneStudent()
        {
            DeleteOneStudent = new RelayCommand(() =>
            {
                if (studentSelectedSubscribed != null)
                {
                    RemoveSubscribedAction();
                }
            });
        }

        private void DefineCommandDeleteAllStudents()
        {
            DeleteAllStudents = new RelayCommand(() =>
            {
                if (StudentsSubscribed.Count > 0)
                {
                    StudentsSubscribed.ToList().ForEach(Students.Add);
                    StudentsSubscribed.Clear();
                }
            });
        }

        private ObservableCollection<Student> students;
        public ObservableCollection<Student> Students
        {
            get { return students; }
            set
            {
                students = value;
                RaisePropertyChanged(nameof(Students));
                //GetAllTeachersAction();
            }
        }

        public ObservableCollection<Student> studentsSubscribed = new ObservableCollection<Student>();
        public ObservableCollection<Student> StudentsSubscribed
        {
            get { return studentsSubscribed; }
            set
            {
                //courseNew.Students = value;
                studentsSubscribed = value;
            }
        }

        private Student studentSelected;
        public Student StudentSelected
        {
            get { return studentSelected; }
            set
            {
                studentSelected = value;
            }
        }

        public ObservableCollection<Student> studentSubscribed;
        public ObservableCollection<Student> StudentSubscribed
        {
            get { return studentSubscribed; }
            set
            {
                studentSubscribed = value;
                RaisePropertyChanged(nameof(StudentSubscribed));
            }
        }

        private Student studentSelectedSubscribed;
        public Student StudentSelectedSubscribed
        {
            get
            {
                if (courseNew != null)
                {
                    courseNew.Students = studentsSubscribed;
                }
                else if (courseUpdate != null)
                {
                    courseUpdate.Students = studentsSubscribed;
                }
                return studentSelectedSubscribed;
            }
            set
            {
                studentSelectedSubscribed = value;
            }
        }

        public void RemoveSubscribedHasPresence()
        {
            Students.Add(studentSelectedSubscribed);
            StudentsSubscribed.Remove(studentSelectedSubscribed);
            courseUpdate.Students = StudentsSubscribed;
        }

        public void RemoveSubscribedAction()
        {
            bool hasPresences = checkPresenceBeforeDelete(courseUpdate, studentSelectedSubscribed);
            if (hasPresences == false)
            {
                RemoveSubscribedHasPresence();
            }
            else
            {
                MsgConfirmUpdate();
            }
        }

        /**
         * Crée un message d'alerte lorsque l'on tente de désinscrie un étudiant possedant déjà des présences
         **/
        public void MsgConfirmUpdate()
        {
            const string message = "Attention ce changement aura un impact sur les occurences du cours et sur les éventuels présences déjà encodées. Confirmez-vous le changment ?";
            const string caption = "Confirmation";
            var choix = MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (choix == MessageBoxResult.Yes)
            {
                Students.Add(StudentSelectedSubscribed);
                StudentsSubscribed.Remove(StudentSelectedSubscribed);
                courseUpdate.Students = StudentsSubscribed;
            }

        }

        private void DefineStudentsBindings(Course course)
        {
            Students = MainView.GetAllStudentButCurrentCoursesAction((course.Students).ToList());
            StudentsSubscribed = new ObservableCollection<Student>(course.Students);
        }

        /**
         * Vérifie si un étudiant possède au moins une présence avant de le supprimer du datagrid "étudiants inscrits"
        **/
        private bool checkHasPresenceBeforeUnselect(List<Courseoccurrence> listCo, Student student)
        {
            bool hasPresences = false;
            foreach (Courseoccurrence co in listCo)
            {
                hasPresences = checkHasPresenceBeforeUnselect((co.Presence).ToList(), student);
                if (hasPresences == true)
                {
                    return true;
                }
            }
            return false;
        }

        private bool checkHasPresenceBeforeUnselect(List<Presence> listPresences, Student student)
        {
            foreach (Presence p in listPresences)
            {
                if (p.Student1.Equals(student))
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * Vérifie si un étudiant possède au moins une présence avant de le supprimer du datagrid "étudiants inscrits"
         **/
        private bool checkPresenceBeforeDelete(Course course, Student student)
        {
            List<Courseoccurrence> listCo = getOccurencesBycourse(course, student);
            return checkHasPresenceBeforeUnselect(listCo, student);
        }

        /**
         * Récupère la liste de toutes les présences
         **/
        private List<Courseoccurrence> getOccurencesBycourse(Course course, Student student)
        {
            var query = from co in App.Model.Courseoccurrence
                        where co.Course.Code == course.Code
                        select co;
            return new List<Courseoccurrence>(query);
        }

        private void DefineCommands(Course course)
        {
            DefineCommandAddOneStudent();
            DefineCommandAddAllStudents();
            DefineCommandDeleteOneStudent();
            DefineCommandDeleteAllStudents();
        }

        ////////////////// Fin commandes Datagrid inscriptions ////////////

        /**
        * Récupère la liste de toutes les présences
        **/
        private Course GetCourseByCode(Course course)
        {
            var query = from c in App.Model.Course
                        where c.Code == course.Code
                        select c;
            return query.FirstOrDefault();
        }

    }
}
