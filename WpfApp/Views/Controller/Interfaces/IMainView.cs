namespace WpfApp.Views.Controller.Interfaces
{
	interface IMainView : IController
	{
		void ShowView();
		MainWindow GetMainWindow();
	}
}
