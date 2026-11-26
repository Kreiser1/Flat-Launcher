namespace Database.Repositories
{
	using Dapper;

	using global::Database.Models;

	using Npgsql;

	public static class GameVersions
	{
		public static async Task<GameVersion> Insert(
			Guid gameId, string name, byte[] data, string hash)
		{
			try
			{
				return await Database.QueryAsync(async connection =>
					await connection.QuerySingleAsync<GameVersion>(@"
                        INSERT INTO game_versions (game_id, name, data, hash)
                        VALUES (@gameId, @name, @data, @hash)
                        RETURNING *;", new { gameId, name, data, hash }));
			}
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.CheckViolation)
			{ throw new DatabaseException("Не удалось добавить версию: некорректное имя.", e); }
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
			{ throw new DatabaseException("Не удалось добавить версию: такая версия уже существует.", e); }
			catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.ForeignKeyViolation)
			{ throw new DatabaseException("Не удалось добавить версию: такой игры не существует.", e); }
		}

		//public static async Task<GameVersion?> Select(Guid id) =>
		//    await Database.QueryAsync(async connection =>
		//        await connection.QuerySingleOrDefaultAsync<GameVersion>(
		//            "SELECT * FROM game_versions WHERE id = @id;", new { id }));

		public static async Task<GameVersion?> Select(Guid gameId, string name) =>
			await Database.QueryAsync(async connection =>
				await connection.QuerySingleOrDefaultAsync<GameVersion>(@"
                    SELECT * FROM game_versions
                    WHERE game_id = @gameId AND name = @name;", new { gameId, name }));

		public static async Task Update(Guid id, bool approved)
		{
			int rowsAffected = 0;

			await Database.QueryAsync(async connection =>
				rowsAffected = await connection.ExecuteAsync(
				@"UPDATE game_versions SET approved = @approved WHERE id = @id;",
				new { id, approved }));

			if (rowsAffected <= 0)
				throw new DatabaseException("Ну удалось обновить данные версии игры: такая версия игры не существует.");
		}

		public static async Task Delete(Guid id) =>
			await Database.QueryAsync(async connection =>
				await connection.ExecuteAsync("DELETE FROM game_versions WHERE id = @id;", new { id }));
	}

	public static class Previews
	{
		//public static async Task<GameVersion.Preview?> Select(Guid id) =>
		//	await Database.QueryAsync(async connection =>
		//		await connection.QuerySingleOrDefaultAsync<GameVersion.Preview>(@"
  //                  SELECT id, game_id, name, approved, date
  //                  FROM game_versions WHERE id = @id;", new { id }));

		public static async Task<GameVersion.Preview?> Select(Guid gameId, string name) =>
			await Database.QueryAsync(async connection =>
				await connection.QuerySingleOrDefaultAsync<GameVersion.Preview>(@"
                    SELECT id, game_id, name, approved, date
                    FROM game_versions
                    WHERE game_id = @gameId AND name = @name;", new { gameId, name }));

		public static async Task<IEnumerable<GameVersion.Preview>> Select(Guid gameId, bool? approved = null)
		{
			return await Database.QueryAsync(async connection =>
				await connection.QueryAsync<GameVersion.Preview>(@"
                    SELECT id, game_id, name, approved, date
                    FROM game_versions
                    WHERE game_id = @gameId
                    AND (@approved IS NULL OR approved = @approved);",
					new { gameId, approved }));
		}
	}
}
