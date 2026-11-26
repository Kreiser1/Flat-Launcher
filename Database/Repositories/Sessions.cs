namespace Database.Repositories
{
    using Dapper;
    using global::Database.Models;
    using Npgsql;

    public static class Sessions
    {
        public static async Task<Session> Insert(Guid userId, string? deviceInfo = null)
        {
            try
            {
                return await Database.QueryAsync(async connection =>
                    await connection.QuerySingleAsync<Session>(@"
                        INSERT INTO sessions (user_id, device_info)
                        VALUES (@userId, @deviceInfo)
                        RETURNING *;", new { userId, deviceInfo }));
            }
            catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            { throw new DatabaseException("Не удалось создать сессию: такого пользователя не существует.", e); }
        }

        public static async Task<Session?> Select(Guid refreshToken) =>
            await Database.QueryAsync(async connection =>
                await connection.QuerySingleOrDefaultAsync<Session>(@"
                    SELECT * FROM sessions WHERE refresh_token = @refreshToken;",
                    new { refreshToken }));

        public static async Task<IEnumerable<Session>> SelectByUser(Guid userId) =>
            await Database.QueryAsync(async connection =>
                await connection.QueryAsync<Session>(@"
                    SELECT * FROM sessions
                    WHERE user_id = @userId",
                    new { userId }));

        public static async Task Delete(Guid refreshToken) =>
            await Database.QueryAsync(async connection =>
                await connection.ExecuteAsync(@"
                    DELETE FROM sessions WHERE refresh_token = @refreshToken;",
                    new { refreshToken }));

        public static async Task DeleteByUser(Guid userId) =>
            await Database.QueryAsync(async connection =>
                await connection.ExecuteAsync(
                    "DELETE FROM sessions WHERE user_id = @userId;", new { userId }));
    }
}
