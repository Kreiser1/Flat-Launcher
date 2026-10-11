namespace Flat_Server.Database
{
	using Models;

	using System.Security.Cryptography;

	using BCrypt.Net;

	using Npgsql;

	using Dapper;

	public static class Users
	{
		public static User Insert(string login, string password, string role = "user", string? nickname = null)
		{
			password = BCrypt.HashPassword(password);

			try
			{
				return Database.Connection.QueryFirst<User>(@"
					INSERT INTO users (login, password_hash, role, nickname)
					VALUES(@login, @password, @role, @nickname)
					RETURNING *;
				", new { login, password, role, nickname });
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

		public static void Update(int id, string? login = null, string? password = null, string? role = null, string? nickname = null, bool? banned = null)
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
				int rowsAffected = Database.Connection.Execute($"UPDATE users SET {string.Join(", ", clauses)} WHERE id = @id;",
				new { id, login, password, role, nickname, banned });

				if (rowsAffected == 0)
					throw new DatabaseException($"Пользователь с ID {id} не найден.");
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

		public static User? Select(int id)
		{
			return Database.Connection.QueryFirstOrDefault<User>(@"
				SELECT * FROM users WHERE id = @id;
				", new { id });
		}

		public static User? Select(string login)
		{
			return Database.Connection.QueryFirstOrDefault<User>(@"
				SELECT * FROM users WHERE login = @login;
				", new { login });
		}

		public static User? Login(string login, string password)
		{
			var user = Database.Connection.QueryFirstOrDefault<User>(@"
				SELECT * FROM users WHERE login = @Login;
				", new { Login = login });

			if (user is not null && !user.Banned)
				if (!BCrypt.Verify(password, user.PasswordHash))
					user = null;

			return user;
		}

		public static User[] Select(string? nickname = null, string? role = null)
		{
			List<string> clauses = new();

			if (nickname is not null)
				{
				clauses.Add("login ILIKE @nickname");
				clauses.Add("nickname ILIKE @nickname");
			}

			if (role is not null)
				clauses.Add("role = @role");

			return Database.Connection.Query<User>($"SELECT * FROM users WHERE {(clauses.Count > 0 ? string.Join(" AND ", clauses) : "TRUE")};",
			new { nickname = $"%{nickname}%", role }).ToArray();
		}

		public static void Delete(int id)
		{
			int rowsAffected = Database.Connection.Execute(@"
					DELETE FROM users WHERE id = @id;
					", new { id });

			if (rowsAffected == 0)
				throw new DatabaseException("Пользователь с таким ID не найден.");
		}
	}
}