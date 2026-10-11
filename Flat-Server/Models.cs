namespace Flat_Server.Database.Models {
	public static class Roles {
		public const string Admin = "administrator";
		public const string Moderator = "moderator";
		public const string User = "user";
	}

	public class User {
		public required int Id { get; set; }
		public required string Login { get; set; }
		public required string PasswordHash { get; set; }
		public string? Nickname { get; set; }
		public required string Role { get; set; }
		public bool Banned { get; set; } = false;
	}

	public class Category {
		public required int Id { get; set; }
		public required string Name { get; set; }
	}

	public class Game {
		public required int Id { get; set; }
		public required int UserId { get; set; }
		public required string Name { get; set; }
		public byte[]? Icon { get; set; }
		public string? Description { get; set; }
		public bool Approved { get; set; } = false;
		public int Likes { get; set; } = 0;
		public int Dislikes { get; set; } = 0;
	}

	public class GameImage {
		public required int Id { get; set; }
		public required int GameId { get; set; }
		public required byte[] Data { get; set; }
	}

	public class GameComment {
		public required int Id { get; set; }
		public required int GameId { get; set; }
		public required int UserId { get; set; }
		public required string Text { get; set; }
		public required DateTime Date { get; set; } = DateTime.Now;
	}

	public class GameVersion {
		public required int Id { get; set; }
		public required int GameId { get; set; }
		public required string Name { get; set; }
		public required string Hash { get; set; }
		public bool Approved { get; set; } = false;
		public required DateTime Date { get; set; } = DateTime.Now;
	}
}
