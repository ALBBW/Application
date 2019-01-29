using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp.Views.Controller.Interfaces
{
    interface IAccountView : IController
    {
        bool? ShowView();
        void Reinitialize();
        void SetWarningLabel(string text);
    }
}
