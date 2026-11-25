namespace Database.Repositories
{
	using Dapper;

	using global::Database.Models;

	using Npgsql;

	public static partial class Games
	{
		public static class Categories
		{
			public static async Task<Game.Category> Insert(Guid gameId, Guid categoryId)
			{
				try
				{
					return await Database.QueryAsync(async connection =>
						await connection.QuerySingleAsync<Game.Category>(@"
                        INSERT INTO games_categories (game_id, category_id)
                        VALUES (@gameId, @categoryId)
                        RETURNING *;", new { gameId, categoryId }));
				}
				catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
				{ throw new DatabaseException("Не удалось добавить категорию игры: эта категория уже назначена игре.", e); }
				catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.ForeignKeyViolation)
				{ throw new DatabaseException("Не удалось добавить категорию игры: такая игра или категория не существует.", e); }
			}

			public static async Task<IEnumerable<Game.Category>> SelectByGame(Guid gameId) =>
				await Database.QueryAsync(async connection =>
					await connection.QueryAsync<Game.Category>(
						"SELECT * FROM games_categories WHERE game_id = @gameId;", new { gameId }));

			public static async Task<IEnumerable<Game.Category>> SelectByCategory(Guid categoryId) =>
				await Database.QueryAsync(async connection =>
					await connection.QueryAsync<Game.Category>(
						"SELECT * FROM games_categories WHERE category_id = @categoryId;", new { categoryId }));

			public static async Task Delete(Guid id) =>
				await Database.QueryAsync(async connection =>
					await connection.ExecuteAsync("DELETE FROM games_categories WHERE id = @id;", new { id }));

			public static async Task Delete(Guid gameId, Guid categoryId) =>
				await Database.QueryAsync(async connection =>
					await connection.ExecuteAsync(@"
                    DELETE FROM games_categories
                    WHERE game_id = @gameId AND category_id = @categoryId;",
						new { gameId, categoryId }));
		}
	}
}
