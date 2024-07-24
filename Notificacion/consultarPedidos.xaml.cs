using static Notificacion.MongoDBManager;

namespace Notificacion;

public partial class consultarPedidos : ContentPage
{
	private MongoDBManager _mongoDBManager;
	public consultarPedidos()
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
		CargarPedidosPendientes();
	}

	private async void CargarPedidosPendientes()
	{
		try
		{
			var pedidosPendientes = await _mongoDBManager.ObtenerPedidosPendientes();
			listViewPedidos.ItemsSource = pedidosPendientes;
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Error al cargar pedidos pendientes: {ex.Message}", "Aceptar");
		}
	}

	private async void btnVerUbicacion_Clicked(object sender, EventArgs e)
	{
		//await Navigation.PushAsync(new mapaNegocio());

		var button = sender as Button;
		var pedido = button.BindingContext as Pedido;

		if (pedido != null)
		{
			await Navigation.PushAsync(new mapaNegocio(pedido.Latitud, pedido.Longitud, pedido.Direccion));
		}
		else
		{
			await DisplayAlert("Error", "No se pudo obtener la ubicación del pedido.", "Aceptar");
		}
	}

	private async void btnFinalizarEntrega_Clicked(object sender, EventArgs e)
	{
		var button = sender as Button;
		var pedido = button.BindingContext as Pedido;

		if (pedido != null)
		{
			bool confirmacion = await DisplayAlert("Confirmación", "¿Está seguro de que desea finalizar la entrega?", "Sí", "No");

			if (confirmacion)
			{
				try
				{
					// Actualizar el estatus del pedido a "Finalizado"
					await _mongoDBManager.ActualizarEstatusPedido(pedido._id, "Finalizado");

					// Refrescar la lista de pedidos
					CargarPedidosPendientes();

					await DisplayAlert("Éxito", "La entrega del pedido ha sido finalizada.", "Aceptar");
				}
				catch (Exception ex)
				{
					await DisplayAlert("Error", $"Error al finalizar la entrega del pedido: {ex.Message}", "Aceptar");
				}
			}
		}
		else
		{
			await DisplayAlert("Error", "No se pudo obtener el pedido para finalizar la entrega.", "Aceptar");
		}
	}
}