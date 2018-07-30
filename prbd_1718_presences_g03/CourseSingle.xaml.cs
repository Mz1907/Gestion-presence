using System;
using System.Linq;
using System.Windows.Input;
using PRBD_Framework;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace prbd_1718_presences_g03
{
    public partial class CourseSingle : UserControlBase
    {
        public ICommand DeleteCourse { get; set; }
        public ICommand CloseThis { get; set; }

        public Course courseNew;
        private Course courseUpdate; // représente le cours dans la vue "update"
        private DateTime tempDateDebut; //utilisé pour vérifier si les dates sont modifiées lors de mise à jour de cours
        private DateTime tempDateFin;

        public Course getCourseToUpdate(Course course)
        {
            var query = from c in App.Model.Course
                        where c.Code == course.Code
                        select c;
            return query.FirstOrDefault();
        }

        public CourseSingle(MainView mainView, Course course, bool isNew)
        {
            this.IsAdmin = App.CurrentUser.isAdmin();
            if (isNew)
            {
                this.courseNew = App.Model.Course.Create();
                App.currentCourse = courseNew; // utilisé dans la commande save de mainView
                App.isNew = true;
                InitComp(course, isNew);
                CreateTabInscriptions(course, isNew);
                CreateTabHistorique(mainView, course);
                //Si CourseSingle est notofié que Mainview est en train de save le nouveau cours
                // alors CourseSingle va notifier à son tours Mainview qu'il faut fermer CourseSingle
                App.Messenger.Register(App.MSG_SAVING_NEW_COURSE, () => { App.Messenger.NotifyColleagues(App.MSG_CLOSE_TAB, Parent); });
            }
            else
            {
                this.courseUpdate = getCourseToUpdate(course);
                this.tempDateDebut = new DateTime(courseUpdate.StartDate.Year, courseUpdate.StartDate.Month, courseUpdate.StartDate.Day);
                this.tempDateFin = new DateTime(courseUpdate.FinishDate.Year, courseUpdate.FinishDate.Month, courseUpdate.FinishDate.Day);
                InitComp(course, isNew);
                CreateTabInscriptions(courseUpdate, isNew);
                CreateTabHistorique(mainView, courseUpdate);
            }

            DeleteCourse = new RelayCommand(() =>
            {
                if (this.courseNew != null)
                {
                    Console.WriteLine("Impossible supprimer cours sans utiliser delete on cascade");
                    this.MsgDelete();
                }
                else
                {
                    Console.WriteLine("Impossible supprimer cours sans utiliser delete on cascade");
                    this.MsgDelete();
                }
                App.Messenger.NotifyColleagues(App.MSG_CLOSE_TAB, Parent);
            });

            CloseThis = new RelayCommand(() => { App.Messenger.NotifyColleagues(App.MSG_CLOSE_TAB, Parent); });

            //DataTable table = BuildDataTable(course);
            //HorrairesPresences = table.DefaultView;
        }


        private void InitComp(Course course, bool isNew)
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
            DataContext = this;
            //DefineCommands(course);
            DefineComponents(course, isNew);
        }


        private void DefineComponents(Course course, bool isNew)
        {
            if (isNew == false)
            {
                DefineComponents(course);
            }
            else
            {
                App.Model.Course.Add(this.courseNew);
                DefineComponentsNewCourse(course);
                courseNew.CourseOccurrence = courseNew.computeCourseOccurences();
            }
        }


        private void DefineComponentsNewCourse(Course course)
        {
            Code = 0;
            Title = "";
            Teachers = MainView.GetAllTeachersAction();
            //Students = MainView.GetAllStudentsAction(); //DefineStudentsBindings(course); //se fait dans HistoriqueView

            DaysOfWeek = Helper.getAllStringDaysCollection();
            DateDebut = new DateTime(2017, 09, 01);
            DateFin = new DateTime(2017, 10, 01);
        }

        private void DefineComponents(Course course)
        {
            Code = course.Code;
            Title = course.Title;
            DefineTeacherBindings(course);
            DefineDatePickersBindings(course);
            //DefineStudentsBindings(course); //se fait dans HistoriqueView
        }

        private void DefineDatePickersBindings(Course course)
        {
            DaysOfWeek = Helper.getAllStringDaysCollection();
            DayOfWeekIndex = course.DayOfWeek;

            DateDebut = course.StartDate;
            DateFin = course.FinishDate;

            StartTime = course.StartTime;
            EndTime = course.EndTime;
        }

        private void DefineTeacherBindings(Course course)
        {
            //Combobox Teacher
            Teachers = MainView.GetAllTeachersAction();
            TeacherComboIndex = Teachers.IndexOf(course.User);
        }


        public int TeacherComboIndex { get; set; }
        public ObservableCollection<String> DaysOfWeek { get; set; }
        public bool IsAdmin { get; set; }

        private int dayOfWeekIndex;
        public int DayOfWeekIndex
        {
            get { return dayOfWeekIndex; }
            set
            {
                dayOfWeekIndex = value;

                if (courseNew != null)
                {
                    courseNew.DayOfWeek = value;
                }
                else if (courseUpdate != null)
                {
                    courseUpdate.DayOfWeek = value;
                }
                RaisePropertyChanged(nameof(DayOfWeekIndex));
            }
        }

        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                if (courseNew != null)
                {
                    courseNew.Title = value;
                }
                else if (this.courseUpdate != null)
                {
                    courseUpdate.Title = value;
                }

                title = value;
                ValidateTitle();
                RaisePropertyChanged(nameof(Title));
            }
        }

        private ObservableCollection<Courseoccurrence> courseOccurrence;
        public ObservableCollection<Courseoccurrence> CourseOccurrence
        {
            get { return CourseOccurrence; }
            set
            {
                if (this.DateDebut != null && this.DateFin != null && (this.DateDebut != this.DateFin))
                {
                    courseNew.CourseOccurrence = courseNew.computeCourseOccurences();
                    courseOccurrence = courseNew.computeCourseOccurences();
                    ValidateTitle();
                    //RaisePropertyChanged(nameof(CourseOccurrence));
                }
            }
        }

        private int code;
        public int Code
        {
            get { return code; }
            set
            {
                code = value;

                if (courseNew != null)
                {
                    courseNew.Code = value;
                }
                else if (courseUpdate != null)
                {
                    courseUpdate.Code = value;
                }

                RaisePropertyChanged(nameof(Code));
            }
        }


        private ObservableCollection<User> teachers;
        public ObservableCollection<User> Teachers
        {
            get { return teachers; }
            set
            {
                teachers = value;
            }
        }

        private DateTime dateDebut;
        public DateTime DateDebut
        {
            get { return dateDebut; }
            set
            {
                if (courseNew != null)
                {
                    courseNew.StartDate = value;
                }
                else if (courseUpdate != null)
                {
                    courseUpdate.StartDate = value;
                    DateChangedMofidyOccurences();

                }
                dateDebut = value;
                setCourseOccurences();
                RaisePropertyChanged(nameof(DateDebut));
                ValidateDate();

            }
        }

        private DateTime dateFin;
        public DateTime DateFin
        {
            get { return dateFin; }
            set
            {
                dateFin = value;

                if (courseNew != null)
                {
                    courseNew.FinishDate = value;
                }
                else if (courseUpdate != null)
                {
                    courseUpdate.FinishDate = value;
                    DateChangedMofidyOccurences();
                }
                setCourseOccurences();
                RaisePropertyChanged(nameof(DateFin));
                ValidateDate();

            }
        }

        private TimeSpan startTime;
        public TimeSpan StartTime
        {
            get { return startTime; }
            set
            {
                startTime = value;

                if (courseNew != null)
                {
                    courseNew.StartTime = value;
                }
                else if (courseUpdate != null)
                {
                    courseUpdate.StartTime = value;
                }
                ValidateTime();
                RaisePropertyChanged(nameof(StartTime));
            }
        }

        private TimeSpan endTime;
        public TimeSpan EndTime
        {
            get { return endTime; }
            set
            {

                if (courseNew != null)
                {
                    courseNew.EndTime = value;
                }
                else if (courseUpdate != null)
                {
                    courseUpdate.EndTime = value;
                }

                endTime = value;
                ValidateTime();
                RaisePropertyChanged(nameof(EndTime));
            }
        }

        private User teacherSelected;
        public User TeacherSelected
        {
            get { return teacherSelected; }
            set
            {
                courseNew.User = value;

                if (courseNew != null)
                {
                    courseNew.User = value;
                }
                else if (courseUpdate != null)
                {
                    courseUpdate.User = value;
                }

                teacherSelected = value;
                RaisePropertyChanged(nameof(TeacherSelected));
            }
        }

        private void ValidateTitle()
        {
            ClearErrors();

            if (string.IsNullOrEmpty(Title))
            {
                AddError("Title", "Le titre est requis");
            }

            RaiseErrors();
        }

        private void ValidateDate()
        {
            ClearErrors();
            Boolean isAssigned = (DateDebut != null) && (DateFin != null);

            if (isAssigned && DateDebut > DateFin)
            {
                AddError("DateDebut", "Doit être < = date de fin");
                AddError("DateFin", "Doit être > = date de début");
                NotifyAllFields();
            }
            else if (!HasErrors)
            {
                ClearErrors();
                if (courseNew != null)
                {
                    courseNew.StartDate = DateDebut; // la date est valide on peut la binder avec la variable newCourse
                    courseNew.FinishDate = DateFin; // la date est valide on peut la binder avec la variable newCourse
                }
                else if (this.courseUpdate != null)
                {
                    courseUpdate.StartDate = DateDebut; // la date est valide on peut la binder avec la variable newCourse
                    courseUpdate.FinishDate = DateFin; // la date est valide on peut la binder avec la variable newCourse
                }

                NotifyAllFields();
            }

            RaiseErrors();
        }

        private void ValidateTime()
        {
            ClearErrors();
            Boolean isAssigned = DateDebut != null && DateFin != null;
            if (isAssigned)
            {
                DateTime d1 = new DateTime() + StartTime;
                DateTime d2 = new DateTime() + EndTime;
                if (d1 > d2)
                {
                    AddError("StartTime", "Doit être < = heure de fin");
                    AddError("EndTime", "Doit être > = heure de début");
                }
                else if (isAssigned && !HasErrors && this.courseNew != null)
                {
                    courseNew.StartDate = DateDebut; // la date est valide on peut la binder avec la variable newCourse
                    courseNew.FinishDate = DateFin; // la date est valide on peut la binder avec la variable newCourse
                }
                else if (isAssigned && !HasErrors && this.courseUpdate != null)
                {
                    courseUpdate.StartDate = DateDebut; // la date est valide on peut la binder avec la variable newCourse
                    courseUpdate.FinishDate = DateFin; // la date est valide on peut la binder avec la variable newCourse
                }
            }

            RaiseErrors();
        }

        private void setCourseOccurences()
        {
            if (canCourseOccurences() && courseNew != null)
            {
                courseNew.CourseOccurrence = this.courseNew.computeCourseOccurences();
            }
        }

        private Boolean canCourseOccurences()
        {
            if (this.DateDebut != null && this.DateFin != null)
            {
                if (DateDebut < DateFin)
                {
                    return true;
                }
            }
            return false;
        }

        ////Création tab items DataGrid inscription ///
        private void CreateTabInscriptions(Course course, bool isNew)
        {
            TabItem tab = new TabItem()
            {
                Header = "Inscriptions",
                Content = new InscriptionsView(course, isNew)
            };

            Dispatcher.InvokeAsync(() => tab.Focus());
            UpdateLayout();

            tabControlCourseSingle.Items.Add(tab);
        }
        //////////////////////////////////////////

        private void CreateTabHistorique(MainView mainView, Course course)
        {
            TabItem tab = new TabItem()
            {
                Header = "Historique des présences",
                Content = new HistoriqueView(mainView, course)
            };

            Dispatcher.InvokeAsync(() => tab.Focus());
            UpdateLayout();

            tabControlCourseSingle.Items.Add(tab);
        }

        private void NotifyAllFields()
        {
            RaisePropertyChanged(nameof(Title));
            RaisePropertyChanged(nameof(this.DateDebut));
            RaisePropertyChanged(nameof(this.DateFin));
            RaisePropertyChanged(nameof(this.StartTime));
            RaisePropertyChanged(nameof(this.EndTime));
        }


        /**
         * Crée un message d'alerte lorsque l'on tente de désinscrie un étudiant possedant déjà des présences
         **/
        public void MsgDelete()
        {
            string message = "Note : il m'est impossible de supprimer un cours car \"cascade ondelete\" indisponible sur mon projet étant donné que ma bdd se recrée automatiquement A LA MOINDRE OPERATION CRUD EFFECTUEE . . . ";
            message += " J'ai aussi essayé de supprimer manuellement (course + presences + courseOccurence), mais impossible car les contraintes de clés étrangères empêche cela. Mon code de suppression manuelle est mis en commentaire dans CourseSingle.Xaml.cs";

            string caption = "Confirmation";

            var choix = MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (choix == MessageBoxResult.Yes)
            {
                //renvoyer vers MaineView                
            }
        }

        /**
        * Crée un message d'alerte lorsque l'on tente de mettre à jour un cours dont nous avons modifié les dates
        * 
        **/
        public void MsgUpdate()
        {
            string message = "Note : les dates du cours ont été modifiées. Ceci devrait logiquement avoir un impact sur les anciennes dates d'occurence de cours";
            message += " Mais il m'est impossible de supprimer un cours car \"cascade ondelete\" indisponible sur mon projet";
            message += " J'ai aussi essayé de supprimer manuellement (course + presences + courseOccurence), mais impossible car les contraintes de clés étrangères empêche cela. Mon code de suppression manuelle est mis en commentaire dans CourseSingle.Xaml.cs";

            string caption = "Confirmation";

            var choix = MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (choix == MessageBoxResult.Yes)
            {
                // Si user à modifier la date, il faut :
                //1. Recalculer les occurences de cours
                //2. Supprimer les anciennes cocurences et anciennes presences (impossible à faire je n'ai pas accès au delete cascade)
                courseUpdate.CourseOccurrence = courseUpdate.computeCourseOccurences(); // ré-assigne une liste de course occurences
                                                                                        //DELETE des anciennes dates occurences et des anciennes precense peut être fait uniquement avec mecanisme de ondeletecascade
                                                                                        // Je ne parviens pas à installer ce mécanisme car ma bdd se recrée à chaque opération CRUD
                this.tempDateDebut = courseUpdate.StartDate;
                this.tempDateFin = courseUpdate.FinishDate;
            }
            else
            {
                //user click sur non, il faut remettre les dates à leur valeurs initiales
                this.courseUpdate.StartDate = this.tempDateDebut;
                DateDebut = this.tempDateDebut;
                this.RaisePropertyChanged(nameof(DateDebut));
                this.courseUpdate.FinishDate = this.tempDateFin;
                DateFin = this.tempDateFin;
                this.RaisePropertyChanged(nameof(DateFin));
            }

        }

        /**
         * Vérifie dans le model si la date de la vue est différente de la dat eencodée en bdd
         * Ceci afin de voir s'il est necessaire d'afficher une fenetre d'avetissement
         */
        private bool IsDateChanged()
        {
            foreach (Course c in App.Model.Course)
            {
                if (c.Code == courseUpdate.Code)
                {
                    if (courseUpdate.StartDate != this.tempDateDebut || courseUpdate.FinishDate != this.tempDateFin)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /**
         * Scénario: user met à jour un cour et risque de modifier DateDebut ou DateFin 
         * et ceci va avoir un impact sur les courseOccurences
         * Il faut donc vérifier si user modifie les dates début/fin du cours 
         * et le cas échant créer les nouvelles occurences associées
         */
        private void DateChangedMofidyOccurences()
        {
            if (IsDateChanged())
            {
                this.MsgUpdate();
            }
        }


        /**
         * Code que j'ai utilisé pour tenter de supprimer manuellement le cours + ses presences + ses courseOccurences associées 
         * sans le mécanisme de cascade
         * Impossible de supprimer ni un cours, ni une courseOccurence, ni une presence
         * Il semble qu'il soit obligatoire d'utiliser le delete en cascade pour des raisons de sécurité impsés par SQLSERVER
         * afin de maintenir une bdd cohérente
         */

        //Récupère les présences liées à un cours avant la suppression de ce cours
        //private List<Presence> GetMappedPresences(List<Courseoccurrence> listCo)
        //{
        //    List<Presence> listPresences = new List<Presence>();
        //    foreach (Courseoccurrence co in listCo)
        //    {
        //        foreach (Presence p in co.Presence)
        //        {
        //            listPresences.Add(p);
        //        }
        //    }
        //    return listPresences;
        //}

        //private void DeletePresenceMapped(List<Presence> presencesMapped)
        //{
        //    foreach (Presence p in App.Model.Presence)
        //    {
        //        if (presencesMapped.Contains(p))
        //        {
        //            App.Model.Presence.Remove(p);
        //        }
        //    }

        //}

        //private void DeleteCourseOccurence(List<Courseoccurrence> listCo)
        //{
        //    foreach (Courseoccurrence co in App.Model.Courseoccurrence)
        //    {
        //        if (listCo.Contains(co))
        //        {
        //            App.Model.Courseoccurrence.Remove(co);
        //        }
        //    }
        //}

        //private bool DeleteCourseInModel(Course course) // note : utiisation de delete en cascade impossible chez moi car problème instalation db
        //{
        //    List<Courseoccurrence> coMapped = course.CourseOccurrence.ToList(); // récupé les co de cours, qu'il faudra supprimer
        //    //List<Presence> presencesMapped = GetMappedPresences(coMapped);

        //    //DeletePresenceMapped(presencesMapped);
        //    DeleteCourseOccurence(coMapped);
        //    //foreach (Course c in App.Model.Course)
        //    //{
        //    //    if(c.Code == course.Code)
        //    //    {
        //    //        App.Model.Course.Remove(c);
        //    //    }
        //    //}
        //    return false;
        //}

        ////////////////////////////   FIN OPERATION DE DELETE MANUELLE D'UN COURS    /////////////////////////////////////////////////////////

    }

}
