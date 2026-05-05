using Microsoft.Data.SqlClient;

namespace TravelAgency_Secure.Data
{
    public static class DatabaseInitializer
    {
        private static readonly string connectionString =
            "Server=localhost\\SQLEXPRESS;Database=TravelSecurityDB;Trusted_Connection=True;TrustServerCertificate=True";

        public static void Initialize()
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            AddColumnIfNotExists(connection, "Users", "FirstName", "NVARCHAR(50) NULL");
            AddColumnIfNotExists(connection, "Users", "LastName", "NVARCHAR(50) NULL");
            AddColumnIfNotExists(connection, "Users", "IDNumber", "NVARCHAR(9) NULL");
            AddColumnIfNotExists(connection, "Users", "CreditCardNumber", "NVARCHAR(19) NULL");
            AddColumnIfNotExists(connection, "Users", "ValidDate", "NVARCHAR(5) NULL");
            AddColumnIfNotExists(connection, "Users", "CVC", "NVARCHAR(3) NULL");
        }

        private static void AddColumnIfNotExists(SqlConnection connection, string tableName, string columnName, string columnType)
        {
            string checkColumnQuery = @"
                IF COL_LENGTH(@tableName, @columnName) IS NULL
                BEGIN
                    DECLARE @sql NVARCHAR(MAX);
                    SET @sql = 'ALTER TABLE ' + QUOTENAME(@tableName) + 
                               ' ADD ' + QUOTENAME(@columnName) + ' ' + @columnType;
                    EXEC sp_executesql @sql;
                END";

            using SqlCommand command = new SqlCommand(checkColumnQuery, connection);
            command.Parameters.AddWithValue("@tableName", tableName);
            command.Parameters.AddWithValue("@columnName", columnName);
            command.Parameters.AddWithValue("@columnType", columnType);

            command.ExecuteNonQuery();
        }
    }
}