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
    /// Interaktionslogik für PacMan.xaml (Das Menü für das PacMan-Spiel)
    /// </summary>
    public partial class PacMan : Window
    {
        private string _loggedInUsername; // Speichert den Benutzernamen
        private string _loggedInUserID;   // Speichert die UserID

        // Standardkonstruktor
        public PacMan()
        {
            InitializeComponent();
            _loggedInUsername = "Gast";
            _loggedInUserID = null;
        }

        // Benutzernamen und die UserID zu empfangen
        public PacMan(string username, string userID)
        {
            InitializeComponent();
            _loggedInUsername = username;
            _loggedInUserID = userID; // UserID speichern
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            // Öffne das PacManSpiel und übergib den Benutzernamen und die UserID
            PacManSpiel gameWindow = new PacManSpiel(_loggedInUsername, _loggedInUserID);
            gameWindow.Show();
            this.Close(); // Menü-Fenster schließen
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            //Benutzernamen und die UserID
            Katalog gameWindow = new Katalog(_loggedInUsername, _loggedInUserID);
            gameWindow.Show();
            this.Close(); // Menü-Fenster schließen
        }
    }
}
