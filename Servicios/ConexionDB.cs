using MySql.Data.MySqlClient;

namespace SistemaResiduos.Servicios
{
    public class ConexionDB
    {
        private readonly string connectionString;

        public ConexionDB(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("MySqlConnection");
        }

        public MySqlConnection ObtenerConexion()
        {
            return new MySqlConnection(connectionString);
        }
    }
}