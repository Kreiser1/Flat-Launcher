namespace Flat_Client
{
	using System.Configuration;
	using System.Data;
	using System.Net.Http;
	using System.Windows;

	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static HttpClient http = new();
	}

}
