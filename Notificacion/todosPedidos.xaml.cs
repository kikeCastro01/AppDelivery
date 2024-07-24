using static Notificacion.MongoDBManager;

namespace Notificacion;

public partial class todosPedidos : ContentPage
{
	private MongoDBManager _mongoDBManager;

	public List<Pedido> Pedidos { get; set; }
	public todosPedidos()
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
		CargarPedidos();
	}

	private async void CargarPedidos()
	{
		try
		{
			var pedidos = await _mongoDBManager.ObtenerPedidos();

			pedidosDataGrid.ItemsSource = pedidos;
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Error al cargar pedidos finalizados: {ex.Message}", "Aceptar");
		}
	}
}