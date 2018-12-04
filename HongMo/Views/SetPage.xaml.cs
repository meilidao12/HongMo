using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HongMo.Models;
using static System.Diagnostics.Debug;
using ProtocolFamily.Modbus;
using Services;
using System.Reflection;
using System.Threading;
namespace HongMo.Views
{
    /// <summary>
    /// SetPage.xaml 的交互逻辑
    /// </summary>
    public partial class SetPage : Page
    {
        const string RunModelAddr = "0100"; //2字节
        const string SerialPortsAddr = "0102"; //4字节
        const string NetworkProtocolAddr = "0106"; //2字节
        const string LocalIPAddr = "0108"; //4字节
        const string LocalPortAddr = "0112"; //2字节
        const string ServerIPAddr = "0114"; //4字节
        const string ServerPortAddr = "0118"; //2字节
        const string Sub_MaskAddr = "0124"; //4字节 子网掩码
        const string Gateway_IPAddr = "0120"; //4字节 网关
        const string RS_232_BaudAddr = "0200"; //4字节
        const string RS_485_1_BaudAddr = "0300"; //4字节
        const string RS_485_2_BaudAddr = "0400"; //4字节
        Modbus md = new Modbus();
        List<string> FunctionNames;
        IniHelper ini = new IniHelper(System.IO.Directory.GetCurrentDirectory() + @"\Set.ini");
        public SetPage()
        {
            InitializeComponent();
            this.Loaded += SetPage_Loaded;
        }

        private void SetPage_Loaded(object sender, RoutedEventArgs e)
        {
            List<RunModel> runModels = new List<RunModel>();
            runModels.Add(new RunModel { Name = "透传", Value = "0" });
            runModels.Add(new RunModel { Name = "Modbus", Value = "1" });
            this.cmbRunModel.ItemsSource = runModels;
            this.cmbRunModel.DisplayMemberPath = "Name";
            this.cmbRunModel.SelectedValuePath = "Value";
            this.cmbRunModel.SelectedValue = ini.ReadIni("Config", "RunModel");

            this.chkRS232.IsChecked = LogicConvertToBool(ini.ReadIni("Config", "SerialPorts").Substring(0,1));
            this.RS485_1.IsChecked = LogicConvertToBool(ini.ReadIni("Config", "SerialPorts").Substring(1, 1));
            this.RS485_2.IsChecked = LogicConvertToBool(ini.ReadIni("Config", "SerialPorts").Substring(2, 1));

            this.TCPClient.IsChecked = !LogicConvertToBool(ini.ReadIni("Config", "NetworkProtocol"));
            this.UDPServer.IsChecked = LogicConvertToBool(ini.ReadIni("Config", "NetworkProtocol"));

            this.LocalIP.Text = ini.ReadIni("Config", "LocalIP");
            this.Sub_Mask.Text = ini.ReadIni("Config", "Sub_Mask");
            this.Gateway_IP.Text = ini.ReadIni("Config", "Gateway_IP");
            this.LocalPort.Text = ini.ReadIni("Config", "LocalPort");
            this.ServerIP.Text = ini.ReadIni("Config", "ServerIP");
            this.ServerPort.Text = ini.ReadIni("Config", "ServerPort");

            List<BaudModel> baudModels = new List<BaudModel>();
            baudModels.Add(new BaudModel { Name = "2400", Value = "2400" });
            baudModels.Add(new BaudModel { Name = "4800", Value = "4800" });
            baudModels.Add(new BaudModel { Name = "9600", Value = "9600" });
            baudModels.Add(new BaudModel { Name = "19200", Value = "19200" });
            baudModels.Add(new BaudModel { Name = "38400", Value = "38400" });
            baudModels.Add(new BaudModel { Name = "57600", Value = "57600" });
            this.cmbRS232BaudRate.ItemsSource = baudModels;
            this.cmbRS232BaudRate.DisplayMemberPath = "Name";
            this.cmbRS232BaudRate.SelectedValuePath = "Value";
            this.cmbRS232BaudRate.SelectedValue = ini.ReadIni("Config", "RS_232_Baud");
            this.cmbRS485_1BaudRate.ItemsSource = baudModels;
            this.cmbRS485_1BaudRate.DisplayMemberPath = "Name";
            this.cmbRS485_1BaudRate.SelectedValuePath = "Value";
            this.cmbRS485_1BaudRate.SelectedValue = ini.ReadIni("Config", "RS_485_1_Baud");
            this.cmbRS485_2BaudRate.ItemsSource = baudModels;
            this.cmbRS485_2BaudRate.DisplayMemberPath = "Name";
            this.cmbRS485_2BaudRate.SelectedValuePath = "Value";
            this.cmbRS485_2BaudRate.SelectedValue = ini.ReadIni("Config", "RS_485_2_Baud");
        }

