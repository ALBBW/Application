using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WpfApp.Controller;
using WpfApp.Model;
using WpfApp.Views.Controller.Interfaces;

namespace WpfApp.Views.Controller
{
	public sealed class MeetingPanelController : IMeetingPanelView, IView
	{
		MasterController mctrl;
		MeetingPanel mp;
		IController ic;

		public MeetingPanelController()
		{
			mp = new MeetingPanel();
			mp.DataContext = this;
			ic = this;
			ic.InstantiateMasterController();
			ic.Start();
		}

		void IController.HideView()
		{
			mp.Hide();
		}

		void IController.InstantiateMasterController()
		{
			if (MasterController.Instance != null)
			{
				mctrl = MasterController.Instance;
			}
		}

		void IController.Start()
		{

		}

		void IMeetingPanelView.ShowView()
		{
			if (!mp.IsVisible)
			{
				ExternFunctions.POINT mousepos = new ExternFunctions.POINT();
				ExternFunctions.GetCursorPos(out mousepos);
				mp.Left = mousepos.X;
				mp.Top = mousepos.Y;
				ArrayList templist = mctrl.GetAllDays();
				List<MainModel.JSONItem> markeditems = new List<MainModel.JSONItem>();
				string tempmeeting = "";
				string hoveredcelltext = ViewsConstructor.GetMainWindowController(mctrl.ctrlList).hoveredcell.Text.Substring(0, 1);

				foreach (object item in templist)
				{
					if (item.GetType() != typeof(MainModel.JSONHeader) && ((MainModel.JSONItem)item).meeting != null)
					{
						markeditems.Add((MainModel.JSONItem)item);
					}
				}

				templist = null;

				foreach (MainModel.JSONItem item in markeditems)
				{
					tempmeeting = "";

					foreach (MainModel.MeetingObject meeting in item.meeting)
					{
						if
						(
							item.meeting.Count > 5
						)
						{
							if (meeting != item.meeting.ElementAt(5))
							{
								if (hoveredcelltext == item.date.Day.ToString())
								{
									tempmeeting += meeting.time.ToShortTimeString() + " : " + meeting.subject + "\n";
								}
							}
							else
							{
								tempmeeting += "...\n";
							}
						}
						else
						{
							if (hoveredcelltext == item.date.Day.ToString())
							{
								tempmeeting += meeting.time.ToShortTimeString() + " : " + meeting.subject + "\n";
							}
						}
					}

					if (hoveredcelltext == item.date.Day.ToString())
					{
						mp.TB_Meeting.Text = tempmeeting;
					}
				}
			}

			mp.Show();
		}

		#region Events

		#endregion
	}
}
