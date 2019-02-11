using System.Collections.Generic;
using WpfApp.Views.Controller.Interfaces;

namespace WpfApp.Views.Controller
{
	public static class ViewsConstructor
	{
		public static void InstantiateViewControllers(List<IView> views)
		{
			views.Add(new AccountLoginController());
			views.Add(new MeetingPanelController());
			views.Add(new EatingPlanController());
		}

		public static void ShowMeetingPanel(List<IView> views)
		{
			foreach (object ctrl in views)
			{
				if (ctrl is IMeetingPanelView)
				{
					((IMeetingPanelView)ctrl).ShowView();
				}
			}
		}

		public static void HideMeetingPanel(List<IView> views)
		{
			foreach (object ctrl in views)
			{
				if (ctrl is IMeetingPanelView)
				{
					((IMeetingPanelView)ctrl).HideView();
				}
			}
		}

		public static void ShowEatingPlan(List<IView> views)
		{
			foreach (object ctrl in views)
			{
				if (ctrl is IEatingPlan)
				{
					((IEatingPlan)ctrl).ShowView();
				}
			}
		}

		public static void HideEatingPlan(List<IView> views)
		{
			foreach (object ctrl in views)
			{
				if (ctrl is IEatingPlan)
				{
					((IEatingPlan)ctrl).HideView();
				}
			}
		}

		public static MainWindow GetMainWindow(List<IView> views)
		{
			foreach (object ctrl in views)
			{
				if (ctrl is IMainView)
				{
					return ((IMainView)ctrl).GetMainWindow();
				}
			}

			return null;
		}

		public static MainWindowController GetMainWindowController(List<IView> views)
		{
			foreach (object ctrl in views)
			{
				if (ctrl is IMainView)
				{
					return (MainWindowController)ctrl;
				}
			}

			return null;
		}

		public static bool? ShowAccountLoginView(List<IView> views, bool firstOpening, bool loginError)
		{
			foreach (object ctrl in views)
			{
				if (ctrl is IAccountView)
				{
					if (firstOpening && !loginError)
					{
						return ((IAccountView)ctrl).ShowView();
					}
					else if (!firstOpening && loginError)
					{
						return ((IAccountView)ctrl).Reinitialize("Fehler beim Login! Der Benutzername oder das Passwort war falsch.");
					}
					else
					{
						return ((IAccountView)ctrl).Reinitialize("Der Datenbankserver antwortet nicht!");
					}
				}
			}

			return false;
		}

		public static void SetEatingPlanItemsSource(List<IView> views)
		{
			foreach (object ctrl in views)
			{
				if (ctrl is IEatingPlan)
				{
					((EatingPlanController)ctrl).SetItemsSource();
				}
			}
		}
	}
}
