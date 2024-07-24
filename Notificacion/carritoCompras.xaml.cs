using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using static Notificacion.MongoDBManager;
using FirebaseAdmin.Messaging;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;

namespace Notificacion;

public partial class carritoCompras : ContentPage
{
	private MongoDBManager _mongoDBManager;
	public carritoCompras()
	{
		InitializeComponent();
		lblNombrePedido.Text = AppState.NombreUsuario;
		ReadFireBaseAdminSdk();
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

	public void UpdateLocationInfo(string address, double latitude, double longitude)
	{
		lblDireccion.Text = address;
		lblLatitud.Text = latitude.ToString();
		lblLongitud.Text = longitude.ToString();
	}
	private async void ReadFireBaseAdminSdk()
	{
		var stream = await FileSystem.OpenAppPackageFileAsync("admin_sdk.json");
		var reader = new StreamReader(stream);
		var jsonContent = reader.ReadToEnd();

		if (FirebaseMessaging.DefaultInstance == null)
		{
			FirebaseApp.Create(new AppOptions()
			{
				Credential = GoogleCredential.FromJson(jsonContent)
			});
		}
	}

	private async void CargarPlatillos()
	{
		try
		{
			// Obtener todos los platillos de la base de datos
			var platillos = await _mongoDBManager.ObtenerPlatillos();

			// Filtrar los platillos para mostrar solo los que están agregados al carrito
			var platillosEnCarrito = platillos.Where(p => p.AgregadoAlCarrito == true).ToList();

			// Establecer la imagen para cada platillo
			foreach (var platillo in platillosEnCarrito)
			{
				platillo.ImagenSource = ImageSource.FromStream(() => new MemoryStream(platillo.Imagen));
			}

			// Establecer la lista filtrada como origen de datos para el ListView
			listViewPlatillos.ItemsSource = platillosEnCarrito;

		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Error al cargar platillos en el carrito: {ex.Message}", "Aceptar");
		}
	}

	private async void btnRealizarPedido_Clicked(object sender, EventArgs e)
	{
		try
		{
			// Verificar si hay platillos en el carrito
			var platillosEnCarrito = (List<Platillo>)listViewPlatillos.ItemsSource;
			if (platillosEnCarrito == null || !platillosEnCarrito.Any())
			{
				await DisplayAlert("Aviso", "Por favor, agregue platillos al carrito antes de realizar el pedido.", "Aceptar");
				return;
			}

			// Verificar si la ubicación ha sido seleccionada
			if (string.IsNullOrWhiteSpace(lblDireccion.Text) || string.IsNullOrWhiteSpace(lblLatitud.Text) || string.IsNullOrWhiteSpace(lblLongitud.Text))
			{
				await DisplayAlert("Aviso", "Por favor, seleccione una ubicación antes de realizar el pedido.", "Aceptar");
				return;
			}

			// Obtener el total del pedido desde el label
			decimal totalPedido = decimal.TryParse(lblTotal.Text.Replace("Total: ", "").Replace("$", ""), out decimal total) ? total : 0;

			// Guardar los detalles de los platillos en el pedido
			List<string> detallesPedido = new List<string>();
			// Recoge los datos de las etiquetas
			string nombrePedido = lblNombrePedido.Text;
			double latitud = double.Parse(lblLatitud.Text);
			double longitud = double.Parse(lblLongitud.Text);
			string direccion = lblDireccion.Text;

			string cuerpoNotificacion = "Detalles del pedido:\n";

			foreach (var platillo in platillosEnCarrito)
			{
				// Obtener la cantidad ingresada para este platillo desde el evento del Entry
				decimal cantidad = platillo.Cantidad;

				// Si la cantidad es mayor que cero, agregar los detalles del platillo a la lista
				if (cantidad > 0)
				{
					string detalle = $"{cantidad} x {platillo.Nombre} - {platillo.Subtotal:C}";
					detallesPedido.Add($"{cantidad} {platillo.Nombre}, {platillo.Subtotal:C}");
					cuerpoNotificacion += detalle + "\n";
				}
			}

			// Guardar el pedido en la colección
			await _mongoDBManager.GuardarPedidoEnColeccion(nombrePedido, detallesPedido, totalPedido, latitud, longitud, direccion);

			// Actualizar los platillos en el carrito en MongoDB
			await _mongoDBManager.ActualizarPlatillosEnCarrito(platillosEnCarrito);

			// Limpiar la lista de platillos en el ListView
			listViewPlatillos.ItemsSource = null;

			// Establecer el texto del lblTotal en "$0.00"
			lblTotal.Text = "Total: $0.00";

			// Mostrar mensaje de éxito
			await DisplayAlert("Éxito", "Pedido realizado correctamente", "OK");


			await EnviarNotificacionAsync($"Tienes un nuevo pedido de {lblNombrePedido.Text}", cuerpoNotificacion);

			// Generar y mostrar el PDF
			await GenerarYPresentarPDFAsync(nombrePedido, detallesPedido, totalPedido, direccion);
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Error al realizar el pedido: {ex.Message}", "Aceptar");
		}
	}

	private async Task GenerarYPresentarPDFAsync(string nombrePedido, List<string> detallesPedido, decimal totalPedido, string direccion)
	{
		// Crear un nuevo documento PDF
		var pdfDocument = new PdfDocument();
		var page = pdfDocument.Pages.Add();
		var graphics = page.Graphics;
		var font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);

		// Obtener el ancho de la página y el ancho del texto "Yummy Delivery"
		float pageWidth = page.GetClientSize().Width;
		float textWidth = font.MeasureString("Yummy Delivery").Width;

		// Calcular la posición x para centrar el texto "Yummy Delivery"
		float xCenteredText = (pageWidth - textWidth) / 2;

		// Agregar el texto "Yummy Delivery" centrado
		graphics.DrawString($"Yummy Delivery", font, PdfBrushes.Black, new Syncfusion.Drawing.PointF(xCenteredText, 50));

		// Continuar con los detalles del pedido
		// Asegúrate de ajustar las coordenadas y los valores según sea necesario
		graphics.DrawString($"Nombre de Pedido: {nombrePedido}", font, PdfBrushes.Black, new Syncfusion.Drawing.PointF(50, 70));
		graphics.DrawString($"Dirección de entrega: {direccion}", font, PdfBrushes.Black, new Syncfusion.Drawing.PointF(50, 90));
		graphics.DrawString("Detalles del pedido:", font, PdfBrushes.Black, new Syncfusion.Drawing.PointF(50, 110));

		int y = 130;
		foreach (var detalle in detallesPedido)
		{
			graphics.DrawString(detalle, font, PdfBrushes.Black, new Syncfusion.Drawing.PointF(70, y));
			y += 20;
		}

		graphics.DrawString($"Total: {totalPedido:C}", font, PdfBrushes.Black, new Syncfusion.Drawing.PointF(50, y + 20));
		graphics.DrawString($"Fecha y hora: {DateTime.Now}", font, PdfBrushes.Black, new Syncfusion.Drawing.PointF(50, y + 40));
		graphics.DrawString($"Gracias por su compra!", font, PdfBrushes.Black, new Syncfusion.Drawing.PointF(50, y + 60));

		// Guardar el PDF en el sistema de archivos
		var pdfFilePath = Path.Combine(FileSystem.CacheDirectory, $"pedido.pdf");
		using (var stream = new FileStream(pdfFilePath, FileMode.Create))
		{
			pdfDocument.Save(stream);
		}

		// Abrir el PDF generado
		await Launcher.OpenAsync(new OpenFileRequest
		{
			File = new ReadOnlyFile(pdfFilePath)
		});
	}

