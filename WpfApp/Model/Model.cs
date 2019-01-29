
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Media;

namespace WpfApp
{
    public class Model
    {
        ArrayList listOfDays = new ArrayList();
        ArrayList alldays;
        string datajsonread = "";
        string datajsonwrite = "";
        string loginjsonread = "";
        string loginjsonwrite = "";
        //string[] useraccountdata;
        string[] useraccountdata = new string[] { "test", "login" };
        NetworkStream ReadStream;
        NetworkStream WriteStream;
        IPEndPoint MediatorAddress = new IPEndPoint(new IPAddress(new byte[] { 10, 122, 122, 171 }), 9010);
        //IPEndPoint MediatorAddress = new IPEndPoint(IPAddress.Loopback, 9010);
        IPEndPoint WPFAddress = new IPEndPoint(IPAddress.Loopback, 9000);
        char meetingsign = '*';
        public enum TransmissionReason { ReceiveCalendarData, ReceiveAccountData }

        public class LoginItem
        {
            public string loginname { get; set; }
            public string password { get; set; }

            public LoginItem(string loginname, string password)
            {
                this.loginname = loginname;
                this.password = password;
            }
        }

        public class Week
        {
            public string MO { get; set; }
            public string DI { get; set; }
            public string MI { get; set; }
            public string DO { get; set; }
            public string FR { get; set; }
            public string SA { get; set; }
            public string SO { get; set; }
        }

        public class MeetingObject
        {
            public DateTime time { get; set; }
            public string sender { get; set; }
            public string subject { get; set; }
            public string info { get; set; }

            public MeetingObject(DateTime time, string sender, string subject, string info)
            {
                this.time = time;
                this.sender = sender;
                this.subject = subject;
                this.info = info;
            }
        }

        public class JSONItem
        {
            public DateTime date { get; set; } = new DateTime();
            public int lateness { get; set; } = 0;
            public char initial { get; set; } = '?';
            public List<MeetingObject> meeting { get; set; }

            public JSONItem(DateTime date, char initial, int lateness = 0, List<MeetingObject> meeting = null)
            {
                this.date = date;
                initial = char.ToUpper(initial);

                if (initial == 'E' || initial == 'U' || initial == 'K' || initial == '?')
                {
                    this.initial = initial;
                }
                else
                {
                    if (initial == 'A' & lateness == 0)
                    {
                        this.initial = initial;
                        this.lateness = lateness;
                    }
                    else
                    {
                        if (lateness > 0)
                        {
                            this.initial = 'U';
                        }
                        else
                        {
                            this.lateness = 0;
                        }
                    }
                }

                if (
                        meeting != null &&
                        meeting.Count > 0 &&
                        meeting[0].sender != string.Empty &
                        meeting[0].subject != string.Empty &
                        meeting[0].info != string.Empty
                   )
                {
                    this.meeting = meeting;
                }
                else
                {
                    this.meeting = null;
                }
            }
        }

        public class JSONHeader
        {
            public string sender { get; set; }
            public short port { get; set; }
            public string reason { get; set; }

            public JSONHeader(byte[] ipv4, short port, TransmissionReason reason)
            {
                if (ipv4.Length == 4)
                {
                    sender = ipv4[0] + "." + ipv4[1] + "." + ipv4[2] + "." + ipv4[3];
                }
                else
                {
                    sender = "0.0.0.0";
                    Console.WriteLine("IP-Format war nicht korrekt!");
                }

                this.port = port;

                switch (reason)
                {
                    case TransmissionReason.ReceiveAccountData:
                        this.reason = "ReceiveAccountData";
                        break;
                    case TransmissionReason.ReceiveCalendarData:
                        this.reason = "ReceiveCalendarData";
                        break;
                }
            }
        }

        #region Eigenschaften
        public ArrayList GetlistOfDays()
        {
            return listOfDays;
        }

        public void SetlistOfDays(ArrayList listOfDays)
        {
            this.listOfDays = listOfDays;
        }

        //useracc
        public string[] GetUserAccountData()
        {
            return useraccountdata;
        }

        public void SetUserAccountData(string name, string password)
        {
            useraccountdata[0] = name;
            useraccountdata[1] = password;
        }
        public void SetUserAccountData(string[] accountdata)
        {
            useraccountdata = accountdata;
        }

        public string GetReadLoginJSON()
        {
            return loginjsonread;
        }

        public string GetWriteLoginJSON()
        {
            return loginjsonwrite;
        }

        public void SetReadLoginJSON(string loginjson)
        {
            loginjsonread = loginjson;
        }

        public void SetWriteLoginJSON(string loginjson)
        {
            loginjsonwrite = loginjson;
        }

        public string GetReadDataJSON()
        {
            return datajsonread;
        }

        public string GetWriteDataJSON()
        {
            return datajsonwrite;
        }

        public object GetWriteDataJSONObject()
        {
            return datajsonwrite.Clone();
        }

        public void SetReadDataJSON(string datajson)
        {
            datajsonread = datajson;
        }

        public void SetWriteDataJSON(string datajson)
        {
            datajsonwrite = datajson;
        }

        public object GetReadLoginJSONObject()
        {
            return loginjsonread.Clone();
        }

        public object GetReadDataJSONObject()
        {
            return datajsonread.Clone();
        }

        public NetworkStream GetReadStream()
        {
            return ReadStream;
        }

        public void SetReadStream(NetworkStream ReadStream)
        {
            this.ReadStream = ReadStream;
        }

        public NetworkStream GetWriteStream()
        {
            return WriteStream;
        }

        public void SetWriteStream(NetworkStream WriteStream)
        {
            this.WriteStream = WriteStream;
        }

        public IPEndPoint GetMediatorAddress()
        {
            return MediatorAddress;
        }

        public void SetMediatorAddress(IPEndPoint MediatorAddress)
        {
            this.MediatorAddress = MediatorAddress;
        }

        public IPEndPoint GetWPFAddress()
        {
            return WPFAddress;
        }

        public void SetWPFAddress(IPEndPoint WPFAddress)
        {
            this.WPFAddress = WPFAddress;
        }

        public char GetMeetingSign()
        {
            return meetingsign;
        }

        public ArrayList GetAllDays()
        {
            return alldays;
        }

        public void SetAllDays(ArrayList list)
        {
            alldays = list;
        }
        #endregion
    }
}
