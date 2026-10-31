namespace Flat_Server.Database
{
	using Dapper;

	using Models;

	using Npgsql;

	public static class Categories
	{
		public static async Task<Category> InsertAsync(string name)
		{
			return await Database.QueryAsync(connection => connection.QueryFirstAsync(@"
				INSERT INTO categories (name) VALUES (@name)
				RETURNING *
			", new { name }));
		}

		public static async Task UpdateAsync(int id, string name)
		{
			try
			{
				int rowsAffected = await Database.QueryAsync(connection =>
					connection.ExecuteAsync("UPDATE categories SET name = @name WHERE id = @id;",
					new { id, name }));

				if (rowsAffected <= 0)
					throw new DatabaseException("Категория с таким ID не найдена.");
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

		public static async Task<Category?> SelectAsync(int id)
		{
			return await Database.QueryAsync(connection => connection.QueryFirstOrDefaultAsync<Category>(@"
				SELECT * FROM categories WHERE id = @id;
				", new { id }));
		}

		public static async Task<IEnumerable<Category>> SelectAsync(string? name = null)
		{
			return await Database.QueryAsync(connection => connection.QueryAsync<Category>($"SELECT * FROM categories WHERE {(name is not null ? $"name ILIKE @name" : "TRUE")};",
			new { name = $"%{name}%" }));
		}

		public static async Task DeleteAsync(int id)
		{
			int rowsAffected = await Database.QueryAsync(connection => connection.ExecuteAsync(@"
					DELETE FROM categories WHERE id = @id;
					", new { id }));

			if (rowsAffected <= 0)
				throw new DatabaseException("Категория с таким ID не найдена.");
		}
	}
}
