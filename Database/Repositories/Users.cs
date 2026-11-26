namespace Database.Repositories
{
	using Dapper;

	using global::Database.Models;

	using Npgsql;

	public static class Users
	{
		public static async Task<User> Insert(string login, string passwordHash, string role, string? nickname = null)
		{
			try
			{
				return await Database.QueryAsync(async connection =>
					await connection.QuerySingleAsync<User>(@"
						INSERT INTO users (login, password_hash, role, nickname)
						VALUES (@login, @passwordHash, @role, @nickname)
						RETURNING *;",
						new { login, passwordHash, role, nickname }));
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.CheckViolation)
			{ throw new DatabaseException("Не удалось добавить пользователя: некорректные данные.", e); }
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
			{ throw new DatabaseException("Не удалось добавить пользователя: нарушение уникальности.", e); }
		}

		public static async Task<User?> Select(Guid id) =>
			await Database.QueryAsync(async connection =>
				await connection.QuerySingleOrDefaultAsync<User?>(
					"SELECT * FROM users WHERE id = @id;", new { id }));

		public static async Task<User?> Select(string login) =>
			await Database.QueryAsync(async connection =>
				await connection.QuerySingleOrDefaultAsync<User?>(
					"SELECT * FROM users WHERE login = @login;", new { login }));

		public static async Task<IEnumerable<User>> Select(
			int offset, int count, string? login = null, string? role = null,
			string? nickname = null)
		{
			offset = Math.Max(0, offset);
			count = Math.Max(0, count);

			if (count == 0)
				return [];

			return await Database.QueryAsync(async connection =>
				await connection.QueryAsync<User>(
					@"SELECT * FROM users
					WHERE (@login IS NULL OR login ILIKE @pattern1)
					AND (@role IS NULL OR role ILIKE @role)
					AND (@nickname IS NULL OR nickname ILIKE @pattern2)
					LIMIT @count
					OFFSET @offset;",
				new { login, pattern1 = $"%{login}%", role, nickname, pattern2 = $"%{nickname}%", offset, count }));
		}

		public static async Task Delete(Guid id)
		{
			await Database.QueryAsync(async connection =>
			await connection.ExecuteAsync(@"
				DELETE FROM users WHERE id = @id;	
			", new { id }));
		}

		public static async Task Delete(string login)
		{
			await Database.QueryAsync(async connection =>
			await connection.ExecuteAsync(@"
				DELETE FROM users WHERE login = @login;	
			", new { login }));
		}

		public static async Task<User> Update(
			Guid id, string? login = null, string? passwordHash = null,
			string? nickname = null, string? role = null, bool? banned = null)
		{
			List<string> clauses = [];

			if (login is not null)
				clauses.Add("login = @login");

			if (passwordHash is not null)
				clauses.Add("password_hash = @passwordHash");

			if (nickname is not null)
				clauses.Add("nickname = @nickname");

			if (role is not null)
				clauses.Add("role = @role");

			if (banned is not null)
				clauses.Add("banned = @banned");

			if (clauses.Count <= 0)
				throw new DatabaseException("Не удалось обновить данные пользователя: нет данных для обновления.");

			User? user = await Database.QueryAsync(async connection =>
				await connection.QuerySingleOrDefaultAsync<User>($@"
					UPDATE users
					SET {string.Join(", ", clauses)}
					WHERE id = @id
					RETURNING *;",
				new { id, login, passwordHash, nickname, role, banned }));

			return user ?? throw new DatabaseException("Не удалось обновить данные пользователя: такого пользователя не существует.");
		}

		public static async Task<User> Update(
			string? login, string? passwordHash = null,
			string? nickname = null, string? role = null, bool? banned = null)
		{
			List<string> clauses = [];

			if (passwordHash is not null)
				clauses.Add("password_hash = @passwordHash");

			if (nickname is not null)
				clauses.Add("nickname = @nickname");

			if (role is not null)
				clauses.Add("role = @role");

			if (banned is not null)
				clauses.Add("banned = @banned");

			if (clauses.Count <= 0)
				throw new DatabaseException("Не удалось обновить данные пользователя: нет данных для обновления.");

			User? user = await Database.QueryAsync(async connection =>
				await connection.QuerySingleOrDefaultAsync<User>($@"
					UPDATE users
					SET {string.Join(", ", clauses)}
					WHERE login = @login
					RETURNING *;",
				new { login, passwordHash, nickname, role, banned }));

			return user ?? throw new DatabaseException("Не удалось обновить данные пользователя: такого пользователя не существует.");
		}
	}
}
