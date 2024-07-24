using Microsoft.Maui.Maps;
using Microsoft.Maui.Controls.Maps;



namespace Notificacion;

public partial class mapaCliente : ContentPage
{
	public event EventHandler<LocationSelectedEventArgs> LocationSelected;
	private carritoCompras carritoPage;

	public mapaCliente(carritoCompras carrito)
	{
		InitializeComponent();
		carritoPage = carrito;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		// Set initial position to Guasave, Sinaloa, México
		var initialLocation = new Location(25.563418584680342, -108.46468761563301);
		mapa.MoveToRegion(MapSpan.FromCenterAndRadius(initialLocation, Distance.FromKilometers(5)));
	}

	public class LocationSelectedEventArgs : EventArgs
	{
		public string Address { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
	}

	private async void mapa_MapClicked(object sender, Microsoft.Maui.Controls.Maps.MapClickedEventArgs e)
	{
		var latitude = e.Location.Latitude;
		var longitude = e.Location.Longitude;

		var placemarks = await Geocoding.Default.GetPlacemarksAsync(latitude, longitude);

		string address = null;
		if (placemarks != null && placemarks.Any())
		{
			var placemark = placemarks.First();
			address = $"{placemark.Thoroughfare}, {placemark.Locality}, {placemark.AdminArea}, {placemark.CountryName}";
		}

		var result = await DisplayAlert("Ubicación Seleccionada", $"Address: {address}\nLatitude: {latitude}\nLongitude: {longitude}", "OK", "Cancelar");

		if (result)
		{
			// Agregar un pin al mapa
			var pin = new Pin
			{
				Label = "Destino del pedido",
				Address = address,
				Type = PinType.Place,
				Location = new Location(latitude, longitude)
			};

			// Limpiar los pines anteriores si es necesario
			mapa.Pins.Clear();
			mapa.Pins.Add(pin);
			mapa.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(latitude, longitude), Distance.FromKilometers(1)));

			// Invocar el evento de selección de ubicación
			LocationSelected?.Invoke(this, new LocationSelectedEventArgs { Address = address, Latitude = latitude, Longitude = longitude });
			// Actualizar las etiquetas en carritoCompras
			carritoPage.UpdateLocationInfo(address, latitude, longitude);
		}
		else
		{
			await DisplayAlert("Operación Cancelada", "La operación de selección de ubicación fue cancelada.", "OK");
		}
	}
}