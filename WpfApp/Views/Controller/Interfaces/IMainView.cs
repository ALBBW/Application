using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp.Views.Controller.Interfaces
{
    interface IMainView : IController
    {
        void ShowView();
        MainWindow GetMainWindow();
    }
}
