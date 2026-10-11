namespace Flat_Server
{
	namespace Schema
	{
		public record LoginRequest(string Login, string Password);
		public record LoginResponse(int Id, string Login, string Nickname, string Role, string Token);
		public record UserResponse(int Id, string Nickname, string Role);
		public record UserCreateRequest(string Login, string Password, string Role, string? Nickname = null);
		public record UserUpdateRequest(string? Login = null, string? NewPassword = null, string? OldPassword = null, string? Role = null, string? NewNickName = null);
		public record UserLoginRequest(string Login, string Password);
		public record ErrorResponse(string Message);
	}
}