        private void btnRead_Click(object sender, RoutedEventArgs e)
        {
            FunctionNames = new List<string>();
            FunctionNames.Add("ReadRunModel");
            FunctionNames.Add("ReadSerialPorts");
            FunctionNames.Add("ReadNetworkProtocol");
            FunctionNames.Add("ReadLocalIP");
            FunctionNames.Add("ReadSub_Mask");
            FunctionNames.Add("ReadGateway_IP");
            FunctionNames.Add("ReadLocalPort");
            FunctionNames.Add("ReadServerIP");
            FunctionNames.Add("ReadServerPort");
            FunctionNames.Add("ReadRS_232_Baud");
            FunctionNames.Add("ReadRS_485_1_Baud");
            FunctionNames.Add("ReadRS_485_2_Baud");
            ExcuteFunction();
        }

        private void btnWrite_Click(object sender, RoutedEventArgs e)
        {
            FunctionNames = new List<string>();
            FunctionNames.Add("SendRunModel");
            FunctionNames.Add("SendSerialPorts");
            FunctionNames.Add("SendNetworkProtocol");
            FunctionNames.Add("SendLocalIP");
            FunctionNames.Add("SendSub_Mask");
            FunctionNames.Add("SendGateway_IP");
            FunctionNames.Add("SendLocalPort");
            FunctionNames.Add("SendServerIP");
            FunctionNames.Add("SendServerPort");
            FunctionNames.Add("SendRS_232_Baud");
            FunctionNames.Add("SendRS_485_1_Baud");
            FunctionNames.Add("SendRS_485_2_Baud");
            if (!this.CheckFormat()) return;
            ExcuteFunction();
        }

