using SwiftAPI.Data;
using SwiftAPI.Models;
using Microsoft.Data.Sqlite;
using System.Data;
using SwiftAPI.Interfaces;

namespace SwiftAPI.Repositories
{
    public class SwiftMessageRepository : ISwiftMessageRepository
    {
        private readonly ILogger<SwiftMessageRepository> _logger;
        private readonly DatabaseConnectionFactory _connectionFactory;

        public SwiftMessageRepository(ILogger<SwiftMessageRepository> logger, DatabaseConnectionFactory connectionFactory)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
        }

        public async Task<(SwiftMessage?, MT799?)> AddMT799(SwiftMessage? swiftMessage, MT799? mt799)
        {
            if (swiftMessage == null)
            {
                _logger.LogError("Cannot insert MT799: SwiftMessage is null.");
                throw new ArgumentNullException(nameof(swiftMessage), "SwiftMessage cannot be null.");
            }

            if (mt799 == null)
            {
                _logger.LogError("Cannot insert MT799: MT799 is null.");
                throw new ArgumentNullException(nameof(mt799), "MT799 cannot be null.");
            }

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var sqlSwiftMessage = @"
                INSERT INTO SwiftMessages (BasicHeader, ApplicationHeader, UserHeader, TextBlock, Trailer) 
                VALUES (@BasicHeader, @ApplicationHeader, @UserHeader, @TextBlock, @Trailer);
                SELECT Id, BasicHeader, ApplicationHeader, UserHeader, TextBlock, Trailer, CreatedAt
                FROM SwiftMessages
                WHERE Id = last_insert_rowid();";

            var sqlMT799 = @"
                INSERT INTO MT799 (SwiftMessageId, Field20, Field21, Field79) 
                VALUES (@SwiftMessageId, @Field20, @Field21, @Field79);
                SELECT Id, SwiftMessageId
                FROM MT799
                WHERE Id = last_insert_rowid();";

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Insert into SwiftMessages
                await using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction as SqliteTransaction;
                    command.CommandText = sqlSwiftMessage;
                    command.Parameters.Add(new SqliteParameter("@BasicHeader", DbType.String) { Value = swiftMessage.BasicHeader });
                    command.Parameters.Add(new SqliteParameter("@ApplicationHeader", DbType.String) { Value = swiftMessage.ApplicationHeader ?? (object)DBNull.Value });
                    command.Parameters.Add(new SqliteParameter("@UserHeader", DbType.String) { Value = swiftMessage.UserHeader ?? (object)DBNull.Value });
                    command.Parameters.Add(new SqliteParameter("@TextBlock", DbType.String) { Value = swiftMessage.TextBlock ?? (object)DBNull.Value });
                    command.Parameters.Add(new SqliteParameter("@Trailer", DbType.String) { Value = swiftMessage.Trailer ?? (object)DBNull.Value });
                    
                    await using var reader = await command.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        swiftMessage.Id = reader.GetInt32(reader.GetOrdinal("Id"));
                        swiftMessage.CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"));
                        _logger.LogInformation("Swift message inserted on route mt799 successfully with Id: {Id}", swiftMessage.Id);
                    }
                    else
                    {
                        _logger.LogError("Failed to insert Swift message on route mt799.");
                        throw new Exception("Failed to insert Swift message on route mt799");
                    }
                    reader.Close();
                }

                _logger.LogInformation("CreatedAt: {CreatedAt}", swiftMessage.CreatedAt);

                // Insert the MT799
                await using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction as SqliteTransaction;
                    command.CommandText = sqlMT799;
                    command.Parameters.Add(new SqliteParameter("@SwiftMessageId", DbType.Int32) { Value = swiftMessage.Id });
                    command.Parameters.Add(new SqliteParameter("@Field20", DbType.String) { Value = mt799.Field20 });
                    command.Parameters.Add(new SqliteParameter("@Field21", DbType.String) { Value = mt799.Field21 ?? (object)DBNull.Value });
                    command.Parameters.Add(new SqliteParameter("@Field79", DbType.String) { Value = mt799.Field79 });

                    await using var reader = await command.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        mt799.Id = reader.GetInt32(reader.GetOrdinal("Id"));
                        mt799.SwiftMessageId = reader.GetInt32(reader.GetOrdinal("SwiftMessageId"));
                        _logger.LogInformation("MT799 inserted successfully with Id: {Id}, SwiftMessageId: {SwiftMessageId}", mt799.Id, mt799.SwiftMessageId);
                    }
                    else
                    {
                        _logger.LogError("Failed to insert MT799.");
                        throw new Exception("Failed to insert MT799.");
                    }
                    reader.Close();
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Swift MT799 Transaction committed successfully.");

                _logger.LogInformation("Logging the inserted MT799's ID: {insertedMT799.Id}, SwiftMessageId: {insertedMT799.SwiftMessageId}", mt799.Id, mt799.SwiftMessageId);

                return (swiftMessage, mt799);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert Swift MT799. Rolling back transaction.");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<(SwiftMessage?, MT799?)> GetMT799(int id)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sqlMT799 = "SELECT * FROM MT799 WHERE Id = @Id;";
            const string sqlSwiftMessage = "SELECT * FROM SwiftMessages WHERE Id = @Id;";

            MT799? mt799;
            SwiftMessage? swiftMessage;

            try
            {
                // Query mt799 record
                await using (var command = connection.CreateCommand())
                {
                    command.CommandText = sqlMT799;
                    command.Parameters.Add(new SqliteParameter("@Id", DbType.Int32) { Value = id });
                    await using var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        _logger.LogInformation("MT799 record found with Id: {Id}", id);
                        mt799 = new MT799
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            SwiftMessageId = reader.GetInt32(reader.GetOrdinal("SwiftMessageId")),
                            Field20 = reader.GetString(reader.GetOrdinal("Field20")),
                            Field21 = reader.IsDBNull(reader.GetOrdinal("Field21")) ? null : reader.GetString(reader.GetOrdinal("Field21")),
                            Field79 = reader.GetString(reader.GetOrdinal("Field79"))
                        };
                    } 
                    else
                    {
                        _logger.LogWarning("MT799 record not found with Id: {Id}", id);
                        throw new Exception("MT799 record not found.");
                    }
                }

                await using (var command = connection.CreateCommand())
                {
                    command.CommandText = sqlSwiftMessage;
                    command.Parameters.Add(new SqliteParameter("@Id", DbType.Int32) { Value = mt799.SwiftMessageId });
                    await using var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        _logger.LogInformation("Swift message found with Id: {Id}", mt799.SwiftMessageId);
                        swiftMessage = new SwiftMessage
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            BasicHeader = reader.GetString(reader.GetOrdinal("BasicHeader")),
                            ApplicationHeader = reader.IsDBNull(reader.GetOrdinal("ApplicationHeader")) ? null : reader.GetString(reader.GetOrdinal("ApplicationHeader")),
                            UserHeader = reader.IsDBNull(reader.GetOrdinal("UserHeader")) ? null : reader.GetString(reader.GetOrdinal("UserHeader")),
                            TextBlock = reader.IsDBNull(reader.GetOrdinal("TextBlock")) ? null : reader.GetString(reader.GetOrdinal("TextBlock")),
                            Trailer = reader.IsDBNull(reader.GetOrdinal("Trailer")) ? null : reader.GetString(reader.GetOrdinal("Trailer")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                        };
                    }
                    else
                    {
                        _logger.LogWarning("Swift message not found with Id: {Id}", id);
                        throw new Exception("Swift message not found.");
                    }
                }

                return (swiftMessage, mt799);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get MT799 message.");
                throw;
            }
        }
    }
}
