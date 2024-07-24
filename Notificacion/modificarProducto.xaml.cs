using static Notificacion.MongoDBManager;

namespace Notificacion;

public partial class modificarProducto : ContentPage
{
	private Platillo _platillo;
	private MongoDBManager _mongoDBManager;
	private byte[] _imagenPlatillo;
	public modificarProducto(Platillo platillo, MongoDBManager mongoDBManager)
	{
		InitializeComponent();
		_platillo = platillo;
		_mongoDBManager = mongoDBManager;

		imgPlatillo.Source = ImageSource.FromStream(() => new MemoryStream(_platillo.Imagen));
		_imagenPlatillo = _platillo.Imagen;
		tbNombre.Text = _platillo.Nombre;
		tbDescripcion.Text = _platillo.Descripcion;
		tbPrecio.Text = _platillo.Precio.ToString();
	}

	private async void btnGuardar_Clicked(object sender, EventArgs e)
	{
		try
		{
			// Update the platillo data
			_platillo.Nombre = tbNombre.Text;
			_platillo.Descripcion = tbDescripcion.Text;
			_platillo.Precio = decimal.TryParse(tbPrecio.Text, out decimal precio) ? precio : _platillo.Precio;
			_platillo.Imagen = _imagenPlatillo; // Update the image

			// Save the updated platillo to the database
			await _mongoDBManager.ModificarPlatillo(_platillo);

			// Mostrar mensaje de éxito
			await DisplayAlert("Éxito", "Platillo modificado correctamente", "OK");

			// Navigate back to the previous page
			await Navigation.PopAsync();
		}
		catch (Exception ex)
		{
			// Mostrar aviso de error al usuario
			await DisplayAlert("Error", $"Error al guardar el platillo: {ex.Message}", "Aceptar");
		}
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
					_platillo.Imagen = memoria.ToArray();
				}

				// Actualiza la imagen en la interfaz de usuario
				imgPlatillo.Source = ImageSource.FromStream(() => new MemoryStream(_platillo.Imagen));
			}
		}
		catch (Exception ex)
		{
			// Mostrar aviso de error al usuario
			await DisplayAlert("Error", $"Error al cargar la foto: {ex.Message}", "Aceptar");
		}
	}
}