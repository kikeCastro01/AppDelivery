using static Notificacion.MongoDBManager;

namespace Notificacion;

public partial class consultarProductos : ContentPage
{
	private MongoDBManager _mongoDBManager;

	public consultarProductos()
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


	private async void btnEliminarPlatillo_Clicked(object sender, EventArgs e)
	{
		try
		{
			// Obtener el botón que desencadenó el evento
			var button = sender as Button;

			// Obtener el platillo asociado al botón
			var platillo = button?.BindingContext as Platillo;

			// Verificar si el platillo es válido
			if (platillo != null)
			{
				// Llamar al método en MongoDBManager para eliminar el platillo de la base de datos
				await _mongoDBManager.EliminarPlatillo(platillo);

				// Recargar la lista de platillos
				CargarPlatillos();

				// Mostrar un mensaje de éxito
				await DisplayAlert("Éxito", "Platillo eliminado correctamente", "OK");
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Error al eliminar el platillo: {ex.Message}", "Aceptar");
		}
	}

	//private async void btnModificarPlatillo_Clicked(object sender, EventArgs e)
	//{
	//	try
	//	{
	//		var button = sender as Button;
	//		var platillo = button?.BindingContext as Platillo;
	//		if (platillo != null)
	//		{
	//			await Navigation.PushAsync(new modificarProducto(platillo, _mongoDBManager));
	//		}
	//	}
	//	catch (Exception ex)
	//	{
	//		await DisplayAlert("Error", $"Error al editar el platillo: {ex.Message}", "Aceptar");
	//	}
	//}
}