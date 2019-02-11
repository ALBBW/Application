using System.Windows;
using WpfApp.Controller;
using WpfApp.Views.Controller.Interfaces;

namespace WpfApp.Views.Controller
{
	public sealed class EatingPlanController : IView, IEatingPlan
	{
		MasterController mctrl;
		EatingPlan ep;
		IController ic;

		public EatingPlanController()
		{
			ep = new EatingPlan();
			ic = this;
			ic.InstantiateMasterController();
			ic.Start();
		}

		void IController.Start()
		{
			ep.Btn_Close.Click += HideView;
		}

		void IController.InstantiateMasterController()
		{
			if (MasterController.Instance != null)
			{
				mctrl = MasterController.Instance;
			}
		}

		void IController.HideView()
		{
			ep.Hide();
		}

		void IEatingPlan.ShowView()
		{
			ep.Show();
		}

		public void SetItemsSource()
		{
			ep.DGrid.ItemsSource = mctrl.eatingItemList.EatingItems;
		}

		#region Events
		void HideView(object sender, RoutedEventArgs e)
		{
			ic.HideView();
		}
		#endregion
	}
}
