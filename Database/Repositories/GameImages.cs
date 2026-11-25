namespace Database.Repositories
{
    using Dapper;
    using global::Database.Models;
    using Npgsql;

    public static class GameImages
    {
        public static async Task<Game.Image> Insert(Guid gameId, byte[] data)
        {
            try
            {
                return await Database.QueryAsync(async connection =>
                    await connection.QuerySingleAsync<Game.Image>(@"
                        INSERT INTO game_images (game_id, data)
                        VALUES (@gameId, @data)
                        RETURNING *;", new { gameId, data }));
            }
            catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            { throw new DatabaseException("Не удалось добавить изображение: такая игра не существует.", e); }
        }

        //public static async Task<Game.Image?> Select(Guid id) =>
        //    await Database.QueryAsync(async connection =>
        //        await connection.QuerySingleOrDefaultAsync<Game.Image>(
        //            "SELECT * FROM game_images WHERE id = @id;", new { id }));

        public static async Task<IEnumerable<Game.Image>> Select(Guid gameId) =>
            await Database.QueryAsync(async connection =>
                await connection.QueryAsync<Game.Image>(
                    "SELECT * FROM game_images WHERE game_id = @gameId;", new { gameId }));

        public static async Task Delete(Guid id) =>
            await Database.QueryAsync(async connection =>
                await connection.ExecuteAsync("DELETE FROM game_images WHERE id = @id;", new { id }));
    }
}
