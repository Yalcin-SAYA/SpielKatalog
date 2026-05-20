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
using Microsoft.Data.SqlClient;
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;
using SqlCommand = Microsoft.Data.SqlClient.SqlCommand; // Hinzugefügt für SQL-Verbindung

namespace ProjektGameKatalog
{
    /// <summary>
    /// Interaktionslogik für PacManSpielLV2.xaml
    /// </summary>
    public partial class PacManSpielLV2 : Window
    {
        DispatcherTimer gameTimer = new DispatcherTimer();

        // Steuerungs Flags Pacman
        bool goLeft, goRight, goDown, goUp;
        bool noLeft, noRight, noDown, noUp;

        int speed = 8;
        Rect pacmanHitBox;

        int score = 0;

        private string _loggedInUsername; // Speichert den Benutzernamen
        private string _loggedInUserID;   // Speichert die UserID

        // Startpositionen Pacman
        const double pacmanStartLeft = 25;
        const double pacmanStartTop = 104;

        // Schrittanzahl für Geisterbewegung
        const int ghostMoveStep = 160;

        int orangeGuySpeed = 10;
        int redGuySpeed = 10;
        int pinkGuySpeed = 10;
        int greenGuySpeed = 10;
        int blueGuySpeed = 10;
        int aquaGuySpeed = 10;
        int tanGuySpeed = 10;
        int hotpinkGuySpeed = 10;
        int greyGuySpeed = 10;
        int violetGuySpeed = 10;

        // Schrittzähler für jeden Geist (für die Bewegung in CheckGhostCollision)
        int orangeGuySteps = ghostMoveStep;
        int redGuySteps = ghostMoveStep;
        int pinkGuySteps = ghostMoveStep;
        int greenGuySteps = ghostMoveStep;
        int blueGuySteps = ghostMoveStep;
        int aquaGuySteps = ghostMoveStep;
        int tanGuySteps = ghostMoveStep;
        int hotpinkGuySteps = ghostMoveStep;
        int greyGuySteps = ghostMoveStep;
        int violetGuySteps = ghostMoveStep;




        // Benutzernamen und die UserID zu empfangen
        public PacManSpielLV2(string username, string userID)
        {
            InitializeComponent();
            _loggedInUsername = username;
            _loggedInUserID = userID; // UserID speichern
            GameSetUp();
        }

        // für Tests ohne Login
        public PacManSpielLV2() : this("Gast", null) // mit Standardwerten aufrufen
        {
            // Initialisierung, falls nötig
        }


        private void CanvasKeyDown(object sender, KeyEventArgs e)
        {
            MyCanvas.Focus();

            // Bewegung & Bildrotation
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

        private void GameSetUp()
        {
            MyCanvas.Focus();
            gameTimer.Tick += GameLoop;
            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Start();

            // Initialisiere die Schrittzähler für alle Geister
            orangeGuySteps = ghostMoveStep;
            redGuySteps = ghostMoveStep;
            pinkGuySteps = ghostMoveStep;
            greenGuySteps = ghostMoveStep;
            blueGuySteps = ghostMoveStep;
            aquaGuySteps = ghostMoveStep;
            tanGuySteps = ghostMoveStep;
            hotpinkGuySteps = ghostMoveStep;
            greyGuySteps = ghostMoveStep;
            violetGuySteps = ghostMoveStep;


            pacman.Fill = LoadImage("pacman.jpg");
            redGuy.Fill = LoadImage("red.jpg");
            orangeGuy.Fill = LoadImage("orange.jpg");
            pinkGuy.Fill = LoadImage("pink.jpg");
            greyGuy.Fill = LoadImage("red.jpg");
            violetGuy.Fill = LoadImage("orange.jpg");
            aquaGuy.Fill = LoadImage("pink.jpg");
            tanGuy.Fill = LoadImage("red.jpg");
            hotpinkGuy.Fill = LoadImage("red.jpg");
            greenGuy.Fill = LoadImage("pink.jpg");
        }

        private ImageBrush LoadImage(string fileName)
        {
            var imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri($"\\Images\\{fileName}"));
            return imageBrush;
        }