        private void ExcuteFunction()
        {
            try
            {
                Type p = this.GetType();
                foreach (var item in FunctionNames)
                {
                    MethodInfo method = p.GetMethod(item);
                    string result = (string)method.Invoke(this, null);
                    Dispatcher.BeginInvoke(new Action(delegate{
                        this.lstResult.Items.Add(result);
                    }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// 运行模式
        /// </summary>
        /// <returns></returns>
        public string SendRunModel()
        {
            md.SlaveId = "01";
            md.Address = RunModelAddr;
            md.Flag = false;
            byte[] a = new byte[2];
            a[0] = 0;
            a[1] = byte.Parse(this.cmbRunModel.SelectedValue.ToString());
            Send(md.CreateModbusSendData(a));
            if (!md.Flag) return "运行模式写入失败";
            ini.WriteIni("Config", "RunModel", a[1].ToString());
            return "运行模式写入成功";
        }
        public string ReadRunModel()
        {
            md.SlaveId = "01";
            md.Address = RunModelAddr;
            md.Flag = false;
            Send(md.CreateModbusSendDataForRead(2));
            if (!md.Flag) return "运行模式读取失败";
            this.cmbRunModel.SelectedValue = md.ReceiveData.Substring(9, 1);
            return "运行模式读取成功";
        }

        /// <summary>
        /// 写入串口号
        /// </summary>
        /// <returns></returns>
        public string SendSerialPorts()
        {
            md.SlaveId = "01";
            md.Address = SerialPortsAddr;
            md.Flag = false;
            byte[] a = new byte[4];
            a[0] = GetCheckBoxValue(this.chkRS232);
            a[1] = GetCheckBoxValue(this.RS485_1);
            a[2] = GetCheckBoxValue(this.RS485_2);
            a[3] = 0;
            Send(md.CreateModbusSendData(a));
            if (!md.Flag) return "使能串口写入失败";
            ini.WriteIni("Config", "SerialPorts", a[0].ToString()+a[1].ToString()+a[2].ToString());
            return "使能串口写入成功";
        }
        public string ReadSerialPorts()
        {
            md.SlaveId = "01";
            md.Address = SerialPortsAddr;
            md.Flag = false;
            Send(md.CreateModbusSendDataForRead(4));
            if (!md.Flag) return "使能串口读取失败";
            this.chkRS232.IsChecked = LogicConvertToBool(md.ReceiveData.Substring(7, 1));
            this.RS485_1.IsChecked = LogicConvertToBool(md.ReceiveData.Substring(9, 1));
            this.RS485_2.IsChecked = LogicConvertToBool(md.ReceiveData.Substring(11, 1));
            return "使能串口读取成功";
        }

        /// <summary>
        /// 写入网络通讯协议
        /// </summary>
        /// <returns></returns>
        public string SendNetworkProtocol()
        {
            md.SlaveId = "01";
            md.Address = NetworkProtocolAddr;
            md.Flag = false;
            byte[] a = new byte[2];
            a[0] = 0;
            a[1] = GetRadioButtonValue(this.UDPServer);
            Send(md.CreateModbusSendData(a));
            if (!md.Flag) return "网络通讯协议写入失败";
            ini.WriteIni("Config", "NetworkProtocol", a[1].ToString());
            return "网络通讯协议写入成功";
        }
        public string ReadNetworkProtocol()
        {
            md.SlaveId = "01";
            md.Address = NetworkProtocolAddr;
            md.Flag = false;
            Send(md.CreateModbusSendDataForRead(2));
            if (!md.Flag) return "网络通讯协议读取失败";
            this.TCPClient.IsChecked = !LogicConvertToBool(md.ReceiveData.Substring(9, 1));
            this.UDPServer.IsChecked = LogicConvertToBool(md.ReceiveData.Substring(9, 1));
            return "网络通讯协议读取成功";
        }

        /// <summary>
        /// 写入模块IP
        /// </summary>
        /// <returns></returns>
        public string SendLocalIP()
        {
            md.SlaveId = "01";
            md.Address = LocalIPAddr;
            md.Flag = false;
            byte[] a = new byte[4];
            for(int i=0;i<4;i++)
            {
                a[i] =byte.Parse(this.LocalIP.Text.Split('.').ToArray()[i]);
            }
            Send(md.CreateModbusSendData(a));
            if (!md.Flag) return "模块IP写入失败";
            ini.WriteIni("Config", "LocalIP", this.LocalIP.Text);
            return "模块IP写入成功";
        }
        public string ReadLocalIP()
        {
            md.SlaveId = "01";
            md.Address = LocalIPAddr;
            md.Flag = false;
            Send(md.CreateModbusSendDataForRead(4));
            if (!md.Flag) return "模块IP读取失败";
            MathHelper math = new MathHelper();
            byte[] a =math.HexConvertToByte(md.ReceiveData.Substring(6, 8));
            this.LocalIP.Text = string.Format("{0}.{1}.{2}.{3}", a[0].ToString(), a[1].ToString() , a[2].ToString() ,a[3].ToString());
            return "模块IP读取成功";
        }

        /// <summary>
        /// 写入子网掩码
        /// </summary>
        /// <returns></returns>
        public string SendSub_Mask()
        {
            md.SlaveId = "01";
            md.Address = Sub_MaskAddr;
            md.Flag = false;
            byte[] a = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                a[i] = byte.Parse(this.Sub_Mask.Text.Split('.').ToArray()[i]);
            }
            Send(md.CreateModbusSendData(a));
            if (!md.Flag) return "子网掩码写入失败";
            ini.WriteIni("Config", "Sub_Mask", this.Sub_Mask.Text);
            return "子网掩码写入成功";
        }
        public string ReadSub_Mask()
        {
            md.SlaveId = "01";
            md.Address = Sub_MaskAddr;
            md.Flag = false;
            Send(md.CreateModbusSendDataForRead(4));
            if (!md.Flag) return "子网掩码读取失败";
            MathHelper math = new MathHelper();
            byte[] a = math.HexConvertToByte(md.ReceiveData.Substring(6, 8));
            this.Sub_Mask.Text = string.Format("{0}.{1}.{2}.{3}", a[0].ToString(), a[1].ToString(), a[2].ToString(), a[3].ToString());
            return "子网掩码读取成功";
        }

        /// <summary>
        /// 写入网关
        /// </summary>
        /// <returns></returns>
        public string SendGateway_IP()
        {
            md.SlaveId = "01";
            md.Address = Gateway_IPAddr;
            md.Flag = false;
            byte[] a = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                a[i] = byte.Parse(this.Gateway_IP.Text.Split('.').ToArray()[i]);
            }
            Send(md.CreateModbusSendData(a));
            if (!md.Flag) return "网关写入失败";
            ini.WriteIni("Config", "Gateway_IP", this.Gateway_IP.Text);
            return "网关写入成功";
        }
        public string ReadGateway_IP()
        {
            md.SlaveId = "01";
            md.Address = Gateway_IPAddr;
            md.Flag = false;
            Send(md.CreateModbusSendDataForRead(4));
            if (!md.Flag) return "网关读取失败";
            MathHelper math = new MathHelper();
            byte[] a = math.HexConvertToByte(md.ReceiveData.Substring(6, 8));
            this.Gateway_IP.Text = string.Format("{0}.{1}.{2}.{3}", a[0].ToString(), a[1].ToString(), a[2].ToString(), a[3].ToString());
            return "网关读取成功";
        }

        /// <summary>
        /// 写入模块端口号
        /// </summary>
        /// <returns></returns>
        public string SendLocalPort()
        {
            md.SlaveId = "01";
            md.Address = LocalPortAddr;
            md.Flag = false;
            byte[] a = BitConverter.GetBytes(Convert.ToInt16(this.LocalPort.Text));
            Array.Reverse(a); //反转数组
            Send(md.CreateModbusSendData(a));
            if (!md.Flag) return "模块端口号写入失败";
            ini.WriteIni("Config", "LocalPort", this.LocalPort.Text);
            return "模块端口号写入成功";
        }
        public string ReadLocalPort()
        {
            md.SlaveId = "01";
            md.Address = LocalPortAddr;
            md.Flag = false;
            Send(md.CreateModbusSendDataForRead(2));
            if (!md.Flag) return "模块端口号读取失败";
            MathHelper math = new MathHelper();
            byte[] a = math.HexConvertToByte(md.ReceiveData.Substring(6, 4));
            Array.Reverse(a);
            this.LocalPort.Text = BitConverter.ToInt16(a, 0).ToString();
            return "模块端口号读取成功";
        }

        /// <summary>
        /// 写入服务器IP
        /// </summary>
        /// <returns></returns>
        public string SendServerIP()
        {
            md.SlaveId = "01";
            md.Address = ServerIPAddr;
            md.Flag = false;
            byte[] a = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                a[i] = byte.Parse(this.ServerIP.Text.Split('.').ToArray()[i]);
            }
            Send(md.CreateModbusSendData(a));
            if (!md.Flag) return "服务器IP写入失败";
            ini.WriteIni("Config", "ServerIP", this.ServerIP.Text);
            return "服务器IP写入成功";
        }
        public string ReadServerIP()
        {
            md.SlaveId = "01";
            md.Address = ServerIPAddr;
            md.Flag = false;
            Send(md.CreateModbusSendDataForRead(4));
            if (!md.Flag) return "服务器IP读取失败";
            MathHelper math = new MathHelper();
            byte[] a = math.HexConvertToByte(md.ReceiveData.Substring(6, 8));
            this.ServerIP.Text = string.Format("{0}.{1}.{2}.{3}", a[0].ToString(), a[1].ToString(), a[2].ToString(), a[3].ToString());
            return "服务器IP读取成功";
        }

        /// <summary>
        /// 写入服务器端口号
        /// </summary>
        /// <returns></returns>
        public string SendServerPort()
        {
            md.SlaveId = "01";
            md.Address = ServerPortAddr;
            md.Flag = false;
            byte[] a = BitConverter.GetBytes(Convert.ToInt16(this.ServerPort.Text));
            Array.Reverse(a); //反转数组
            Send(md.CreateModbusSendData(a));
            if (!md.Flag) return " 服务器端口号写入失败";
            ini.WriteIni("Config", "ServerPort", this.ServerPort.Text);
            return "服务器端口号写入成功";
        }
        public string ReadServerPort()
        {
            md.SlaveId = "01";
            md.Address = ServerPortAddr;
            md.Flag = false;
            Send(md.CreateModbusSendDataForRead(2));
            if (!md.Flag) return "服务器端口号读取失败";
            MathHelper math = new MathHelper();
            byte[] a = math.HexConvertToByte(md.ReceiveData.Substring(6, 4));
            Array.Reverse(a);
            this.ServerPort.Text = BitConverter.ToInt16(a, 0).ToString();
            return "服务器端口号读取成功";
        }

        /// <summary>
        /// 写rs232波特率
        /// </summary>
        /// <returns></returns>
        public string SendRS_232_Baud()
        {
            md.SlaveId = "01";
            md.Address = RS_232_BaudAddr;
            md.Flag = false;
            byte[] a = BitConverter.GetBytes(Convert.ToInt32(this.cmbRS232BaudRate.SelectedValue));
            Array.Reverse(a); //反转数组
            Send(md.CreateModbusSendData(a));
            if (!md.Flag) return "RS_232波特率写入失败";
            ini.WriteIni("Config", "RS_232_Baud", this.cmbRS232BaudRate.SelectedValue.ToString());
            return "RS_232波特率写入成功";
        }
        public string ReadRS_232_Baud()
        {
            md.SlaveId = "01";
            md.Address = RS_232_BaudAddr;
            md.Flag = false;
            Send(md.CreateModbusSendDataForRead(4));
            if (!md.Flag) return "RS_232波特率读取失败";
            MathHelper math = new MathHelper();
            byte[] a = math.HexConvertToByte(md.ReceiveData.Substring(6, 8));
            Array.Reverse(a);
            this.cmbRS232BaudRate.SelectedValue = BitConverter.ToInt32(a, 0).ToString();
            return "RS_232波特率读取成功";
        }

        /// <summary>
        /// 写rs485_1波特率
        /// </summary>
        /// <returns></returns>
        public string SendRS_485_1_Baud()
        {
            md.SlaveId = "01";
            md.Address = RS_485_1_BaudAddr;
            md.Flag = false;
            byte[] a = BitConverter.GetBytes(Convert.ToInt32(this.cmbRS485_1BaudRate.SelectedValue));
            Array.Reverse(a); //反转数组
            Send(md.CreateModbusSendData(a));
            if (!md.Flag) return "RS_485_1波特率写入失败";
            ini.WriteIni("Config", "RS_485_1_Baud", this.cmbRS485_1BaudRate.SelectedValue.ToString());
            return "RS_485_1波特率写入成功";
        }
        public string ReadRS_485_1_Baud()
        {
            md.SlaveId = "01";
            md.Address = RS_485_1_BaudAddr;
            md.Flag = false;
            Send(md.CreateModbusSendDataForRead(4));
            if (!md.Flag) return "RS_485_1波特率读取失败";
            MathHelper math = new MathHelper();
            byte[] a = math.HexConvertToByte(md.ReceiveData.Substring(6, 8));
            Array.Reverse(a);
            this.cmbRS485_1BaudRate.SelectedValue = BitConverter.ToInt32(a, 0).ToString();
            return "RS_485_1波特率读取成功";
        }

        /// <summary>
        /// 写rs485_2波特率
        /// </summary>
        /// <returns></returns>
        public string SendRS_485_2_Baud()
        {
            md.SlaveId = "01";
            md.Address = RS_485_2_BaudAddr;
            md.Flag = false;
            byte[] a = BitConverter.GetBytes(Convert.ToInt32(this.cmbRS485_2BaudRate.SelectedValue));
            Array.Reverse(a); //反转数组
            Send(md.CreateModbusSendData(a));
            if (!md.Flag) return "RS_485_2波特率写入失败";
            ini.WriteIni("Config", "RS_485_2_Baud", this.cmbRS485_2BaudRate.SelectedValue.ToString());
            return "RS_485_2波特率写入成功";
        }
        public string ReadRS_485_2_Baud()
        {
            md.SlaveId = "01";
            md.Address = RS_485_2_BaudAddr;
            md.Flag = false;
            Send(md.CreateModbusSendDataForRead(4));
            if (!md.Flag) return "RS_485_2波特率读取失败";
            MathHelper math = new MathHelper();
            byte[] a = math.HexConvertToByte(md.ReceiveData.Substring(6, 8));
            Array.Reverse(a);
            this.cmbRS485_2BaudRate.SelectedValue = BitConverter.ToInt32(a, 0).ToString();
            return "RS_485_2波特率读取成功";
        }

        private void Send(string data)
        {
            COMMHelper comm = new COMMHelper();
            string com = ini.ReadIni("Ini", "COM");
            comm.OpenPort(com, "9600");
            comm.DataReceiveEvent += Comm_DataReceiveEvent;
            comm.SendAsString(data);
            Thread.Sleep(300);
            comm.ClosePort();
        }

        private byte GetRadioButtonValue(RadioButton radioButton)
        {
            if (radioButton.IsChecked == true)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private byte GetCheckBoxValue(CheckBox checkBox)
        {
            if(checkBox.IsChecked == true)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Comm_DataReceiveEvent(object sender, COMMEventArgs e)
        {
            string data = e.BackDataAsHex.Substring(0,e.BackDataAsHex.Length - 4);
            string crcdata = e.BackDataAsHex.Substring(e.BackDataAsHex.Length - 4,4);
            if(crcdata == CRC.ToModbusCRC16(data))
            {
                md.ReceiveData = data; 
                md.Flag = true;
            }
        }

        /// <summary>
        /// 检查输入是否有误
        /// </summary>
        /// <returns></returns>
        private bool CheckFormat()
        {
            try
            {
                if ((this.chkRS232.IsChecked == true || this.RS485_1.IsChecked == true || this.RS485_2.IsChecked == true) == false)
                {
                    MessageBox.Show("请选择可用的串口");
                    return false;
                }
                CheckIP(this.LocalIP);
                CheckIP(this.ServerIP);
                CheckPort(this.LocalPort);
                CheckPort(this.ServerPort);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 检查IP地址
        /// </summary>
        /// <param name="textBox"></param>
        private void CheckIP(TextBox textBox)
        {
            try
            {
                string[] localIP = textBox.Text.Split('.');
                if (localIP.Length != 4) throw new Exception();
                foreach (var item in localIP)
                {
                    if (int.Parse(item) >= 255)
                    {
                        throw new Exception();
                    }
                }
            }
            catch
            {
                MessageBox.Show("IP输入有误");
                throw new Exception();
            }
        }

        /// <summary>
        /// 检查端口号
        /// </summary>
        /// <param name="textBox"></param>
        private void CheckPort(TextBox textBox)
        {
            try
            {
                if (int.Parse(textBox.Text) > 65534) throw new Exception();
            }
            catch
            {
                MessageBox.Show("端口号输入有误");
                throw new Exception();
            }
        }
        
        private bool LogicConvertToBool(string logic)
        {
            switch(logic)
            {
                case "1":
                    return true;
                case "0":
                    return false;
                default:
                    throw new Exception("非法值转换为布尔类型");
                    
            }
        }
    }
}
