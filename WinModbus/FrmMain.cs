using System.IO.Ports;
using NewLife.IoT.Protocols;
using NewLife.Log;
using NewLife.Serial.Protocols;

namespace WinFormsApp1;

public partial class FrmMain : Form
{
    private Byte _host = 1;
    private Modbus _modbus;
    private ILog _log;

    public FrmMain() => InitializeComponent();

    private void Form1_Load(Object sender, EventArgs e)
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
            _host = (Byte)numHost.Value;
            _modbus = new ModbusRtu
            {
                PortName = cbPorts.SelectedItem + "",
                Baudrate = (Int32)numBaudrate.Value,
                Log = _log,
            };

            _modbus.Open();

            btn.Text = "断开";
            groupBox1.Enabled = false;
            groupBox2.Enabled = true;
        }
        else
        {
            _modbus.Dispose();

            btn.Text = "连接";
            groupBox1.Enabled = true;
            groupBox2.Enabled = false;
        }
    }

    private void btnOpen1_Click(Object sender, EventArgs e)
    {
        var btn = sender as Button;
        var addr = btn.Tag.ToInt() - 1;
        _modbus.WriteCoil(_host, (UInt16)addr, 0xFF00);
    }

    private void btnClose1_Click(Object sender, EventArgs e)
    {
        var btn = sender as Button;
        var addr = btn.Tag.ToInt() - 1;
        _modbus.WriteCoil(_host, (UInt16)addr, 0);
    }

    private void btnOpenAll_Click(Object sender, EventArgs e)
    {
        //for (var i = 0; i < 4; i++)
        //{
        //    _modbus.WriteCoil(_host, (UInt16)i, 0xFF00);
        //}
        _modbus.WriteCoils(_host, 0, new UInt16[] { 0xFF00, 0xFF00, 0xFF00, 0xFF00, 0xFF00, 0xFF00, 0xFF00, 0xFF00 });
    }

    private void btnCloseAll_Click(Object sender, EventArgs e)
    {
        //for (var i = 0; i < 4; i++)
        //{
        //    _modbus.WriteCoil(_host, (UInt16)i, 0);
        //}
        _modbus.WriteCoils(_host, 0, new UInt16[] { 0, 0, 0, 0, 0, 0, 0, 0 });
    }
}