	private async Task EnviarNotificacionAsync(string titulo, string mensaje)
	{
		string _deviceToken = await ObtenerTokenDispositivo();
		var androidNotificationObject = new Dictionary<string, string>
			{
				{ "NavigationID", "2" },
			    { "DetallesPedido", mensaje }  // Agregar los detalles del pedido a los datos
			};

		var message = new Message
		{
			Token = _deviceToken,
			Notification = new Notification
			{
				Title = titulo,
				Body = mensaje,
			},
			Data = androidNotificationObject,
		};

		try
		{
			var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
			Console.WriteLine("Successfully sent message: " + response);
		}
		catch (FirebaseMessagingException ex)
		{
			Console.WriteLine("Error sending message: " + ex);
		}
	}

	private async Task<string> ObtenerTokenDispositivo()
	{
		var stream = await FileSystem.OpenAppPackageFileAsync("token.txt");
		using var reader = new StreamReader(stream);
		return await reader.ReadToEndAsync();
	}

	private async void entryCantidad_TextChanged(object sender, TextChangedEventArgs e)
	{
		// Obtener la entrada de texto que desencadenó el evento
		var entry = sender as Entry;

		// Obtener el platillo asociado a la entrada de texto
		var platillo = entry?.BindingContext as Platillo;

		// Verificar si el platillo y el valor ingresado son válidos
		if (platillo != null && int.TryParse(entry.Text, out int cantidad))
		{

			platillo.Cantidad = cantidad; // Actualizar la cantidad del platillo
										  // Calcular el subtotal para este platillo (cantidad * precio)
			decimal subtotal = cantidad * platillo.Precio;

			// Actualizar el subtotal del platillo en la interfaz
			platillo.Subtotal = subtotal;

			// Actualizar el total sumando los subtotales de todos los platillos
			decimal total = listViewPlatillos.ItemsSource.Cast<Platillo>().Sum(p => p.Subtotal);

			// Mostrar el total en la interfaz
			lblTotal.Text = $"Total: {total:C}";
		}
		else
		{
			//// Si el valor ingresado no es válido, mostrar un mensaje de error
			//await DisplayAlert("Error", "Ingrese una cantidad valida", "Aceptar");
		}
	}

