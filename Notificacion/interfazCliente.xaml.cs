using static Notificacion.MongoDBManager;

namespace Notificacion;

public partial class interfazCliente : ContentPage
{
	private MongoDBManager _mongoDBManager;
	public interfazCliente()
	{
		InitializeComponent();

		// Configura tu conexión a MongoDB
		string connectionString = MongoDBManager.ConnectionString;
		string databaseName = "Restaurante";
		string usuariosCollectionName = "Usuarios";
		string collectionName = "Platillos";
		string pedidosCollectionName = "Pedidos";
		_mongoDBManager = new MongoDBManager(connectionString, databaseName, usuariosCollectionName, collectionName, pedidosCollectionName);

		// Cargar platillos desde la base de datos y establecer como origen de datos para la lista
		CargarPlatillos();
	}

	private async void CargarPlatillos()
	{
		try
		{
			var platillos = await _mongoDBManager.ObtenerPlatillos();

			// Establecer la imagen para cada platillo
			foreach (var platillo in platillos)
			{
				platillo.ImagenSource = ImageSource.FromStream(() => new MemoryStream(platillo.Imagen));
			}

			listViewPlatillos.ItemsSource = platillos;
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Error al cargar platillos: {ex.Message}", "Aceptar");
		}
	}

	private async void btnCarritoCompras_Clicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new carritoCompras());
	}

	private async void btnAgregarAlCarrito_Clicked(object sender, EventArgs e)
	{
		try
		{
			// Obtener el platillo asociado al botón que se hizo clic
			Button button = (Button)sender;
			Platillo platillo = (Platillo)button.BindingContext;

			// Obtener el estado más reciente del platillo desde la base de datos
			var platilloActualizado = await _mongoDBManager.ObtenerPlatilloPorId(platillo._id);

			// Verificar si el platillo ya está en el carrito
			if (platilloActualizado != null && platilloActualizado.AgregadoAlCarrito)
			{
				// Mostrar mensaje indicando que el platillo ya está en el carrito
				await DisplayAlert("Aviso", "Este platillo ya está en el carrito.", "Aceptar");
			}
			else
			{
				// Actualizar la propiedad AgregadoAlCarrito del platillo
				platillo.AgregadoAlCarrito = true;

				// Actualizar el platillo en MongoDB
				await _mongoDBManager.ActualizarPlatillo(platillo);

				// Mostrar mensaje cuando se actualice correctamente
				await DisplayAlert("Éxito", "Platillo agregado al carrito correctamente", "OK");
			}
		}
		catch (Exception ex)
		{
			// Mostrar aviso de error al usuario
			await DisplayAlert("Error", $"Error al agregar el platillo al carrito: {ex.Message}", "Aceptar");
		}
	}

	private async void OnBackButtonClicked(object sender, EventArgs e)
	{
		// Redirigir a la página de inicio de sesión
		Application.Current.MainPage = new NavigationPage(new MainPage());
	}

}