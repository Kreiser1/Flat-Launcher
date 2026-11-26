namespace Database.Models
{
	/*
	CREATE TABLE IF NOT EXISTS games (
		id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
		author_id UUID NOT NULL,
		name VARCHAR(64) NOT NULL,
		icon BYTEA,
		date TIMESTAMPTZ NOT NULL DEFAULT now(),
		description TEXT,
		approved BOOLEAN NOT NULL DEFAULT FALSE,
		CONSTRAINT FK_author_id FOREIGN KEY (author_id) REFERENCES users(id) ON DELETE CASCADE ON UPDATE CASCADE,
		CONSTRAINT CHK_name CHECK (name ~ '^[a-zA-Zа-яА-ЯёЁ0-9:]+(?: [a-zA-Zа-яА-ЯёЁ0-9:]+)*$' AND LENGTH(name) >= 3),
		CONSTRAINT CHK_description CHECK (description ~ '^[a-zA-Zа-яА-ЯёЁ0-9\s.,!?;:«»""""''() -]+$' AND LENGTH(description) >= 3),
		CONSTRAINT UQ_user_game UNIQUE (author_id, name)
	);
	*/

	/*
	CREATE TABLE IF NOT EXISTS games_categories (
		id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
		game_id UUID NOT NULL,
		category_id UUID NOT NULL,
		CONSTRAINT FK_game_id FOREIGN KEY (game_id) REFERENCES games(id) ON DELETE CASCADE ON UPDATE CASCADE,
		CONSTRAINT FK_category_id FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE CASCADE ON UPDATE CASCADE,
		CONSTRAINT UQ_game_category UNIQUE(game_id, category_id)
	);

	CREATE TABLE IF NOT EXISTS game_images (
		id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
		game_id UUID NOT NULL,
		data BYTEA NOT NULL,
		CONSTRAINT FK_game_id FOREIGN KEY (game_id) REFERENCES games(id) ON DELETE CASCADE ON UPDATE CASCADE
	);

	CREATE TABLE IF NOT EXISTS game_comments (
		id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
		game_id UUID NOT NULL,
		user_id UUID NOT NULL,
		text TEXT NOT NULL,
		date TIMESTAMPTZ NOT NULL DEFAULT now(),
		CONSTRAINT FK_game_id FOREIGN KEY (game_id) REFERENCES games(id) ON DELETE CASCADE ON UPDATE CASCADE,
		CONSTRAINT FK_user_id FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE ON UPDATE CASCADE,
		CONSTRAINT CHK_text CHECK (text ~ '^[a-zA-Zа-яА-ЯёЁ0-9\s.,!?;:«»""""''() -]+$' AND LENGTH(text) >= 3)
	);

	CREATE TABLE IF NOT EXISTS game_ratings (
		id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
		game_id UUID NOT NULL,
		user_id UUID NOT NULL,
		value INT NOT NULL,
		CONSTRAINT FK_game_id FOREIGN KEY (game_id) REFERENCES games(id) ON DELETE CASCADE ON UPDATE CASCADE,
		CONSTRAINT FK_user_id FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE ON UPDATE CASCADE,
		CONSTRAINT UQ_game_user UNIQUE(game_id, user_id)
	);
	*/

	public class Game
	{
		public required Guid Id { get; set; }
		public required Guid AuthorId { get; set; }
		public required string Name { get; set; }
		public byte[]? Icon { get; set; }
		public DateTime Date { get; set; } = DateTime.UtcNow;
		public string? Description { get; set; }
		public bool Approved { get; set; } = false;

		public class Category
		{
			public required Guid Id { get; set; }
			public required Guid GameId { get; set; }
			public required Guid CategoryId { get; set; }
		}

		public class Image
		{
			public required Guid Id { get; set; }
			public required Guid GameId { get; set; }
			public required byte[] Data { get; set; }
		}

		public class Comment
		{
			public required Guid Id { get; set; }
			public required Guid GameId { get; set; }
			public required Guid UserId { get; set; }
			public required string Text { get; set; }
			public DateTime Date { get; set; } = DateTime.UtcNow;
		}

		public class Rating
		{
			public required Guid Id { get; set; }
			public required Guid GameId { get; set; }
			public required Guid UserId { get; set; }
			public required int Value { get; set; }
		}
	}
}
