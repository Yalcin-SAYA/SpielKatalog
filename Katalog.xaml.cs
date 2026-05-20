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
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;


namespace ProjektGameKatalog
{
    public partial class Katalog : Window
    {
        private string _loggedInUsername;
        private string _loggedInUserID; // Privates Feld zum Speichern der UserID

        //für Tests ohne Login
        //Standardwerte für den Benutzernamen und die UserID.
        public Katalog()
        {
            InitializeComponent();
            _loggedInUsername = "Gast"; // Standardwert
            _loggedInUserID = null;     // Keine UserID für Gast
            UsernameLabel.Content = "Willkommen, " + _loggedInUsername;
        }

        //Benutzernamen und die UserID zu empfangen
        public Katalog(string username, string userID)
        {
            InitializeComponent();
            _loggedInUsername = username;
            _loggedInUserID = userID; // Die empfangene UserID speichern
            UsernameLabel.Content = "Willkommen, " + _loggedInUsername;
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Beim Logout einfach ein neues MainWindow öffnen.
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Spiel1_Click(object sender, RoutedEventArgs e)
        {
            SpaceShooter gameWindow = new SpaceShooter(_loggedInUsername, _loggedInUserID);
            gameWindow.Show();
            this.Close();
        }

        private void Spiel2_Click(object sender, RoutedEventArgs e)
        {
            PacMan gameWindow = new PacMan(_loggedInUsername, _loggedInUserID);
            gameWindow.Show();
            this.Close();
        }

        private void Spiel3_Click(object sender, RoutedEventArgs e)
        {
            RPGMenu gameWindow = new RPGMenu(_loggedInUsername, _loggedInUserID);
            gameWindow.Show();
            this.Close();
        }
    }
}
