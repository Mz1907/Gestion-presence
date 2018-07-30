using System.Linq;
using System.Windows;
using System.Windows.Input;
using PRBD_Framework;

namespace prbd_1718_presences_g03
{
    /// <summary>
    /// Logique d'interaction pour LoginView.xaml
    /// </summary>
    public partial class LoginView : WindowBase
    {
        public ICommand Login { get; set; }
        public ICommand Cancel { get; set; }

        public LoginView()
        {
            InitializeComponent();

            Login = new RelayCommand(LoginAction, () => { return pseudo != null && password != null && !HasErrors; });
            Cancel = new RelayCommand(() => Close());
            DataContext = this;
        }

        private User Validation()
        {
            ClearErrors();

            var query = from u in App.Model.User
                        where u.Pseudo.Equals(Pseudo)
                        select u;
            User user = query.SingleOrDefault();

            if (string.IsNullOrEmpty(Pseudo))
            {
                AddError("Pseudo", "Requis");
            }
            if (Pseudo != null)
            {
                if (Pseudo.Length < 3)
                {
                    AddError("Pseudo", "Le pseudo doit faire au moins 3 caractères");
                }
                else if (Pseudo.Length >= 3)
                {
                    if (user == null)
                    {
                        AddError("Pseudo", "N'existe pas");
                    }
                    else
                    {
                        if (Password != null)
                        {
                            if (!(user.Password.Equals(Password)))
                            {
                                AddError("Password", "Mauvais mot de passe");
                            }
                        }
                    }
                }
            }

            RaiseErrors();

            return user;
        }

        private void LoginAction()
        {
            var user = Validation(); // on recherche le membre 
            if (!HasErrors) // si aucune erreurs
            {
                App.CurrentUser = user; //L'utilsiateur devient disponible pour toute l'application
                ShowMainView(); //va afficher AdminView ou bien TeacherView
                Close(); // fermeture de la fenêtre de login
            }
        }

        private static void ShowMainView()
        {
            var mainView = new MainView();
            mainView.Show();
            Application.Current.MainWindow = mainView; //ici on force l'affichage de la fenêtre "vue admin"
        }

        private string pseudo;
        public string Pseudo
        {
            get { return pseudo; }
            set
            {
                pseudo = value;
                Validation();
                RaisePropertyChanged(nameof(Pseudo));
            }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                Validation();
                RaisePropertyChanged(nameof(Password));
            }
        }
    }
}
