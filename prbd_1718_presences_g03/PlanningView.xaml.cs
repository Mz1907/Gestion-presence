using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace prbd_1718_presences_g03
{
    /// <summary>
    /// Logique d'interaction pour PlanningView.xaml
    /// </summary>
    public partial class PlanningView : UserControlBase
    {
        public ICommand WeekLast { get; set; }
        public ICommand WeekNext { get; set; }
        public ICommand ShowPresences { get; set; }


        public PlanningView()
        {
            Initcomp();
            DatePlanningSelected = DateTime.Now;
            WeekLast = new RelayCommand(ComputeLastWeek);
            WeekNext = new RelayCommand(ComputeNextWeek);
            ShowPresences = new RelayCommand<PlanningBuilder>(plBuilderSelected => AfficherPresenceDoubleClicked(plBuilderSelected));
            Plannings = BuildPlanningsBuilder(Helper.FindMonday(DateTime.Now), Helper.FindFriday(DateTime.Now));
        }

        public void Initcomp()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
            DataContext = this;
        }

        /**
         * Renvoie la première collection de ObservableCollection<PlanningListView>
         */
        public ObservableCollection<PlanningBuilder> BuildPlanningsBuilder(DateTime mondayDate, DateTime fridayDate)
        {
            ObservableCollection<PlanningBuilder> res = new ObservableCollection<PlanningBuilder>();
            List<Courseoccurrence> CoursesOccurences = GetCoInInterval(mondayDate, fridayDate);

            int cptdays = (int)(fridayDate - mondayDate).TotalDays; // 

            for (int i = 0; i <= cptdays; ++i) // pour chaque jour de la semaine nous devons créer un PlanningListView
            {
                DateTime dateduJour = mondayDate.AddDays(i);
                PlanningBuilder pl = new PlanningBuilder(dateduJour);
                res.Add(pl);
            }
            return res;
        }

        private List<Courseoccurrence> GetCoInInterval(DateTime d1, DateTime d2)
        {
            var query = from co in App.Model.Courseoccurrence
                            //where (co.Course.User.Id == App.CurrentUser.Id) TOTO ceci doit être implémenté
                        where (co.Date.CompareTo(d1) >= 0)
                        where (co.Date.CompareTo(d2) <= 0)
                        select co;

            var listForDebug = new ObservableCollection<Courseoccurrence>(query).ToList();
            return new List<Courseoccurrence>(query);
        }



        public void AfficherPresenceDoubleClicked(PlanningBuilder pl)
        {
            if (pl.ListCo.FirstOrDefault() != null)
            {
                App.currentCo = pl.ListCo.FirstOrDefault();
                App.currentCourse = pl.ListCo.First().Course;
                Console.WriteLine(pl);
                App.Messenger.NotifyColleagues(App.MSG_SHOW_PRESENCE_FORM);
            }
        }


        private void ComputeLastWeek()
        {
            DatePlanningSelected = Helper.FindMonday(DatePlanningSelected.AddDays(-7));
            Plannings = BuildPlanningsBuilder(Helper.FindMonday(DatePlanningSelected), Helper.FindFriday(DatePlanningSelected));
        }

        private void ComputeNextWeek()
        {
            DatePlanningSelected = Helper.FindMonday(DatePlanningSelected.AddDays(7));
            Plannings = BuildPlanningsBuilder(Helper.FindMonday(DatePlanningSelected), Helper.FindFriday(DatePlanningSelected));
        }


        private ObservableCollection<PlanningBuilder> plannings;
        public ObservableCollection<PlanningBuilder> Plannings
        {
            get { return plannings; }
            set
            {
                plannings = value;
                RaisePropertyChanged(nameof(Plannings));
            }
        }

        private ObservableCollection<Course> courses;
        public ObservableCollection<Course> Courses
        {
            get { return courses; }
            set
            {
                courses = value;
                RaisePropertyChanged(nameof(Courses));
            }
        }


        private DateTime datePlanningSelected;
        public DateTime DatePlanningSelected
        {
            get { return datePlanningSelected; }
            set
            {
                datePlanningSelected = value;
                RaisePropertyChanged(nameof(DatePlanningSelected));
            }
        }

        private PlanningBuilder planningBuilderSelected;
        public PlanningBuilder PlanningBuilderSelected
        {
            get { return planningBuilderSelected; }
            set
            {
                planningBuilderSelected = value;
                RaisePropertyChanged(nameof(PlanningBuilderSelected));
            }
        }


        /*
         * Classe imbriquée faciitant la construction d'une collection observable
         */
        public class PlanningBuilder : UserControlBase
        {
            public String Jour { get; set; }
            public List<Courseoccurrence> ListCo { get; set; }
            public DateTime DateCourse { get; set; } // utilise pour la commande show presences lorsque l'on double click sur un planning
            /**
             * d1 : début de semaine
             * d2 : fin de semaine
             */
            public PlanningBuilder(DateTime dateDuJour)
            {
                DateCourse = dateDuJour;
                Jour = ConvertToJour(dateDuJour);
                //1. Get tous les occurences de cours entre ces 2 dates
                this.ListCo = GetCourseOccurences1Jour(dateDuJour);
            }

            private String ConvertToJour(DateTime dateDuJour)
            {
                int dayOfWeek = (int)(dateDuJour.DayOfWeek);
                String res = Helper.GetDayOfWeekFr(dayOfWeek);
                return res;
            }


            /**
             * @param DateTime dateDuJour : la date du jour dont il faut afficher la (les) ligne(s)
             * ex: lundi 
             *           08:00 - 11:00 PRM2: principe algo
             *           13:00 - 17:00 BDGE             * 
             */
            public String BuildLabelOneDay()
            {
                StringBuilder res = new StringBuilder(Jour);
                res.AppendLine();

                if (ListCo.Count() > 0)
                {
                    res.AppendLine();

                    foreach (Courseoccurrence co in ListCo)
                    {
                        res.AppendLine(BuildLabelFromCourseInfo(co.Course));
                    }
                }
                return res.ToString();
            }

            private String BuildLabelFromCourseInfo(Course course)
            {
                String label = "     " + String.Format("{0:hh}:{0:mm}", course.StartTime);
                label += " - " + String.Format("{0:hh}:{0:mm}", course.EndTime);
                label += " : " + course.Title;
                return label;
            }

            /**
             * Retourne une liste observable de course occurences
             * ayant lieu entre 2 dates et étant associées à un professeur
             */
            private List<Courseoccurrence> GetCourseOccurences1Jour(DateTime dateDuJour)
            {
                var query = from co in App.Model.Courseoccurrence
                            where (co.Course.User.Id == App.CurrentUser.Id)
                            where (co.Date == dateDuJour.Date)
                            select co;

                var listForDebug = new ObservableCollection<Courseoccurrence>(query).ToList();
                return new List<Courseoccurrence>(query);
            }

            public override string ToString()
            {
                return BuildLabelOneDay();
            }

        }

    }
}
