namespace Database.Repositories
{
	using Dapper;

	using global::Database.Models;

	using Npgsql;

	public static class GameComments
	{
		public static async Task<Game.Comment> Insert(Guid gameId, Guid userId, string text)
		{
			try
			{
				return await Database.QueryAsync(async connection =>
					await connection.QuerySingleAsync<Game.Comment>(@"
                        INSERT INTO game_comments (game_id, user_id, text)
                        VALUES (@gameId, @userId, @text)
                        RETURNING *;", new { gameId, userId, text }));
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.CheckViolation)
			{ throw new DatabaseException("Не удалось добавить комментарий: некорректный текст.", e); }
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.ForeignKeyViolation)
			{ throw new DatabaseException("Не удалось добавить комментарий: такая игра или пользователь не существует.", e); }
		}

		public static async Task<IEnumerable<Game.Comment>> Select(Guid gameId, int offset, int count)
		{
			offset = Math.Max(0, offset);
			count = Math.Max(0, count);

			if (count == 0)
				return [];

			return await Database.QueryAsync(async connection =>
				await connection.QueryAsync<Game.Comment>(@"
                    SELECT * FROM game_comments
                    WHERE game_id = @gameId
                    LIMIT @count
					OFFSET @offset;", new { gameId, count, offset }));
		}

		public static async Task<Game.Comment> Update(Guid id, string text)
		{
			try
			{
				Game.Comment? comment = await Database.QueryAsync(async connection =>
					await connection.QuerySingleOrDefaultAsync<Game.Comment>(@"
                        UPDATE game_comments SET text = @text
                        WHERE id = @id RETURNING *;", new { id, text }));

				return comment ?? throw new DatabaseException("Не удалось обновить комментарий: такой комментарий не найден.");
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.CheckViolation)
			{ throw new DatabaseException("Не удалось обновить комментарий: некорректный текст.", e); }
		}

		public static async Task Delete(Guid id) =>
			await Database.QueryAsync(async connection =>
				await connection.ExecuteAsync("DELETE FROM game_comments WHERE id = @id;", new { id }));
	}
}
