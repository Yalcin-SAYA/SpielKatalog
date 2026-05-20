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
using System.Windows.Threading;
using System.Data.SqlClient; // Hinzugefügt für SQL-Verbindung
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;
using SqlCommand = Microsoft.Data.SqlClient.SqlCommand;


namespace ProjektGameKatalog
{
    /// <summary>
    /// Interaktionslogik für PacManSpiel.xaml
    /// </summary>
    public partial class PacManSpiel : Window
    {

        DispatcherTimer gameTimer = new DispatcherTimer();

        // Steuerungs Flags für Pacman
        bool goLeft, goRight, goDown, goUp;
        bool noLeft, noRight, noDown, noUp;

        int speed = 8;
        Rect pacmanHitBox;
        int ghostSpeed = 10;
        int ghostMoveStep = 160;    // Schritte der Geister
        int currentGhostStep;       // Aktuelle Schrittanzahl für Richtungswechsel
        int score = 0;

        private string _loggedInUsername; // Speichert den Benutzernamen
        private string _loggedInUserID;   // Speichert die UserID

        // Startpositionen aus XAML
        const double pacmanStartLeft = 50;
        const double pacmanStartTop = 104;

        const double redGuyStartLeft = 173;
        const double redGuyStartTop = 29;

        const double orangeGuyStartLeft = 651;
        const double orangeGuyStartTop = 104;

        const double pinkGuyStartLeft = 173;
        const double pinkGuyStartTop = 404;

        //Benutzernamen und die UserID zu empfangen
        public PacManSpiel(string username, string userID)
        {
            InitializeComponent();
            _loggedInUsername = username;
            _loggedInUserID = userID; // UserID speichern
            GameSetUp();
        }

        public PacManSpiel() : this("Gast", null) 
        {

        }

