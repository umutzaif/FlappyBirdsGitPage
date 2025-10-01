using static System.Formats.Asn1.AsnWriter;

namespace flappyBirdOne
{
    public partial class Form1 : Form
    {
        int gravity = 4;
        int pipeSpeed = 8; // Borularýn sola kayma hýzý
        int score = 0;
        int level_upper = 0;
        Random rand = new Random();
        bool isGameOver = false; // Oyun bitiþ durumu
        int highScore = 0; // En yüksek skor

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;  // Klavye olaylarýný form yakalasýn
            this.KeyDown += gameKeyisDown;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            gameTimer.Stop();   // Açýlýþta timer çalýþmasýn
            btnStart.Visible = true;
            btnRestart.Visible = false;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void gameTimer_Tick_1(object sender, EventArgs e)
        {
            // Kuþ aþaðý düþüyor
            bird.Top += gravity;

            // Borular sola kayýyor
            wallUp.Left -= pipeSpeed;
            wallDown.Left -= pipeSpeed;

            // Borular ekran dýþýna çýktýysa yeniden saðdan gelsin
            if (wallUp.Right < 0)
            {
                ResetPipes();
                score++;
                level_upper++;
                if (level_upper == 3)
                {
                pipeSpeed++; // Her 3 skor artýþýnda boru hýzý artsýn
                level_upper = 0; // Sayaç sýfýrlanýr
                }
                label1.Text = "Score: " + score;
            }

            // Çarpýþma kontrolü
            if (bird.Bounds.IntersectsWith(wallUp.Bounds) ||
                bird.Bounds.IntersectsWith(wallDown.Bounds) ||
                bird.Bounds.IntersectsWith(ground.Bounds) ||
                bird.Top < -25) // Yukarýya çok çýkarsa
            {
                EndGame();
            }
            
        }
        private void EndGame()
        {
            gameTimer.Stop();
            btnRestart.Visible = true;  // Oyun bitince buton çýksýn
            isGameOver = true; // Oyun bitti
            MessageBox.Show("Oyun Bitti! Skor: " + score + "\nEn yüksek Skor: " + highScore);
        }

        private void StartGame()
        {
            score = 0;
            label1.Text = "Score: 0";

            bird.Top = 150;
            ResetPipes();

            btnStart.Visible = false;
            btnRestart.Visible = false;

            this.ActiveControl = null;  // Space tuþu butonu tetiklemesin

            isGameOver = false; // Oyun baþlýyor
            gameTimer.Start();
        

        }



        private void gameKeyisDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && !isGameOver)
            {
                bird.Top -= 18;
            }
        }
        private void ResetPipes()
        {
            int minGapY = 70; // Boru boþluðunun minimum üst noktasý
            int maxGapY = this.ClientSize.Height - 200; // Boru boþluðunun maksimum üst noktasý

            int gapY = rand.Next(minGapY, maxGapY); // Boþluðun üst noktasý rastgele seçiliyor
            int gapHeight = 150; // Kuþun geçeceði boþluk

            wallUp.Left = this.ClientSize.Width;
            wallDown.Left = this.ClientSize.Width;

            wallUp.Top = gapY - wallUp.Height;
            wallDown.Top = gapY + gapHeight;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            RestartGame();
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            StartGame();
        }

        private void RestartGame()
        {
            gameTimer.Stop(); // Önce timer'ý durdur
            if(score > highScore)
            {
                highScore = score; // Yeni yüksek skor
            }
            score = 0;
            level_upper = 0;
            pipeSpeed = 8; // Boru hýzý baþlangýç deðerine sýfýrlanýr
            label1.Text = "Score: 0";

            bird.Top = 150;  // Kuþun baþlangýç konumu

            ResetPipes();    // Borularý rastgele konumlandýr

            isGameOver = false; // Oyun baþlýyor
            btnRestart.Visible = false;

            this.ActiveControl = null; // Space tuþu butonu tetiklemesin

            gameTimer.Start(); // Sonra timer'ý baþlat
        }

       
    }
}
