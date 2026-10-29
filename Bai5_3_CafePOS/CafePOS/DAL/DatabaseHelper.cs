using Microsoft.Data.SqlClient;

namespace CafePOS.DAL
{
    public static class DatabaseHelper
    {
        public static string ConnectionString =
            @"Data Source=LAPTOP-MKT1NFM3\SQLEXPRESS;Initial Catalog=CafePOSDB;Integrated Security=True;TrustServerCertificate=True";
        public static SqlConnection GetConnection() => new SqlConnection(ConnectionString);
    }
}
