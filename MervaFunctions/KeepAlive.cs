using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MervaKeepAliveFunction
{
    public class SqlKeepAlive
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public SqlKeepAlive(
            ILoggerFactory loggerFactory,
            IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<SqlKeepAlive>();
            _configuration = configuration;
        }

        [Function("SqlKeepAlive")]
        public async Task Run(
            [TimerTrigger("0 */30 * * * *")] TimerInfo timer)
            //Seconds to test locally
            //[TimerTrigger("*/10 * * * * *")] TimerInfo timer)            
        {
            try
            {
                var connectionString =
                    _configuration.GetConnectionString("MervaDb");

                await using var connection =
                    new SqlConnection(connectionString);

                await connection.OpenAsync();
                var log = $"Database keep alive executed at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";

                var commandText = $"INSERT INTO dbo.Logs ([Log]) VALUES('{log}')";
                var command = new SqlCommand(commandText, connection);

                await command.ExecuteScalarAsync();

                _logger.LogInformation("Database ping successful.");
            }catch(SqlException ex)
            {
                _logger.LogError(ex,"Sql KeepAlive Failed");
            }
        }
    }
}