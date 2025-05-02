using System.Data;
using Npgsql;

namespace Currencey.Api.Database;

public interface IDbConnectionFactory
{
    Task<IDbConnection>CreateConnectionAsync(CancellationToken cancellationToken = default);
}

public class NpgsDbConnectionFactory : IDbConnectionFactory
{
   readonly string _connectionString;

   public NpgsDbConnectionFactory(string connectionString)
   {
       _connectionString = connectionString;
   }

   public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(_connectionString);
         await connection.OpenAsync(cancellationToken);
         return connection; 
    }
}
