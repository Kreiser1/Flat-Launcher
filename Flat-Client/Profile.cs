namespace Flat_Client
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	public static class Profile
	{
		public static int? Id { get; private set; }
		public static string? Nickname { get; private set; }
		public static string? Role { get; set; }

		public static async Task Load() {
			(Id, Nickname, Role) = (null, null, null);

			try {

			} catch {

			}
		}
	}
}
