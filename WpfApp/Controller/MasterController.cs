using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using SHA3;
using System.Net.NetworkInformation;
using WpfApp.Views.Controller.Interfaces;
using WpfApp.Views.Controller;

namespace WpfApp.Controller
{
    public sealed class MasterController
    {
        static MasterController instance = null;
        static object padlock = new object();
        Model mdl;
        ArrayList ctrlList;
        public Thread[] writethreads { get; set; }
        public Thread[] readthreads { get; set; }
        bool[] threadEndingFlag = new bool[] { false, false };
        bool? loginsuccess;
        //string publickey;

        public MasterController()
        {
            instance = this;
            mdl = new Model();
            ctrlList = new ArrayList();
            InstantiateViewControllers();
            writethreads = new Thread[2];
            readthreads = new Thread[2];
            Start();
        }

        public static MasterController Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new MasterController();
                    }

                    return instance;
                }
            }
        }

        public void AddController(object ctrl)
        {
            if (ctrl.GetType().IsSealed && ctrl is IController)
            {
                ctrlList.Add(ctrl);
            }
            else
            {
                MessageBox.Show
                (
                    "Der einzutragende Controller hat gegen die\n" +
                    "Konventionen des MasterControllers verstoßen!\n" +
                    "Alle einzutragenden Controller müssen versiegelt sein und das Interface \"IController\" implementieren.\n\n" +
                    "[" + ctrl.GetType() + "]",
                    "Controller Konventions Verstoß",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        void InstantiateViewControllers()
        {
            AccountLoginController alctrl = new AccountLoginController();
            ctrlList.Add(alctrl);
            MeetingPanelController mpctrl = new MeetingPanelController();
            ctrlList.Add(mpctrl);
        }

        void Start()
        {
            ArrayList temp = null;
            bool loggedin = false;
            bool? loginloop = false;
            string[] dayslist = null;

            /*MeetingPanel mp = new MeetingPanel();
            mp.ShowDialog();*/

            foreach (object ctrl in ctrlList)
            {
                if (ctrl is IAccountView)
                {
                    loginsuccess = ((IAccountView)ctrl).ShowView();
                }
            }

            //Tritt ein wenn Login-Dialog geschlossen ist
            if (loginsuccess == false)
            {
                while (!loggedin & loginloop == false)
                {
                    PrepareLoginJSONForTransmission(mdl.GetUserAccountData()[0], mdl.GetUserAccountData()[1]);
                    StartThread(0, mdl.GetWriteDataJSONObject(), mdl.GetReadLoginJSONObject());

                    //temp = parseLoginJSONString(mdl.GetReadLoginJSON());
                    temp = new ArrayList() { new Model.JSONHeader(new byte[] { 100, 100, 100, 100 }, 0, Model.TransmissionReason.ReceiveAccountData), new Model.LoginItem("test", "login") };    //TODO: nur zum Testen, später auskommentieren

                    if (
                        temp != null &&
                        ((Model.LoginItem)temp[1]).loginname == mdl.GetUserAccountData()[0] &&
                        ((Model.LoginItem)temp[1]).password == mdl.GetUserAccountData()[1]
                       )
                    {
                        threadEndingFlag[0] = true;
                        loggedin = true;
                    }
                    else
                    {
                        foreach (object ctrl in ctrlList)
                        {
                            if (ctrl is IAccountView)
                            {
                                ((IAccountView)ctrl).Reinitialize();

                                if (temp != null)
                                {
                                    ((IAccountView)ctrl).SetWarningLabel("Fehler beim Login! Der Benutzername oder das Passwort war falsch.");
                                }
                                else
                                {
                                    ((IAccountView)ctrl).SetWarningLabel("Der Datenbankserver antwortet nicht!");
                                }

                                loginloop = ((IAccountView)ctrl).ShowView();
                            }
                        }
                    }
                }
                
                DetermineDaysInMonth(ref dayslist);
                PrepareDataJSONForTransmission(dayslist);
                StartThread(1, mdl.GetWriteDataJSONObject(), mdl.GetReadDataJSONObject());

                //temp = parseDataJSONString(mdl.GetReadDataJSON());

                string testjson = "{{\"sender\":\"192.68.0.50\",\"port\":8080,\"reason\":\"ReceiveData\"},{\"date\":\"2018-12-26T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2018-12-27T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2018-12-28T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2018-12-29T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2018-12-30T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-01T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":[{\"time\":\"2019-01-05T12:00:00\",\"sender\":\"peter.albbw@gmail.com\",\"subject\":\"TestTermin\",\"info\":\"Dies ist ein Termin zum Testen\"}]},{\"date\":\"2019-01-02T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-03T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-04T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-05T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":[{\"time\":\"2019-01-09T12:30:00\",\"sender\":\"peter.albbw@gmail.com\",\"subject\":\"TestTermin1\",\"info\":\"DiesisteinTerminzumTesten\"}]},{\"date\":\"2019-01-06T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-07T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-08T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-09T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-10T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-12T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-01T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-13T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-14T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-15T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-16T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-17T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-18T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-19T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-20T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-21T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-22T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-23T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-24T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-25T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-26T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-27T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-28T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-29T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-30T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null}}";
                temp = parseDataJSONString(testjson);

                threadEndingFlag[1] = true;
                mdl.SetReadLoginJSON("");
                mdl.SetReadDataJSON("");
                AddInfosIntoTheCalendar(dayslist, temp);
                mdl.SetAllDays(temp);
            }
        }

        public PingReply SendPing(Ping ping)
        {
            return ping.Send(mdl.GetMediatorAddress().Address);
        }

        public void SaveTemporaryUserAccount(string loginname, string password)
        {
            mdl.SetUserAccountData(loginname, password);
        }

        public void StartThread(int whichThread, object writeobj, object readobj)
        {
            writethreads[whichThread] = new Thread(new ParameterizedThreadStart(TransferJSONStringToMediator));
            writethreads[whichThread].Start(new ArrayList() { writeobj, 0 });

            while (writethreads[whichThread].IsAlive)
            {

            }
            
            readthreads[whichThread] = new Thread(new ParameterizedThreadStart(ReceiveJSONFromMediator));
            readthreads[whichThread].Start(new ArrayList() { readobj, 0 });
        }

        /*void SetText(string text)
        {
            if (richTextBox1.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(SetText);
                Invoke(d, new object[] { text });
            }
            else
            {
                richTextBox1.Text = text;
            }
        }

        string GetText()
        {
            if (textBox1.InvokeRequired)
            {
                GetStringFromControl d = new GetStringFromControl(GetText);
                return (string)Invoke(d);
            }
            else
            {
                return textBox1.Text;
            }
        }*/

        async void ReceiveJSONFromMediator(object argument)
        {
            if (((IList)argument)[0].GetType() != typeof(string))
            {
                return;
            }
            else if ((string)((IList)argument)[0] != "")
            {
                return;
            }

            try
            {
                TcpListener listener = null;
                listener = new TcpListener(mdl.GetMediatorAddress());
                listener.Start();

                while (mdl.GetReadDataJSON().Length == 0)
                {
                    int temp = (int)((ArrayList)argument)[1];

                    if
                    (
                        !writethreads[temp].IsAlive &&
                        threadEndingFlag[temp]
                    )
                    {
                        return;
                    }

                    await Task.Delay(100);
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                    NetworkStream ns = tcpClient.GetStream();

                    using (MemoryStream ms = new MemoryStream())
                    {
                        await ns.CopyToAsync(ms);
                        argument = Encoding.UTF8.GetString(ms.ToArray());

                    }
                }
            }
            catch (SocketException /*ex*/)
            {
                //SetText(ex.Message);
                //Console.WriteLine(ex.Message);
            }
        }

        async void TransferJSONStringToMediator(object JSONString)
        {
            string JSON = "";

            if (((ArrayList)JSONString)[0].GetType() == typeof(string))
            {
                JSON = (string)((ArrayList)JSONString)[0];
            }

            try
            {
                using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    s.Connect(mdl.GetMediatorAddress());
                    byte[] sendByte = Encoding.UTF8.GetBytes(JSON);

                    using (NetworkStream ns = new NetworkStream(s))
                    {
                        await ns.WriteAsync(sendByte, 0, sendByte.Length);
                        s.Shutdown(SocketShutdown.Both);
                    }
                }
            }
            catch (ArgumentNullException)
            {
                MessageBox.Show("Das übergebene Argument ist kein String-Datentyp!", "JSON-Check", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (SocketException /*ex*/)
            {
                
            }
        }

        IPAddress GetLocalIPAddress()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                int ipcounter = 0;
                IPAddress[] Addresslist;

                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipcounter++;
                    }
                }

                if (ipcounter > 1)
                {
                    MessageBox.Show("Es wurden zu viele lokale IP-Adressen gefunden!", "IP-Adressen-Lokalisierungs Warnung", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }

                Addresslist = new IPAddress[ipcounter];
                ipcounter = 0;

                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        Addresslist[ipcounter] = ip;
                        ipcounter++;
                    }
                }

                return Addresslist[0];
            }

            Console.WriteLine("No network adapters with an IPv4 address in the system!");
            return null;
        }

        /*public string GetSHA3FromString(string text)
        {
            if (text != "" || text != "")
            {
                byte[] bytes = null;
                byte[] r = null;
                SHA3Unmanaged hash = null;
                UTF8Encoding encoding = new UTF8Encoding();

                hash = new SHA3Unmanaged(512);
                bytes = encoding.GetBytes(text);
                r = hash.ComputeHash(bytes);
                string output = "";

                foreach (byte c in r)
                {
                    output += c.ToString("X2");
                }

                return output;
            }

            return "";
        }*/

        /*public string GetAESFromString(string text)
        {
            if (text != "" || text != "")
            {
                byte[] bytes = null;
                string output = "";
                RSACryptoServiceProvider rsaCrypto = new RSACryptoServiceProvider(2048);
                RSAParameters privKey = rsaCrypto.ExportParameters(true);
                RSAParameters pubkey = rsaCrypto.ExportParameters(false);
                StringWriter sw = new StringWriter();
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                xs.Serialize(sw, pubkey);
                publickey = sw.ToString();
                rsaCrypto = new RSACryptoServiceProvider();
                rsaCrypto.ImportParameters(pubkey);
                string plainTextData = text;
                byte[] bytesplainTextData = Encoding.UTF8.GetBytes(plainTextData);
                byte[] bytesCypherText = rsaCrypto.Encrypt(bytesplainTextData, false);
                string cypherText = "";

                foreach (byte c in bytesCypherText)
                {
                    cypherText += c.ToString("X2");
                }

                using (AesManaged aes = new AesManaged())
                {
                    aes.Key = bytesCypherText;  //zu lang
                    aes.IV = bytesCypherText;   //zu lang
                    ICryptoTransform crypto = aes.CreateEncryptor(aes.Key, aes.IV);
                    MemoryStream ms = new MemoryStream();

                    using (CryptoStream cs = new CryptoStream(ms, crypto, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamw = new StreamWriter(cs))
                        {
                            streamw.Write(cypherText);
                            bytes = ms.ToArray();
                            
                            foreach (byte c in bytes)
                            {
                                output += c.ToString("X2");
                            }
                        }
                    }
                }

                return output;
            }

            return "";
        }*/

        private Model.JSONHeader parseJSONHeader(string JSONString)
        {
            ArrayList DeserializedJSONList;
            Model.JSONHeader header = new Model.JSONHeader(new byte[] { 0, 0, 0, 0 }, 0, Model.TransmissionReason.ReceiveAccountData);
            string temp2;
            char[] temp;

            temp = JSONString.ToCharArray();
            temp[0] = '[';
            temp[temp.Length - 1] = ']';
            temp2 = "";

            foreach (char c in temp)
            {
                temp2 += c;
            }

            DeserializedJSONList = JsonConvert.DeserializeObject<ArrayList>(temp2);

            if (DeserializedJSONList != null)
            {
                foreach (object item in DeserializedJSONList)
                {
                    foreach (JProperty jobj in (IList)item)
                    {
                        switch (jobj.Name)
                        {
                            case "sender":
                                try
                                {
                                    IPAddress.Parse(jobj.Value.ToString());
                                    header.sender = jobj.Value.ToString();
                                }
                                catch (FormatException)
                                {
                                    MessageBox.Show("IP-Format ist falsch!", "Parsing-Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                                }

                                break;
                            case "port":
                                try
                                {
                                    header.port = short.Parse(jobj.Value.ToString());
                                }
                                catch (FormatException)
                                {
                                    MessageBox.Show("Der Port konnte nicht ermittelt werden!", "Parsing-Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                                }

                                break;
                            case "reason":
                                header.reason = jobj.Value.ToString() == "Account" ? "Account" : "ReceiveData";

                                break;
                        }
                    }
                }

                return header;
            }
            else
            {
                return null;
            }
        }

        public ArrayList parseLoginJSONString(string JSONString)
        {
            ArrayList ArrayToReturn = new ArrayList();
            ArrayList DeserializedJSONList;
            Model.JSONHeader header = parseJSONHeader(JSONString);
            Model.LoginItem loginItem = new Model.LoginItem("", "");
            string temp2;
            char[] temp;

            temp = JSONString.ToCharArray();
            temp[0] = '[';
            temp[temp.Length - 1] = ']';
            temp2 = "";

            foreach (char c in temp)
            {
                temp2 += c;
            }

            DeserializedJSONList = JsonConvert.DeserializeObject<ArrayList>(temp2);

            if (DeserializedJSONList != null)
            {
                foreach (object item in DeserializedJSONList)
                {
                    foreach (JProperty jobj in (IList)item)
                    {
                        switch (jobj.Name)
                        {
                            case "loginname":
                                loginItem.loginname = jobj.Value.ToString();

                                break;
                            case "password":
                                loginItem.password = jobj.Value.ToString();

                                break;
                        }
                    }
                }

                ArrayToReturn.Add(header);
                ArrayToReturn.Add(loginItem);
                return ArrayToReturn;
            }
            else
            {
                return null;
            }
        }

        public ArrayList parseDataJSONString(string JSONString)
        {
            ArrayList ArrayToReturn = new ArrayList();
            ArrayList DeserializedJSONList;
            Model.JSONHeader header = parseJSONHeader(JSONString);
            Model.JSONItem jsonitem;
            string temp2;
            char[] temp;
            
            temp = JSONString.ToCharArray();
            temp[0] = '[';
            temp[temp.Length - 1] = ']';
            temp2 = "";

            foreach (char c in temp)
            {
                temp2 += c;
            }

            DeserializedJSONList = JsonConvert.DeserializeObject<ArrayList>(temp2);

            if (DeserializedJSONList != null)
            {
                foreach (object item in DeserializedJSONList)
                {
                    if (item != DeserializedJSONList[0])
                    {
                        jsonitem = new Model.JSONItem(DateTime.Now, '0', 0, null);
                        jsonitem.meeting = new List<Model.MeetingObject>();
                        jsonitem.meeting.Add(new Model.MeetingObject(DateTime.Now, "", "", ""));

                        foreach (JProperty jobj in (IList)item)
                        {
                            switch (jobj.Name)
                            {
                                case "date":
                                    try
                                    {
                                        jsonitem.date = DateTime.Parse(jobj.Value.ToString());
                                    }
                                    catch (FormatException)
                                    {
                                        MessageBox.Show("Datum-Format ist falsch!", "Parsing-Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }

                                    break;
                                case "lateness":
                                    try
                                    {
                                        jsonitem.lateness = int.Parse(jobj.Value.ToString());
                                    }
                                    catch (FormatException)
                                    {
                                        MessageBox.Show("Format für die Verspätungen ist falsch!", "Parsing-Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }

                                    break;
                                case "initial":
                                    try
                                    {
                                        jsonitem.initial = char.Parse(jobj.Value.ToString());
                                    }
                                    catch (FormatException)
                                    {
                                        MessageBox.Show("Format für die Anwesenheit ist falsch!", "Parsing-Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }

                                    break;
                                case "meeting":
                                    int i = 0;
                                    bool isNull = false;

                                    foreach (object array in jobj)
                                    {
                                        if (array.GetType() == typeof(JValue))
                                        {
                                            if (((JValue)array).Value == null)
                                            {
                                                isNull = true;
                                            }
                                            else
                                            {
                                                foreach (JObject arrayItem in (JValue)array)
                                                {
                                                    foreach (JProperty innerItem in (IList)arrayItem)
                                                    {
                                                        switch (innerItem.Name)
                                                        {
                                                            case "time":
                                                                try
                                                                {
                                                                    jsonitem.meeting.ElementAt(i).time = DateTime.Parse(innerItem.Value.ToString());
                                                                }
                                                                catch (FormatException)
                                                                {
                                                                    MessageBox.Show("Datum-Format ist falsch!", "Parsing-Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                                                                }

                                                                break;
                                                            case "sender":
                                                                jsonitem.meeting.ElementAt(i).sender = innerItem.Value.ToString();

                                                                break;
                                                            case "subject":
                                                                jsonitem.meeting.ElementAt(i).subject = innerItem.Value.ToString();

                                                                break;
                                                            case "info":
                                                                jsonitem.meeting.ElementAt(i).info = innerItem.Value.ToString();

                                                                break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (((JArray)array).Count < 1)
                                            {
                                                isNull = true;
                                            }
                                            else
                                            {
                                                foreach (JObject arrayItem in (JArray)array)
                                                {
                                                    foreach (JProperty innerItem in (IList)arrayItem)
                                                    {
                                                        switch (innerItem.Name)
                                                        {
                                                            case "time":
                                                                try
                                                                {
                                                                    jsonitem.meeting.ElementAt(i).time = DateTime.Parse(innerItem.Value.ToString());
                                                                }
                                                                catch (FormatException)
                                                                {
                                                                    MessageBox.Show("Datum-Format ist falsch!", "Parsing-Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                                                                }

                                                                break;
                                                            case "sender":
                                                                jsonitem.meeting.ElementAt(i).sender = innerItem.Value.ToString();

                                                                break;
                                                            case "subject":
                                                                jsonitem.meeting.ElementAt(i).subject = innerItem.Value.ToString();

                                                                break;
                                                            case "info":
                                                                jsonitem.meeting.ElementAt(i).info = innerItem.Value.ToString();

                                                                break;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        i++;
                                    }

                                    if (isNull)
                                    {
                                        jsonitem.meeting = null;
                                    }

                                    break;
                            }
                        }

                        ArrayToReturn.Add(jsonitem);
                    }
                    else
                    {

                        ArrayToReturn.Add(header);
                    }
                }

                return ArrayToReturn;
            }
            else
            {
                return null;
            }
        }

        public void PrepareLoginJSONForTransmission(string loginname, string password)
        {
            ArrayList jsonList = new ArrayList();
            string temp2;
            char[] temp;
            //string passwordencrypted = GetAESFromString(password);

            jsonList.Add(new Model.JSONHeader
            (
                GetLocalIPAddress().GetAddressBytes(),
                9000,
                Model.TransmissionReason.ReceiveAccountData)
            );

            jsonList.Add(new Model.LoginItem
            (
                loginname,
                password)
            );
            
            temp2 = JsonConvert.SerializeObject(jsonList, Formatting.Indented);
            temp = temp2.ToCharArray();
            temp[0] = '{';
            temp[temp.Length - 1] = '}';
            temp2 = "";

            foreach (char c in temp)
            {
                temp2 += c;
            }

            mdl.SetWriteLoginJSON(temp2);
        }

        public void PrepareDataJSONForTransmission(string[] days)
        {
            ArrayList jsonList = new ArrayList();
            int numberOfDaysTodaysMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            bool firstendarrived = false;
            bool lastendarrived = false;
            string temp2;
            char[] temp;

            jsonList.Add(new Model.JSONHeader
            (
                GetLocalIPAddress().GetAddressBytes(),
                9000,
                Model.TransmissionReason.ReceiveCalendarData)
            );

            foreach (string day in days)
            {
                if (!firstendarrived && int.Parse(day) < int.Parse(days[0]))
                {
                    firstendarrived = true;
                }
                else if (!firstendarrived)
                {
                    if (DateTime.Now.Month - 1 != 0)
                    {
                        jsonList.Add(new Model.JSONItem
                        (
                            new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, int.Parse(day)),
                            '?'
                        ));
                    }
                    else
                    {
                        jsonList.Add(new Model.JSONItem
                        (
                            new DateTime(DateTime.Now.Year - 1, 12, int.Parse(day)),
                            '?'
                        ));
                    }
                }

                if (lastendarrived)
                {
                    if (DateTime.Now.Month + 1 != 13)
                    {
                        jsonList.Add(new Model.JSONItem
                        (
                            new DateTime(DateTime.Now.Year, DateTime.Now.Month + 1, int.Parse(day)),
                            '?'
                        ));
                    }
                    else
                    {
                        jsonList.Add(new Model.JSONItem
                        (
                            new DateTime(DateTime.Now.Year + 1, 1, int.Parse(day)),
                            '?'
                        ));
                    }
                }

                if (firstendarrived && !lastendarrived && int.Parse(day) <= numberOfDaysTodaysMonth)
                {
                    jsonList.Add(new Model.JSONItem
                    (
                        new DateTime(DateTime.Now.Year, DateTime.Now.Month, int.Parse(day)),
                        '?'
                    ));

                    if (int.Parse(day) == numberOfDaysTodaysMonth)
                    {
                        lastendarrived = true;
                    }
                }
            }
            
            temp2 = JsonConvert.SerializeObject(jsonList, Formatting.Indented);
            temp = temp2.ToCharArray();
            temp[0] = '{';
            temp[temp.Length - 1] = '}';
            temp2 = "";

            foreach (char c in temp)
            {
                temp2 += c;
            }

            mdl.SetWriteDataJSON(temp2);
        }

        public void DetermineDaysInMonth(ref string[] dayslist)
        {
            int today = DateTime.Now.Day;
            int numberOfDaysLastMonth = 0;
            int numberOfDaysTodaysMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            int numberOfWeeks = 5;
            int startday;
            bool firstendarrived = false;
            DateTime dt;
            
            //ermittelt wieviele Tage vom letzten Tag des vergangen Monats abgezogen werden
            while (today > 0)
            {
                today -= 7;
            }

            //ermittelt wieviele Tage der vergangene Monat hat
            if (DateTime.Now.Month > 1)
            {
                numberOfDaysLastMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month - 1);
            }
            else
            {
                numberOfDaysLastMonth = DateTime.DaysInMonth(DateTime.Now.Year - 1, 12);
            }

            //setzt den Startpunkt auf den Wochentag des vergangenen Monats anhand des heutigen Wochentages
            if (today < 0)
            {
                startday = numberOfDaysLastMonth - (today * -1);
            }
            else
            {
                startday = numberOfDaysLastMonth - today;
            }

            //Korrektur des Startbeginns
            while (startday < numberOfDaysLastMonth)
            {
                startday += 7;
            }

            startday -= startday - numberOfDaysLastMonth;

            //wandelt den Startpunkt in ein Datum um
            if (DateTime.Now.Month > 1)
            {
                dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, startday);
            }
            else
            {
                dt = new DateTime(DateTime.Now.Year - 1, 12, startday);
            }

            //Array für die Woche
            string[] dayarray = new string[7];
            //Array für den Monat
            dayslist = new string[7 * numberOfWeeks];

            //Iteration vom Startpunkt aus bis zum 42. Feld
            //(für jede Woche)
            for (int i = 0, k = 0; i < numberOfWeeks; i++)
            {
                //Zurücksetzung auf 1, wenn letzter Tag des jeweiligen Monats erreicht wurde
                //(für jeden "Wochentag")
                for (int j = 0; j < 7; j++, k++)
                {
                    if (!firstendarrived && startday > numberOfDaysLastMonth)
                    {
                        startday = 1;
                        firstendarrived = true;
                    }

                    if (firstendarrived && startday > numberOfDaysTodaysMonth)
                    {
                        startday = 1;
                    }
                    
                    dayarray[j] = startday.ToString();
                    dayslist[k] = startday.ToString();
                    startday += 1;
                }

                //Zuweisung des Wochenarrays in die Monatsliste
                mdl.GetlistOfDays().Add(new Model.Week()
                {
                    MO = dayarray[0],
                    DI = dayarray[1],
                    MI = dayarray[2],
                    DO = dayarray[3],
                    FR = dayarray[4],
                    SA = dayarray[5],
                    SO = dayarray[6]
                });
            }

            //rufe die Update-Funktion des Kalenders auf
            //CallUpdate();
        }

        string CheckDayForMeeting(Model.JSONItem item, string rest)
        {
            switch (item.meeting)
            {
                case null:
                    return rest + item.lateness.ToString();
                default:
                    return rest + item.lateness.ToString() + " " + mdl.GetMeetingSign();
            }
        }

        void AddInfosIntoTheCalendar(string[] dayslist, ArrayList listOfDays)
        {
            if (listOfDays != null)
            {
                try
                {
                    for (int i = 0; i < dayslist.Length; i += 7)
                    {
                        int weeknumber = (int)Math.Floor(((double)i) / 7);
                        ((Model.Week)mdl.GetlistOfDays()[weeknumber]).MO = CheckDayForMeeting((Model.JSONItem)listOfDays[i + 1], dayslist[i] + "\t");
                        ((Model.Week)mdl.GetlistOfDays()[weeknumber]).DI = CheckDayForMeeting((Model.JSONItem)listOfDays[i + 2], dayslist[i + 1] + "\t");
                        ((Model.Week)mdl.GetlistOfDays()[weeknumber]).MI = CheckDayForMeeting((Model.JSONItem)listOfDays[i + 3], dayslist[i + 2] + "\t");
                        ((Model.Week)mdl.GetlistOfDays()[weeknumber]).DO = CheckDayForMeeting((Model.JSONItem)listOfDays[i + 4], dayslist[i + 3] + "\t");
                        ((Model.Week)mdl.GetlistOfDays()[weeknumber]).FR = CheckDayForMeeting((Model.JSONItem)listOfDays[i + 5], dayslist[i + 4] + "\t");
                        ((Model.Week)mdl.GetlistOfDays()[weeknumber]).SA = CheckDayForMeeting((Model.JSONItem)listOfDays[i + 6], dayslist[i + 5] + "\t");
                        ((Model.Week)mdl.GetlistOfDays()[weeknumber]).SO = CheckDayForMeeting((Model.JSONItem)listOfDays[i + 7], dayslist[i + 6] + "\t");
                    }
                }
                catch (FormatException)
                {
                    MessageBox.Show("Datum-Format ist falsch!", "Parsing-Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void ShowMeetingPanel()
        {
            foreach (object ctrl in ctrlList)
            {
                if (ctrl is IMeetingPanelView)
                {
                    ((IMeetingPanelView)ctrl).ShowView();
                }
            }
        }

        public void HideMeetingPanel()
        {
            foreach (object ctrl in ctrlList)
            {
                if (ctrl is IMeetingPanelView)
                {
                    ((IMeetingPanelView)ctrl).HideView();
                }
            }
        }

        public ArrayList GetList()
        {
            return mdl.GetlistOfDays();
        }

        public char GetMeetingSign()
        {
            return mdl.GetMeetingSign();
        }

        public bool[] GetThreadEndingFlag()
        {
            return threadEndingFlag;
        }

        public object GetReadDataJSONObject()
        {
            return mdl.GetReadDataJSONObject();
        }

        public object GetWriteDataJSONObject()
        {
            return mdl.GetWriteDataJSONObject();
        }

        public MainWindow GetMainWindow()
        {
            foreach (object ctrl in ctrlList)
            {
                if (ctrl is IMainView)
                {
                    return ((IMainView)ctrl).GetMainWindow();
                }
            }

            return null;
        }

        public MainWindowController GetMainWindowController()
        {
            foreach (object ctrl in ctrlList)
            {
                if (ctrl is IMainView)
                {
                    return (MainWindowController)ctrl;
                }
            }

            return null;
        }

        public ArrayList GetAllDays()
        {
            return mdl.GetAllDays();
        }
    }
}
