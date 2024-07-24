namespace Notificacion
{
	public partial class App : Application
	{
		public App()
		{
			Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzI5NTY5OEAzMjM1MmUzMDJlMzBmeTlOcGtMSFRXVGR0T1IrcTRVQ215MUJlbklWZFpKYzZYUCtYMEs1WmowPQ==");
			InitializeComponent();

			MainPage = new NavigationPage(new MainPage());
		}
	}
}
