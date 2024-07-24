using static Notificacion.MongoDBManager;

namespace Notificacion;

public partial class nuevoUsuario : ContentPage
{
	private byte[] _imgUsuario;
	private MongoDBManager _mongoDBManager;
	public nuevoUsuario()
	{
		InitializeComponent();

		// Configura tu conexi�n a MongoDB
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
			generoSeleccionado = ""; // Ning�n CheckBox est� seleccionado, establecer el g�nero como vac�o
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
			generoSeleccionado = ""; // Ning�n CheckBox est� seleccionado, establecer el g�nero como vac�o
		}
	}

	private async void btnGuardar_Clicked(object sender, EventArgs e)
	{
		// Validar la entrada...
		if (string.IsNullOrWhiteSpace(tbNombre.Text) ||
			string.IsNullOrWhiteSpace(tbTelefono.Text) ||
			string.IsNullOrWhiteSpace(tbEmail.Text) ||
			string.IsNullOrWhiteSpace(txtContrase�a.Text) ||
			string.IsNullOrWhiteSpace(txtRepetirContrase�a.Text) || imgUsuario.Source == null)
		{
			await DisplayAlert("Error", "Por favor, complete todos los campos obligatorios.", "Aceptar");
			return;
		}

		// M�todo para verificar si una cadena contiene solo n�meros
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
		// Validar que el tel�fono solo contenga d�gitos y tenga una longitud m�xima de 10
		if (!EsNumero(tbTelefono.Text))
		{
			await DisplayAlert("Error", "El n�mero de tel�fono no es valido.", "OK");
			return; // Evita continuar con la operaci�n de guardar
		}
		else if (tbTelefono.Text.Length != 10)
		{
			await DisplayAlert("Error", "El n�mero de tel�fono no es valido.", "OK");
			return; // Evita continuar con la operaci�n de guardar
		}

		// Validar que las contrase�as coincidan...
		if (txtContrase�a.Text != txtRepetirContrase�a.Text)
		{
			await DisplayAlert("Error", "Las contrase�as no coinciden.", "Aceptar");
			return;
		}

		// Determinar el g�nero seleccionado...
		string genero = cbMasculino.IsChecked ? "Masculino" : cbFemenino.IsChecked ? "Femenino" : null;

		// Crear una instancia de Usuario con los datos de la interfaz...
		var nuevoUsuario = new Usuario
		{
			Nombre = tbNombre.Text,
			Telefono = tbTelefono.Text,
			Email = tbEmail.Text,
			Foto = _imgUsuario, // Implementa este m�todo para obtener la foto como byte array
			Genero = genero,
			FechaNacimiento = dpFechaNacimiento.Date,
			Contrase�a = txtContrase�a.Text
			// Otros campos si es necesario
		};

		try
		{
			// Insertar el nuevo usuario en MongoDB...
			await _mongoDBManager.InsertarUsuario(nuevoUsuario);
			await DisplayAlert("�xito", "Usuario creado exitosamente.", "Aceptar");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Error al guardar el usuario: {ex.Message}", "Aceptar");
		}
	}
}