namespace Database.Repositories
{
    using Dapper;
    using global::Database.Models;
    using Npgsql;

    public static class GameRatings
    {
        public static async Task<Game.Rating> Insert(Guid gameId, Guid userId, int value)
        {
            try
            {
                return await Database.QueryAsync(async connection =>
                    await connection.QuerySingleAsync<Game.Rating>(@"
                        INSERT INTO game_ratings (game_id, user_id, value)
                        VALUES (@gameId, @userId, @value)
                        RETURNING *;", new { gameId, userId, value }));
            }
            catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
            { throw new DatabaseException("Не удалось добавить оценку игры: пользователь уже оценил эту игру.", e); }
            catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            { throw new DatabaseException("Не удалось добавить оценку игры: такая игра или пользователь не существует.", e); }
        }

        //public static async Task<Game.Rating?> Select(Guid id) =>
        //    await Database.QueryAsync(async connection =>
        //        await connection.QuerySingleOrDefaultAsync<Game.Rating>(
        //            "SELECT * FROM game_ratings WHERE id = @id;", new { id }));

        public static async Task<Game.Rating?> Select(Guid gameId, Guid userId) =>
            await Database.QueryAsync(async connection =>
                await connection.QuerySingleOrDefaultAsync<Game.Rating>(@"
                    SELECT * FROM game_ratings
                    WHERE game_id = @gameId AND user_id = @userId;",
                    new { gameId, userId }));

        public static async Task<IEnumerable<Game.Rating>> Select(Guid gameId) =>
            await Database.QueryAsync(async connection =>
                await connection.QueryAsync<Game.Rating>(
                    "SELECT * FROM game_ratings WHERE game_id = @gameId;", new { gameId }));

        public static async Task<Game.Rating> Update(Guid gameId, Guid userId, int value)
        {
            Game.Rating? rating = await Database.QueryAsync(async connection =>
                await connection.QuerySingleOrDefaultAsync<Game.Rating>(@"
                    UPDATE game_ratings SET value = @value
                    WHERE game_id = @gameId AND user_id = @userId
                    RETURNING *;", new { gameId, userId, value }));

            return rating ?? throw new DatabaseException("Не удалось обновить оценку: такая оценка не существует.");
        }

        public static async Task Delete(Guid gameId, Guid userId) =>
            await Database.QueryAsync(async connection =>
                await connection.ExecuteAsync(@"
                    DELETE FROM game_ratings
                    WHERE game_id = @gameId AND user_id = @userId;", new { gameId, userId }));
    }
}
