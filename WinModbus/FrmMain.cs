using System.IO.Ports;
using NewLife.IoT.Protocols;
using NewLife.Log;
using NewLife.Serial.Protocols;

namespace WinModbus;

public partial class FrmMain : Form
{
    private Modbus _modbus;
    private ILog _log;

    public FrmMain()
    {
        InitializeComponent();
    }

    private void FrmMain_Load(Object sender, EventArgs e)
    {
        cbPorts.DataSource = SerialPort.GetPortNames();

        _log = new TextControlLog
        {
            Control = richTextBox1,
            Level = LogLevel.Debug
        };
    }

    private void btnConnect_Click(Object sender, EventArgs e)
    {
        var btn = sender as Button;
        if (btn.Text == "连接")
        {
            _modbus = new ModbusRtu
            {
                PortName = cbPorts.SelectedItem + "",
                Baudrate = (Int32)numBaudrate.Value,
                Log = _log,
            };

            _modbus.Open();

            btn.Text = "断开";
            groupBox1.Enabled = false;
            groupBox2.Enabled = false;
            groupBox3.Enabled = true;
        }
        else
        {
            _modbus.Dispose();

            btn.Text = "连接";
            groupBox1.Enabled = true;
            groupBox2.Enabled = true;
            groupBox3.Enabled = false;
        }
    }

    private void btnConnect2_Click(Object sender, EventArgs e)
    {
        var btn = sender as Button;
        if (btn.Text == "连接")
        {
            _modbus = new ModbusTcp
            {
                Server = $"{txtAddress.Text}:{numPort.Value}",
                Log = _log,
            };

            _modbus.Open();

            btn.Text = "断开";
            groupBox1.Enabled = false;
            groupBox2.Enabled = false;
            groupBox3.Enabled = true;
        }
        else
        {
            _modbus.Dispose();

            btn.Text = "连接";
            groupBox1.Enabled = true;
            groupBox2.Enabled = true;
            groupBox3.Enabled = false;
        }
    }

    private void btnSwitch_Click(Object sender, EventArgs e)
    {
        var host = (Byte)numHost.Value;
        var frm = new FrmSwitch
        {
            Modbus = _modbus,
            Host = host,
        };
        frm.Show();
    }
}
