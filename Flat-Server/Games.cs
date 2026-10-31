namespace Flat_Server.Database
{
	using Models;

	using Npgsql;

	using Dapper;

	using System.Collections;

	public static class Games
	{
		public static async Task<Game> InsertAsync(
			int userId,
			string name,
			byte[]? icon = null,
			string? description = null,
			int[]? categories = null,
			byte[][]? images = null
		)
		{
			try
			{
				return await Database.TransactAsync(async connection =>
				{
					Game game = await connection.QueryFirstAsync(@"
                        INSERT INTO games (user_id, name, icon, description)
                        VALUES(@user_id, @name, @icon, @description)
                        RETURNING *;
                    ", new { user_id = userId, name, icon, description });

					if (categories is not null)
						foreach (int category_id in categories)
							await connection.ExecuteAsync(@"
                                INSERT INTO games_categories (game_id, category_id)
                                VALUES (@game_id, @category_id);
                            ", new { game_id = game.Id, category_id });

					if (images is not null)
						foreach (byte[] data in images)
							await connection.ExecuteAsync(@"
                                INSERT INTO game_images (game_id, data)
                                VALUES (@game_id, @data);
                            ", new { game_id = game.Id, data });

					return game;
				});
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
			{
				throw new DatabaseException("Игра с таким названием уже существует.");
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.CheckViolation)
			{
				throw new DatabaseException("Некорректное имя или описание.");
			}
			catch (PostgresException)
			{
				throw new DatabaseException("Не удалось создать новую игру.");
			}
		}

		public static async Task UpdateAsync(
			int id,
			int? userId = null,
			string? name = null,
			byte[]? icon = null,
			string? description = null,
			bool? approved = null,
			int[]? categories = null,
			byte[][]? images = null)
		{
			List<string> clauses = new();

			if (userId is not null)
				clauses.Add("user_id = @user_id");

			if (name is not null)
				clauses.Add("name = @name");

			if (icon is not null)
				clauses.Add("icon = @icon");

			if (description is not null)
				clauses.Add("description = @description");

			if (approved is not null)
				clauses.Add("approved = @approved");

			try
			{
				await Database.TransactAsync(async connection =>
				{
					int? existingId = await connection.QueryFirstOrDefaultAsync(
						"SELECT id FROM games WHERE id = @id;", new { id });

					if (existingId is null)
						throw new DatabaseException("Игра с таким ID не найдена.");

					if (clauses.Count > 0)
					{
						await connection.ExecuteAsync(
							$"UPDATE games SET {string.Join(", ", clauses)} WHERE id = @id;",
							new { id, user_id = userId, name, icon, description, approved });
					}

					if (categories is not null)
					{
						await connection.ExecuteAsync(
							"DELETE FROM games_categories WHERE game_id = @id;", new { id });

						foreach (int category_id in categories)
							await connection.ExecuteAsync(@"
                                INSERT INTO games_categories (game_id, category_id)
                                VALUES (@id, @category_id);
                            ", new { id, category_id });
					}

					if (images is not null)
					{
						await connection.ExecuteAsync(
							"DELETE FROM game_images WHERE game_id = @id;", new { id });

						foreach (byte[] data in images)
							await connection.ExecuteAsync(@"
                                INSERT INTO game_images (game_id, data)
                                VALUES (@id, @data);
                            ", new { id, data });
					}

					return true;
				});
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
			{
				throw new DatabaseException("Игра с таким названием уже существует.");
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.CheckViolation)
			{
				throw new DatabaseException("Некорректное имя или описание.");
			}
			catch (PostgresException)
			{
				throw new DatabaseException("Не удалось обновить данные игры.");
			}
		}

		public static async Task<Game?> SelectAsync(int id)
		{
			return await Database.QueryAsync(connection => connection.QueryFirstOrDefaultAsync<Game>(@"
                SELECT * FROM games WHERE id = @id;
                ", new { id }));
		}

		public static async Task<IEnumerable<Game>> SelectAsync(int offset, int count, string? name = null, bool approved = true)
		{
			if (count <= 0)
				return [];

			if (offset < 0)
				offset = 0;

			return await Database.QueryAsync(connection => connection.QueryAsync<Game>(
				$"SELECT * FROM games WHERE {(name is not null ? "name ILIKE @name" : "TRUE")} AND (approved = @approved) ORDER BY date DESC LIMIT @count OFFSET @offset;",
				new { name = $"%{name}%", offset, count, approved }));
		}

		public static async Task<IEnumerable<Game>> SelectAsync(int offset, int count, int? userId = null, bool approved = true)
		{
			if (count <= 0)
				return [];

			if (offset < 0)
				offset = 0;

			return await Database.QueryAsync(connection => connection.QueryAsync<Game>(
				$"SELECT * FROM games WHERE {(userId is not null ? "user_id = @user_id" : "TRUE")} AND (approved = @approved) ORDER BY date DESC LIMIT @count OFFSET @offset;",
				new { user_id = userId, offset, count, approved }));
		}

		public static async Task<IEnumerable<Category>> GetCategoriesAsync(int id)
		{
			return await Database.QueryAsync(connection => connection.QueryAsync<Category>(@"
                SELECT categories.id, categories.name
                FROM categories
                JOIN games_categories ON categories.id = games_categories.category_id
                WHERE games_categories.game_id = @id;
            ", new { id }));
		}

		public static async Task<IEnumerable<GameImage>> GetImagesAsync(int id)
		{
			return await Database.QueryAsync(connection => connection.QueryAsync<GameImage>(
				"SELECT * FROM game_images WHERE game_id = @id;", new { id }));
		}

		public static async Task DeleteAsync(int id)
		{
			int rowsAffected = await Database.QueryAsync(connection => connection.ExecuteAsync(@"
                DELETE FROM games WHERE id = @id;
                ", new { id }));

			if (rowsAffected == 0)
				throw new DatabaseException("Игра с таким ID не найдена.");
		}

		public static async Task<GameComment> InsertCommentAsync(int id, int user_id, string text)
		{
			try
			{
				return await Database.QueryAsync(connection => connection.QueryFirstAsync<GameComment>(@"
                    INSERT INTO game_comments (game_id, user_id, text)
                    VALUES (@id, @user_id, @text)
                    RETURNING *;
                ", new { id, user_id, text }));
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.CheckViolation)
			{
				throw new DatabaseException("Некорректный текст комментария.");
			}
			catch (PostgresException)
			{
				throw new DatabaseException("Не удалось создать комментарий.");
			}
		}

		public static async Task<IEnumerable<GameComment>> GetCommentsAsync(int id, int offset, int count)
		{
			if (count <= 0)
				return [];

			if (offset < 0)
				offset = 0;

			return await Database.QueryAsync(connection => connection.QueryAsync<GameComment>(@"
                SELECT * FROM game_comments
                WHERE game_id = @id
                ORDER BY date DESC, id DESC
                LIMIT @count OFFSET @offset;
            ", new { id, offset, count }));
		}

		public static async Task<IEnumerable<GameComment>> GetCommentsAsync(int id, int user_id)
		{
			return await Database.QueryAsync(connection => connection.QueryAsync<GameComment>(
				"SELECT * FROM game_comments WHERE game_id = @id AND user_id = @user_id;",
				new { id, user_id }));
		}
	}
}