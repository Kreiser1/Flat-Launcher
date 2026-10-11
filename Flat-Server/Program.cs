using System.IdentityModel.Tokens.Jwt;
using System.Text;

using Flat_Server;
using Flat_Server.Database;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

internal class Program {
	private static void Main(string[] args) {
		var builder = WebApplication.CreateBuilder(args);

		string secret = builder.Configuration["Auth:Secret"]
			?? throw new InvalidOperationException("Секрет JWT не предоставлен.");

		byte[] key = Encoding.ASCII.GetBytes(secret);

		builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateIssuer = false,
				ValidateAudience = false,
				ClockSkew = TimeSpan.Zero
			};

			options.Events = new JwtBearerEvents
			{
				OnMessageReceived = context => {
					string? token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
					context.Token = token;
					return Task.CompletedTask;
				}
			};
		});

		builder.Services.AddAuthorization();
		builder.Services.AddCors(options => options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

		var app = builder.Build();

		Database.Connect(app.Configuration.GetConnectionString("Default") ?? "");
		Auth.Initialize(secret);

		API.Initialize(app);

		app.UseCors("AllowAll");
		app.UseAuthentication();
		app.UseAuthorization();
		app.Run();
	}
}