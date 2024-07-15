using Microsoft.Data.Sqlite;

namespace SwiftAPI.Data
{
    public class DatabaseInitializer
    {
        private readonly DatabaseConnectionFactory _connectionFactory;

        public DatabaseInitializer(DatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task InitializeAsync()
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            // Execute schema script
            await ExecuteSqlScriptAsync(connection, "SqlScripts/InitialSchema.sql");

            // Optionally, execute seed data script
            // await ExecuteSqlScriptAsync(connection, "Scripts/SeedData.sql");
        }

        private static async Task ExecuteSqlScriptAsync(SqliteConnection connection, string scriptPath)
        {
            var script = await File.ReadAllTextAsync(scriptPath);
            await using var command = connection.CreateCommand();
            command.CommandText = script;
            await command.ExecuteNonQueryAsync();
        }
    }
}
