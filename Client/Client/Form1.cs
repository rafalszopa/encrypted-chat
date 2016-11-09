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
        Encryption encryption = Encryption.none;

        string DebugMsg = "";

        public void GetMessage()
        {
            while (true)
            {
                try
                {
                    int buffSize = Client.ReceiveBufferSize;
                    byte[] inStream = new byte[buffSize];

                    networkStream.Read(inStream, 0, buffSize);
                    string received = Encoding.ASCII.GetString(inStream);

                    JObject jsonObj = JObject.Parse(received);

                    if (!isKeyExchanged)
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
                                new JProperty("a", dh.A.ToString()));

                            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(toSend.ToString());

                            networkStream.Write(bytesToSend, 0, bytesToSend.Length);

                            Message = "g: " + dh.g + "\n" + "p: " + dh.p;
                            Message += "To wysyłamy serwerowi: " + dh.A;

                            //displayMessage();
                        }
                        else if (jsonObj["b"] != null && jsonObj.Count == 1)
                        {
                            BigInteger _B;
                            BigInteger.TryParse(jsonObj["b"].ToString(), out _B);
                            dh.SetB(_B);
                            Message = "\n\nUstalony tajny klucz: " + dh.Key;
                            isKeyExchanged = true;
                            displayMessage();
                        }
                    }
                    else if (jsonObj["msg"] != null && jsonObj["from"] != null && jsonObj.Count == 2)
                    {
                        if(encryption == Encryption.cezar)
                            Message = "[" + jsonObj["from"].ToString() + "]: " + CaesarShift.Decode(Base64.Decode(jsonObj["msg"].ToString()), (int)dh.Key);

                        else if(encryption == Encryption.xor)
                            Message = "[" + jsonObj["from"].ToString() + "]: " + XOR.Code(Base64.Decode(jsonObj["msg"].ToString()), (int)dh.Key);
                        
                        else if(encryption == Encryption.none)
                            Message = "[" + jsonObj["from"].ToString() + "]: " + Base64.Decode(jsonObj["msg"].ToString());

                        displayMessage();
                    }
                    else
                    {
                        // Json object is incorrect!
                    }

                } catch (Exception Ex)
                {
                    // Log exception
                    Message = "!!! " + Ex.Message;
                    displayMessage();
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
                ChatTextBox.Text +=  Environment.NewLine + Message + "\n";
            }
        }

        public void UpdateLabel()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(UpdateLabel));
            }
            else
            {
                //label5.Text = Environment.NewLine + " >> " + DebugMsg + "\n";
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

                JObject request = new JObject(
                    new JProperty("request", "keys"));

                //label5.Text += request.ToString();

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
                Message = "!!! " + ex.Message;
                displayMessage();
                return;
            }

            ConnectBtn.Text = "Disconnect";
            NickTextBox.Enabled = false;
            IPtextBox.Enabled = false;
            PortTextBox.Enabled = false;
            SendBtn.Enabled = true;
            MessageTextBox.Enabled = true;
            buttonApply.Enabled = true;
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
                Message = "!!! " + ex.Message;
                displayMessage();
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

            string messageFromTextbox = MessageTextBox.Text;
            string messageToSend = "";

            if (encryption == Encryption.cezar)
                messageToSend = CaesarShift.Encode(messageFromTextbox, (int)dh.Key);

            else if (encryption == Encryption.xor)
                messageToSend = XOR.Code(messageFromTextbox, (int)dh.Key);

            else if (encryption == Encryption.none)
                messageToSend = messageFromTextbox;

            JObject toSend = new JObject(
                    new JProperty("msg", Base64.Encode(messageToSend)),
                    new JProperty("from", Nick));

            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(toSend.ToString());

            //label5.Text = toSend.ToString();
            //label5.Text += "\n" + dh.Key.ToString();

            networkStream.Write(bytesToSend, 0, bytesToSend.Length);

            // When data was sent
            ChatTextBox.Text += $"[Me]: {MessageTextBox.Text}\n";
            MessageTextBox.Text = "";
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

        private void buttonApply_Click(object sender, EventArgs e)
        {
            string encryptionMode = "";

            if (radioButtonNone.Checked && encryption != Encryption.none)
            {
                encryptionMode = "none";
                encryption = Encryption.none;
            }

            else if (radioButtonCaesar.Checked && encryption != Encryption.cezar)
            {
                encryptionMode = "cezar";
                encryption = Encryption.cezar;
            }
            else if (radioButtonXOR.Checked && encryption != Encryption.xor)
            {
                encryptionMode = "xor";
                encryption = Encryption.xor;
            }
            else
            {
                return;
            }

            JObject toSend = new JObject(new JProperty("encryption", encryptionMode));
            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(toSend.ToString());
            networkStream.Write(bytesToSend, 0, bytesToSend.Length);
        }
    }
}
