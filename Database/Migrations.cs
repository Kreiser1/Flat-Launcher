namespace Database
{
	public static partial class Database
	{
		private static string[] _migrations = [
			@"
CREATE TABLE IF NOT EXISTS users (
	id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	login VARCHAR(64) NOT NULL UNIQUE,
	password_hash VARCHAR(512) NOT NULL,
	nickname VARCHAR(64),
	role VARCHAR(16) NOT NULL,
	date TIMESTAMPTZ NOT NULL DEFAULT now(),
	banned BOOLEAN NOT NULL DEFAULT FALSE,
	CONSTRAINT CHK_login CHECK (login ~ '^[a-zA-Z0-9]+$' AND LENGTH(login) >= 3),
	CONSTRAINT CHK_nickname CHECK (nickname ~ '^[a-zA-Zа-яА-ЯёЁ0-9_]+( [a-zA-Zа-яА-ЯёЁ0-9_]+)*$' AND LENGTH(nickname) >= 3),
	CONSTRAINT CHK_role CHECK (role IN ('administrator', 'moderator', 'user'))
);

CREATE TABLE IF NOT EXISTS categories (
	id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	name VARCHAR(64) NOT NULL UNIQUE,
	CONSTRAINT CHK_category_name CHECK (name ~ '^[a-zA-Zа-яА-ЯёЁ0-9:]+(?: [a-zA-Zа-яА-ЯёЁ0-9:]+)*$' AND LENGTH(name) >= 3)
);

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

CREATE TABLE IF NOT EXISTS sessions (
	refresh_token UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	user_id UUID NOT NULL,
	device_info VARCHAR(128),
	date TIMESTAMPTZ NOT NULL DEFAULT now(),
	CONSTRAINT FK_user_id FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE ON UPDATE CASCADE
);
			",
			@"
CREATE EXTENSION IF NOT EXISTS pg_trgm;

CREATE INDEX IF NOT EXISTS idx_users_login_trgm    ON users USING gin (login gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_users_nickname_trgm ON users USING gin (nickname gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_users_role_date     ON users (role, date DESC);

CREATE INDEX IF NOT EXISTS idx_games_author_date   ON games (author_id, date DESC);
CREATE INDEX IF NOT EXISTS idx_games_approved_date ON games (approved, date DESC);
CREATE INDEX IF NOT EXISTS idx_games_name_trgm     ON games USING gin (name gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_games_date          ON games (date DESC);

CREATE INDEX IF NOT EXISTS idx_games_categories_category      ON games_categories (category_id);
CREATE INDEX IF NOT EXISTS idx_games_categories_category_game ON games_categories (category_id, game_id);

CREATE INDEX IF NOT EXISTS idx_game_images_game ON game_images (game_id, id);

CREATE INDEX IF NOT EXISTS idx_game_comments_game_date ON game_comments (game_id, date DESC);
CREATE INDEX IF NOT EXISTS idx_game_comments_user      ON game_comments (user_id);

CREATE INDEX IF NOT EXISTS idx_game_ratings_game ON game_ratings (game_id);
CREATE INDEX IF NOT EXISTS idx_game_ratings_user ON game_ratings (user_id);

CREATE INDEX IF NOT EXISTS idx_game_versions_game_approved_date ON game_versions (game_id, approved, date DESC);
CREATE INDEX IF NOT EXISTS idx_game_versions_game_date          ON game_versions (game_id, date DESC);

CREATE INDEX IF NOT EXISTS idx_sessions_user_date ON sessions (user_id, date DESC);

CREATE INDEX IF NOT EXISTS idx_categories_name_trgm ON categories USING gin (name gin_trgm_ops);
			"
		];
	}
}
