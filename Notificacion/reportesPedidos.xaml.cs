using static Notificacion.MongoDBManager;


namespace Notificacion;

public partial class reportesPedidos : ContentPage
{
	private MongoDBManager _mongoDBManager;
	public List<Pedido> Pedidos { get; set; }
	public reportesPedidos()
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

	private void OnRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (consultarFechaRadioButton.IsChecked)
		{
			fecha1DatePicker.IsEnabled = true;
			fecha2DatePicker.IsEnabled = true;
		}
		else if (consultarTodoRadioButton.IsChecked)
		{
			fecha1DatePicker.IsEnabled = false;
			fecha2DatePicker.IsEnabled = false;
		}
	}

	private async void CargarPedidos(DateTime? fechaInicio = null, DateTime? fechaFin = null)
	{
		try
		{
			var pedidos = await _mongoDBManager.ObtenerPedidos();

			if (fechaInicio.HasValue && fechaFin.HasValue)
			{
				pedidos = pedidos.FindAll(p => p.FechaHora >= fechaInicio.Value && p.FechaHora <= fechaFin.Value);
			}

			pedidosDataGrid.ItemsSource = pedidos;

			// Calcular y mostrar el total en el Entry
			decimal totalSum = pedidos.Sum(p => p.Total);
			totalEntry.Text = totalSum.ToString("C"); // Formato de moneda
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Error al cargar pedidos: {ex.Message}", "Aceptar");
		}
	}
	private async void btnConsultar_Clicked(object sender, EventArgs e)
	{
		if (consultarFechaRadioButton.IsChecked)
		{
			DateTime fechaInicio = fecha1DatePicker.Date;
			DateTime fechaFin = fecha2DatePicker.Date;
			CargarPedidos(fechaInicio, fechaFin);
		}
		else
		{
			CargarPedidos();
		}
	}
}