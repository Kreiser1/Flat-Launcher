namespace Flat_Server.Database
{
	using Dapper;

	using Models;

	using Npgsql;

	public static class Categories
	{
		public static Category Insert(string name)
		{
			return Database.Connection.QueryFirst<Category>(@"
				INSERT INTO categories (name) VALUES (@name)
				RETURNING *
			", new { name });
		}

		public static void Update(int id, string name)
		{
			try
			{
				int rowsAffected = Database.Connection.Execute("UPDATE categories SET name = @name WHERE id = @id;",
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
			return Database.Connection.QueryFirstOrDefault<Category>(@"
				SELECT * FROM categories WHERE id = @id;
				", new { id });
		}

		public static Category[] Select(string? name = null)
		{
			return Database.Connection.Query<Category>($"SELECT * FROM categories WHERE {(name is not null ? $"name ILIKE @name" : "TRUE")};",
			new { name = $"%{name}%" }).ToArray();
		}

		public static void Delete(int id)
		{
			int rowsAffected = Database.Connection.Execute(@"
					DELETE FROM categories WHERE id = @id;
					", new { id });

			if (rowsAffected == 0)
				throw new DatabaseException("Категория с таким ID не найдена.");
		}
	}
}