	private void btnDecrementarCantidad_Clicked(object sender, EventArgs e)
	{
		// Obtener el botón que desencadenó el evento
		var button = sender as Button;

		// Obtener el platillo asociado al botón
		var platillo = button?.BindingContext as Platillo;

		// Verificar si el platillo es válido y la cantidad es mayor a 1
		if (platillo != null && platillo.Cantidad > 1)
		{
			platillo.Cantidad--; // Decrementar la cantidad
			UpdateEntryText(button, platillo.Cantidad);
		}
	}

	private void btnIncrementarCantidad_Clicked(object sender, EventArgs e)
	{
		// Obtener el botón que desencadenó el evento
		var button = sender as Button;

		// Obtener el platillo asociado al botón
		var platillo = button?.BindingContext as Platillo;

		// Verificar si el platillo es válido
		if (platillo != null)
		{
			platillo.Cantidad++; // Incrementar la cantidad
			UpdateEntryText(button, platillo.Cantidad);
		}
	}

	private void UpdateEntryText(Button button, int cantidad)
	{
		var parent = button.Parent;
		var entry = parent.FindByName<Entry>("entryCantidad");
		if (entry != null)
		{
			entry.Text = cantidad.ToString();
		}
	}

	private async void btnEliminarCarrito_Clicked(object sender, EventArgs e)
	{
		try
		{
			// Obtener el platillo asociado al botón
			var button = sender as Button;
			var platillo = button?.BindingContext as Platillo;

			// Verificar si el platillo es válido
			if (platillo != null)
			{
				// Eliminar el platillo del carrito
				platillo.AgregadoAlCarrito = false;

				// Actualizar la base de datos MongoDB
				await _mongoDBManager.ActualizarPlatillo(platillo);

				// Volver a cargar los platillos en el carrito
				CargarPlatillos();

				// Mostrar mensaje de éxito
				await DisplayAlert("Éxito", "Platillo eliminado del carrito correctamente", "OK");
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Error al eliminar platillo del carrito: {ex.Message}", "Aceptar");
		}
	}

	private async void btnMapa_Clicked(object sender, EventArgs e)
	{
		var mapaPage = new mapaCliente(this);
		await Navigation.PushAsync(mapaPage);
	}
}