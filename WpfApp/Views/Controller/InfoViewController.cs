using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp.Controller;
using WpfApp.Views.Controller.Interfaces;

namespace WpfApp.Views.Controller
{
    public sealed class InfoViewController : IInfoView
    {
        MasterController mctrl;
        InfoView iv;
        IController ic;

        public InfoViewController()
        {
            iv = new InfoView();
            iv.DataContext = this;
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

        }

        void IInfoView.ShowView()
        {
            iv.Show();
        }

        void IController.HideView()
        {
            iv.Hide();
        }

        #region Eigenschaften
        
        #endregion
    }
}
