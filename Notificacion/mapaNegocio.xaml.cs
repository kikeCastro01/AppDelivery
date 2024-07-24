using Newtonsoft.Json.Linq;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Controls.Maps;

namespace Notificacion;

public partial class mapaNegocio : ContentPage
{
	private Location initialLocation1;
	private Location initialLocation2;
	private Pin initialPin1;
	private Pin initialPin2;
	private Polyline routePolyline;
	private readonly string googleMapsApiKey = "AIzaSyAAACJl1UKnjT0OkGxbSAAq193QC2ZSG9Q";
	public mapaNegocio(double latitude, double longitude, string address)
	{
		InitializeComponent();
		// Definir las ubicaciones predefinidas
		initialLocation1 = new Location(25.37966017040639, -108.45886051654816); // Ubicación 1
		initialLocation2 = new Location(latitude, longitude);

		// Mover el mapa al centro de la primera ubicación
		mapa.MoveToRegion(MapSpan.FromCenterAndRadius(initialLocation1, Distance.FromKilometers(5)));
		mapa.MoveToRegion(MapSpan.FromCenterAndRadius(initialLocation2, Distance.FromKilometers(5)));

		// Crear el pin para la primera ubicación
		initialPin1 = new Pin
		{
			Label = "Yummy Delivery",
			Address = "Enrique Castro, Palos Verdes, Sinaloa, México",
			Type = PinType.Place,
			Location = initialLocation1,
			MarkerId = "initialLocationPin1"
		};
		mapa.Pins.Add(initialPin1);

		// Crear el pin para la segunda ubicación utilizando la dirección proporcionada
		initialPin2 = new Pin
		{
			Label = "Destino del pedido",
			Address = address,
			Type = PinType.Place,
			Location = initialLocation2,
			MarkerId = "secondLocationPin"
		};
		mapa.Pins.Add(initialPin2);
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		// Dibujar la ruta entre las dos ubicaciones predefinidas
		await DrawRoute(initialLocation1, initialLocation2);
	}

	private async Task DrawRoute(Location origin, Location destination)
	{
		var requestUrl = $"https://maps.googleapis.com/maps/api/directions/json?origin={origin.Latitude},{origin.Longitude}&destination={destination.Latitude},{destination.Longitude}&key={googleMapsApiKey}";

		using (var client = new HttpClient())
		{
			var response = await client.GetAsync(requestUrl);
			if (response.IsSuccessStatusCode)
			{
				var json = await response.Content.ReadAsStringAsync();
				var route = JObject.Parse(json);

				var routes = route["routes"];
				if (routes != null && routes.HasValues)
				{
					var overviewPolyline = routes[0]?["overview_polyline"];
					if (overviewPolyline != null)
					{
						var points = overviewPolyline["points"]?.ToString();
						if (!string.IsNullOrEmpty(points))
						{
							var decodedPoints = DecodePolyline(points);

							// Clear previous route
							if (routePolyline != null)
							{
								mapa.MapElements.Remove(routePolyline);
							}

							// Add new route to the map
							routePolyline = new Polyline
							{
								StrokeColor = Colors.Blue,
								StrokeWidth = 3
							};

							foreach (var point in decodedPoints)
							{
								routePolyline.Geopath.Add(new Location(point.Latitude, point.Longitude));
							}

							mapa.MapElements.Add(routePolyline);
							return;
						}
					}
				}
			}
			await DisplayAlert("Error", "No se pudo obtener la ruta.", "OK");
		}
	}

	private List<Location> DecodePolyline(string encodedPoints)
	{
		if (string.IsNullOrWhiteSpace(encodedPoints))
			return null;

		var poly = new List<Location>();
		int index = 0;
		int lat = 0;
		int lng = 0;

		while (index < encodedPoints.Length)
		{
			int b;
			int shift = 0;
			int result = 0;

			do
			{
				b = encodedPoints[index++] - 63;
				result |= (b & 0x1f) << shift;
				shift += 5;
			} while (b >= 0x20);

			int dlat = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
			lat += dlat;

			shift = 0;
			result = 0;

			do
			{
				b = encodedPoints[index++] - 63;
				result |= (b & 0x1f) << shift;
				shift += 5;
			} while (b >= 0x20);

			int dlng = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
			lng += dlng;

			poly.Add(new Location(Convert.ToDouble(lat) / 1E5, Convert.ToDouble(lng) / 1E5));
		}

		return poly;
	}

	public class LocationSelectedEventArgs : EventArgs
	{
		public string Address { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
	}
}