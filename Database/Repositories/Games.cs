namespace Database.Repositories
{
	using Dapper;

	using global::Database.Models;

	using Npgsql;

	public static partial class Games
	{
		public static async Task<Game> Insert(Guid authorId, string name, byte[]? icon = null, string? description = null)
		{
			try
			{
				return await Database.QueryAsync(async connection =>
					await connection.QuerySingleAsync<Game>(@"
                        INSERT INTO games (author_id, name, icon, description)
                        VALUES (@authorId, @name, @icon, @description)
                        RETURNING *;", new { authorId, name, icon, description }));
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.CheckViolation)
			{ throw new DatabaseException("Не удалось добавить игру: некорректные данные.", e); }
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
			{ throw new DatabaseException("Не удалось добавить игру: такая игра уже существует.", e); }
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.ForeignKeyViolation)
			{ throw new DatabaseException("Не удалось добавить игру: автор не существует.", e); }
		}

		public static async Task<Game?> Select(Guid id) =>
			await Database.QueryAsync(async connection =>
				await connection.QuerySingleOrDefaultAsync<Game>(
					"SELECT * FROM games WHERE id = @id;", new { id }));

		public static async Task<IEnumerable<Game>> Select(
			int offset, int count, Guid? authorId = null, string? name = null,
			bool? approved = null, Guid? categoryId = null)
		{
			offset = Math.Max(0, offset);
			count = Math.Max(0, count);

			if (count == 0)
				return [];

			return await Database.QueryAsync(async connection =>
				await connection.QueryAsync<Game>(@"
                    SELECT games.*
                    FROM games
                    WHERE (@authorId IS NULL OR games.author_id = @authorId)
                    AND (@name IS NULL OR games.name ILIKE @pattern)
                    AND (@approved IS NULL OR games.approved = @approved)
                    AND (@categoryId IS NULL OR EXISTS (
                    SELECT 1 FROM games_categories
                    WHERE games_categories.game_id = games.id AND games_categories.category_id = @categoryId))
                    LIMIT @count OFFSET @offset;",
					new { authorId, name, pattern = $"%{name}%", approved, categoryId, count, offset }));
		}

		public static async Task<Game> Update(
			Guid id, string? name = null, byte[]? icon = null,
			string? description = null, bool? approved = null)
		{
			List<string> clauses = [];

			if (name is not null)
				clauses.Add("name = @name");

			if (icon is not null)
				clauses.Add("icon = @icon");

			if (description is not null)
				clauses.Add("description = @description");

			if (approved is not null)
				clauses.Add("approved = @approved");

			if (clauses.Count <= 0)
				throw new DatabaseException("Не удалось обновить игру: нет данных для обновления.");

			try
			{
				Game? game = await Database.QueryAsync(async connection =>
					await connection.QuerySingleOrDefaultAsync<Game>($@"
                        UPDATE games SET {string.Join(", ", clauses)}
                        WHERE id = @id RETURNING *;",
						new { id, name, icon, description, approved }));

				return game ?? throw new DatabaseException("Не удалось обновить игру: такой игры не существует.");
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.CheckViolation)
			{ throw new DatabaseException("Не удалось обновить данные игры: некорректные данные.", e); }
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
			{ throw new DatabaseException("Не удалось обновить данные игры: такая игра уже существует.", e); }
		}

		public static async Task Delete(Guid id) =>
			await Database.QueryAsync(async connection =>
				await connection.ExecuteAsync("DELETE FROM games WHERE id = @id;", new { id }));
	}
}
