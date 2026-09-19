namespace Flat_Server.Database.Models {
	class User {
		public required int Id { get; set; }
		public required string Login { get; set; }
		public required string PasswordHash { get; set; }
		public required string Nickname { get; set; }
	}
}
