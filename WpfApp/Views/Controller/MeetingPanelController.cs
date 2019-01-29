using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp.Controller;
using WpfApp.Views.Controller.Interfaces;

namespace WpfApp.Views.Controller
{
    public sealed class MeetingPanelController : IMeetingPanelView
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

            mctrl.AddController(this);
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
                List<Model.JSONItem> markeditems = new List<Model.JSONItem>();

                foreach (object item in templist)
                {
                    if (item.GetType() != typeof(Model.JSONHeader) && ((Model.JSONItem)item).meeting != null)
                    {
                        markeditems.Add((Model.JSONItem)item);
                    }
                }

                templist = null;

                foreach (Model.JSONItem item in markeditems)
                {
                    if (mctrl.GetMainWindowController().hoveredcell.Text.Substring(0, 1) == item.date.Day.ToString())
                    {
                        string temptime = "";
                        string tempsubject = "";
                        string tempinfo = "";

                        foreach (Model.MeetingObject meeting in item.meeting)
                        {
                            temptime += meeting.time.ToShortDateString() + " : " + meeting.time.ToShortTimeString() + "\n\n";
                            tempsubject += meeting.subject + "\n\n";
                            tempinfo += meeting.info + "\n\n";

                        }

                        mp.TB_Date.Text = temptime;
                        mp.TB_Subject.Text = tempsubject;
                        mp.TB_Info.Text = tempinfo;
                    }
                }
            }

            mp.Show();
        }

        #region Events
        
        #endregion
    }
}
