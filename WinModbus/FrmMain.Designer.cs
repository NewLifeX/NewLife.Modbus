namespace WinModbus
{
    partial class FrmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBox1 = new GroupBox();
            label5 = new Label();
            label3 = new Label();
            numBaudrate = new NumericUpDown();
            cbPorts = new ComboBox();
            numHost = new NumericUpDown();
            btnConnect = new Button();
            groupBox2 = new GroupBox();
            label2 = new Label();
            label1 = new Label();
            txtAddress = new TextBox();
            numPort = new NumericUpDown();
            groupBox3 = new GroupBox();
            btnSwitch = new Button();
            label4 = new Label();
            richTextBox1 = new RichTextBox();
            btnConnect2 = new Button();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numBaudrate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numHost).BeginInit();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(numBaudrate);
            groupBox1.Controls.Add(cbPorts);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(460, 66);
            groupBox1.TabIndex = 13;
            groupBox1.TabStop = false;
            groupBox1.Text = "ModbusRTU参数";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(274, 33);
            label5.Name = "label5";
            label5.Size = new Size(69, 20);
            label5.TabIndex = 14;
            label5.Text = "波特率：";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(19, 33);
            label3.Name = "label3";
            label3.Size = new Size(54, 20);
            label3.TabIndex = 13;
            label3.Text = "串口：";
            // 
            // numBaudrate
            // 
            numBaudrate.Location = new Point(349, 30);
            numBaudrate.Maximum = new decimal(new int[] { 99999999, 0, 0, 0 });
            numBaudrate.Name = "numBaudrate";
            numBaudrate.Size = new Size(99, 27);
            numBaudrate.TabIndex = 9;
            numBaudrate.TextAlign = HorizontalAlignment.Right;
            numBaudrate.Value = new decimal(new int[] { 9600, 0, 0, 0 });
            // 
            // cbPorts
            // 
            cbPorts.FormattingEnabled = true;
            cbPorts.Location = new Point(78, 29);
            cbPorts.Name = "cbPorts";
            cbPorts.Size = new Size(182, 28);
            cbPorts.TabIndex = 8;
            // 
            // numHost
            // 
            numHost.Location = new Point(78, 29);
            numHost.Margin = new Padding(2);
            numHost.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numHost.Name = "numHost";
            numHost.Size = new Size(64, 27);
            numHost.TabIndex = 10;
            numHost.TextAlign = HorizontalAlignment.Right;
            numHost.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(488, 24);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(82, 42);
            btnConnect.TabIndex = 12;
            btnConnect.Text = "连接";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(txtAddress);
            groupBox2.Controls.Add(numPort);
            groupBox2.Location = new Point(12, 84);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(460, 66);
            groupBox2.TabIndex = 14;
            groupBox2.TabStop = false;
            groupBox2.Text = "ModbusTcp参数";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(274, 29);
            label2.Name = "label2";
            label2.Size = new Size(54, 20);
            label2.TabIndex = 13;
            label2.Text = "端口：";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(19, 29);
            label1.Name = "label1";
            label1.Size = new Size(54, 20);
            label1.TabIndex = 12;
            label1.Text = "地址：";
            // 
            // txtAddress
            // 
            txtAddress.Location = new Point(78, 26);
            txtAddress.Name = "txtAddress";
            txtAddress.Size = new Size(182, 27);
            txtAddress.TabIndex = 11;
            txtAddress.Text = "127.0.0.1";
            // 
            // numPort
            // 
            numPort.Location = new Point(349, 26);
            numPort.Margin = new Padding(2);
            numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numPort.Name = "numPort";
            numPort.Size = new Size(99, 27);
            numPort.TabIndex = 10;
            numPort.TextAlign = HorizontalAlignment.Right;
            numPort.Value = new decimal(new int[] { 512, 0, 0, 0 });
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(btnSwitch);
            groupBox3.Controls.Add(numHost);
            groupBox3.Controls.Add(label4);
            groupBox3.Location = new Point(12, 156);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(460, 66);
            groupBox3.TabIndex = 15;
            groupBox3.TabStop = false;
            groupBox3.Text = "通用参数";
            // 
            // btnSwitch
            // 
            btnSwitch.Location = new Point(274, 20);
            btnSwitch.Name = "btnSwitch";
            btnSwitch.Size = new Size(82, 42);
            btnSwitch.TabIndex = 14;
            btnSwitch.Text = "开关控制";
            btnSwitch.UseVisualStyleBackColor = true;
            btnSwitch.Click += btnSwitch_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(19, 32);
            label4.Name = "label4";
            label4.Size = new Size(54, 20);
            label4.TabIndex = 13;
            label4.Text = "站号：";
            // 
            // richTextBox1
            // 
            richTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBox1.Location = new Point(11, 227);
            richTextBox1.Margin = new Padding(2);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(563, 318);
            richTextBox1.TabIndex = 16;
            richTextBox1.Text = "";
            // 
            // btnConnect2
            // 
            btnConnect2.Location = new Point(488, 96);
            btnConnect2.Name = "btnConnect2";
            btnConnect2.Size = new Size(82, 42);
            btnConnect2.TabIndex = 17;
            btnConnect2.Text = "连接";
            btnConnect2.UseVisualStyleBackColor = true;
            btnConnect2.Click += btnConnect2_Click;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(585, 556);
            Controls.Add(btnConnect2);
            Controls.Add(richTextBox1);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(btnConnect);
            Controls.Add(groupBox1);
            Name = "FrmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Modbus测试工具";
            Load += FrmMain_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numBaudrate).EndInit();
            ((System.ComponentModel.ISupportInitialize)numHost).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private NumericUpDown numHost;
        private NumericUpDown numBaudrate;
        private ComboBox cbPorts;
        private Button btnConnect;
        private GroupBox groupBox2;
        private NumericUpDown numPort;
        private Label label1;
        private TextBox txtAddress;
        private Label label2;
        private GroupBox groupBox3;
        private Label label4;
        private Label label3;
        private Label label5;
        private RichTextBox richTextBox1;
        private Button btnSwitch;
        private Button btnConnect2;
    }
}