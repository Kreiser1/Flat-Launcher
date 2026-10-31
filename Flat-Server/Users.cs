namespace Flat_Server.Database
{
	using Models;

	using BCrypt.Net;

	using Npgsql;

	using Dapper;

	public static class Users
	{
		public static async Task<User> InsertAsync(string login, string password, string role = "user", string? nickname = null)
		{
			password = BCrypt.HashPassword(password);

			try
			{
				return await Database.QueryAsync(connection => connection.QueryFirstAsync<User>(@"
					INSERT INTO users (login, password_hash, role, nickname)
					VALUES(@login, @password, @role, @nickname)
					RETURNING *;
				", new { login, password, role, nickname }));
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
			{
				throw new DatabaseException("Пользователь с таким логином уже существует.");
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.CheckViolation)
			{
				throw new DatabaseException("Некорректный логин или никнейм.");
			}
			catch (PostgresException)
			{
				throw new DatabaseException("Не удалось создать нового пользователя.");
			}
		}

		public static async Task UpdateAsync(int id, string? login = null, string? password = null, string? role = null, string? nickname = null, bool? banned = null)
		{
			List<string> clauses = new();

			if (login is not null)
				clauses.Add("login = @login");

			if (password is not null)
			{
				password = BCrypt.HashPassword(password);
				clauses.Add("password_hash = @password");
			}

			if (role is not null)
				clauses.Add("role = @role");

			if (nickname is not null)
				clauses.Add("nickname = @nickname");

			if (banned is not null)
				clauses.Add("banned = @banned");

			if (clauses.Count == 0)
				return;

			try
			{
				int rowsAffected = await Database.QueryAsync(connection => connection.ExecuteAsync(
					$"UPDATE users SET {string.Join(", ", clauses)} WHERE id = @id;",
					new { id, login, password, role, nickname, banned }));

				if (rowsAffected <= 0)
					throw new DatabaseException("Пользователь с таким ID не найден.");
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
			{
				throw new DatabaseException("Пользователь с таким логином уже существует.");
			}
			catch (PostgresException)
			{
				throw new DatabaseException("Не удалось обновить данные пользователя.");
			}
		}

		public static async Task<User?> SelectAsync(int id)
		{
			return await Database.QueryAsync(connection => connection.QueryFirstOrDefaultAsync<User>(@"
				SELECT * FROM users WHERE id = @id;
				", new { id }));
		}

		public static async Task<User?> SelectAsync(string login)
		{
			return await Database.QueryAsync(connection => connection.QueryFirstOrDefaultAsync<User>(@"
				SELECT * FROM users WHERE login = @login;
				", new { login }));
		}

		public static async Task<User?> LoginAsync(string login, string password)
		{
			User? user = await Database.QueryAsync(connection => connection.QueryFirstOrDefaultAsync<User>(@"
				SELECT * FROM users WHERE login = @Login;
				", new { Login = login }));

			return user is null || user.Banned || !BCrypt.Verify(password, user.PasswordHash) ? null : user;
		}

		public static async Task<IEnumerable<User>> SelectAsync(string? nickname = null, string? role = null)
		{
			List<string> clauses = new();

			if (nickname is not null)
				clauses.Add("(login ILIKE @nickname OR nickname ILIKE @nickname)");

			if (role is not null)
				clauses.Add("role = @role");

			return await Database.QueryAsync(connection => connection.QueryAsync<User>(
				$"SELECT * FROM users WHERE {(clauses.Count > 0 ? string.Join(" AND ", clauses) : "TRUE")};",
				new { nickname = $"%{nickname}%", role }));
		}

		public static async Task DeleteAsync(int id)
		{
			int rowsAffected = await Database.QueryAsync(connection => connection.ExecuteAsync(@"
					DELETE FROM users WHERE id = @id;
					", new { id }));

			if (rowsAffected <= 0)
				throw new DatabaseException("Пользователь с таким ID не найден.");
		}
	}
}