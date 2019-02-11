namespace WpfApp.Views.Controller.Interfaces
{
	interface IAccountView : IController
	{
		bool? ShowView();
		bool? Reinitialize(string text);
	}
}
