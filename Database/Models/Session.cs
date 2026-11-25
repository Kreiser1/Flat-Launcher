namespace Database.Models
{
	/*
	CREATE TABLE IF NOT EXISTS sessions (
		refresh_token UUID PRIMARY KEY DEFAULT gen_random_uuid(),
		user_id UUID NOT NULL,
		device_info VARCHAR(128),
		date TIMESTAMPTZ NOT NULL DEFAULT now(),
		CONSTRAINT FK_user_id FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE ON UPDATE CASCADE
	);
	*/

	public class Session
	{
		public required Guid RefreshToken { get; set; }
		public required Guid UserId { get; set; }
		public string? DeviceInfo { get; set; }
		public DateTime Date { get; set; } = DateTime.UtcNow;
	}
}
