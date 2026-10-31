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

		/*
CREATE TABLE IF NOT EXISTS sessions (
	refresh_token UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	user_id INT NOT NULL,
	info VARCHAR(128),
	CONSTRAINT FK_user_id FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE ON UPDATE CASCADE,
);
 */

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

			if (rowsAffected == 0)
				throw new DatabaseException("Категория с таким ID не найдена.");
		}
	}
}
