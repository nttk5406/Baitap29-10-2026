
using Microsoft.Data.SqlClient;

namespace InventoryManagement.DAL
{
    public static class DatabaseHelper
    {
        public static string ConnectionString =
            @"Data Source=LAPTOP-MKT1NFM3\SQLEXPRESS;Initial Catalog=InventoryDB;Integrated Security=True;TrustServerCertificate=True";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}
