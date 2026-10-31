namespace Flat_Server.Database
{
	using System.Security.Cryptography;

	using Dapper;

	using Models;

	using Npgsql;

	public static class GameVersions
	{
		public static async Task<GameVersion> InsertAsync(int game_id, string name, byte[] data)
		{
			try
			{
				return await Database.QueryAsync(connection => connection.QueryFirstAsync<GameVersion>(@"
                    INSERT INTO game_versions (game_id, name, data, hash)
                    VALUES (@game_id, @name, @data, @hash)
                    RETURNING id, game_id, name, hash, date, approved
                ", new
				{
					game_id,
					name,
					data,
					hash = Convert.ToHexStringLower(SHA256.HashData(data)),
				}));
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

		public static async Task UpdateAsync(int game_id, string name, bool? approved = null, byte[]? data = null)
		{
			List<string> clauses = new();
			string hash = string.Empty;

			if (approved is not null)
				clauses.Add("approved = @approved");

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
				int rowsAffected = await Database.QueryAsync(connection => connection.ExecuteAsync(
					$"UPDATE game_versions SET {string.Join(", ", clauses)} WHERE game_id = @game_id AND name = @name;",
					new { game_id, name, approved, data, hash }));

				if (rowsAffected <= 0)
					throw new DatabaseException($"Такая версия игры не найдена не найдена.");
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

		public static async Task<GameVersion?> SelectAsync(int game_id, string name)
		{
			return await Database.QueryAsync(connection => connection.QueryFirstOrDefaultAsync<GameVersion>(@"
                SELECT id, game_id, name, hash, approved
                FROM game_versions
                WHERE game_id = @game_id AND name = @name;
                ", new { game_id, name }));
		}

		public static async Task<byte[]?> DownloadAsync(int game_id, string name)
		{
			return await Database.QueryAsync(connection => connection.QueryFirstOrDefaultAsync<byte[]>(@"
                SELECT data FROM game_versions
                WHERE game_id = @game_id AND name = @name;
                ", new { game_id, name }));
		}

		public static async Task<GameVersion[]> SelectAsync(int game_id)
		{
			return (await Database.QueryAsync(connection => connection.QueryAsync<GameVersion>(@"
                SELECT id, game_id, name, hash, approved
                FROM game_versions
                WHERE game_id = @game_id;
                ", new { game_id }))).ToArray();
		}

		public static async Task DeleteAsync(int game_id, string name)
		{
			int rowsAffected = await Database.QueryAsync(connection => connection.ExecuteAsync(@"
                DELETE FROM game_versions WHERE game_id = @game_id AND name = @name;
                ", new { game_id, name }));

			if (rowsAffected <= 0)
				throw new DatabaseException("Версия игры с таким ID не найдена.");
		}
	}
}