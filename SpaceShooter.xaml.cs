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
using ProjektGameKatalog;

namespace ProjektGameKatalog
{
    /// <summary>
    /// Interaktionslogik für SpaceShooter.xaml (Das Menü für das SpaceShooter-Spiel)
    /// </summary>
    public partial class SpaceShooter : Window
    {
        //Menü Musik
        MediaPlayer musik = new MediaPlayer();

        private string _loggedInUsername; // Speichert den Benutzernamen
        private string _loggedInUserID;   // Speichert die UserID

        // UserID vom Katalog-Fenster zu empfangen
        public SpaceShooter(string username, string userID)
        {
            InitializeComponent();
            _loggedInUsername = username;
            _loggedInUserID = userID; // UserID speichern

          
        }

        // für Tests ohne Login)
        // Setzt Standardwerte für den Benutzernamen und die UserID.
        public SpaceShooter()
        {
            InitializeComponent();
            _loggedInUsername = "Gast";
            _loggedInUserID = null;

            // Musik starten
            musik.Open(new Uri("C:\\Users\\Kirito Alice\\source\\repos\\ProjektGameKatalog\\ProjektGameKatalog\\Sound\\To Be Hero X Opening.mp3"));
            musik.Volume = 0.008;
            musik.Play();
        }


        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            musik.Stop();
            // Öffne das SpaceShooterSpiel und übergib den Benutzernamen und die UserID
            SpaceShooterSpiel gameWindow = new SpaceShooterSpiel(_loggedInUsername, _loggedInUserID);
            gameWindow.Show();
            this.Close();
        }


        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            musik.Stop();
            // Kehre zum Katalog-Fenster zurück und übergib den Benutzernamen und die UserID
            Katalog gameWindow = new Katalog(_loggedInUsername, _loggedInUserID);
            gameWindow.Show();
            this.Close();
        }
    }
}
