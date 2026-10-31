namespace Flat_Server.Database
{
	using Dapper;

	using Models;

	using Npgsql;

	public static class Sessions
	{
		public static async Task<Session> InsertAsync(int userId, string? info = null)
		{
			try
			{
				return await Database.QueryAsync(connection => connection.QueryFirstAsync(@"
					INSERT INTO sessions (user_id, info) VALUES (@user_id, @info)
					RETURNING *;
				", new { user_id = userId, info }));
			}
			catch (PostgresException)
			{
				throw new DatabaseException("Не удалось создать сессию.");
			}
		}

		public static async Task<Session?> SelectAsync(Guid refreshToken)
		{
			return await Database.QueryAsync(connection => connection.QueryFirstOrDefaultAsync(@"
				SELECT * FROM sessions WHERE refresh_token = @refresh_token;
				", new { refresh_token = refreshToken }));
		}

		public static async Task<Session?> SelectAsync(int userId)
		{
			return await Database.QueryAsync(connection => connection.QueryFirstOrDefaultAsync(@"
				SELECT * FROM sessions WHERE user_id = @user_id;
				", new { user_id = userId }));
		}

		public static async Task DeleteAsync(Guid refreshToken)
		{
			int rowsAffected = await Database.QueryAsync(connection => connection.ExecuteAsync(@"
				DELETE FROM sessions WHERE refresh_token = @refresh_token;
				", new { refresh_token = refreshToken }));

			if (rowsAffected <= 0)
				throw new DatabaseException("Категория с таким ID не найдена.");
		}
	}
}
