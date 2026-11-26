namespace Database.Models
{
	/*
	CREATE TABLE IF NOT EXISTS game_versions (
		id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
		game_id UUID NOT NULL,
		name VARCHAR(16) NOT NULL,
		data BYTEA NOT NULL,
		hash VARCHAR(64) NOT NULL,
		approved BOOLEAN NOT NULL DEFAULT FALSE,
		date TIMESTAMPTZ NOT NULL DEFAULT now(),
		CONSTRAINT CHK_name CHECK (name ~ '^\d+(\.\d+)*$'),
		CONSTRAINT UQ_game_version UNIQUE (game_id, name),
		CONSTRAINT UQ_game_hash UNIQUE (game_id, hash),
		CONSTRAINT FK_game_id FOREIGN KEY (game_id) REFERENCES games(id) ON DELETE CASCADE ON UPDATE CASCADE
	);
	*/

	public class GameVersion
	{
		public required Guid Id { get; set; }
		public required Guid GameId { get; set; }
		public required string Name { get; set; }
		public required byte[] Data { get; set; }
		public required string Hash { get; set; }
		public bool Approved { get; set; } = false;
		public DateTime Date { get; set; } = DateTime.UtcNow;

		public class Preview
		{
			public required Guid Id { get; set; }
			public required Guid GameId { get; set; }
			public required string Name { get; set; }
			public bool Approved { get; set; } = false;
			public DateTime Date { get; set; } = DateTime.UtcNow;
		}
	}
}
