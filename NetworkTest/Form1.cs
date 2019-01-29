using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkTest
{
    public partial class Form1 : Form
    {
        TcpClient client;
        TcpListener listener;
        Thread writethread;
        Thread readthread;
        NetworkStream ReadStream { get; set; }
        NetworkStream WriteStream { get; set; }
        public string jsonstring { get; set; }
        IPEndPoint mediatorAddress { get; set; } = new IPEndPoint(IPAddress.Loopback, 9000);
        IPEndPoint WPFAddress { get; set; } = new IPEndPoint(IPAddress.Loopback, 9010);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            readthread = new Thread(new ThreadStart(ReceiveMessageFromWPF));
        }

        private void ReceiveMessageFromWPF()
        {
            listener = new TcpListener(mediatorAddress);
            listener.Start();

            byte[] bytes = new byte[256];
            string data = null;
            ReadStream = client.GetStream();
            int i;

            while ((i = ReadStream.Read(bytes, 0, bytes.Length)) != 0)
            {
                data = (Encoding.UTF8.GetString(bytes, 0, i)).ToUpper();

                byte[] msg = Encoding.UTF8.GetBytes(data);
                ReadStream.Write(msg, 0, msg.Length);
            }

            richTextBox1.Text = ReadStream.ToString();
            client.Close();
        }

        public void TransferJSONStringToWPF()
        {
            using (client = new TcpClient(WPFAddress))
            {
                client.Connect(WPFAddress);
                WriteStream = client.GetStream();
                char[] chars = new char[256];
                byte[] bytes = new byte[256];
                int i;

                using (StreamReader sr = new StreamReader(jsonstring))
                {
                    while ((i = sr.ReadBlock(chars, 0, chars.Length)) != 0)
                    {
                        bytes = Encoding.UTF8.GetBytes(chars);
                        WriteStream.Write(bytes, 0, bytes.Length);
                    }
                }
            }

            writethread.Abort();
        }
    }
}
