namespace Flat_Server
{
	using System.Security.Claims;

	using Flat_Server.Database;
	using Flat_Server.Database.Models;

	using Microsoft.AspNetCore.Authorization;

	using Schema;

	internal static class API
	{
		private static bool IsAdmin(ClaimsPrincipal user) => user.IsInRole(Roles.Admin);
		private static int? GetUserId(ClaimsPrincipal user)
		{
			var claim = user.FindFirst(ClaimTypes.NameIdentifier);
			return claim is not null && int.TryParse(claim.Value, out int id) ? id : null;
		}

		public static void Initialize(WebApplication app)
		{
			app.MapPost("/api/auth/login", (LoginRequest request) =>
			{
				User? user = Users.Login(request.Login, request.Password);

				if (user is null)
					return Results.Unauthorized();

				string token = Auth.GenerateToken(user);

				return Results.Ok(new LoginResponse(user.Id, user.Login, user.Nickname ?? user.Login, user.Role, token));
			});

			app.MapPost("/api/auth/register", (UserCreateRequest request) =>
			{
				try
				{
					var user = Users.Insert(request.Login, request.Password, Roles.User, request.Nickname);
					var token = Auth.GenerateToken(user);
					return Results.Ok(new LoginResponse(user.Id, user.Login, user.Nickname ?? user.Login, user.Role, token));
				}
				catch (DatabaseException e)
				{
					return Results.Conflict(new ErrorResponse(e.Message));
				}
			});

			app.MapGet("/api/users/get", [Authorize] (ClaimsPrincipal user) =>
			{
				int? userId = GetUserId(user);

				if (userId is null)
					return Results.Unauthorized();

				User dbUser = Users.Select(userId ?? int.MinValue);

				return dbUser is null
				? Results.NotFound(new ErrorResponse("Пользователь не найден."))
				: Results.Ok(new UserResponse(dbUser.Id, dbUser.Nickname ?? dbUser.Login, dbUser.Role));
			});
		}
	}
}
