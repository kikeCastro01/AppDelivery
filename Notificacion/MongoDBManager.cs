using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notificacion
{
	public class MongoDBManager
	{
		// Cadena de conexión estática
		public static string ConnectionString { get; } = "mongodb://192.168.1.30:27017";
		private readonly IMongoCollection<Usuario> _usuariosCollection;
		private readonly IMongoCollection<Platillo> _platillosCollection;
		private readonly IMongoCollection<Pedido> _pedidosCollection;

		public MongoDBManager(string connectionString, string databaseName, string usuariosCollectionName, string collectionName, string pedidosCollectionName)
		{
			var client = new MongoClient(connectionString);
			var database = client.GetDatabase(databaseName);
			_usuariosCollection = database.GetCollection<Usuario>(usuariosCollectionName);
			_platillosCollection = database.GetCollection<Platillo>(collectionName);
			_pedidosCollection = database.GetCollection<Pedido>(pedidosCollectionName);
		}

		public class Usuario
		{
			[BsonId]
			public ObjectId _id { get; set; }
			public string Nombre { get; set; }
			public string Telefono { get; set; }
			public string Email { get; set; }
			public byte[] Foto { get; set; }
			public string Genero { get; set; }
			public DateTime FechaNacimiento { get; set; }
			public string Contraseña { get; set; }
			// Otros campos si son necesarios
		}
		public class Platillo
		{
			[BsonId]
			public ObjectId _id { get; set; }
			public byte[] Imagen { get; set; }
			public string Nombre { get; set; }
			public string Descripcion { get; set; }
			public decimal Precio { get; set; }
			public ImageSource ImagenSource { get; set; }
			public bool AgregadoAlCarrito { get; set; }

			public int Cantidad { get; set; } = 1; // Inicialmente establecido en 1
			public decimal Subtotal { get; set; }
		}

		public class Pedido
		{
			[BsonId]
			public ObjectId _id { get; set; }
			public string NombrePedido { get; set; }
			public List<string> Detalles { get; set; }
			public decimal Total { get; set; }
			public double Latitud {  get; set; }
			public double Longitud { get; set; }
			public string Direccion { get; set; }
			public DateTime FechaHora { get; set; }
			public string Estatus { get; set; }

		}

		public async Task InsertarUsuario(Usuario usuario)
		{
			try
			{
				await _usuariosCollection.InsertOneAsync(usuario);
			}
			catch (Exception ex)
			{
				// Manejar la excepción según tus necesidades
				throw new Exception($"Error al insertar el usuario: {ex.Message}");
			}
		}

		// Método en MongoDBManager para verificar credenciales
		public async Task<bool> VerificarCredenciales(string nombreUsuario, string contraseña)
		{
			// Buscar el usuario en la base de datos
			var usuario = await _usuariosCollection.Find(u => u.Nombre == nombreUsuario && u.Contraseña == contraseña).FirstOrDefaultAsync();

			return usuario != null; // Si el usuario es encontrado, las credenciales son válidas
		}
		public async Task<List<Platillo>> ObtenerPlatillos()
		{
			try
			{
				var platillos = await _platillosCollection.Find(new BsonDocument()).ToListAsync();
				return platillos;
			}
			catch (Exception ex)
			{
				// Manejar la excepción según tus necesidades
				throw new Exception($"Error al obtener los platillos: {ex.Message}");
			}
		}

		public async Task InsertarPlatillo(Platillo platillo)
		{
			try
			{
				await _platillosCollection.InsertOneAsync(platillo);
			}
			catch (Exception ex)
			{
				// Manejar la excepción según tus necesidades
				throw new Exception($"Error al insertar el platillo: {ex.Message}");
			}
		}

		public async Task ActualizarPlatillo(Platillo platillo)
		{
			try
			{
				var filter = Builders<Platillo>.Filter.Eq("_id", platillo._id);
				var update = Builders<Platillo>.Update.Set("AgregadoAlCarrito", platillo.AgregadoAlCarrito)
													  .Set("Cantidad", platillo.Cantidad) // Agrega otras propiedades si es necesario
													  .Set("Subtotal", platillo.Subtotal); // Asegúrate de actualizar todas las propiedades relevantes

				await _platillosCollection.UpdateOneAsync(filter, update);
			}
			catch (Exception ex)
			{
				// Manejar la excepción según tus necesidades
				throw new Exception($"Error al actualizar el platillo: {ex.Message}");
			}
		}

		public async Task<Platillo> ObtenerPlatilloPorId(ObjectId id)
		{
			try
			{
				var filter = Builders<Platillo>.Filter.Eq("_id", id);
				return await _platillosCollection.Find(filter).FirstOrDefaultAsync();
			}
			catch (Exception ex)
			{
				// Manejar la excepción según tus necesidades
				throw new Exception($"Error al obtener el platillo por ID: {ex.Message}");
			}
		}

		public async Task EliminarPlatillo(Platillo platillo)
		{
			try
			{
				var filter = Builders<Platillo>.Filter.Eq("_id", platillo._id);
				await _platillosCollection.DeleteOneAsync(filter);
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al eliminar el platillo: {ex.Message}");
			}
		}

		public async Task ModificarPlatillo(Platillo platillo)
		{
			var filter = Builders<Platillo>.Filter.Eq(p => p._id, platillo._id);
			var update = Builders<Platillo>.Update
				.Set(p => p.Nombre, platillo.Nombre)
				.Set(p => p.Descripcion, platillo.Descripcion)
				.Set(p => p.Precio, platillo.Precio)
				.Set(p => p.Imagen, platillo.Imagen);
			await _platillosCollection.ReplaceOneAsync(filter, platillo);
		}

		public async Task GuardarPedidoEnColeccion(string nombrePedido, List<string> detallesPedido, decimal total, double latitud, double longitud, string direccion)
		{
			try
			{
				var pedido = new Pedido
				{
					NombrePedido = nombrePedido,
					Detalles = detallesPedido,
					Total = total,
					Latitud = latitud,
					Longitud = longitud,
					Direccion = direccion,
					FechaHora = DateTime.Now, // Asignar la fecha y hora actual
					Estatus = "Pendiente"
				};

				// Convertir el total a formato de moneda antes de guardar en la base de datos
				pedido.Total = decimal.Round(total, 2); // Redondear el total a dos decimales
				await _pedidosCollection.InsertOneAsync(pedido);
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al guardar el pedido: {ex.Message}");
			}
		}

		public async Task ActualizarPlatillosEnCarrito(List<Platillo> platillosEnCarrito)
		{
			try
			{

				foreach (var platillo in platillosEnCarrito)
				{
					var filter = Builders<Platillo>.Filter.Eq("_id", platillo._id);
					var update = Builders<Platillo>.Update.Set(p => p.AgregadoAlCarrito, false);
					await _platillosCollection.UpdateOneAsync(filter, update);
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al actualizar los platillos en el carrito: {ex.Message}");
			}
		}

		public async Task<List<Pedido>> ObtenerPedidos()
		{
			try
			{
				var pedidos = await _pedidosCollection.Find(new BsonDocument()).ToListAsync();
				return pedidos;
			}
			catch (Exception ex)
			{
				// Manejar la excepción según tus necesidades
				throw new Exception($"Error al obtener los platillos: {ex.Message}");
			}
		}

		public async Task<List<Pedido>> ObtenerPedidosPendientes()
		{
			try
			{
				var filter = Builders<Pedido>.Filter.Eq(p => p.Estatus, "Pendiente");
				var pedidos = await _pedidosCollection.Find(filter).ToListAsync();
				return pedidos;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los pedidos pendientes: {ex.Message}");
			}
		}

		public async Task<List<Pedido>> ObtenerPedidosFinalizados()
		{
			try
			{
				var filter = Builders<Pedido>.Filter.Eq(p => p.Estatus, "Finalizado");
				var pedidos = await _pedidosCollection.Find(filter).ToListAsync();
				return pedidos;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los pedidos pendientes: {ex.Message}");
			}
		}

		public async Task ActualizarEstatusPedido(ObjectId idPedido, string nuevoEstatus)
		{
			try
			{
				var filter = Builders<Pedido>.Filter.Eq("_id", idPedido);
				var update = Builders<Pedido>.Update.Set("Estatus", nuevoEstatus);
				await _pedidosCollection.UpdateOneAsync(filter, update);
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al actualizar el estatus del pedido: {ex.Message}");
			}
		}

	}
}
