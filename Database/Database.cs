namespace Database
{
	using Dapper;

	using Npgsql;

	public static partial class Database
	{
		private static string _connectionString;

		static Database()
		{
			DefaultTypeMap.MatchNamesWithUnderscores = true;
			_connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") ?? string.Empty;
		}

		internal static async Task QueryAsync(Func<NpgsqlConnection, Task> body)
		{
			try
			{
				await using NpgsqlConnection connection = new(_connectionString);
				await connection.OpenAsync();

				await body(connection);
			}
			catch (PostgresException e) { throw new DatabaseException("Не удалось выполнить запрос к базе данных.", e); }
			catch (NpgsqlException e) { throw new DatabaseException("Не удалось подключиться к базе данных.", e); }
		}

		internal static async Task<T> QueryAsync<T>(Func<NpgsqlConnection, Task<T>> body)
		{
			try
			{
				await using NpgsqlConnection connection = new(_connectionString);
				await connection.OpenAsync();

				return await body(connection);
			}
			catch (PostgresException e) { throw new DatabaseException("Не удалось выполнить запрос к базе данных.", e); }
			catch (NpgsqlException e) { throw new DatabaseException("Не удалось подключиться к базе данных.", e); }
		}

		internal static async Task<T> TransactAsync<T>(Func<NpgsqlConnection, Task<T>> body)
		{
			try
			{
				await using NpgsqlConnection connection = new(_connectionString);
				await connection.OpenAsync();

				await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

				try
				{
					T result = await body(connection);
					await transaction.CommitAsync();
					return result;
				}
				catch
				{
					await transaction.RollbackAsync();
					throw;
				}
			}
			catch (PostgresException e) { throw new DatabaseException("Не удалось выполнить запрос к базе данных.", e); }
			catch (NpgsqlException e) { throw new DatabaseException("Не удалось подключиться к базе данных.", e); }
		}

		internal static async Task TransactAsync(Func<NpgsqlConnection, Task> body)
		{
			try
			{
				await using NpgsqlConnection connection = new(_connectionString);
				await connection.OpenAsync();

				await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

				try
				{
					await body(connection);
					await transaction.CommitAsync();
				}
				catch
				{
					await transaction.RollbackAsync();
					throw;
				}
			}
			catch (PostgresException e) { throw new DatabaseException("Не удалось выполнить запрос к базе данных.", e); }
			catch (NpgsqlException e) { throw new DatabaseException("Не удалось подключиться к базе данных.", e); }
		}

		public static async Task MigrateAsync()
		{
			await QueryAsync(async connection =>
			{
				foreach (string migration in _migrations)
					await connection.ExecuteAsync(migration);
			});
		}
	}
}
