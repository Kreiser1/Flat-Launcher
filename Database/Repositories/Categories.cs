namespace Database.Repositories
{
	using Dapper;

	using global::Database.Models;

	using Npgsql;

	public static class Categories
	{
		public static async Task<Category> Insert(string name)
		{
			try
			{
				return await Database.QueryAsync(async connection =>
					await connection.QuerySingleAsync<Category>(@"
                        INSERT INTO categories (name)
                        VALUES (@name)
                        RETURNING *;", new { name }));
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.CheckViolation)
			{ throw new DatabaseException("Не удалось добавить категорию: некорректное имя.", e); }
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
			{ throw new DatabaseException("Не удалось добавить категорию: категория с таким именем уже существует.", e); }
		}

		public static async Task<Category?> Select(Guid id) =>
			await Database.QueryAsync(async connection =>
				await connection.QuerySingleOrDefaultAsync<Category>(
					"SELECT * FROM categories WHERE id = @id;", new { id }));

		public static async Task<Category?> Select(string name) =>
			await Database.QueryAsync(async connection =>
				await connection.QuerySingleOrDefaultAsync<Category>(
					"SELECT * FROM categories WHERE name = @name;", new { name }));

		public static async Task<IEnumerable<Category>> Select(int offset, int count, string? name = null)
		{
			offset = Math.Max(0, offset);
			count = Math.Max(0, count);

			if (count == 0)
				return [];

			return await Database.QueryAsync(async connection =>
				await connection.QueryAsync<Category>(@"
                    SELECT * FROM categories
                    WHERE (@name IS NULL OR name ILIKE @pattern)
                    LIMIT @count OFFSET @offset;",
					new { name, pattern = $"%{name}%", count, offset }));
		}

		public static async Task<Category> Update(Guid id, string name)
		{
			try
			{
				Category? result = await Database.QueryAsync(async connection =>
					await connection.QuerySingleOrDefaultAsync<Category>(@"
                        UPDATE categories SET name = @name
                        WHERE id = @id RETURNING *;", new { id, name }));

				return result ?? throw new DatabaseException("Категория не найдена.");
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.CheckViolation)
			{ throw new DatabaseException("Не удалось обновить категорию: некорректное имя.", e); }
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
			{ throw new DatabaseException("Не удалось обновить категорию: категория с таким именем уже существует.", e); }
		}

		public static async Task Delete(Guid id) =>
			await Database.QueryAsync(async connection =>
				await connection.ExecuteAsync("DELETE FROM categories WHERE id = @id;", new { id }));
	}
}
