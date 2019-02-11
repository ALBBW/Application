using System;
using System.Globalization;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp.Controller;
using WpfApp.Model;
using WpfApp.Views.Controller.Interfaces;

namespace WpfApp.Views.Controller
{
	public sealed class MainWindowController : IMainView, IView
	{
		MasterController mctrl;
		MainWindow mw;
		Timer time1;
		IController ic;
		public TextBlock hoveredcell { get; private set; }

		public MainWindowController(MainWindow mw)
		{
			this.mw = mw;
			ic = this;
			ic.InstantiateMasterController();
			ic.Start();
		}

		void IController.InstantiateMasterController()
		{
			if (MasterController.Instance != null)
			{
				mctrl = MasterController.Instance;
			}

			mctrl.AddController(this);
		}

		void IController.Start()
		{
			mw.Closing += OnMainWindowClosing;
			mw.DGrid.ItemsSource = mctrl.GetList();
			mw.DGrid.SizeChanged += new SizeChangedEventHandler(DGrid_SizeChanged);
			mw.lblMonth.Content = DateTime.Now.Date.ToString("MMMM", CultureInfo.CreateSpecificCulture("de-DE"));
			mw.DGrid.ItemContainerGenerator.StatusChanged += CustomizeCells;
			mw.DGrid.FontWeight = FontWeights.Bold;
			mw.Btn_EatingPlan.Click += Btn_EatingPlan_Click;
			time1 = new Timer(60000);
			time1.Elapsed += Time1_Elapsed;
			time1.Start();
		}

		void OnMainWindowClosing(object sender, EventArgs e)
		{
			mctrl.GetThreadEndingFlag()[0] = true;
			mctrl.GetThreadEndingFlag()[1] = true;
			mctrl.GetThreadEndingFlag()[2] = true;

			while (true)
			{
				if (
				!(
					mctrl.writethreads[0].IsAlive &
					mctrl.readthreads[0].IsAlive &
					mctrl.writethreads[1].IsAlive &
					mctrl.readthreads[1].IsAlive &
					mctrl.writethreads[2].IsAlive &
					mctrl.readthreads[2].IsAlive
				))
				{
					Application.Current.Shutdown();
					return;
				}
			}
		}

		void Time1_Elapsed(object sender, ElapsedEventArgs e)
		{
			string[] dayslist = null;
			mctrl.DetermineDaysInMonth(ref dayslist);
			mctrl.PrepareDataJSONForTransmission(dayslist);

			if ((string)mctrl.GetReadDataJSONObject() != string.Empty)
			{
				mctrl.AddInfosIntoTheCalendar(dayslist, mctrl.parseDataJSONString((string)mctrl.GetReadDataJSONObject()));
				mctrl.SetReadDataJSON("");
			}

			if ((string)mctrl.GetReadEatingPlanJSONObject() != string.Empty)
			{
				//string testjson = "{{\"sender\":\"10.122.122.110\",\"port\":9010,\"reason\":\"ReceiveEatingPlanData\"},{\"EatingItemDate\":\"2019 - 02 - 04T00: 00:00\",\"EatingItemDescription\":\"Essen1\"},{\"EatingItemDate\":\"2019 - 02 - 05T00: 00:00\",\"EatingItemDescription\":\"Essen2\"},{\"EatingItemDate\":\"2019 - 02 - 06T00: 00:00\",\"EatingItemDescription\":\"Essen3\"},{\"EatingItemDate\":\"2019 - 02 - 07T00: 00:00\",\"EatingItemDescription\":\"Essen4\"},{\"EatingItemDate\":\"2019 - 02 - 08T00: 00:00\",\"EatingItemDescription\":\"Essen5\"}}";
				//mctrl.eatingItemList = new MainModel.EatingItemList(new System.Collections.ObjectModel.ObservableCollection<MainModel.EatingItem>(mctrl.parseEatingPlanJSONString(testjson)));
				mctrl.eatingItemList = new MainModel.EatingItemList
				(
					new System.Collections.ObjectModel.ObservableCollection<MainModel.EatingItem>
					(
						mctrl.parseEatingPlanJSONString(mctrl.GetReadEatingPlanJSON())
					)
				);
				ViewsConstructor.SetEatingPlanItemsSource(mctrl.ctrlList);
				mctrl.SetReadEatingPlanJSON("");
			}

			mctrl.GetThreadEndingFlag()[1] = true;
			mctrl.GetThreadEndingFlag()[2] = true;
			mctrl.GetThreadEndingFlag()[1] = false;
			mctrl.GetThreadEndingFlag()[2] = false;
			//Console.WriteLine("Timer Event " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second);
			mctrl.StartThread(1, mctrl.GetWriteDataJSONObject(), mctrl.GetReadDataJSONObject());
			mctrl.PrepareEatingPlanJSONForTransmission();
			mctrl.StartThread(2, mctrl.GetWriteEatingPlanJSONObject(), mctrl.GetReadEatingPlanJSONObject());
			time1.Interval = 60000;
			time1.Start();
		}

		void IMainView.ShowView()
		{
			mw.Show();
		}

		void IController.HideView()
		{
			mw.Hide();
		}

		/*public void SetWindowModal(Account_Login al)
        {
            mw.Owner = al;
        }*/

		#region Events
		public void DGrid_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (e.WidthChanged)
			{
				((DataGrid)sender).RowHeight = ((DataGrid)sender).Columns.ElementAt(0).Width.DesiredValue;
			}
		}

		void OnCellMouseEnter(object sender, MouseEventArgs e)
		{
			hoveredcell = (TextBlock)sender;
			ViewsConstructor.ShowMeetingPanel(mctrl.ctrlList);
		}

		void OnCellMouseLeave(object sender, MouseEventArgs e)
		{
			ViewsConstructor.HideMeetingPanel(mctrl.ctrlList);
		}

		void CustomizeCells(object sender, EventArgs e)
		{
			DataGridRow row;

			for (int i = 0; i < mw.DGrid.Items.Count; i++)
			{
				row = (DataGridRow)mw.DGrid.ItemContainerGenerator.ContainerFromIndex(i);

				switch (row)
				{
					case null: break;
					default:
						for (int col = 0; col < 7; col++)
						{
							FrameworkElement cellcontent = mw.DGrid.Columns[col].GetCellContent(row);
							cellcontent.Margin = new Thickness(0, 0, 0, 0);

							if (cellcontent != null && ((TextBlock)cellcontent).Text.Contains(mctrl.GetMeetingSign()))
							{
								((TextBlock)cellcontent).Background = Brushes.Green;
								((TextBlock)cellcontent).Foreground = Brushes.White;
								((TextBlock)cellcontent).MouseEnter += OnCellMouseEnter;
								((TextBlock)cellcontent).MouseLeave += OnCellMouseLeave;
							}
						}

						break;
				}
			}
		}

		void Btn_EatingPlan_Click(object sender, RoutedEventArgs e)
		{
			ViewsConstructor.ShowEatingPlan(mctrl.ctrlList);
		}
		#endregion

		#region Eigenschaften
		MainWindow IMainView.GetMainWindow()
		{
			return mw;
		}
		#endregion
	}
}
