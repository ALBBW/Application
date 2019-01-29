using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp.Controller;
using WpfApp.Views.Controller.Interfaces;
using System.Timers;

namespace WpfApp.Views.Controller
{
    public sealed class MainWindowController : IMainView, IView
    {
        MasterController mctrl;
        MainWindow mw;
        Timer time1;
        IController ic;
        public TextBlock hoveredcell { get; set; }
        
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
            time1 = new Timer(60000);
            time1.Elapsed += Time1_Elapsed;
            time1.Start();
        }

        void OnMainWindowClosing(object sender, EventArgs e)
        {
            mctrl.GetThreadEndingFlag()[1] = true;

            if (!(mctrl.writethreads[1].IsAlive & mctrl.readthreads[1].IsAlive))
            {
                Application.Current.Shutdown();
            }
        }

        private void Time1_Elapsed(object sender, ElapsedEventArgs e)
        {
            string[] dayslist = null;
            mctrl.DetermineDaysInMonth(ref dayslist);
            mctrl.PrepareDataJSONForTransmission(dayslist);
            mctrl.GetThreadEndingFlag()[1] = true;
            mctrl.GetThreadEndingFlag()[1] = false;
            //Console.WriteLine("Timer Event " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second);
            mctrl.StartThread(1, mctrl.GetWriteDataJSONObject(), mctrl.GetReadDataJSONObject());
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
            mctrl.ShowMeetingPanel();
        }

        void OnCellMouseLeave(object sender, MouseEventArgs e)
        {
            mctrl.HideMeetingPanel();
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
        #endregion

        #region Eigenschaften
        MainWindow IMainView.GetMainWindow()
        {
            return mw;
        }
        #endregion
    }
}
