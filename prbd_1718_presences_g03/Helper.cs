using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prbd_1718_presences_g03
{
    /**
     * Classe de Helper offrant quelques méthodes statiques utiles
     */
    class Helper
    {
        public static ObservableCollection<String> getAllStringDaysCollection()
        {
            String[] arrDays = { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi", "Dimanche" };
            var days = new ObservableCollection<string>(new List<String>(arrDays));
            return days;
        }


        public static String getDayToString(int dayNum)
        {
            List<String> l = new List<String>(getAllStringDaysCollection());
            return l[dayNum];
        }

        public enum Jours { Dimanche, Lundi, Mardi, Mercredi, Jeudi, Vendredi, Samedi }

        /*
         * Renvoi un jour de la semaine en français 
         */
        public static String GetDayOfWeekFr(int dayOfWeekInt)
        {
            Jours jourByIndex = (Jours)dayOfWeekInt;
            Console.WriteLine(" je suis dans res et voici ce que cela renvoi " + jourByIndex.ToString());
            return jourByIndex.ToString();
        }

        /**
         * Trouve le lundi de la semaine. 
         * Utilisé quand on clique sur un des boutons du planning entourant le datePicker
         * Note: lundi = 1
         */
        public static DateTime FindMonday(DateTime planingDateSelected)
        {
            int dayOfweek = (int)planingDateSelected.DayOfWeek;
            if (dayOfweek == 0) // si on est dimanche on met le lundi suivant
            {
                planingDateSelected = planingDateSelected.AddDays(1);
            }
            else if (dayOfweek == 6) // si on est samedi on met le lundi suivant
            {
                planingDateSelected = planingDateSelected.AddDays(2);
            }
            else if (dayOfweek > 1) // si on est un jour de la semaine, on met le lundi de la SEMAINE EN COURS 
            {
                planingDateSelected = planingDateSelected.AddDays(1 - dayOfweek);
            }
            return planingDateSelected;
        }

        /*
         * Même principe que FindMonday() mais pour chercher le vendredi
         * Note: Vendredi = 5
         */
        public static DateTime FindFriday(DateTime planingDateSelected)
        {
            int dayOfweek = (int)planingDateSelected.DayOfWeek;

            if(dayOfweek == 0)
            {
                planingDateSelected = planingDateSelected.AddDays(5);
            }

            else if (dayOfweek == 6) // si on est samedi, il faut metttre la date à vendredi de la semaine prochaine
            {
                planingDateSelected = planingDateSelected.AddDays(6);
            }

            else if (dayOfweek < 5) // si on est de lundi à jeudi 
            {
                planingDateSelected = planingDateSelected.AddDays(5 - dayOfweek);
            }

            return planingDateSelected;
        }
    }
}
