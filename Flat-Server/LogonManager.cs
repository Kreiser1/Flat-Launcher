namespace Flat_Server
{
	using System.Text;

	using Microsoft.IdentityModel.Tokens;

	using Database.Models;
	using System.Security.Claims;
	using System.IdentityModel.Tokens.Jwt;
	using Flat_Server.Database;
	using System.Security.Cryptography;

	public class LogonManager
	{
		private SymmetricSecurityKey _symmetricKey;

		public LogonManager()
		{
			_symmetricKey = new(RandomNumberGenerator.GetBytes(32));
		}

		public string CreateAccessToken(User user, DateTime expirationTime)
		{
			if (DateTime.UtcNow <= expirationTime)
				throw new ArgumentException("Некорректное время истечения токена доступа.");

			Claim[] claims =
			[
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.Login),
				new Claim(ClaimTypes.Role, user.Role)
			];

			SigningCredentials credentials = new(_symmetricKey, SecurityAlgorithms.HmacSha256);

			JwtSecurityToken token = new(
				issuer: null,
				audience: null,
				claims: claims,
				expires: expirationTime,
				signingCredentials: credentials
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public (int Id, string Login, string Role)? LoginWithAccessToken(string token)
		{
			try
			{
				JwtSecurityTokenHandler tokenHandler = new();
				TokenValidationParameters validationParameters = new()
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = _symmetricKey,
					ValidateIssuer = false,
					ValidateAudience = false,
					ClockSkew = TimeSpan.Zero
				};

				ClaimsPrincipal? principal = tokenHandler.ValidateToken(token, validationParameters, out _);

				if (principal is null)
					return null;

				int userId;

				if (!int.TryParse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId))
					return null;

				string? login = principal.FindFirst(ClaimTypes.Name)?.Value;
				string? role = principal.FindFirst(ClaimTypes.Role)?.Value;

				return login is null || role is null ? null : (userId, login, role);
			}
			catch (SecurityTokenValidationException)
			{
				return null;
			}
		}

		public async Task<string?> LoginWithRefreshTokenAsync(Guid refreshToken, DateTime expirationTime)
		{
			if (DateTime.UtcNow <= expirationTime)
				throw new ArgumentException("Некорректное время истечения токена доступа.");

			int? userId = (await Sessions.SelectAsync(refreshToken))?.Id;

			if (userId is null)
				return null;

			User? user = await Users.SelectAsync((int)userId);

			if (user is null)
				return null;

			return CreateAccessToken(user, expirationTime);
		}

		public async Task<Guid?> LoginWithCredentialsAsync(string login, string password)
		{
			User? user = await Users.LoginAsync(login, password);

			if (user is null)
				return null;

			return (await Sessions.InsertAsync(user.Id)).RefreshToken;
		}
	}
}
