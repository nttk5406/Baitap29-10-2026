using Microsoft.Data.SqlClient;

namespace HotelManagement.DAL
{
    public static class DatabaseHelper
    {
        public static string ConnectionString =
            @"Data Source=LAPTOP-MKT1NFM3\SQLEXPRESS;Initial Catalog=HotelManagementDB;Integrated Security=True;TrustServerCertificate=True";

        public static SqlConnection GetConnection() => new SqlConnection(ConnectionString);
    }
}
