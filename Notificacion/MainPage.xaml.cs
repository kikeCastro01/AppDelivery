using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Plugin.Firebase.CloudMessaging;
using FirebaseAdmin.Messaging;

namespace Notificacion
{
	public partial class MainPage : ContentPage
	{
		private MongoDBManager _mongoDBManager;
		public MainPage()
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

		private async void btnIngresar_Clicked(object sender, EventArgs e)
		{
			try
			{
				string nombreUsuario = User.Text;
				string contraseña = ePassword.Text;

				// Verificar si el usuario y la contraseña no están vacíos
				if (string.IsNullOrEmpty(nombreUsuario) || string.IsNullOrEmpty(contraseña))
				{
					await DisplayAlert("Error", "Por favor, ingrese usuario y contraseña", "Aceptar");
					return;
				}

				// Verificar si el usuario y la contraseña coinciden con el usuario específico para la interfaz de restaurante
				if (nombreUsuario == "yummy" && contraseña == "1")
				{
					// Si el usuario es el específico para el restaurante, redirigir a la interfaz de restaurante
					Application.Current.MainPage = new interfazRestaurante();
				}
				else
				{
					// Verificar si las credenciales son válidas en la base de datos para cualquier otro usuario
					bool credencialesValidas = await _mongoDBManager.VerificarCredenciales(nombreUsuario, contraseña);

					if (credencialesValidas)
					{
						// Guardar el nombre de usuario en la clase auxiliar
						AppState.NombreUsuario = nombreUsuario;
						// Si las credenciales son válidas para cualquier otro usuario, redirigir a la interfaz de clientes
						Application.Current.MainPage = new NavigationPage(new interfazCliente());
					}
					else
					{
						// Mostrar mensaje de error si las credenciales son incorrectas
						await DisplayAlert("Error", "El usuario no existe", "Aceptar");
					}
				}
			}
			catch (NullReferenceException ex)
			{
				// Manejar la excepción
				await DisplayAlert("Error", "Se produjo un error: " + ex.Message, "Aceptar");
			}
			catch (Exception ex)
			{
				// Otro tipo de excepción no prevista
				await DisplayAlert("Error", "Se produjo un error: " + ex.Message, "Aceptar");
			}
		}

		private void btnNewUser_Clicked(object sender, EventArgs e)
		{
			Application.Current.MainPage = new NavigationPage(new nuevoUsuario());

		}
	}

}
