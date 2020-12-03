using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace Server_ProfiChat
{
    public partial class Form1 : Form
    {

        static readonly object _lock = new object();
        static readonly Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>();
        TcpListener ServerSocket;
        TcpClient client;
        bool isConnected = false;

        public Form1()
        {
            InitializeComponent();
        }

        public void handle_clients(object o)
        {
            int id = (int)o;

            //TcpClient client;

            lock (_lock) client = list_clients[id];

            while (isConnected==true)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int byte_count = stream.Read(buffer, 0, buffer.Length);

                if (byte_count == 0)
                {
                    break;
                }

                string data = Encoding.ASCII.GetString(buffer, 0, byte_count);
                broadcast(data);
                //Console.WriteLine(data);
                //var chatline = txtChat.Text;
                txtChat.Invoke(new Action(() =>
                {
                    txtChat.Text += data;
                }));
                
                //Form1 formObj = new Form1();
                //formObj.txtChat.Text += data;
            }
            /*
            lock (_lock) list_clients.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
            */
        }

        public static void broadcast(string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data + Environment.NewLine);

            lock (_lock)
            {
                foreach (TcpClient c in list_clients.Values)
                {
                    NetworkStream stream = c.GetStream();

                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }








        private async void btnConnect_Click(object sender, EventArgs e)
        {
            int count = 1;

            string serverIP = txtServerIP.Text;
            int serverPort = Int32.Parse(txtServerPort.Text);

            ServerSocket = new TcpListener(IPAddress.Parse(serverIP), serverPort);
            ServerSocket.Start();
            lblPower.ForeColor = System.Drawing.Color.Lime;
            lblStatus.Text = "On";
            isConnected = true;
            Console.WriteLine(isConnected);

            while (true)
            {
                //TcpClient client = ServerSocket.AcceptTcpClient();
                //TcpClient
                client = await ServerSocket.AcceptTcpClientAsync();
                lock (_lock) list_clients.Add(count, client);
                Console.WriteLine("Someone connected!!");

                Thread t = new Thread(handle_clients);
                t.Start(count);
                count++;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {

        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            /*
            ServerSocket.Stop();
            lblPower.ForeColor = System.Drawing.Color.Red;
            lblStatus.Text = "Off";
            isConnected = false;
            Console.WriteLine(isConnected);
            */
            
        }

        private void btnShowClients_Click(object sender, EventArgs e)
        {
         
            txtClients.Text = "";

            foreach (KeyValuePair<int, TcpClient> kvp in list_clients)
            {
                
                txtClients.Text += "Key:" +kvp.Key+ "Value:" +kvp.Value+ "\n";
                Console.WriteLine("Key: {0}, Value: {1}", kvp.Key, kvp.Value);
            };
         
        }

        private void ShowClients ()
        {

        }
    }
}
