namespace Notificacion;

public partial class cerrarSesionFlyoutPage : ContentPage
{
	public cerrarSesionFlyoutPage()
	{
		InitializeComponent();
	}

	private void btnCerrarSesion_Clicked(object sender, EventArgs e)
	{
		// Redirigir a la p�gina de inicio de sesi�n
		Application.Current.MainPage = new NavigationPage(new MainPage());
	}
}