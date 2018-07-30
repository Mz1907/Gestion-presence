using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace prbd_1718_presences_g03
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Entities Model { get; } = new Entities();
        public const string MSG_COURSE_DETAILS = "MSG_NEW_COURSE";
        public const string MSG_NEW_COURSE = "MSG_NOUVEAU_COURS";
        public const string MSG_SHOW_PRESENCE_FORM = "MSG_SHOW_PRESENCE_FORM";
        public const string MSG_SHOW_PLANNING_TAB = "MSG_SHOW_PLANNING_TAB";
        public const string MSG_DELETE_SINGLE_COURSE = "MSG_DELETE_SINGLE_COURSE";
        public const string MSG_CLOSE_TAB = "MSG_CLOSE_TAB";
        public const string MSG_SAVING_NEW_COURSE = "MSG_SAVING_NEW_COURS"; // utilisé dans courseSingle pour fermer courseSingle

        //public const string MSG_DATAGRIS_INSCRIPTION = "MSG_DATAGRID_INSCRIPTION";
        public static Messenger Messenger { get; } = new Messenger();
        public static User CurrentUser { get; set; } // pour login

        //Utilisé lors de la création de l'onglet des présences visible par le proff uniquement
        public static Course currentCourse { get; set; }
        public static Courseoccurrence currentCo { get; set; }
        public static bool isNew { get; set; }

        public App()
        {
            ColdStart();
        }

        private void ColdStart()
        {
            Model.User.Find(1);
        }

        private void PrepareDatabase()
        {
            // Donne une valeur à la propriété "DataProperty" qui est utilisée comme dossier de base dans App.config pour
            // la connection string vers la DB. Cette valeur est calculée en chemin relatif à partir du dossier de 
            // l'exécutable, soit <dossier projet>/bin/Debug.
            var projectPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\"));
            var dbPath = Path.GetFullPath(Path.Combine(projectPath, "database"));
            Console.WriteLine("Database path: " + dbPath);
            AppDomain.CurrentDomain.SetData("DataDirectory", projectPath);

            // Si la base de données n'existe pas, la créer en exécutant le script SQL
            if (!File.Exists(Path.Combine(dbPath, @"prbd_1718_presences_g03.mdf")))
            {
                Console.WriteLine("Creating database...");
                string script = File.ReadAllText(Path.Combine(dbPath, @"prbd_1718_presences_g03.sql"));

                // dans le script, on remplace "{DBPATH}" par le dossier où on veut créer la DB
                script = script.Replace("{DBPATH}", dbPath);

                // On splitte le contenu du script en une liste de strings, chacune contenant une commande SQL.
                // Pour faire le split, on se sert des commandes "GO" comme délimiteur.
                IEnumerable<string> commandStrings = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

                // On se connecte au driver de base de données "(localdb)\MSSQLLocalDB" qui permet de travailler avec des
                // fichiers de données SQL Server attachés sans nécessiter qu'une instance de SQL Server ne soit présente.
                string sqlConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=True";
                SqlConnection connection = new SqlConnection(sqlConnectionString);
                connection.Open();
                // On exécute les commandes SQL une par une.
                foreach (string commandString in commandStrings)
                    if (commandString.Trim() != "")
                        using (var command = new SqlCommand(commandString, connection))
                            command.ExecuteNonQuery();
                connection.Close();
            }
        }

    }
}