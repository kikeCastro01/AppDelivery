namespace Notificacion;

public partial class cerrarSesionFlyoutPage : ContentPage
{
	public cerrarSesionFlyoutPage()
	{
		InitializeComponent();
	}

	private void btnCerrarSesion_Clicked(object sender, EventArgs e)
	{
		// Redirigir a la página de inicio de sesión
		Application.Current.MainPage = new NavigationPage(new MainPage());
	}
}