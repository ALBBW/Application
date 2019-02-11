using System;
using System.Net.NetworkInformation;
using System.Windows;
using WpfApp.Controller;
using WpfApp.Views.Controller.Interfaces;

namespace WpfApp.Views.Controller
{
	public sealed class AccountLoginController : IAccountView, IView
	{
		MasterController mctrl;
		Account_Login al;
		IAccountView ic;

		public AccountLoginController()
		{
			al = new Account_Login();
			al.DataContext = this;
			ic = this;
			ic.InstantiateMasterController();
			ic.Start();
		}

		bool? IAccountView.ShowView()
		{
			return al.ShowDialog();
		}

		void IController.HideView()
		{
			al.Hide();
		}

		bool? IAccountView.Reinitialize(string message)
		{
			ic.HideView();
			al = new Account_Login();
			al.Btn_login.Click += Button_Click;
			al.lbl_warning.Text = message;
			return al.ShowDialog();
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
			al.Btn_login.Click += Button_Click;
		}

		#region Events
		public void Button_Click(object sender, RoutedEventArgs e)
		{
			Ping p = new Ping();
			PingReply pr = mctrl.SendPing(p);

			if (pr.Status != IPStatus.Success)
			{
				al.lbl_warning.Text = "Der Mediator wurde nicht erreicht!";
				//return;
			}

			if (al.txtb_loginalias.Text != "" && al.pwb_loginpassword.Password != "")
			{
				mctrl.SaveTemporaryUserAccount(al.txtb_loginalias.Text, al.pwb_loginpassword.Password);
			}

			ic.HideView();
		}

		void OnLoginWindowClosing(object sender, EventArgs e)
		{
			mctrl.GetThreadEndingFlag()[1] = true;

			if
			(
				mctrl.writethreads != null & mctrl.readthreads != null &&
				!(mctrl.writethreads[1].IsAlive & mctrl.readthreads[1].IsAlive)
			)
			{
				Application.Current.Shutdown();
			}
		}
		#endregion

		#region Eigenschaften
		public string GetLoginname()
		{
			return al.txtb_loginalias.Text;
		}

		public string GetPassword()
		{
			return al.pwb_loginpassword.Password;
		}
		#endregion
	}
}
