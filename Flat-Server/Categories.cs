namespace Flat_Server.Database
{
	using Dapper;

	using Models;

	using Npgsql;

	public static class Categories
	{
		public static Category Insert(string name)
		{
			using var connection = Database.Connection;

			return connection.QueryFirst<Category>(@"
				INSERT INTO categories (name) VALUES (@name)
			", new { name });
		}

		public static void Update(int id, string name)
		{
			using var connection = Database.Connection;

			try
			{
				int rowsAffected = connection.Execute("UPDATE categories SET (name = @name) WHERE id = @id;",
				   new { id, name });

				if (rowsAffected == 0)
					throw new DatabaseException($"Категория с ID {id} не найдена.");
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
			{
				throw new DatabaseException("Категория с таким названием уже существует.");
			}
			catch (PostgresException)
			{
				throw new DatabaseException("Не удалось переименовать категорию.");
			}
		}

		public static Category? Select(int id)
		{
			using var connection = Database.Connection;

			return connection.QueryFirstOrDefault<Category>(@"
				SELECT * FROM categories WHERE id = @id;
				", new { id });
		}

		public static Category[] Select(string? name = null)
		{
			using var connection = Database.Connection;

			return connection.Query<Category>($"SELECT * FROM categories WHERE {(name is not null ? $"name ILIKE @name" : "TRUE")};",
			new { name = $"%{name}%" }).ToArray();
		}

		public static void Delete(int id)
		{
			using var connection = Database.Connection;

			int rowsAffected = connection.Execute(@"
					DELETE FROM categories WHERE id = @id;
					", new { id });

			if (rowsAffected == 0)
				throw new DatabaseException("Категория с таким ID не найдена.");
		}
	}
}
