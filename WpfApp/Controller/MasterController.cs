using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfApp.Model;
using WpfApp.Views.Controller;
using WpfApp.Views.Controller.Interfaces;

namespace WpfApp.Controller
{
	public sealed class MasterController
	{
		static MasterController instance = null;
		static object padlock = new object();
		MainModel mdl;
		public List<IView> ctrlList { get; set; }
		public Thread[] writethreads { get; set; }
		public Thread[] readthreads { get; set; }
		bool[] threadEndingFlag = new bool[] { false, false, false };
		bool? loginsuccess;
		public MainModel.EatingItemList eatingItemList;
		public delegate void boolVisibilityDelegate(bool visibility);

		//string publickey;

		public MasterController()
		{
			instance = this;
			mdl = new MainModel();
			ctrlList = new List<IView>();
			//eatingItemList = new MainModel.EatingItemList(new System.Collections.ObjectModel.ObservableCollection<MainModel.EatingItem>(new List<MainModel.EatingItem>() { new MainModel.EatingItem(new DateTime(2019, 2, 4).ToShortDateString(), "Suppe"), new MainModel.EatingItem(new DateTime(2019, 2, 5).ToShortDateString(), "Gullarsch"), new MainModel.EatingItem(new DateTime(2019, 2, 6).ToShortDateString(), "Nutteln"), new MainModel.EatingItem(new DateTime(2019, 2, 7).ToShortDateString(), "Schweinetitte"), new MainModel.EatingItem(new DateTime(2019, 2, 8).ToShortDateString(), "Schimmelkuchen") }));
			InstantiateViewControllers();
			writethreads = new Thread[3];
			readthreads = new Thread[3];
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
			if (ctrl.GetType().IsSealed && ctrl is IView)
			{
				ctrlList.Add((IView)ctrl);
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
			ViewsConstructor.InstantiateViewControllers(ctrlList);
		}

		void Start()
		{
			ArrayList temp = null;
			bool loggedin = false;
			bool firstOpening = true;
			bool? loginloop = false;
			string[] dayslist = null;

			loginsuccess = ViewsConstructor.ShowAccountLoginView(ctrlList, firstOpening, false);

			//Tritt ein wenn Login-Dialog geschlossen ist
			if (loginsuccess == false)
			{
				while (!loggedin & loginloop == false)
				{
					PrepareLoginJSONForTransmission(mdl.GetUserAccountData()[0], mdl.GetUserAccountData()[1]);
					StartThread(0, mdl.GetWriteDataJSONObject(), mdl.GetReadLoginJSONObject());

					//temp = parseLoginJSONString(mdl.GetReadLoginJSON());
					temp = new ArrayList() { new MainModel.JSONHeader(new byte[] { 100, 100, 100, 100 }, 0, MainModel.TransmissionReason.ReceiveAccountData), new MainModel.LoginItem("test", "login") };    //TODO: nur zum Testen, später auskommentieren

					if
					(
						temp != null &&
						((MainModel.LoginItem)temp[1]).loginname == mdl.GetUserAccountData()[0] &&
						((MainModel.LoginItem)temp[1]).password == mdl.GetUserAccountData()[1]
					)
					{
						threadEndingFlag[0] = true;
						loggedin = true;
					}
					else
					{
						firstOpening = false;

						if (temp != null)
						{
							loginloop = ViewsConstructor.ShowAccountLoginView(ctrlList, firstOpening, true);
						}
					}
				}

				DetermineDaysInMonth(ref dayslist);
				PrepareDataJSONForTransmission(dayslist);
				StartThread(1, mdl.GetWriteDataJSONObject(), mdl.GetReadDataJSONObject());

				//temp = parseDataJSONString(mdl.GetReadDataJSON());

				string testjson = "{{\"sender\":\"10.122.122.110\",\"port\":9000,\"reason\":\"ReceiveCalendarData\"},{\"date\":\"2019-01-28T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-29T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-30T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-01-31T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-01T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-02T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-03T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-04T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-05T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":[{\"time\":\"2019-02-05T12:30:00\",\"sender\":\"peter.albbw@gmail.com\",\"subject\":\"TestTermin1\",\"info\":\"Dies ist ein Termin zum Testen\"},{\"time\":\"2019-02-05T14:00:00\",\"sender\":\"peter.albbw@gmail.com\",\"subject\":\"TestTermin2\",\"info\":\"Dies ist ein weiterer Termin zum Testen\"},{\"time\":\"2019-02-05T14:00:00\",\"sender\":\"peter.albbw@gmail.com\",\"subject\":\"TestTermin2\",\"info\":\"Dies ist ein weiterer Termin zum Testen\"},{\"time\":\"2019-02-05T14:00:00\",\"sender\":\"peter.albbw@gmail.com\",\"subject\":\"TestTermin2\",\"info\":\"Dies ist ein weiterer Termin zum Testen\"},{\"time\":\"2019-02-05T14:00:00\",\"sender\":\"peter.albbw@gmail.com\",\"subject\":\"TestTermin2\",\"info\":\"Dies ist ein weiterer Termin zum Testen\"},{\"time\":\"2019-02-05T14:00:00\",\"sender\":\"peter.albbw@gmail.com\",\"subject\":\"TestTermin2\",\"info\":\"Dies ist ein weiterer Termin zum Testen\"},{\"time\":\"2019-02-05T14:00:00\",\"sender\":\"peter.albbw@gmail.com\",\"subject\":\"TestTermin2\",\"info\":\"Dies ist ein weiterer Termin zum Testen\"},{\"time\":\"2019-02-05T14:00:00\",\"sender\":\"peter.albbw@gmail.com\",\"subject\":\"TestTermin2\",\"info\":\"Dies ist ein weiterer Termin zum Testen\"}]},{\"date\":\"2019-02-06T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-07T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-08T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-09T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":[{\"time\":\"2019-02-09T09:15:00\",\"sender\":\"peter.albbw@gmail.com\",\"subject\":\"TestTermin2\",\"info\":\"DiesisteinTerminzumTesten\"}]},{\"date\":\"2019-02-10T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-11T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-12T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-13T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-14T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-15T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-16T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-17T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-18T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-19T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-20T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-21T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-22T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-23T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-24T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-25T00:00:00\",\"lateness\":500,\"initial\":\"U\",\"meeting\":null},{\"date\":\"2019-02-26T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-27T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-02-28T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-03-01T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-03-02T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-03-03T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-03-04T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-03-05T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-03-06T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-03-07T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-03-08T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-03-09T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null},{\"date\":\"2019-03-10T00:00:00\",\"lateness\":0,\"initial\":\"?\",\"meeting\":null}}";
				temp = parseDataJSONString(testjson);

				threadEndingFlag[1] = true;
				mdl.SetReadLoginJSON("");
				mdl.SetReadDataJSON("");
				mdl.SetWriteLoginJSON("");
				mdl.SetWriteDataJSON("");
				AddInfosIntoTheCalendar(dayslist, temp);
				mdl.SetAllDays(temp);

				PrepareEatingPlanJSONForTransmission();
				StartThread(2, mdl.GetWriteEatingPlanJSONObject(), mdl.GetReadEatingPlanJSONObject());

				//testjson = "{{\"sender\":\"10.122.122.110\",\"port\":9010,\"reason\":\"ReceiveEatingPlanData\"},{\"EatingItemDate\":\"2019 - 02 - 04T00: 00:00\",\"EatingItemDescription\":\"Essen1\"},{\"EatingItemDate\":\"2019 - 02 - 05T00: 00:00\",\"EatingItemDescription\":\"Essen2\"},{\"EatingItemDate\":\"2019 - 02 - 06T00: 00:00\",\"EatingItemDescription\":\"Essen3\"},{\"EatingItemDate\":\"2019 - 02 - 07T00: 00:00\",\"EatingItemDescription\":\"Essen4\"},{\"EatingItemDate\":\"2019 - 02 - 08T00: 00:00\",\"EatingItemDescription\":\"Essen5\"}}";

				threadEndingFlag[2] = true;

				if ((string)mdl.GetReadEatingPlanJSONObject() != string.Empty)
				{
					eatingItemList = new MainModel.EatingItemList(new System.Collections.ObjectModel.ObservableCollection<MainModel.EatingItem>(parseEatingPlanJSONString(mdl.GetReadEatingPlanJSON())));
					//eatingItemList = new MainModel.EatingItemList(new System.Collections.ObjectModel.ObservableCollection<MainModel.EatingItem>(parseEatingPlanJSONString(testjson)));
					ViewsConstructor.SetEatingPlanItemsSource(ctrlList);
				}

				mdl.SetReadEatingPlanJSON("");
				mdl.SetWriteEatingPlanJSON("");
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

		void SetVisibilityWarningIcon(bool visible)
		{
			if (ViewsConstructor.GetMainWindow(ctrlList) != null)
			{
				ViewsConstructor.GetMainWindow(ctrlList).WarningImage.Dispatcher.Invoke
				(
					() =>
					{
						if (visible)
						{
							ViewsConstructor.GetMainWindow(ctrlList).WarningImage.Visibility = Visibility.Visible;
						}
						else
						{
							ViewsConstructor.GetMainWindow(ctrlList).WarningImage.Visibility = Visibility.Hidden;
						}
					}
				);
			}
		}

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

			ArrayList tempList = (ArrayList)argument;

			try
			{
				TcpListener listener = null;
				listener = new TcpListener(mdl.GetMediatorAddress());
				listener.Start();

				while (mdl.GetReadDataJSON().Length == 0)
				{
					int temp = (int)tempList[1];

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
					SetVisibilityWarningIcon(false);

					using (MemoryStream ms = new MemoryStream())
					{
						await ns.CopyToAsync(ms);
						argument = Encoding.UTF8.GetString(ms.ToArray());
					}
				}
			}
			catch (SocketException /*ex*/)
			{
				SetVisibilityWarningIcon(true);
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
					SetVisibilityWarningIcon(false);

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
				SetVisibilityWarningIcon(true);
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

		private MainModel.JSONHeader parseJSONHeader(string JSONString)
		{
			ArrayList DeserializedJSONList;
			MainModel.JSONHeader header = new MainModel.JSONHeader(new byte[] { 0, 0, 0, 0 }, 0, MainModel.TransmissionReason.ReceiveAccountData);
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
			MainModel.JSONHeader header = parseJSONHeader(JSONString);
			MainModel.LoginItem loginItem = new MainModel.LoginItem("", "");
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
			MainModel.JSONHeader header = parseJSONHeader(JSONString);
			MainModel.JSONItem jsonitem;
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
						jsonitem = new MainModel.JSONItem(DateTime.Now, '0', 0, null);
						jsonitem.meeting = new List<MainModel.MeetingObject>();

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
									bool isNull = false;

									foreach (object array in jobj)
									{
										int i = 0;

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

													i++;
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
													jsonitem.meeting.Add(new MainModel.MeetingObject(DateTime.Now, "", "", ""));

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

													i++;
												}
											}
										}
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

		public List<MainModel.EatingItem> parseEatingPlanJSONString(string JSONString)
		{
			List<MainModel.EatingItem> ArrayToReturn = new List<MainModel.EatingItem>();
			ArrayList DeserializedJSONList;
			string temp2;
			char[] temp;

			if ((string)mdl.GetReadEatingPlanJSONObject() != string.Empty)
			{
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
					int i = 0;

					foreach (object item in DeserializedJSONList)
					{
						if (i > 0)
						{
							MainModel.EatingItem eatingPlanItem = new MainModel.EatingItem("", "");
							eatingPlanItem.EatingItemDate = DateTime.Parse(((JProperty)((IList)item)[0]).Value.ToString()).ToShortDateString();
							eatingPlanItem.EatingItemDescription = ((JProperty)((IList)item)[1]).Value.ToString();
							ArrayToReturn.Add(eatingPlanItem);
						}

						i++;
					}

					return ArrayToReturn;
				}
				else
				{
					return null;
				}
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

			jsonList.Add(new MainModel.JSONHeader
			(
				GetLocalIPAddress().GetAddressBytes(),
				9000,
				MainModel.TransmissionReason.ReceiveAccountData)
			);

			jsonList.Add(new MainModel.LoginItem
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

			jsonList.Add(new MainModel.JSONHeader
			(
				GetLocalIPAddress().GetAddressBytes(),
				9000,
				MainModel.TransmissionReason.ReceiveCalendarData)
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
						jsonList.Add(new MainModel.JSONItem
						(
							new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, int.Parse(day)),
							'?'
						));
					}
					else
					{
						jsonList.Add(new MainModel.JSONItem
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
						jsonList.Add(new MainModel.JSONItem
						(
							new DateTime(DateTime.Now.Year, DateTime.Now.Month + 1, int.Parse(day)),
							'?'
						));
					}
					else
					{
						jsonList.Add(new MainModel.JSONItem
						(
							new DateTime(DateTime.Now.Year + 1, 1, int.Parse(day)),
							'?'
						));
					}
				}

				if (firstendarrived && !lastendarrived && int.Parse(day) <= numberOfDaysTodaysMonth)
				{
					jsonList.Add(new MainModel.JSONItem
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

		public void PrepareEatingPlanJSONForTransmission()
		{
			ArrayList jsonlist = new ArrayList();
			DateTime today = DateTime.Now.Date;
			string date;
			string jsonstring;
			char[] temp;

			jsonlist.Add(new MainModel.JSONHeader
			(
				GetLocalIPAddress().GetAddressBytes(),
				9000,
				MainModel.TransmissionReason.ReceiveEatingPlanData)
			);

			switch (today.DayOfWeek)
			{
				case DayOfWeek.Tuesday:
					today = new DateTime(today.Year, today.Month, today.Day - 1);
					break;
				case DayOfWeek.Wednesday:
					today = new DateTime(today.Year, today.Month, today.Day - 2);
					break;
				case DayOfWeek.Thursday:
					today = new DateTime(today.Year, today.Month, today.Day - 3);
					break;
				case DayOfWeek.Friday:
					today = new DateTime(today.Year, today.Month, today.Day - 4);
					break;
			}

			for (int i = 0; i < 5; i++)
			{
				date = today.Year.ToString() + "-" + today.Month.ToString("00") + "-" + (today.Day + i).ToString("00") + "T00:00:00";
				jsonlist.Add(new MainModel.EatingItem(date, "?"));
			}

			jsonstring = JsonConvert.SerializeObject(jsonlist, Formatting.Indented);
			temp = jsonstring.ToCharArray();
			temp[0] = '{';
			temp[temp.Length - 1] = '}';
			jsonstring = "";

			foreach (char c in temp)
			{
				jsonstring += c;
			}

			mdl.SetWriteEatingPlanJSON(jsonstring);
		}

		public void DetermineDaysInMonth(ref string[] dayslist)
		{
			int today = DateTime.Now.Day;
			int numberOfDaysLastMonth = 0;
			int numberOfDaysTodaysMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
			int numberOfWeeks = 6;
			int startday;
			bool firstendarrived = false;
			bool firstdayofinCalendarisone = false;
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
				//numberOfDaysLastMonth = DateTime.DaysInMonth(DateTime.Now.Year - 1, 12);
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

			//wandelt den Startpunkt in ein Datum um
			if (DateTime.Now.Month > 1)
			{
				dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, startday);
			}
			else
			{
				dt = new DateTime(DateTime.Now.Year - 1, 12, startday);
			}

			//Korrektur des Startbeginns
			switch (dt.DayOfWeek)
			{
				case DayOfWeek.Tuesday:
					startday -= 1;
					break;
				case DayOfWeek.Wednesday:
					startday -= 2;
					break;
				case DayOfWeek.Thursday:
					startday -= 3;
					break;
				case DayOfWeek.Friday:
					startday -= 4;
					break;
				case DayOfWeek.Saturday:
					startday -= 5;
					break;
				case DayOfWeek.Sunday:
					startday -= 6;
					break;
			}

			if (startday + 7 <= numberOfDaysLastMonth)
			{
				startday += 7;
			}
			else
			{
				if (startday + 7 - numberOfDaysLastMonth == 1)
				{
					startday = 1;
					firstdayofinCalendarisone = true;
				}
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
					if (!firstdayofinCalendarisone)
					{
						if (!firstendarrived && startday > numberOfDaysLastMonth)
						{
							startday = 1;
							firstendarrived = true;
						}
					}
					else
					{
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
				mdl.GetlistOfDays().Add(new MainModel.Week()
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

		string CheckDayForMeeting(MainModel.JSONItem item, string rest)
		{
			switch (item.meeting)
			{
				case null:
					return rest + item.lateness.ToString() + item.initial;
				default:
					return rest + item.lateness.ToString() + item.initial + mdl.GetMeetingSign();
			}
		}

		public void AddInfosIntoTheCalendar(string[] dayslist, ArrayList listOfDays)
		{
			if (listOfDays != null)
			{
				try
				{
					for (int i = 0; i < dayslist.Length; i += 7)
					{
						int weeknumber = (int)Math.Floor(((double)i) / 7);
						((MainModel.Week)mdl.GetlistOfDays()[weeknumber]).MO = CheckDayForMeeting((MainModel.JSONItem)listOfDays[i + 1], dayslist[i] + "  ");
						((MainModel.Week)mdl.GetlistOfDays()[weeknumber]).DI = CheckDayForMeeting((MainModel.JSONItem)listOfDays[i + 2], dayslist[i + 1] + "  ");
						((MainModel.Week)mdl.GetlistOfDays()[weeknumber]).MI = CheckDayForMeeting((MainModel.JSONItem)listOfDays[i + 3], dayslist[i + 2] + "  ");
						((MainModel.Week)mdl.GetlistOfDays()[weeknumber]).DO = CheckDayForMeeting((MainModel.JSONItem)listOfDays[i + 4], dayslist[i + 3] + "  ");
						((MainModel.Week)mdl.GetlistOfDays()[weeknumber]).FR = CheckDayForMeeting((MainModel.JSONItem)listOfDays[i + 5], dayslist[i + 4] + "  ");
						((MainModel.Week)mdl.GetlistOfDays()[weeknumber]).SA = CheckDayForMeeting((MainModel.JSONItem)listOfDays[i + 6], dayslist[i + 5] + "  ");
						((MainModel.Week)mdl.GetlistOfDays()[weeknumber]).SO = CheckDayForMeeting((MainModel.JSONItem)listOfDays[i + 7], dayslist[i + 6] + "  ");
					}
				}
				catch (FormatException)
				{
					MessageBox.Show("Datum-Format ist falsch!", "Parsing-Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
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

		public object GetReadEatingPlanJSONObject()
		{
			return mdl.GetReadEatingPlanJSONObject();
		}

		public object GetWriteEatingPlanJSONObject()
		{
			return mdl.GetWriteEatingPlanJSONObject();
		}

		public ArrayList GetAllDays()
		{
			return mdl.GetAllDays();
		}

		public void SetReadDataJSON(string jsonstring)
		{
			mdl.SetReadDataJSON(jsonstring);
		}

		public void SetReadEatingPlanJSON(string jsonstring)
		{
			mdl.SetReadEatingPlanJSON(jsonstring);
		}

		public string GetReadEatingPlanJSON()
		{
			return mdl.GetReadEatingPlanJSON();
		}
	}
}
