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
                                                rate NUMERIC(4, 2) NOT NULL,
                                                update_at TIMESTAMP WITH TIME ZONE NOT NULL
        );
        
        
        DO $$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM currency) THEN
                INSERT INTO currency (id, name, symbol, rate, update_at)
                VALUES
                    ('USD', 'United States Dollar', '$', 1.00, NOW() + INTERVAL '2 hours'),
                    ('EUR', 'Euro', '€', 0.85, NOW() + INTERVAL '2 hours'),
                    ('GBP', 'British Pound Sterling', '£', 0.75, NOW() + INTERVAL '2 hours');
            END IF;
        END;
        $$;
        """);

        await connection.ExecuteAsync("""
                                      Create Table IF NOT EXISTS currency_history (
                                          id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                                          currency_id VARCHAR(3) NOT NULL,
                                          rate NUMERIC(4, 2) NOT NULL,
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
                                        -- Create a trigger function
                                      CREATE OR REPLACE FUNCTION insert_ON_UpdateCurrency()
                                          RETURNS TRIGGER AS $$
                                      BEGIN
                                          -- Add your logic here. For example, log changes.
                                          INSERT INTO currency_history (currency_id, rate, create_at)
                                          VALUES (NEW.id, NEW.rate, NOW() + INTERVAL '2 hour');
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
