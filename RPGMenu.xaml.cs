using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace ProjektGameKatalog
{
    /// <summary>
    /// Interaktionslogik für RPGMenu.xaml
    /// </summary>
    public partial class RPGMenu : Window
    {
        private string _loggedInUsername; // Speichert den Benutzernamen
        private string _loggedInUserID;   // Speichert die UserID

        // Standardkonstruktor
        //public RPGMenu()
        //{
        //    InitializeComponent();
        //    _loggedInUsername = "Gast";
        //    _loggedInUserID = null;
        //}

        // Benutzernamen und die UserID zu empfangen
        public RPGMenu(string username, string userID)
        {
            InitializeComponent();
            _loggedInUsername = username;
            _loggedInUserID = userID; // UserID speichern
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            // Öffne das RPG und übergib den Benutzernamen und die UserID
            RPG gameWindow = new RPG(_loggedInUsername, _loggedInUserID);
            gameWindow.Show();
            this.Close();
        }


        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            // Kehre zum Katalog-Fenster zurück und übergib den Benutzernamen und die UserID
            Katalog gameWindow = new Katalog(_loggedInUsername, _loggedInUserID);
            gameWindow.Show();
            this.Close();
        }
    }
}
