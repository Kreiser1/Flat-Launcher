namespace Database.Models
{
	/*
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
	*/

	public static class Roles
	{
		public const string Administrator = "administrator";
		public const string Moderator = "moderator";
		public const string User = "user";
	}

	public class User
	{
		public required Guid Id { get; set; }
		public required string Login { get; set; }
		public required string PasswordHash { get; set; }
		public string? Nickname { get; set; }
		public required string Role { get; set; }
		public DateTime Date { get; set; } = DateTime.UtcNow;
		public bool Banned { get; set; } = false;
	}
}
