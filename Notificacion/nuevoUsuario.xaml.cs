using static Notificacion.MongoDBManager;

namespace Notificacion;

public partial class nuevoUsuario : ContentPage
{
	private byte[] _imgUsuario;
	private MongoDBManager _mongoDBManager;
	public nuevoUsuario()
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
					_imgUsuario = memoria.ToArray();
				}

				// Actualiza la imagen en la interfaz de usuario
				imgUsuario.Source = ImageSource.FromStream(() => new MemoryStream(_imgUsuario));
			}
		}
		catch (Exception ex)
		{
			// Mostrar aviso de error al usuario
			await DisplayAlert("Error", $"Error al cargar la foto: {ex.Message}", "Aceptar");
		}

	}

	private string generoSeleccionado;
	private void cbMasculino_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (cbMasculino.IsChecked)
		{
			generoSeleccionado = "Masculino";
			cbFemenino.IsChecked = false;
		}
		else if (!cbFemenino.IsChecked)
		{
			generoSeleccionado = ""; // Ningún CheckBox está seleccionado, establecer el género como vacío
		}
	}

	private void cbFemenino_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (cbFemenino.IsChecked)
		{
			generoSeleccionado = "Femenino";
			cbMasculino.IsChecked = false;
		}
		else if (!cbMasculino.IsChecked)
		{
			generoSeleccionado = ""; // Ningún CheckBox está seleccionado, establecer el género como vacío
		}
	}

	private async void btnGuardar_Clicked(object sender, EventArgs e)
	{
		// Validar la entrada...
		if (string.IsNullOrWhiteSpace(tbNombre.Text) ||
			string.IsNullOrWhiteSpace(tbTelefono.Text) ||
			string.IsNullOrWhiteSpace(tbEmail.Text) ||
			string.IsNullOrWhiteSpace(txtContraseña.Text) ||
			string.IsNullOrWhiteSpace(txtRepetirContraseña.Text) || imgUsuario.Source == null)
		{
			await DisplayAlert("Error", "Por favor, complete todos los campos obligatorios.", "Aceptar");
			return;
		}

		// Método para verificar si una cadena contiene solo números
		bool EsNumero(string valor)
		{
			foreach (char c in valor)
			{
				if (!char.IsDigit(c))
				{
					return false;
				}
			}
			return true;
		}
		// Validar que el teléfono solo contenga dígitos y tenga una longitud máxima de 10
		if (!EsNumero(tbTelefono.Text))
		{
			await DisplayAlert("Error", "El número de teléfono no es valido.", "OK");
			return; // Evita continuar con la operación de guardar
		}
		else if (tbTelefono.Text.Length != 10)
		{
			await DisplayAlert("Error", "El número de teléfono no es valido.", "OK");
			return; // Evita continuar con la operación de guardar
		}

		// Validar que las contraseñas coincidan...
		if (txtContraseña.Text != txtRepetirContraseña.Text)
		{
			await DisplayAlert("Error", "Las contraseñas no coinciden.", "Aceptar");
			return;
		}

		// Determinar el género seleccionado...
		string genero = cbMasculino.IsChecked ? "Masculino" : cbFemenino.IsChecked ? "Femenino" : null;

		// Crear una instancia de Usuario con los datos de la interfaz...
		var nuevoUsuario = new Usuario
		{
			Nombre = tbNombre.Text,
			Telefono = tbTelefono.Text,
			Email = tbEmail.Text,
			Foto = _imgUsuario, // Implementa este método para obtener la foto como byte array
			Genero = genero,
			FechaNacimiento = dpFechaNacimiento.Date,
			Contraseña = txtContraseña.Text
			// Otros campos si es necesario
		};

		try
		{
			// Insertar el nuevo usuario en MongoDB...
			await _mongoDBManager.InsertarUsuario(nuevoUsuario);
			await DisplayAlert("Éxito", "Usuario creado exitosamente.", "Aceptar");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Error al guardar el usuario: {ex.Message}", "Aceptar");
		}
	}
}