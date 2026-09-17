namespace Flat_Server {
	using Dapper;
	using Npgsql;

	internal static class Database {
		private static string connectionString = "";

		public static NpgsqlConnection Connection {
			get {
				NpgsqlConnection connection = new(connectionString);
				connection.Open();
				return connection;
			}
		}

		public static void Connect(string connectionString) {
			Database.connectionString = connectionString;

			try {
				using var connection = Connection;

				connection.Execute(@"
					
				");
			}
		}
	}
}
