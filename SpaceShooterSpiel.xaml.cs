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
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;
using SqlCommand = Microsoft.Data.SqlClient.SqlCommand;



namespace ProjektGameKatalog
{
    /// <summary>
    /// Interaktionslogik für SpaceShooterSpiel.xaml
    /// </summary>
    public partial class SpaceShooterSpiel : Window
    {
        //Für Bullet & Hintergrund Musik
        MediaPlayer shootSound = new MediaPlayer();
        MediaPlayer HintergrundMusik = new MediaPlayer();


        DispatcherTimer GameTimer = new DispatcherTimer();
        bool moveLeft, moveRight, moveUp, moveDown;
        List<Rectangle> itemremover = new List<Rectangle>();

        Random random = new Random();

        int EnemySprite;
        int Enemycount = 100;
        int playerSpeed = 10;
        int limit = 50;
        int score = 0;
        int damage = 0;
        int EnemySpeed = 10;

        private string _loggedInUsername; // Speichert den Benutzernamen
        private string _loggedInUserID;   // Speichert die UserID

        Rect playerHitBox;

        // Benutzernamen und die UserID empfangen
        public SpaceShooterSpiel(string username, string userID)
        {
            InitializeComponent();

            _loggedInUsername = username;
            _loggedInUserID = userID; // UserID speichern

            GameTimer.Interval = TimeSpan.FromMilliseconds(20);
            GameTimer.Tick += GameLoop;

            GameTimer.Start();
            MyCanvas.Focus();

            //Sound Bullet & Musik
            shootSound.Open(new Uri("C:\\Users\\Kirito Alice\\source\\repos\\ProjektGameKatalog\\ProjektGameKatalog\\Sound\\Bullet.wav"));
            shootSound.Volume = 0.05; ;

            //Hintergrund V2.0 Neu (besser)
            ImageBrush bg = new ImageBrush();
            bg.ImageSource = new BitmapImage(new Uri("C:\\Users\\Kirito Alice\\source\\repos\\ProjektGameKatalog\\ProjektGameKatalog\\Images\\Hintergrund.png"));
            bg.TileMode = TileMode.None;
            bg.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            MyCanvas.Background = bg;

            //Player
            ImageBrush playerImage = new ImageBrush();
            playerImage.ImageSource = new BitmapImage(new Uri("C:\\Users\\Kirito Alice\\source\\repos\\ProjektGameKatalog\\ProjektGameKatalog\\Images\\Player.png"));
            player.Fill = playerImage;
        }

        //für Tests ohne Login)
        
        public SpaceShooterSpiel() : this("Gast", null)
        {
            // Initialisierung, falls nötig
        }