        //Bewegung und Bildrotation
        private void CanvasKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left && !noLeft)
            {
                goLeft = true; goRight = goUp = goDown = false;
                noRight = noUp = noDown = false;
                pacman.RenderTransform = new RotateTransform(-180, pacman.Width / 2, pacman.Height / 2);
            }
            if (e.Key == Key.Right && !noRight)
            {
                goRight = true; goLeft = goUp = goDown = false;
                noLeft = noUp = noDown = false;
                pacman.RenderTransform = new RotateTransform(0, pacman.Width / 2, pacman.Height / 2);
            }
            if (e.Key == Key.Up && !noUp)
            {
                goUp = true; goLeft = goRight = goDown = false;
                noLeft = noRight = noDown = false;
                pacman.RenderTransform = new RotateTransform(-90, pacman.Width / 2, pacman.Height / 2);
            }
            if (e.Key == Key.Down && !noDown)
            {
                goDown = true; goLeft = goRight = goUp = false;
                noLeft = noRight = noUp = false;
                pacman.RenderTransform = new RotateTransform(90, pacman.Width / 2, pacman.Height / 2);
            }
        }

        // Start und Grafiken
        private void GameSetUp()
        {
            MyCanvas.Focus();
            gameTimer.Tick += GameLoop;
            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Start();

            currentGhostStep = ghostMoveStep;

            // Pacman/Geister Sprits laden
            pacman.Fill = LoadImage("pacman.jpg");
            redGuy.Fill = LoadImage("red.jpg");
            orangeGuy.Fill = LoadImage("orange.jpg");
            pinkGuy.Fill = LoadImage("pink.jpg");
        }

        // methode zum Laden von Bildern
        private ImageBrush LoadImage(string fileName)
        {
            var imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri($"C:\\Users\\Kirito Alice\\source\\repos\\ProjektGameKatalog\\ProjektGameKatalog\\Images\\{fileName}"));
            return imageBrush;
        }

        // Speil Loop
        private void GameLoop(object sender, EventArgs e)
        {
            // Punktestand aktualisieren
            txtScore.Content = "Score: " + score;

            // Pacman bewegen
            if (goRight) Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) + speed);
            if (goLeft) Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) - speed);
            if (goUp) Canvas.SetTop(pacman, Canvas.GetTop(pacman) - speed);
            if (goDown) Canvas.SetTop(pacman, Canvas.GetTop(pacman) + speed);

            // Spielfeldgrenzen prüfen
            CheckBorders();

            pacmanHitBox = new Rect(Canvas.GetLeft(pacman), Canvas.GetTop(pacman), pacman.Width, pacman.Height);

            // Kollisionen mit Wänden
            foreach (var x in MyCanvas.Children.OfType<Rectangle>())
            {
                Rect hitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                // Wände
                if ((string)x.Tag == "wall")
                {
                    CheckWallCollision(x, hitBox);
                }

                // Münzen einsammeln
                if ((string)x.Tag == "coin")
                {
                    if (pacmanHitBox.IntersectsWith(hitBox) && x.Visibility == Visibility.Visible)
                    {
                        x.Visibility = Visibility.Hidden;
                        score++;
                    }
                }

                // Geister bewegen/prüfen
                if ((string)x.Tag == "ghost")
                {
                    CheckGhostCollision(x, hitBox);
                }
            }

            // Gewinnen
            if (score == 85) // Gewinnbedingung
            {
                SaveHighscoreToDatabase(); // Highscore speichern beim Gewinn
                GameOver("Du kannst ja doch was. Bist du bereit für das nächste Level?", true); // True bedeutet Gewinn
            }
        }

        // Pacman Spielfeld nicht verlassen
        private void CheckBorders()
        {
            if (goDown && Canvas.GetTop(pacman) + 80 > Application.Current.MainWindow.Height) { noDown = true; goDown = false; }
            if (goUp && Canvas.GetTop(pacman) < 1) { noUp = true; goUp = false; }
            if (goLeft && Canvas.GetLeft(pacman) - 10 < 1) { noLeft = true; goLeft = false; }
            if (goRight && Canvas.GetLeft(pacman) + 70 > Application.Current.MainWindow.Width) { noRight = true; goRight = false; }
        }

        // Pacman gegen Wände
        private void CheckWallCollision(Rectangle wall, Rect hitBox)
        {
            if (goLeft && pacmanHitBox.IntersectsWith(hitBox)) { Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) + 10); noLeft = true; goLeft = false; }
            if (goRight && pacmanHitBox.IntersectsWith(hitBox)) { Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) - 10); noRight = true; goRight = false; }
            if (goDown && pacmanHitBox.IntersectsWith(hitBox)) { Canvas.SetTop(pacman, Canvas.GetTop(pacman) - 10); noDown = true; goDown = false; }
            if (goUp && pacmanHitBox.IntersectsWith(hitBox)) { Canvas.SetTop(pacman, Canvas.GetTop(pacman) + 10); noUp = true; goUp = false; }
        }

        // Geist Bewegung/Kollision
        private void CheckGhostCollision(Rectangle ghost, Rect hitBox)
        {
            if (pacmanHitBox.IntersectsWith(hitBox))
            {
                SaveHighscoreToDatabase(); // Highscore speichern beim Verlieren
                GameOver("Du wurdest erwischt, du Looser.", false); // False bedeutet verloren
            }

            if (ghost.Name == "orangeGuy")
                Canvas.SetLeft(ghost, Canvas.GetLeft(ghost) - ghostSpeed);
            else
                Canvas.SetLeft(ghost, Canvas.GetLeft(ghost) + ghostSpeed);

            currentGhostStep--;
            if (currentGhostStep < 1)
            {
                currentGhostStep = ghostMoveStep;
                ghostSpeed = -ghostSpeed;
            }
        }

        // Spiel beenden/neustarten/nächstes Level
        private void GameOver(string message, bool isWin)
        {
            gameTimer.Stop();

            MessageBoxResult result = MessageBox.Show(message + "\nNeustarten?", "Pacman aber fake", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (isWin)
                {
                    //nächte Level
                    PacManSpielLV2 nextLevel = new PacManSpielLV2(_loggedInUsername, _loggedInUserID);
                    nextLevel.Show();
                    this.Close();
                }
                else
                {
                    // Spiel neu starten und UserID übergeben
                    PacManSpiel gameWindow = new PacManSpiel(_loggedInUsername, _loggedInUserID);
                    gameWindow.Show();
                    this.Close();
                }
            }
            else
            {
                // Menüfenster öffnen und dieses schließen, UserID übergeben
                PacMan menuWindow = new PacMan(_loggedInUsername, _loggedInUserID);
                menuWindow.Show();
                this.Close();
            }
        }

        // ohne isWin
        private void GameOver(string message)
        {
            GameOver(message, false); // isWin = false auf
        }


        // Spiel zurücksetzen
        private void ResetGame()
        {
            // Position zurücksetzen
            Canvas.SetLeft(pacman, pacmanStartLeft);
            Canvas.SetTop(pacman, pacmanStartTop);

            Canvas.SetLeft(redGuy, redGuyStartLeft);
            Canvas.SetTop(redGuy, redGuyStartTop);

            Canvas.SetLeft(orangeGuy, orangeGuyStartLeft);
            Canvas.SetTop(orangeGuy, orangeGuyStartTop);

            Canvas.SetLeft(pinkGuy, pinkGuyStartLeft);
            Canvas.SetTop(pinkGuy, pinkGuyStartTop);

            // Steuerflags zurücksetzen
            goLeft = goRight = goUp = goDown = false;
            noLeft = noRight = noUp = noDown = false;

            // Punkte zurücksetzen
            score = 0;
            txtScore.Content = "Score: 0";

            // Coins wieder sichtbar machen
            foreach (var x in MyCanvas.Children.OfType<Rectangle>())
            {
                if ((string)x.Tag == "coin")
                {
                    x.Visibility = Visibility.Visible;
                }
            }

            // Ghostspeed und Schritte zurücksetzen
            ghostSpeed = 10;
            ghostMoveStep = 160;
            currentGhostStep = ghostMoveStep;

            MyCanvas.Focus();
            gameTimer.Start();
        }

        /// <summary>
        /// Speichert den PacMan Highscore des aktuellen Benutzers in der Datenbank.
        /// Aktualisiert nur, wenn der neue Score höher ist.
        /// </summary>
        private void SaveHighscoreToDatabase()
        {
            string userID = _loggedInUserID;
            int highscorePacManInt = score; // Der Score dieses Levels

            if (string.IsNullOrEmpty(userID))
            {
                MessageBox.Show("Fehler: Benutzer-ID nicht verfügbar. Highscore kann nicht gespeichert werden.");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(@"Server=LAPTOP-425O6GTS\SQLEXPRESS;Database=SpielKatalog;Trusted_Connection=True;"))
                {
                    connection.Open();

                    SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Highscore WHERE UserID = @UserID", connection);
                    checkCmd.Parameters.AddWithValue("@UserID", userID);
                    int exists = (int)checkCmd.ExecuteScalar();

                    SqlCommand cmd;
                    if (exists == 0)
                    {
                        // Füge einen neuen Highscore-Eintrag ein
                        cmd = new SqlCommand(@"INSERT INTO Highscore (UserID, HighscoreSpace, HighscorePacMan, GoldRPG)
                                               VALUES (@UserID, 0, @HighscorePacMan, 0)", connection);
                        cmd.Parameters.AddWithValue("@UserID", userID);
                        cmd.Parameters.AddWithValue("@HighscorePacMan", highscorePacManInt);
                    }
                    else
                    {
                        // Hole den aktuellen PacMan Highscore
                        SqlCommand getOldHighscoreCmd = new SqlCommand("SELECT HighscorePacMan FROM Highscore WHERE UserID = @UserID", connection);
                        getOldHighscoreCmd.Parameters.AddWithValue("@UserID", userID);
                        object oldHighscoreObj = getOldHighscoreCmd.ExecuteScalar();
                        int oldHighscore = (oldHighscoreObj != DBNull.Value) ? Convert.ToInt32(oldHighscoreObj) : 0;

                        // Aktualisiere nur, wenn der neue Score höher ist
                        if (highscorePacManInt > oldHighscore)
                        {
                            cmd = new SqlCommand(@"UPDATE Highscore SET HighscorePacMan = @HighscorePacMan
                                                   WHERE UserID = @UserID", connection);
                            cmd.Parameters.AddWithValue("@UserID", userID);
                            cmd.Parameters.AddWithValue("@HighscorePacMan", highscorePacManInt);
                        }
                        else
                        {
                            MessageBox.Show("PacMan Highscore wurde nicht aktualisiert, da der aktuelle Score nicht höher ist.");
                            return; // Beende die Methode hier
                        }
                    }

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("PacMan Highscore erfolgreich gespeichert/aktualisiert!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Speichern des PacMan Highscores: " + ex.Message);
            }
        }
    }
}
