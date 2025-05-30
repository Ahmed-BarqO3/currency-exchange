using Dapper;

namespace Currencey.Api.Database;

public class DBInitializer
{
    readonly IDbConnectionFactory _dbConnection;

    public DBInitializer(IDbConnectionFactory dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnection.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync("""
        
            CREATE TABLE IF NOT Exists currency (
                                                id VARCHAR(3) PRIMARY KEY,
                                                name VARCHAR(50) NOT NULL,
                                                symbol VARCHAR(5) NOT NULL,
                                                amount NUMERIC(4, 2) NOT NULL,
                                                update_at TIMESTAMP WITH TIME ZONE NOT NULL
        );
        
        
        DO $$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM currency) THEN
                INSERT INTO currency (id, name, symbol, amount, update_at)
                VALUES
                    ('USD', 'الدولار الأمريكي', '$', 7.11, NOW() + INTERVAL '2 hours'),
                    ('EUR', 'يورو', '€', 7.92, NOW() + INTERVAL '2 hours');
            END IF;
        END;
        $$;
        """);

        await connection.ExecuteAsync("""
                                      Create Table IF NOT EXISTS currency_history (
                                          id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                                          currency_id VARCHAR(3) NOT NULL,
                                          amount NUMERIC(4, 2) NOT NULL,
                                          create_at TIMESTAMP With Time Zone NOT NULL,
                                          FOREIGN KEY (currency_id) REFERENCES currency(id)
                                      );
                                      """);

        await connection.ExecuteAsync("""
                                      Create Table IF Not EXISTS users (
                                          id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                                          username VARCHAR(25) NOT NULL,
                                          password VARCHAR(256) NOT NULL
                                      );
                                      """);


        await connection.ExecuteAsync("""
                                      Create Table IF NOT EXISTS refresh_tokens (
                                          token text PRIMARY KEY,
                                          jwtid text NOT NULL,
                                          creation TIMESTAMP WITH TIME ZONE  NOT NULL,
                                          expiration TIMESTAMP WITH TIME ZONE NOT NULL,
                                          used BOOLEAN NOT NULL,
                                          invalidate BOOLEAN NOT NULL,
                                          userid uuid NOT NULL,
                                          FOREIGN KEY (userid) REFERENCES users(id)
                                      );
                                      """);

        await connection.ExecuteAsync("""
                                        -- Create a trigger function
                                      CREATE OR REPLACE FUNCTION insert_ON_UpdateCurrency()
                                          RETURNS TRIGGER AS $$
                                      BEGIN
                                          -- Add your logic here. For example, log changes.
                                          INSERT INTO currency_history (currency_id, amount, create_at)
                                          VALUES (NEW.id, NEW.amount, NOW() + INTERVAL '2 hour');
                                          RETURN NEW;
                                      END;
                                      $$ LANGUAGE plpgsql;
                                      
                                      CREATE OR REPLACE  TRIGGER  TRG_insert_ON_UpdateCurrency
                                          AFTER UPDATE  ON currency
                                          FOR EACH ROW
                                      EXECUTE FUNCTION insert_ON_UpdateCurrency();
                                      
                                        Create UNIQUE INDEX IF NOT EXISTS username_index on users (username);
                                      """);
    }
}