        private void GameLoop(object sender, EventArgs e)
        {
            txtScore.Content = "Score: " + score;

            // Pacman Bewegung
            if (goRight) Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) + speed);
            if (goLeft) Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) - speed);
            if (goUp) Canvas.SetTop(pacman, Canvas.GetTop(pacman) - speed);
            if (goDown) Canvas.SetTop(pacman, Canvas.GetTop(pacman) + speed);

            // Überprüft, ob Pacman die Ränder des Canvas erreicht
            CheckBorders();

            // Aktualisiert Hitbox von Pacman.
            pacmanHitBox = new Rect(Canvas.GetLeft(pacman), Canvas.GetTop(pacman), pacman.Width, pacman.Height);

            // Zentriert den ScrollViewer auf Pacman und seine größe/weite
            double desiredScrollX = Canvas.GetLeft(pacman) + (pacman.Width / 2) - (gameScrollViewer.ActualWidth / 2);
            double desiredScrollY = Canvas.GetTop(pacman) + (pacman.Height / 2) - (gameScrollViewer.ActualHeight / 2);

            //Scroll-Positionen innerhalb der gültigen Grenzen(kann nicht negativ sein
            desiredScrollX = Math.Max(0, Math.Min(desiredScrollX, MyCanvas.Width - gameScrollViewer.ActualWidth));
            desiredScrollY = Math.Max(0, Math.Min(desiredScrollY, MyCanvas.Height - gameScrollViewer.ActualHeight));

            //Scroll Positionen des ScrollViewers.
            gameScrollViewer.ScrollToHorizontalOffset(desiredScrollX);
            gameScrollViewer.ScrollToVerticalOffset(desiredScrollY);



            //Kollisionen
            foreach (var x in MyCanvas.Children.OfType<Rectangle>())
            {
                Rect hitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                if ((string)x.Tag == "wall")
                {
                    CheckWallCollision(x, hitBox);
                }

                if ((string)x.Tag == "coin")
                {
                    //Münzen sammeln
                    if (pacmanHitBox.IntersectsWith(hitBox) && x.Visibility == Visibility.Visible)
                    {
                        x.Visibility = Visibility.Hidden;
                        score++;
                    }
                }

                if ((string)x.Tag == "ghost")
                {
                    CheckGhostCollision(x, hitBox);
                }
            }

            // Alle Münzen gesammelt!
            if (score == 164) // Passen Sie die Gewinnbedingung an die Anzahl der Münzen in LV2 an
            {
                SaveHighscoreToDatabase(); // Highscore speichern beim Gewinn
                GameOver("Du kannst ja doch was.", true); // True bedeutet Gewinn
            }
        }



        private void CheckBorders()
        {

            double pacmanLeft = Canvas.GetLeft(pacman);
            double pacmanTop = Canvas.GetTop(pacman);
            double canvasWidth = MyCanvas.Width;
            double canvasHeight = MyCanvas.Height;

            // Überprüfen der unteren Grenze
            if (goDown && pacmanTop + pacman.Height + speed > canvasHeight)
            {
                noDown = true;
                goDown = false;
                Canvas.SetTop(pacman, canvasHeight - pacman.Height); // Pacman an den Rand setzen
            }
            else { noDown = false; } // Reset noDown if not at border

            // Überprüfen der oberen Grenze
            if (goUp && pacmanTop - speed < 0)
            {
                noUp = true;
                goUp = false;
                Canvas.SetTop(pacman, 0);
            }
            else { noUp = false; } // Reset wenn am rand

            // Überprüfen der linken Grenze
            if (goLeft && pacmanLeft - speed < 0)
            {
                noLeft = true;
                goLeft = false;
                Canvas.SetLeft(pacman, 0);
            }
            else { noLeft = false; }

            // Überprüfen der rechten Grenze
            if (goRight && pacmanLeft + pacman.Width + speed > canvasWidth)
            {
                noRight = true;
                goRight = false;
                Canvas.SetLeft(pacman, canvasWidth - pacman.Width);
            }
            else { noRight = false; }
        }

        private void CheckWallCollision(Rectangle wall, Rect hitBox)
        {
            if (pacmanHitBox.IntersectsWith(hitBox))
            {
                // Wenn eine Kollision auftritt, bewegt Pacman in die entgegengesetzte Richtung zurück + kein stecken bleiben
                if (goLeft) { Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) + speed); noLeft = true; goLeft = false; }
                if (goRight) { Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) - speed); noRight = true; goRight = false; }
                if (goDown) { Canvas.SetTop(pacman, Canvas.GetTop(pacman) - speed); noDown = true; goDown = false; }
                if (goUp) { Canvas.SetTop(pacman, Canvas.GetTop(pacman) + speed); noUp = true; goUp = false; }
            }
        }

        private void CheckGhostCollision(Rectangle ghost, Rect hitBox)
        {
            // Wenn Pacman einen Geist berührt
            if (pacmanHitBox.IntersectsWith(hitBox))
            {
                SaveHighscoreToDatabase(); // Highscore speichern beim Verlieren
                GameOver("Du wurdest erwischt du Looser, Neustart Ja/Nein", false); // False bedeutet verloren
            }

            double left = Canvas.GetLeft(ghost);
            double top = Canvas.GetTop(ghost);


            //Bewegungen hin und her 

            if (ghost.Name == "redGuy")
            {
                double newLeft = left + redGuySpeed;
                if (newLeft <= 140 || newLeft >= 697)
                {
                    redGuySpeed = -redGuySpeed;
                    newLeft = left + redGuySpeed;
                }
                Canvas.SetLeft(ghost, newLeft);
            }
            else if (ghost.Name == "orangeGuy")
            {
                double newLeft = left + orangeGuySpeed;
                if (newLeft <= 100 || newLeft >= 770)
                {
                    orangeGuySpeed = -orangeGuySpeed;
                    newLeft = left + orangeGuySpeed;
                }
                Canvas.SetLeft(ghost, newLeft);
            }
            else if (ghost.Name == "pinkGuy")
            {
                double newLeft = left + pinkGuySpeed;
                if (newLeft <= 140 || newLeft >= 690)
                {
                    pinkGuySpeed = -pinkGuySpeed;
                    newLeft = left + pinkGuySpeed;
                }
                Canvas.SetLeft(ghost, newLeft);
            }
            else if (ghost.Name == "tanGuy")
            {
                double newLeft = left + tanGuySpeed;
                if (newLeft <= 138 || newLeft >= 661)
                {
                    tanGuySpeed = -tanGuySpeed;
                    newLeft = left + tanGuySpeed;
                }
                Canvas.SetLeft(ghost, newLeft);
            }
            else if (ghost.Name == "hotpinkGuy")
            {
                double newLeft = left + hotpinkGuySpeed;
                if (newLeft <= 138 || newLeft >= 661)
                {
                    hotpinkGuySpeed = -hotpinkGuySpeed;
                    newLeft = left + hotpinkGuySpeed;
                }
                Canvas.SetLeft(ghost, newLeft);
            }
            else if (ghost.Name == "aquaGuy")
            {
                double newLeft = left + aquaGuySpeed;
                if (newLeft <= 859 || newLeft >= 1138)
                {
                    aquaGuySpeed = -aquaGuySpeed;
                    newLeft = left + aquaGuySpeed;
                }
                Canvas.SetLeft(ghost, newLeft);
            }
            else if (ghost.Name == "violetGuy")
            {
                double newLeft = left + violetGuySpeed;
                if (newLeft <= 122 || newLeft >= 697)
                {
                    violetGuySpeed = -violetGuySpeed;
                    newLeft = left + violetGuySpeed;
                }
                Canvas.SetLeft(ghost, newLeft);
            }
            else if (ghost.Name == "greenGuy")
            {
                double newTop = top + greenGuySpeed;
                if (newTop <= 272 || newTop >= 632)
                {
                    greenGuySpeed = -greenGuySpeed;
                    newTop = top + greenGuySpeed;
                }
                Canvas.SetTop(ghost, newTop);
            }
            else if (ghost.Name == "blueGuy")
            {
                double newTop = top + blueGuySpeed;
                if (newTop <= 272 || newTop >= 751)
                {
                    blueGuySpeed = -blueGuySpeed;
                    newTop = top + blueGuySpeed;
                }
                Canvas.SetTop(ghost, newTop);
            }
            else if (ghost.Name == "greyGuy")
            {
                double newLeft = left + greyGuySpeed;
                if (newLeft <= 15 || newLeft >= 1142)
                {
                    greyGuySpeed = -greyGuySpeed;
                    newLeft = left + greyGuySpeed;
                }
                Canvas.SetLeft(ghost, newLeft);
            }
        }


        private void GameOver(string message, bool isWin)
        {
            gameTimer.Stop();
            MessageBoxResult result = MessageBox.Show(message + "\nNeustarten?", "Pacman", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                if (isWin)
                {
                    // Wenn gewonnen,nächstes lEvel
                    PacManSpielLV2 nextLevel = new PacManSpielLV2(_loggedInUsername, _loggedInUserID);
                    nextLevel.Show();
                    this.Close();
                }
                else
                {
                    // Wenn verloren, das aktuelle Level neu starten
                    PacManSpielLV2 gameWindow = new PacManSpielLV2(_loggedInUsername, _loggedInUserID);
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

        // Überladene GameOver-Methode für den Verlustfall ohne isWin Parameter
        private void GameOver(string message)
        {
            GameOver(message, false); // Ruft die Haupt-GameOver-Methode mit isWin = false auf
        }


        //Spiel zurücksetzen
        private void ResetGame()
        {
            
            Canvas.SetLeft(pacman, pacmanStartLeft);
            Canvas.SetTop(pacman, pacmanStartTop);

            // Beispiel für Geister-Reset, passen Sie dies für alle Geister an
            // Canvas.SetLeft(redGuy, redGuyStartLeft);
            // Canvas.SetTop(redGuy, redGuyStartTop);
            // ...

            //Bewegungs-Flags zurück.
            goLeft = goRight = goUp = goDown = false;
            noLeft = noRight = noUp = noDown = false;

            score = 0;
            txtScore.Content = "Score: 0";

            //Alle Münzen wieder sichtbar.
            foreach (var x in MyCanvas.Children.OfType<Rectangle>())
            {
                if ((string)x.Tag == "coin")
                    x.Visibility = Visibility.Visible;
            }

            // Schrittzähler der Geister zurück
            orangeGuySteps = redGuySteps = pinkGuySteps = greenGuySteps = blueGuySteps = aquaGuySteps = tanGuySteps = hotpinkGuySteps = greyGuySteps = violetGuySteps = ghostMoveStep;

            // Geschwindigkeiten zurücksetzen 
            orangeGuySpeed = 10;
            redGuySpeed = 10;
            pinkGuySpeed = 10;
            greenGuySpeed = 10; // War 5 im Original
            blueGuySpeed = 10; // War -7 im Original
            aquaGuySpeed = 10; // War -8 im Original
            tanGuySpeed = 10;  // War -6 im Original
            hotpinkGuySpeed = 10; // War -5 im Original
            greyGuySpeed = 10; // War -4 im Original
            violetGuySpeed = 10; // War 5 im Original

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
