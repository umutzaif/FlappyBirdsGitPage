namespace flappyBirdOne
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            bird = new PictureBox();
            wallUp = new PictureBox();
            wallDown = new PictureBox();
            ground = new PictureBox();
            gameTimer = new System.Windows.Forms.Timer(components);
            label1 = new Label();
            btnRestart = new Button();
            btnStart = new Button();
            ((System.ComponentModel.ISupportInitialize)bird).BeginInit();
            ((System.ComponentModel.ISupportInitialize)wallUp).BeginInit();
            ((System.ComponentModel.ISupportInitialize)wallDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ground).BeginInit();
            SuspendLayout();
            // 
            // bird
            // 
            bird.Image = (Image)resources.GetObject("bird.Image");
            bird.Location = new Point(124, 154);
            bird.Name = "bird";
            bird.Size = new Size(62, 50);
            bird.SizeMode = PictureBoxSizeMode.StretchImage;
            bird.TabIndex = 0;
            bird.TabStop = false;
            bird.Click += pictureBox1_Click;
            // 
            // wallUp
            // 
            wallUp.Image = (Image)resources.GetObject("wallUp.Image");
            wallUp.Location = new Point(500, 0);
            wallUp.Name = "wallUp";
            wallUp.Size = new Size(86, 342);
            wallUp.SizeMode = PictureBoxSizeMode.StretchImage;
            wallUp.TabIndex = 1;
            wallUp.TabStop = false;
            // 
            // wallDown
            // 
            wallDown.Image = (Image)resources.GetObject("wallDown.Image");
            wallDown.Location = new Point(617, 0);
            wallDown.Name = "wallDown";
            wallDown.Size = new Size(86, 342);
            wallDown.SizeMode = PictureBoxSizeMode.StretchImage;
            wallDown.TabIndex = 2;
            wallDown.TabStop = false;
            // 
            // ground
            // 
            ground.Image = (Image)resources.GetObject("ground.Image");
            ground.Location = new Point(1, 339);
            ground.Name = "ground";
            ground.Size = new Size(797, 113);
            ground.SizeMode = PictureBoxSizeMode.StretchImage;
            ground.TabIndex = 3;
            ground.TabStop = false;
            // 
            // gameTimer
            // 
            gameTimer.Enabled = true;
            gameTimer.Interval = 20;
            gameTimer.Tick += gameTimer_Tick_1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.FromArgb(192, 192, 0);
            label1.Font = new Font("Segoe UI", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 162);
            label1.Location = new Point(12, 370);
            label1.Name = "label1";
            label1.Size = new Size(86, 38);
            label1.TabIndex = 4;
            label1.Text = "Skor:";
            label1.Click += label1_Click;
            // 
            // btnRestart
            // 
            btnRestart.Location = new Point(331, 224);
            btnRestart.Name = "btnRestart";
            btnRestart.Size = new Size(94, 29);
            btnRestart.TabIndex = 5;
            btnRestart.Text = "Restart";
            btnRestart.UseVisualStyleBackColor = true;
            btnRestart.Visible = false;
            btnRestart.Click += btnRestart_Click;
            // 
            // btnStart
            // 
            btnStart.Location = new Point(331, 175);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(94, 29);
            btnStart.TabIndex = 6;
            btnStart.Text = "Start";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Visible = false;
            btnStart.Click += btnStart_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Cyan;
            ClientSize = new Size(800, 450);
            Controls.Add(btnStart);
            Controls.Add(btnRestart);
            Controls.Add(label1);
            Controls.Add(ground);
            Controls.Add(wallDown);
            Controls.Add(wallUp);
            Controls.Add(bird);
            KeyPreview = true;
            Name = "Form1";
            Text = "Frappy Bird";
            Load += Form1_Load;
            KeyDown += gameKeyisDown;
            ((System.ComponentModel.ISupportInitialize)bird).EndInit();
            ((System.ComponentModel.ISupportInitialize)wallUp).EndInit();
            ((System.ComponentModel.ISupportInitialize)wallDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)ground).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox bird;
        private PictureBox wallUp;
        private PictureBox wallDown;
        private PictureBox ground;
        private System.Windows.Forms.Timer gameTimer;
        private Label label1;
        private Button btnRestart;
        private Button btnStart;
    }
}
