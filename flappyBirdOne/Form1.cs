using static System.Formats.Asn1.AsnWriter;

namespace flappyBirdOne
{
    public partial class Form1 : Form
    {
        int gravity = 4;
        int pipeSpeed = 8; // Borular�n sola kayma h�z�
        int score = 0;
        int level_upper = 0;
        Random rand = new Random();
        bool isGameOver = false; // Oyun biti� durumu
        int highScore = 0; // En y�ksek skor

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;  // Klavye olaylar�n� form yakalas�n
            this.KeyDown += gameKeyisDown;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            gameTimer.Stop();   // A��l��ta timer �al��mas�n
            btnStart.Visible = true;
            btnRestart.Visible = false;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void gameTimer_Tick_1(object sender, EventArgs e)
        {
            // Ku� a�a�� d���yor
            bird.Top += gravity;

            // Borular sola kay�yor
            wallUp.Left -= pipeSpeed;
            wallDown.Left -= pipeSpeed;

            // Borular ekran d���na ��kt�ysa yeniden sa�dan gelsin
            if (wallUp.Right < 0)
            {
                ResetPipes();
                score++;
                level_upper++;
                if (level_upper == 3)
                {
                pipeSpeed++; // Her 3 skor art���nda boru h�z� arts�n
                level_upper = 0; // Saya� s�f�rlan�r
                }
                label1.Text = "Score: " + score;
            }

            // �arp��ma kontrol�
            if (bird.Bounds.IntersectsWith(wallUp.Bounds) ||
                bird.Bounds.IntersectsWith(wallDown.Bounds) ||
                bird.Bounds.IntersectsWith(ground.Bounds) ||
                bird.Top < -25) // Yukar�ya �ok ��karsa
            {
                EndGame();
            }
            
        }
        private void EndGame()
        {
            gameTimer.Stop();
            btnRestart.Visible = true;  // Oyun bitince buton ��ks�n
            isGameOver = true; // Oyun bitti
            MessageBox.Show("Oyun Bitti! Skor: " + score + "\nEn y�ksek Skor: " + highScore);
        }

        private void StartGame()
        {
            score = 0;
            label1.Text = "Score: 0";

            bird.Top = 150;
            ResetPipes();

            btnStart.Visible = false;
            btnRestart.Visible = false;

            this.ActiveControl = null;  // Space tu�u butonu tetiklemesin

            isGameOver = false; // Oyun ba�l�yor
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
            int minGapY = 70; // Boru bo�lu�unun minimum �st noktas�
            int maxGapY = this.ClientSize.Height - 200; // Boru bo�lu�unun maksimum �st noktas�

            int gapY = rand.Next(minGapY, maxGapY); // Bo�lu�un �st noktas� rastgele se�iliyor
            int gapHeight = 150; // Ku�un ge�ece�i bo�luk

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
            gameTimer.Stop(); // �nce timer'� durdur
            if(score > highScore)
            {
                highScore = score; // Yeni y�ksek skor
            }
            score = 0;
            level_upper = 0;
            pipeSpeed = 8; // Boru h�z� ba�lang�� de�erine s�f�rlan�r
            label1.Text = "Score: 0";

            bird.Top = 150;  // Ku�un ba�lang�� konumu

            ResetPipes();    // Borular� rastgele konumland�r

            isGameOver = false; // Oyun ba�l�yor
            btnRestart.Visible = false;

            this.ActiveControl = null; // Space tu�u butonu tetiklemesin

            gameTimer.Start(); // Sonra timer'� ba�lat
        }

       
    }
}