        private void onKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Left)
            {
                moveLeft = true;
            }
            if (e.Key == Key.Right)
            {
                moveRight = true;
            }
            if (e.Key == Key.Up)
            {
                moveUp = true;
            }
            if (e.Key == Key.Down)
            {
                moveDown = true;
            }
        }

        //Bewegung Spieler
        private void onKeyUp(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Left)
            {
                moveLeft = false;
            }
            if (e.Key == Key.Right)
            {
                moveRight = false;
            }
            if (e.Key == Key.Up)
            {
                moveUp = false;
            }
            if (e.Key == Key.Down)
            {
                moveDown = false;
            }


            if (e.Key == Key.Space)
            {
                Rectangle newBullet = new Rectangle
                {
                    Tag = "bullet",
                    Height = 20,
                    Width = 5,
                    Fill = Brushes.White,
                    Stroke = Brushes.Red
                };

                Canvas.SetTop(newBullet, Canvas.GetTop(player) - newBullet.Height);
                Canvas.SetLeft(newBullet, Canvas.GetLeft(player) + player.Width / 2);
                MyCanvas.Children.Add(newBullet);

                // Sound abspielen
                shootSound.Position = TimeSpan.Zero; // Zurückspulen, falls der Sound sehr kurz ist
                shootSound.Play();
            }
        }

        //Gegner Spawnen lassen
        private void makeEnemies()
        {
            ImageBrush enemySprite = new ImageBrush();
            Enemycount = random.Next(1, 4);
            switch (Enemycount)
            {
                case 1:
                    enemySprite.ImageSource = new BitmapImage(new Uri("C:\\Users\\Kirito Alice\\source\\repos\\ProjektGameKatalog\\ProjektGameKatalog\\Images\\Enemy.png"));
                    break;
                case 2:
                    enemySprite.ImageSource = new BitmapImage(new Uri("C:\\Users\\Kirito Alice\\source\\repos\\ProjektGameKatalog\\ProjektGameKatalog\\Images\\Enemy2.png"));
                    break;
                case 3:
                    enemySprite.ImageSource = new BitmapImage(new Uri("C:\\Users\\Kirito Alice\\source\\repos\\ProjektGameKatalog\\ProjektGameKatalog\\Images\\Enemy3.png"));
                    break;
                case 4:
                    enemySprite.ImageSource = new BitmapImage(new Uri("C:\\Users\\Kirito Alice\\source\\repos\\ProjektGameKatalog\\ProjektGameKatalog\\Images\\Enemy4.png"));
                    break;

                default:
                    enemySprite.ImageSource = new BitmapImage(new Uri("C:\\Users\\Kirito Alice\\source\\repos\\ProjektGameKatalog\\ProjektGameKatalog\\Images\\Enemy.png"));
                    break;
            }
            Rectangle newEnemy = new Rectangle
            {
                Tag = "enemy",
                Height = 50,
                Width = 56,
                Fill = enemySprite
            };
            Canvas.SetTop(newEnemy, -100);
            Canvas.SetLeft(newEnemy, random.Next(30, 430));
            MyCanvas.Children.Add(newEnemy);
            GC.Collect();
        }

        //Spiel Loop-einbettung der Funktione
        private void GameLoop(object sender, EventArgs e)
        {
            playerHitBox = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);
            Enemycount--;

            scoreText.Content = "Score: " + score;
            damageText.Content = "Damaged " + damage;

            if (Enemycount < 0)
            {
                makeEnemies();
                Enemycount = limit;
            }


            // Bewegung Player
            if (moveLeft && Canvas.GetLeft(player) > 0)
            {
                Canvas.SetLeft(player, Canvas.GetLeft(player) - playerSpeed);
            }
            if (moveRight && Canvas.GetLeft(player) + player.Width < MyCanvas.ActualWidth)
            {
                Canvas.SetLeft(player, Canvas.GetLeft(player) + playerSpeed);
            }
            if (moveUp && Canvas.GetTop(player) > 0)
            {
                Canvas.SetTop(player, Canvas.GetTop(player) - playerSpeed);
            }
            if (moveDown && Canvas.GetTop(player) + player.Height < MyCanvas.ActualHeight)
            {
                Canvas.SetTop(player, Canvas.GetTop(player) + playerSpeed);
            }


            //Bullet & Enemy Hitbox etc.

            foreach (var x in MyCanvas.Children.OfType<Rectangle>())
            {
                if (x is Rectangle && (string)x.Tag == "bullet")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) - 20);
                    Rect bullet = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                    if (Canvas.GetTop(x) < 10)
                    {
                        itemremover.Add(x);
                    }

                    foreach (var y in MyCanvas.Children.OfType<Rectangle>())
                    {
                        if (y is Rectangle && (string)y.Tag == "enemy")
                        {
                            Rect enemyHit = new Rect(Canvas.GetLeft(y), Canvas.GetTop(y), y.Width, y.Height);

                            if (bullet.IntersectsWith(enemyHit))
                            {
                                itemremover.Add(x);
                                itemremover.Add(y);
                                score++;
                            }
                        }
                    }
                }


                //Bewegung Gegner & Interaktion

                if (x is Rectangle && (string)x.Tag == "enemy")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) + 10);
                    Rect enemy = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                    //Gegner ab bestimmter Entfernung entfernet(jetzt 700 von oben)
                    if (Canvas.GetTop(x) + 150 > 700)
                    {
                        itemremover.Add(x);
                        damage += 10;
                    }
                    if (playerHitBox.IntersectsWith(enemy))
                    {
                        damage += 5;
                        itemremover.Add(x);
                    }
                }
            }

            //Gegner nach 10 Punkten schneller Spawnen lassen
            if (score > 5)
            {
                limit = 20;
            }
            if (score > 20)
            {
                EnemySpeed = 15;
                limit = 20;
            }
            if (score > 30)
            {
                limit = 30;
                EnemySpeed += score; // EnemySpeed kann hier sehr schnell ansteigen!
                playerSpeed = 15;

            }
            if (damage > 99)
            {
                SaveHighscoreToDatabase(); // Speichere den Highscore, wenn das Spiel vorbei ist

                HintergrundMusik.Stop();
                GameTimer.Stop();
                damageText.Content = "Damaged: 100";
                damageText.Foreground = Brushes.Red;

                var result = MessageBox.Show("Du hast verloren du LOOSER!" + Environment.NewLine + "Du hast " + score + " Gegner besiegt!" + Environment.NewLine + "Neu starten?",
                                             "Game Over", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    ResetGame();
                }
                else
                {
                    // Kehre zum SpaceShooter Menü zurück und übergib den Benutzernamen und die UserID
                    SpaceShooter menuWindow = new SpaceShooter(_loggedInUsername, _loggedInUserID);
                    menuWindow.Show();
                    this.Close();
                }
            }
            foreach (Rectangle y in itemremover)
            {
                MyCanvas.Children.Remove(y);
            }


        }
        private void ResetGame()
        {
            score = 0;
            damage = 0;
            limit = 50;
            EnemySpeed = 10;
            playerSpeed = 10;

            // Canvas leeren (außer Player)
            List<UIElement> toRemove = new List<UIElement>();

            foreach (var item in MyCanvas.Children)
            {
                if (item is Rectangle rect && rect.Tag != null && ((string)rect.Tag == "enemy" || (string)rect.Tag == "bullet"))
                {
                    toRemove.Add(rect);
                }
            }

            foreach (var item in toRemove)
            {
                MyCanvas.Children.Remove(item);
            }

            // Spieler zurücksetzen & Position
            if (!MyCanvas.Children.Contains(player))
            {
                MyCanvas.Children.Add(player);
            }
            Canvas.SetLeft(player, 222);
            Canvas.SetTop(player, 495);

            // Anzeige zurücksetzen
            scoreText.Content = "Score: 0";
            damageText.Content = "Damaged: 0";
            damageText.Foreground = Brushes.White;

            // GameTimer neu starten
            GameTimer.Start();
            MyCanvas.Focus();
            //Musik starten
            HintergrundMusik.Open(new Uri("C:\\Users\\Kirito Alice\\source\\repos\\ProjektGameKatalog\\ProjektGameKatalog\\Sound\\ToBeHeroX(Instrumental).mp3"));
            HintergrundMusik.Volume = 0.05;
            HintergrundMusik.Play();
        }


        private void SaveHighscoreToDatabase()
        {
            // Die UserID wird jetzt aus dem privaten Feld abgerufen
            string userID = _loggedInUserID;
            int highscoreSpaceInt = score;

            // Überprüfe, ob eine UserID verfügbar ist
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

                    // Überprüfen, ob bereits ein Highscore-Eintrag für diese UserID existiert
                    SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Highscore WHERE UserID = @UserID", connection);
                    checkCmd.Parameters.AddWithValue("@UserID", userID);
                    int exists = (int)checkCmd.ExecuteScalar();

                    SqlCommand cmd;
                    if (exists == 0)
                    {
                        // Füge einen neuen Highscore ein, wenn kein Eintrag für diese UserID existiert
                        cmd = new SqlCommand(@"INSERT INTO Highscore (UserID, HighscoreSpace, HighscorePacMan, GoldRPG)
                                               VALUES (@UserID, @HighscoreSpace, 0, 0)", connection);

                        cmd.Parameters.AddWithValue("@UserID", userID);
                        cmd.Parameters.AddWithValue("@HighscoreSpace", highscoreSpaceInt);
                    }
                    else
                    {
                        // Hole den aktuellen Highscore des Benutzers für SpaceShooter
                        SqlCommand getOldHighscoreCmd = new SqlCommand("SELECT HighscoreSpace FROM Highscore WHERE UserID = @UserID", connection);
                        getOldHighscoreCmd.Parameters.AddWithValue("@UserID", userID);
                        object oldHighscoreObj = getOldHighscoreCmd.ExecuteScalar();
                        int oldHighscore = (oldHighscoreObj != DBNull.Value) ? Convert.ToInt32(oldHighscoreObj) : 0;

                        // Aktualisiere den Highscore nur, wenn der neue Score höher ist
                        if (highscoreSpaceInt > oldHighscore)
                        {
                            cmd = new SqlCommand(@"UPDATE Highscore SET HighscoreSpace = @HighscoreSpace
                                                   WHERE UserID = @UserID", connection);

                            cmd.Parameters.AddWithValue("@UserID", userID);
                            cmd.Parameters.AddWithValue("@HighscoreSpace", highscoreSpaceInt);
                        }
                        else
                        {
                            // Wenn der neue Score nicht höher ist, gibt es nichts zu aktualisieren
                            MessageBox.Show("Highscore wurde nicht aktualisiert, da der aktuelle Score nicht höher ist.");
                            return; // Beende die Methode hier
                        }
                    }

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Highscore erfolgreich gespeichert/aktualisiert!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Speichern des Highscores: " + ex.Message);
            }
        }


        private void HintergrundMusik_MediaEnded(object sender, EventArgs e)
        {
            HintergrundMusik.Position = TimeSpan.Zero;
            HintergrundMusik.Play();
        }
    }
}
