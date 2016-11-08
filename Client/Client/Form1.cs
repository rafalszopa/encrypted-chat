using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Numerics;

//  TO DO:
//  - Disable Send button, when the textBox is empty
//  - Focus on MessageTextBox
//  - Send message on Enter hit

namespace Client
{
    public partial class Form1 : Form
    {
        public string Host = "127.0.0.1";
        public int Port = 9000;
        public string Nick = "rafals92";
        public TcpClient Client = null;
        NetworkStream networkStream = null;
        System.Threading.Thread ctThread = null;
        string Message = null;
        bool isKeyExchanged = false;
        DiffieHellman dh = null;

        public void GetMessage()
        {
            while (true)
            {
                try
                {
                    int buffSize = Client.ReceiveBufferSize;
                    byte[] inStream = new byte[buffSize];

                    networkStream.Read(inStream, 0, buffSize);
                    Message = Encoding.ASCII.GetString(inStream);

                    JObject jsonObj = JObject.Parse(Message);

                    if(!isKeyExchanged)
                    {
                        if (jsonObj["p"] != null && jsonObj["g"] != null && jsonObj.Count == 2)
                        {
                            // Sprawdzic czy p oraz g sa poprawne (moga byc funkcje statyczne Diffie=Hellman.cs)
                            BigInteger _p, _g;
                            BigInteger.TryParse(jsonObj["p"].ToString(), out _p);
                            BigInteger.TryParse(jsonObj["g"].ToString(), out _g);
                            dh = new DiffieHellman(_p, _g);

                            // We computed B, so we gonna send it to the server
                            JObject toSend = new JObject(
                                new JProperty("a", dh.A.ToString())
                                );

                            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(toSend.ToString());

                            networkStream.Write(bytesToSend, 0, bytesToSend.Length);


                            Message = "g: " + dh.g + "\n" + "p: " + dh.p;
                            Message += "To wysyłamy serwerowi: " + dh.A;

                            displayMessage();
                        }
                        else if (jsonObj["b"] != null && jsonObj.Count == 1)
                        {
                            BigInteger _B;
                            BigInteger.TryParse(jsonObj["b"].ToString(), out _B);
                            dh.SetB(_B);
                            Message = "\n\nUstalony tajny klucz: " + dh.Key;
                            displayMessage();
                        }
                    }
                    else if (jsonObj["msg"] != null && jsonObj["from"] != null && jsonObj.Count == 2)
                    {
                        //
                    }
                    else
                    {
                        // Json object is incorrect!
                    }
                        

                        // 1. Sprawdz klucze
                        // 2. Wygeneruj A
                        // 3. Wyslij A do serwera
                        // 4. Zmien stage na 

                        // We have received message
                        // 1. Check if json
                        // 2. Check if we are authorized
                        //      - if so, we expect message
                        //      - otherwise, keys etc.

                        //DiffieHellman DH = new DiffieHellman(23, 5);

                    //Message = "p:" + DH.p + ", g: " + DH.g + ", b: " + DH.b + ", B: " + DH.B;

                    //displayMessage();

                } catch (Exception Ex)
                {
                    // Log exception
                }
            }
        }

        public void displayMessage()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(displayMessage));
            }
            else
            {
                ChatTextBox.Text +=  Environment.NewLine + " >> " + Message + "\n";
            }
        }

        public void Connect()
        {
            // Assign nick, host and port variables
            // and check if not empty

            try
            {
                // Connect to the server
                Client = new TcpClient();
                Client.Connect(Host, Port);
                networkStream = Client.GetStream();

                // Send request to the server
                //JObject request = new JObject(@"{'request':'keys'}");
                JObject request = new JObject(
                    new JProperty("request", "keys")
                    );

                label5.Text += request.ToString();

                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(request.ToString());

                networkStream.Write(bytesToSend, 0, bytesToSend.Length);

                ctThread = new System.Threading.Thread(GetMessage);
                ctThread.Start();



                //Does not work
                StatusLabel.ForeColor = Color.Green;
                StatusLabel.Text = "Connection opened successfully";
            }
            catch(SocketException ex)
            {
                Client = null;
                StatusLabel.ForeColor = Color.Red;
                //Does not work
                StatusLabel.Text = $"Can not connect to the server. Error: {ex.Message}";
                MessageTextBox.Text = "Something went wrong";
                return;
            }

            ConnectBtn.Text = "Disconnect";
            NickTextBox.Enabled = false;
            IPtextBox.Enabled = false;
            PortTextBox.Enabled = false;
            SendBtn.Enabled = true;
            MessageTextBox.Enabled = true;
        }

        public void Disconnect()
        {
            try
            {
                ctThread.Abort();
                ctThread = null;

                Client.Close();
                Client = null;
                networkStream = null;
            }
            catch(Exception ex)
            {

            }

            StatusLabel.ForeColor = Color.Green;
            StatusLabel.Text = "Connection closed successfully";

            ConnectBtn.Text = "Connect";
            NickTextBox.Enabled = true;
            IPtextBox.Enabled = true;
            PortTextBox.Enabled = true;
            SendBtn.Enabled = false;
            MessageTextBox.Text = "";
            MessageTextBox.Enabled = false;

            // Null nick, host and port variables
        }
        
        public void SendData()
        {
            if(Client == null)
            {
                StatusLabel.ForeColor = Color.Red;
                StatusLabel.Text = "Connection is not opened";
                return;
            }
            else if (String.IsNullOrWhiteSpace(MessageTextBox.Text))
            {
                StatusLabel.ForeColor = Color.Red;
                StatusLabel.Text = "Nothing to send";
                MessageTextBox.Text = "";
                return;
            }
            else
            {
                StatusLabel.ForeColor = Color.Red;
                StatusLabel.Text = "Connection has lost";
                Disconnect();
                return;
            }
                

            // When data was sent
            //ChatTextBox.Text += $"[Me]: {MessageTextBox.Text}\n";
            //MessageTextBox.Text = "";
        }

        // Main function
        public Form1()
        {
            InitializeComponent();
        }

        private void SendBtn_Click(object sender, EventArgs e)
        {
            SendData();
        }

        private void ConnectBtn_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            string option = btn.Text;

            switch(option)
            {
                case "Connect":
                    Connect();
                    break;
                case "Disconnect":
                    Disconnect();
                    break;
                default:
                    StatusLabel.ForeColor = Color.Red;
                    StatusLabel.Text = "Some error has occured";
                    break;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();
        }
    }
}
