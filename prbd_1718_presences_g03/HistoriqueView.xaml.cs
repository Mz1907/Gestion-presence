using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Input;

namespace prbd_1718_presences_g03
{
    /// <summary>
    /// Logique d'interaction pour HistoriqueView.xaml
    /// </summary>
    public partial class HistoriqueView : UserControlBase
    {
        private MainView mainView;
        private Course course;

        public HistoriqueView(MainView mainView, Course course)
        {
            this.course = course;
            this.mainView = mainView;

            InitializeComponent();
            DataTable table = BuildDataTable(course);
            HorrairesPresences = table.DefaultView;
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
            DataContext = this;
            DefineCommandDisplayCourseDetails();
        }

        public ICommand CourseOccurenceClicked { get; set; }

        //////// Définiton de la commande de création tab présence 
        //////// + résupération de la "commande paramater" (voir XAML)
        //////// on récupère courseOccurence via l'attribut Paramater dans le controle WPF
        private void DefineCommandDisplayCourseDetails()
        {
            CourseOccurenceClicked = new RelayCommand<string>((occurenceSelected) =>
            {
                DateTime occurenceDate = GetOccurenceByDate(occurenceSelected);
                Courseoccurrence clickedCo = GetCourseOccurenceByDate(occurenceDate);
                App.currentCourse = this.course;
                App.currentCo = clickedCo;
                App.Messenger.NotifyColleagues(App.MSG_SHOW_PRESENCE_FORM);
            });
        }

        private Courseoccurrence coursecourseoccurrenceSelected;
        public Courseoccurrence CoursecourseoccurrenceSelected
        {
            get { return coursecourseoccurrenceSelected; }
            set
            {
                if (value != null)
                {
                    coursecourseoccurrenceSelected = value;
                    RaisePropertyChanged(nameof(CoursecourseoccurrenceSelected));
                }
            }
        }
        ///// Fin de la création de tab PRésence

        private DataTable BuildDataTable(Course course)
        {
            var table = new DataTable();
            var columns = new Dictionary<int, int>();
            table.Columns.Add("Etudiant");
            BuildDataTable(course, table, columns);
            return table;
        }

        private DataTable BuildDataTable(Course course, DataTable table, Dictionary<int, int> columns)
        {
            int i = 1;
            foreach (var courseOccurence in course.CourseOccurrence)
            {
                table.Columns.Add(courseOccurence.ToString());
                columns[i] = courseOccurence.Id;
                ++i;
            }

            foreach (var st in course.Students)
            {
                table.Rows.Add(BuildRow(table, st, columns));
            }
            return table;
        }

        private DataRow BuildRow(DataTable table, Student st, Dictionary<int, int> columns)
        {
            DataRow row = table.NewRow();

            row[0] = st.ToString();
            for (int j = 1; j < table.Columns.Count; ++j)
            {
                row[j] = DefineCellText(getStudentPresences(st, columns, j));

            }

            return row;
        }


        private List<Presence> getStudentPresences(Student st, Dictionary<int, int> columns, int j)
        {
            int occurencesCours = columns[j];
            var presences = from pr in App.Model.Presence
                            where pr.CourseOccurence == occurencesCours
                            where pr.Student == st.Id
                            select pr;
            return new List<Presence>(presences);
        }

        private string DefineCellText(List<Presence> listePresence)
        {
            var presence = listePresence.FirstOrDefault();
            String txt = "";

            if (presence == null)
            {
                txt = "?";
            }

            if (listePresence.Count() == 1)
            {
                txt = presence.Present == 1 ? "P" : "A";
            }
            return txt;
        }


        private DataView horrairesPresences;
        public DataView HorrairesPresences
        {
            get { return horrairesPresences; }
            set
            {
                horrairesPresences = value;
                RaisePropertyChanged(nameof(HorrairesPresences));
            }
        }

        /*
         * Reçois un paramètre de type string
         **/
        private DateTime GetOccurenceByDate(string date)
        {
            string[] dateSplited = date.Split('-');
            string year = "20" + dateSplited[2];
            string month = dateSplited[1];
            string day = dateSplited[0];

            DateTime d = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(day));
            return d;
        }

        private Courseoccurrence GetCourseOccurenceByDate(DateTime dateCourse)
        {
            var query = from co in App.Model.Courseoccurrence
                        where co.Date == dateCourse
                        select co;
            return query.FirstOrDefault();
        }

    }
}