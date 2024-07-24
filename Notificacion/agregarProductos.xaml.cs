using static Notificacion.MongoDBManager;

namespace Notificacion;

public partial class agregarProductos : ContentPage
{
	private MongoDBManager _mongoDBManager;
	private byte[] _imagenPlatillo;

	public agregarProductos()
	{
		InitializeComponent();

		// Configura tu conexión a MongoDB
		string connectionString = MongoDBManager.ConnectionString;
		string databaseName = "Restaurante";
		string usuariosCollectionName = "Usuarios";
		string collectionName = "Platillos";
		string pedidosCollectionName = "Pedidos";
		_mongoDBManager = new MongoDBManager(connectionString, databaseName, usuariosCollectionName, collectionName, pedidosCollectionName);
	}

	private async void btnCargarFoto_Clicked(object sender, EventArgs e)
	{
		try
		{
			var resultado = await MediaPicker.PickPhotoAsync();
			if (resultado != null)
			{
				// Convierte la imagen seleccionada a un byte array
				using (var stream = await resultado.OpenReadAsync())
				{
					var memoria = new MemoryStream();
					await stream.CopyToAsync(memoria);
					_imagenPlatillo = memoria.ToArray();
				}

				// Actualiza la imagen en la interfaz de usuario
				imgPlatillo.Source = ImageSource.FromStream(() => new MemoryStream(_imagenPlatillo));
			}
		}
		catch (Exception ex)
		{
			// Mostrar aviso de error al usuario
			await DisplayAlert("Error", $"Error al cargar la foto: {ex.Message}", "Aceptar");
		}
	}

	private async void btnAgregar_Clicked(object sender, EventArgs e)
	{
		try
		{
			// Crea una instancia de Platillo con los datos ingresados por el usuario
			Platillo nuevoPlatillo = new Platillo
			{
				Nombre = tbNombre.Text,
				Descripcion = tbDescripcion.Text,
				Precio = decimal.Parse(tbPrecio.Text), // Asegúrate de manejar conversiones seguras aquí
				Imagen = _imagenPlatillo
			};

			// Inserta el nuevo platillo en MongoDB
			await _mongoDBManager.InsertarPlatillo(nuevoPlatillo);

			// Mostrar mensaje cuando se agregue correctamente
			await DisplayAlert("Éxito", "Platillo agregado correctamente", "OK");
		}
		catch (Exception ex)
		{
			// Mostrar aviso de error al usuario
			await DisplayAlert("Error", $"Error al agregar el platillo: {ex.Message}", "Aceptar");
		}
	}
}