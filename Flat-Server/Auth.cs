namespace Flat_Server
{
	using System.Text;

	using Microsoft.IdentityModel.Tokens;

	using Database.Models;
	using System.Security.Claims;
	using System.IdentityModel.Tokens.Jwt;

	public static class Auth
	{
		private static SymmetricSecurityKey _securityKey = null!;

		public static void Initialize(string secret)
		{
			if (secret.Length < 32)
				throw new ArgumentException("Длина секрета JWT должна быть хотя бы 32.");

			_securityKey = new(Encoding.ASCII.GetBytes(secret));
		}

		public static string GenerateToken(User user)
		{
			Claim[] claims =
			[
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.Login),
				new Claim(ClaimTypes.Role, user.Role)
			];

			SigningCredentials credentials = new(_securityKey, SecurityAlgorithms.HmacSha256);

			JwtSecurityToken token = new(
				issuer: null,
				audience: null,
				claims: claims,
				expires: DateTime.UtcNow.AddDays(7),
				signingCredentials: credentials
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public static ClaimsPrincipal? ValidateToken(string token)
		{
			try
			{
				JwtSecurityTokenHandler tokenHandler = new();
				TokenValidationParameters validationParameters = new()
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = _securityKey,
					ValidateIssuer = false,
					ValidateAudience = false,
					ClockSkew = TimeSpan.Zero
				};

				ClaimsPrincipal? principal = tokenHandler.ValidateToken(token, validationParameters, out _);
				return principal;
			}
			catch
			{
				return null;
			}
		}

		public static int? GetUserId(string token)
		{
			ClaimsPrincipal? principal = ValidateToken(token);
			Claim? userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier);

			return userIdClaim is not null && int.TryParse(userIdClaim.Value, out int userId) ? userId : null;
		}
	}
}
