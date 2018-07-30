using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace prbd_1718_presences_g03
{
    /// <summary>
    /// Logique d'interaction pour PresenceView.xaml
    /// </summary>
    public partial class PresenceView : UserControlBase
    {

        private Courseoccurrence courseOccurrence { get; set; }

        public PresenceView(Course course, Courseoccurrence courseOccurrence)
        {
            this.courseOccurrence = courseOccurrence;
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
            DataContext = this;

            StudentsSubscribedInCourse = course.Students.ToList();
            PresencesInOccurence = courseOccurrence.Presence.ToList();
            StAndPresence = BuildStudentAndPresence(courseOccurrence);
            //commande de sauvegarde
            Save = new RelayCommand(SaveAction);
        }

        /**
         * Construit une collection observable de StudentAndPresence (ceci est une classe interne) 
         * pour la ListeView de la vue PresenceView.XAML
         **/
        private ObservableCollection<StudentAndPresence> BuildStudentAndPresence(Courseoccurrence courseOccurrence)
        {
            ObservableCollection<StudentAndPresence> res = new ObservableCollection<StudentAndPresence>();

            foreach (Student st in StudentsSubscribedInCourse)
            {
                //pour chaque étudiant on regarde s'il possède une présence
                Presence presenceTemp = new Presence(st.Id, courseOccurrence.Id, SetStudentPresent(st));
                StudentAndPresence studentAndPresence = new StudentAndPresence(st, presenceTemp);
                res.Add(studentAndPresence);
            }
            return res;
        }

        /**
         * si un étudiant existe dans la liste des presence, il est présent. 
         * S'il n'existe même pas c'est qu'il est absent.
         */
        private short SetStudentPresent(Student st)
        {
            foreach (Presence p in App.Model.Presence)
            {
                if (p.Student1.Id == st.Id && p.Present == 1 && p.CourseOccurence == this.courseOccurrence.Id)
                {
                    return 1;
                }
            }
            return 0;
        }

        public List<Student> StudentsSubscribedInCourse { get; set; }
        public List<Presence> PresencesInOccurence { get; set; } // presences d'étudiants comprises dans ce courseOccurrence

        private ObservableCollection<StudentAndPresence> stAndPresence;
        public ObservableCollection<StudentAndPresence> StAndPresence
        {
            get
            {
                return stAndPresence;
            }
            set
            {
                stAndPresence = value;
                RaisePropertyChanged(nameof(StAndPresence));
            }
        }

        //pour debug
        private void AfficherStudentAndPresence()
        {
            foreach (StudentAndPresence elem in StAndPresence)
            {
                Console.WriteLine("debug StAndPresence en tant que propriété ligne 109");
                Console.WriteLine(elem);
            }
        }

        ///////// Fonctions liées à la commande save 

        public ICommand Save { get; set; }
        private void SaveAction()
        {
            ModifyPresenceInModel();

        }

        public void ModifyPresenceInModel()
        {
            foreach (StudentAndPresence observableStudentAndPresence in StAndPresence)
            {
                ModifyPresenceInModel(observableStudentAndPresence);
            }
        }

        public void ModifyPresenceInModel(StudentAndPresence studentAndPresence)
        {
            var presenceEventuelle = GetPresenceFromModel(studentAndPresence.InternePresence);
            // Test si une presence est déjà encodée
            // Si c'est la cas : 
            // il faut mettre à jour la présence encodée (type short) avec la nouvelle valeure (0 ou 1)
            if (presenceEventuelle != null && presenceEventuelle.Present != studentAndPresence.PresenceToShort())
            {
                foreach (Presence presenceModel in App.Model.Presence)
                {
                    if (presenceModel.CourseOccurence == studentAndPresence.InternePresence.CourseOccurence && presenceModel.Student == studentAndPresence.InterneStudent.Id)
                    {
                        presenceModel.Present = studentAndPresence.PresenceToShort();
                    }
                }
                App.Model.SaveChanges();
                AfficherPresenceDeModel();
            }
            else if (presenceEventuelle == null && studentAndPresence.PresenceToShort() == 1)
            {
                //si ce n'est pas le cas, c'est la 1er fois que cet étudiant est présent pour cette CO
                //Première fois que cet étudiant est coché présent, il faut insérer une nouvelle entrée s'il est présent uniquement
                Presence presenceNew = App.Model.Presence.Create();

                presenceNew.Student = studentAndPresence.InterneStudent.Id;
                presenceNew.CourseOccurence = studentAndPresence.InternePresence.CourseOccurence;
                presenceNew.Present = 1;
                App.Model.Presence.Add(presenceNew);
                App.Model.SaveChanges();
                AfficherPresenceDeModel();
            }
        }

        //pour debug
        private void AfficherPresenceDeModel()
        {
            foreach (Presence p in App.Model.Presence)
            {
                if (p.CourseOccurence == this.courseOccurrence.Id)
                {
                    Console.WriteLine(p.Student1 + " est présent ?  " + p.Present + "(ClasseexterneDebug)");
                }
            }
        }

        public Presence GetPresenceFromModel(Presence InternePresence)
        {
            var query = from pr in App.Model.Presence
                        where pr.Student == InternePresence.Student
                        where pr.CourseOccurence == InternePresence.CourseOccurence // comparaison de int
                        select pr;
            return query.FirstOrDefault();
        }


        ///////// FIN Fonctions liées à la commande save

        /**
        * Classe interne 
        * Utilisée pour representer (un étudiant et sa présence) dans la ListeView du fichier Presenceview.XAML
        **/
        public class StudentAndPresence : UserControlBase
        {
            public int InterneGroup { get; set; }
            public Student InterneStudent { get; set; }
            public Presence InternePresence { get; set; }

            private bool? isPresent;
            public bool? IsPresent
            {
                get
                {
                    return isPresent;
                }
                set
                {
                    isPresent = value;
                    isAbsent = !isPresent;
                    RaisePropertyChanged(nameof(IsPresent));
                }
            }

            private bool? isAbsent;
            public bool? IsAbsent
            {
                get
                {
                    return isAbsent;
                }
                set
                {
                    isAbsent = value;
                    isPresent = !isAbsent;
                    RaisePropertyChanged(nameof(IsAbsent));
                }
            }

            public StudentAndPresence(Student student, Presence presence)
            {
                InterneGroup = student.Id;
                InterneStudent = student;
                InternePresence = presence;
                IsPresent = (presence.Present == 1) ? true : false;
                IsAbsent = !IsPresent;
            }

            public override String ToString() // pour debug
            {
                return this.InterneStudent.LastName + " " + InterneStudent.FirstName + " est present ?  " + IsPresent + "(ClasseinterneDebug)";
            }

            public short PresenceToShort()
            {
                int res = (IsPresent == true) ? 1 : 0;
                return (short)res;
            }
        }

    }
}
