namespace Flat_Server.Database
{
	using Models;

	using System.Security.Cryptography;

	using BCrypt.Net;

	using Npgsql;

	using Dapper;

	public static class Games
	{
		public static Game Insert(int user_id, string name, byte[]? icon = null, string? description = null, int[]? categories = null, byte[][]? images = null)
		{
			using var connection = Database.Connection;

			try
			{
				using var transaction = connection.BeginTransaction();

				Game game = connection.QueryFirst(@"
					INSERT INTO games (user_id, name, icon, description)
					VALUES(@user_id, @name, @icon, @description)
					RETURNING *;
				", new { user_id, name, icon, description });

				if (categories is not null)
					foreach (int category_id in categories)
						connection.Execute(@"
						INSERT INTO games_categories (game_id, category_id)
						VALUES (@game_id, @category_id);
					", new { game_id = game.Id, category_id });

				if (images is not null)
					foreach (byte[] data in images)
						connection.Execute(@"
						INSERT INTO game_images (game_id, data)
						VALUES (@game_id, @data);
					", new { game_id = game.Id, data });

				transaction.Commit();

				return game;
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

		public static void Update(int id, int? user_id = null, string? name = null, byte[]? icon = null, string? description = null, bool? approved = null, int[]? categories = null, byte[][]? images = null)
		{
			List<string> clauses = new();

			if (user_id is not null)
				clauses.Add("user_id = @user_id");

			if (name is not null)
				clauses.Add("name = @name");

			if (icon is not null)
				clauses.Add("icon = @icon");

			if (description is not null)
				clauses.Add("description = @description");

			if (approved is not null)
				clauses.Add("approved = @approved");

			if (clauses.Count == 0)
				return;

			using var connection = Database.Connection;

			try
			{
				using var transaction = connection.BeginTransaction();

				int rowsAffected = connection.Execute($"UPDATE games SET {string.Join(", ", clauses)} WHERE id = @id;",
				new { id, user_id, name, icon, description, approved });

				if (rowsAffected == 0)
					throw new DatabaseException($"Игра с ID {id} не найдена.");

				if (categories is not null)
				{
					connection.Execute("DELETE FROM games_categories WHERE game_id = @id;", new { id });

					foreach (int category_id in categories)
						connection.Execute(@"
						INSERT INTO games_categories (game_id, category_id)
						VALUES (@id, @category_id);
					", new { id, category_id });
				}

				if (images is not null)
				{
					connection.Execute("DELETE FROM game_images WHERE game_id = @id;", new { id });

					foreach (byte[] data in images)
						connection.Execute(@"
						INSERT INTO game_images (game_id, data)
						VALUES (@id, @data);
					", new { id, data });
				}

				transaction.Commit();
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
			{
				throw new DatabaseException("Игра с таким названием уже существует.");
			}
			catch (PostgresException)
			{
				throw new DatabaseException("Не удалось обновить данные игры.");
			}
		}

		public static Game? Select(int id)
		{
			using var connection = Database.Connection;

			return connection.QueryFirstOrDefault<Game>(@"
				SELECT * FROM games WHERE id = @id;
				", new { id });
		}

		public static Game[] Select(int offset, int count, string? name = null)
		{
			List<string> clauses = new();

			if (name is not null)
				clauses.Add("name ILIKE @name");

			count = Math.Clamp(count, 0, 30);

			if (offset < 0)
				offset = 0;

			using var connection = Database.Connection;

			return connection.Query<Game>($"SELECT * FROM games WHERE {(name is not null ? "name ILIKE @name" : "TRUE")} LIMIT @count OFFSET @offset;",
			new { name = $"%{name}%", offset, count }).ToArray();
		}

		public static Category[] GetCategories(int id)
		{
			using var connection = Database.Connection;

			return connection.Query<Category>($"SELECT * FROM categories JOIN games_categories on categories.id = games_categories.category_id WHERE games_categories.game_id = @id;",
			new { id }).ToArray();
		}

		public static GameImage[] GetImages(int id) {
			using var connection = Database.Connection;

			return connection.Query<GameImage>($"SELECT * FROM game_images WHERE game_id = @id;",
			new { id }).ToArray();
		}

		public static void Delete(int id)
		{
			using var connection = Database.Connection;

			int rowsAffected = connection.Execute(@"
					DELETE FROM games WHERE id = @id;
					", new { id });

			if (rowsAffected == 0)
				throw new DatabaseException("Игра с таким ID не найдена.");
		}

		public static GameComment InsertComment(int id, int user_id, string text)
		{
			using var connection = Database.Connection;

			try
			{
				GameComment gameComment = connection.QueryFirst(@"
					INSERT INTO game_comments (game_id, user_id, text)
					VALUES (@id, @user_id, @text)
					RETURNING *;
				", new { id, user_id, text });

				return gameComment;
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

		public static GameComment[] GetComments(int id, int offset, int count)
		{
			count = Math.Clamp(count, 0, 30);

			if (offset < 0)
				offset = 0;

			using var connection = Database.Connection;

			return connection.Query<GameComment>("SELECT * FROM game_comments WHERE game_id = @id LIMIT @count OFFSET @offset;",
			new { id, offset, count }).ToArray();
		}

		public static GameComment[] GetComments(int id, int user_id)
		{
			using var connection = Database.Connection;

			return connection.Query<GameComment>("SELECT * FROM game_comments WHERE game_id = @id AND user_id = @user_id;",
			new { id, user_id }).ToArray();
		}
	}
}
