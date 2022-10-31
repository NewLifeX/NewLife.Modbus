namespace WinFormsApp1
{
    partial class FrmMain
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
            this.btnOpen1 = new System.Windows.Forms.Button();
            this.btnClose1 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.cbPorts = new System.Windows.Forms.ComboBox();
            this.numBaudrate = new System.Windows.Forms.NumericUpDown();
            this.btnConnect = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.numHost = new System.Windows.Forms.NumericUpDown();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnWriteAddr = new System.Windows.Forms.Button();
            this.numAddr = new System.Windows.Forms.NumericUpDown();
            this.btnReadAddr = new System.Windows.Forms.Button();
            this.btnReadAll = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.numDelay = new System.Windows.Forms.NumericUpDown();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.btnCloseAll = new System.Windows.Forms.Button();
            this.btnOpenAll = new System.Windows.Forms.Button();
            this.btnReadIn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numBaudrate)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numHost)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAddr)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDelay)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOpen1
            // 
            this.btnOpen1.Location = new System.Drawing.Point(26, 35);
            this.btnOpen1.Margin = new System.Windows.Forms.Padding(4);
            this.btnOpen1.Name = "btnOpen1";
            this.btnOpen1.Size = new System.Drawing.Size(100, 50);
            this.btnOpen1.TabIndex = 0;
            this.btnOpen1.Tag = "1";
            this.btnOpen1.Text = "打开1号";
            this.btnOpen1.UseVisualStyleBackColor = true;
            this.btnOpen1.Click += new System.EventHandler(this.btnOpen1_Click);
            // 
            // btnClose1
            // 
            this.btnClose1.Location = new System.Drawing.Point(134, 35);
            this.btnClose1.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose1.Name = "btnClose1";
            this.btnClose1.Size = new System.Drawing.Size(100, 50);
            this.btnClose1.TabIndex = 1;
            this.btnClose1.Tag = "1";
            this.btnClose1.Text = "关闭1号";
            this.btnClose1.UseVisualStyleBackColor = true;
            this.btnClose1.Click += new System.EventHandler(this.btnClose1_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(134, 93);
            this.button3.Margin = new System.Windows.Forms.Padding(4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(100, 50);
            this.button3.TabIndex = 3;
            this.button3.Tag = "2";
            this.button3.Text = "关闭2号";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.btnClose1_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(26, 93);
            this.button4.Margin = new System.Windows.Forms.Padding(4);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(100, 50);
            this.button4.TabIndex = 2;
            this.button4.Tag = "2";
            this.button4.Text = "打开2号";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.btnOpen1_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(134, 151);
            this.button5.Margin = new System.Windows.Forms.Padding(4);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(100, 50);
            this.button5.TabIndex = 5;
            this.button5.Tag = "3";
            this.button5.Text = "关闭3号";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.btnClose1_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(26, 151);
            this.button6.Margin = new System.Windows.Forms.Padding(4);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(100, 50);
            this.button6.TabIndex = 4;
            this.button6.Tag = "3";
            this.button6.Text = "打开3号";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.btnOpen1_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(134, 209);
            this.button7.Margin = new System.Windows.Forms.Padding(4);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(100, 50);
            this.button7.TabIndex = 7;
            this.button7.Tag = "4";
            this.button7.Text = "关闭4号";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.btnClose1_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(26, 209);
            this.button8.Margin = new System.Windows.Forms.Padding(4);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(100, 50);
            this.button8.TabIndex = 6;
            this.button8.Tag = "4";
            this.button8.Text = "打开4号";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.btnOpen1_Click);
            // 
            // cbPorts
            // 
            this.cbPorts.FormattingEnabled = true;
            this.cbPorts.Location = new System.Drawing.Point(23, 30);
            this.cbPorts.Margin = new System.Windows.Forms.Padding(4);
            this.cbPorts.Name = "cbPorts";
            this.cbPorts.Size = new System.Drawing.Size(134, 32);
            this.cbPorts.TabIndex = 8;
            // 
            // numBaudrate
            // 
            this.numBaudrate.Location = new System.Drawing.Point(185, 31);
            this.numBaudrate.Margin = new System.Windows.Forms.Padding(4);
            this.numBaudrate.Maximum = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
            this.numBaudrate.Name = "numBaudrate";
            this.numBaudrate.Size = new System.Drawing.Size(121, 30);
            this.numBaudrate.TabIndex = 9;
            this.numBaudrate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numBaudrate.Value = new decimal(new int[] {
            9600,
            0,
            0,
            0});
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(488, 31);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(4);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(100, 50);
            this.btnConnect.TabIndex = 10;
            this.btnConnect.Text = "连接";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.numHost);
            this.groupBox1.Controls.Add(this.numBaudrate);
            this.groupBox1.Controls.Add(this.cbPorts);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(449, 79);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "参数设置";
            // 
            // numHost
            // 
            this.numHost.Location = new System.Drawing.Point(335, 32);
            this.numHost.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numHost.Name = "numHost";
            this.numHost.Size = new System.Drawing.Size(78, 30);
            this.numHost.TabIndex = 10;
            this.numHost.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numHost.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.btnReadIn);
            this.groupBox2.Controls.Add(this.btnWriteAddr);
            this.groupBox2.Controls.Add(this.numAddr);
            this.groupBox2.Controls.Add(this.btnReadAddr);
            this.groupBox2.Controls.Add(this.btnReadAll);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.numDelay);
            this.groupBox2.Controls.Add(this.richTextBox1);
            this.groupBox2.Controls.Add(this.btnCloseAll);
            this.groupBox2.Controls.Add(this.btnOpenAll);
            this.groupBox2.Controls.Add(this.button3);
            this.groupBox2.Controls.Add(this.btnOpen1);
            this.groupBox2.Controls.Add(this.btnClose1);
            this.groupBox2.Controls.Add(this.button7);
            this.groupBox2.Controls.Add(this.button4);
            this.groupBox2.Controls.Add(this.button8);
            this.groupBox2.Controls.Add(this.button6);
            this.groupBox2.Controls.Add(this.button5);
            this.groupBox2.Enabled = false;
            this.groupBox2.Location = new System.Drawing.Point(13, 103);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(605, 450);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "功能区";
            // 
            // btnWriteAddr
            // 
            this.btnWriteAddr.Location = new System.Drawing.Point(405, 93);
            this.btnWriteAddr.Margin = new System.Windows.Forms.Padding(4);
            this.btnWriteAddr.Name = "btnWriteAddr";
            this.btnWriteAddr.Size = new System.Drawing.Size(100, 50);
            this.btnWriteAddr.TabIndex = 18;
            this.btnWriteAddr.Tag = "1";
            this.btnWriteAddr.Text = "写地址";
            this.btnWriteAddr.UseVisualStyleBackColor = true;
            this.btnWriteAddr.Click += new System.EventHandler(this.btnWriteAddr_Click);
            // 
            // numAddr
            // 
            this.numAddr.Location = new System.Drawing.Point(526, 46);
            this.numAddr.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numAddr.Name = "numAddr";
            this.numAddr.Size = new System.Drawing.Size(78, 30);
            this.numAddr.TabIndex = 17;
            this.numAddr.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // btnReadAddr
            // 
            this.btnReadAddr.Location = new System.Drawing.Point(405, 35);
            this.btnReadAddr.Margin = new System.Windows.Forms.Padding(4);
            this.btnReadAddr.Name = "btnReadAddr";
            this.btnReadAddr.Size = new System.Drawing.Size(100, 50);
            this.btnReadAddr.TabIndex = 16;
            this.btnReadAddr.Tag = "1";
            this.btnReadAddr.Text = "读地址";
            this.btnReadAddr.UseVisualStyleBackColor = true;
            this.btnReadAddr.Click += new System.EventHandler(this.btnReadAddr_Click);
            // 
            // btnReadAll
            // 
            this.btnReadAll.Location = new System.Drawing.Point(268, 209);
            this.btnReadAll.Margin = new System.Windows.Forms.Padding(4);
            this.btnReadAll.Name = "btnReadAll";
            this.btnReadAll.Size = new System.Drawing.Size(100, 50);
            this.btnReadAll.TabIndex = 15;
            this.btnReadAll.Tag = "1";
            this.btnReadAll.Text = "读继电器";
            this.btnReadAll.UseVisualStyleBackColor = true;
            this.btnReadAll.Click += new System.EventHandler(this.btnReadAll_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(268, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(118, 24);
            this.label1.TabIndex = 14;
            this.label1.Text = "延时（毫秒）";
            // 
            // numDelay
            // 
            this.numDelay.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numDelay.Location = new System.Drawing.Point(268, 46);
            this.numDelay.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numDelay.Name = "numDelay";
            this.numDelay.Size = new System.Drawing.Size(103, 30);
            this.numDelay.TabIndex = 13;
            this.numDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.Location = new System.Drawing.Point(7, 266);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(591, 177);
            this.richTextBox1.TabIndex = 10;
            this.richTextBox1.Text = "";
            // 
            // btnCloseAll
            // 
            this.btnCloseAll.Location = new System.Drawing.Point(268, 151);
            this.btnCloseAll.Margin = new System.Windows.Forms.Padding(4);
            this.btnCloseAll.Name = "btnCloseAll";
            this.btnCloseAll.Size = new System.Drawing.Size(100, 50);
            this.btnCloseAll.TabIndex = 9;
            this.btnCloseAll.Tag = "1";
            this.btnCloseAll.Text = "全部关闭";
            this.btnCloseAll.UseVisualStyleBackColor = true;
            this.btnCloseAll.Click += new System.EventHandler(this.btnCloseAll_Click);
            // 
            // btnOpenAll
            // 
            this.btnOpenAll.Location = new System.Drawing.Point(268, 93);
            this.btnOpenAll.Margin = new System.Windows.Forms.Padding(4);
            this.btnOpenAll.Name = "btnOpenAll";
            this.btnOpenAll.Size = new System.Drawing.Size(100, 50);
            this.btnOpenAll.TabIndex = 8;
            this.btnOpenAll.Tag = "1";
            this.btnOpenAll.Text = "全部打开";
            this.btnOpenAll.UseVisualStyleBackColor = true;
            this.btnOpenAll.Click += new System.EventHandler(this.btnOpenAll_Click);
            // 
            // btnReadIn
            // 
            this.btnReadIn.Location = new System.Drawing.Point(405, 209);
            this.btnReadIn.Margin = new System.Windows.Forms.Padding(4);
            this.btnReadIn.Name = "btnReadIn";
            this.btnReadIn.Size = new System.Drawing.Size(100, 50);
            this.btnReadIn.TabIndex = 19;
            this.btnReadIn.Tag = "1";
            this.btnReadIn.Text = "读光耦";
            this.btnReadIn.UseVisualStyleBackColor = true;
            this.btnReadIn.Click += new System.EventHandler(this.btnReadIn_Click);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(629, 566);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnConnect);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "开关控制器";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numBaudrate)).EndInit();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numHost)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAddr)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDelay)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Button btnOpen1;
        private Button btnClose1;
        private Button button3;
        private Button button4;
        private Button button5;
        private Button button6;
        private Button button7;
        private Button button8;
        private ComboBox cbPorts;
        private NumericUpDown numBaudrate;
        private Button btnConnect;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private Button btnCloseAll;
        private Button btnOpenAll;
        private NumericUpDown numHost;
        private RichTextBox richTextBox1;
        private NumericUpDown numDelay;
        private Label label1;
        private Button btnReadAll;
        private Button btnWriteAddr;
        private NumericUpDown numAddr;
        private Button btnReadAddr;
        private Button btnReadIn;
    }
}