namespace Flat_Server.Database
{
	using Dapper;

	using Models;

	using Npgsql;

	using System.Security.Cryptography;

	public static class GameVersions
	{
		public static GameVersion Insert(int game_id, string name, byte[] data)
		{
			try
			{
				return Database.Connection.QueryFirst(@"
					INSERT INTO game_versions (game_id, name, data, hash)
					VALUES (@game_id, @name, @data, @hash)
					RETURNING id, game_id, name, hash, date, approved
				", new { game_id, name, data, hash = Convert.ToHexStringLower(SHA256.HashData(data)), });
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
			{
				throw new DatabaseException("Такая версия уже существует.");
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.CheckViolation)
			{
				throw new DatabaseException("Некорректное имя версии.");
			}
			catch (PostgresException)
			{
				throw new DatabaseException("Не удалось создать новую версию.");
			}
		}

		public static void Update(int game_id, string name, bool? approved = null, byte[]? data = null)
		{
			List<string> clauses = new();

			if (approved is not null)
				clauses.Add("approved = @approved");

			string hash = string.Empty;

			if (data is not null)
			{
				hash = Convert.ToHexStringLower(SHA256.HashData(data));

				clauses.Add("data = @data");
				clauses.Add("hash = @hash");
			}

			if (clauses.Count == 0)
				return;

			try
			{
				int rowsAffected = Database.Connection.Execute($"UPDATE game_versions SET {string.Join(", ", clauses)} WHERE game_id = @game_id AND name = @name;",
				new { game_id, name, approved, data, hash });

				if (rowsAffected == 0)
					throw new DatabaseException($"Версия игры с ID {game_id} {name} не найден.");
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
			{
				throw new DatabaseException("Такая версия уже существует.");
			}
			catch (PostgresException)
			{
				throw new DatabaseException("Не удалось обновить данные версии.");
			}
		}

		public static GameVersion? Select(int game_id, string name)
		{
			return Database.Connection.QueryFirstOrDefault<GameVersion>(@"
				SELECT id, game_id, name, hash, approved
				FROM game_versions WHERE game_id = @game_id AND name = @name;
				", new { game_id, name });
		}

		public static byte[]? Download(int game_id, string name)
		{
			return Database.Connection.QueryFirstOrDefault<byte[]>(@"
				SELECT data FROM game_versions WHERE game_id = @game_id AND name = @name;
				", new { game_id, name });
		}

		public static GameVersion[] Select(int game_id)
		{
			return Database.Connection.Query<GameVersion>(@"
				SELECT id, game_id, name, hash, approved
				FROM game_versions WHERE game_id = @game_id;
				", new { game_id }).ToArray();
		}

		public static void Delete(int game_id, string name)
		{
			int rowsAffected = Database.Connection.Execute(@"
					DELETE FROM game_versions WHERE game_id = @game_id AND name = @name;
					", new { game_id, name });

			if (rowsAffected == 0)
				throw new DatabaseException("Версия игры с таким ID не найдена.");
		}
	}
}
