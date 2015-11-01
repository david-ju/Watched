using System.Diagnostics;

namespace Watched.Kodi
{
	public class GetMoviesResponse
	{
		public int Id { get; set; }
		public MovieResult Result { get; set; }
	}

	public class MovieResult
	{
		public Movie[] Movies { get; set; }
	}

	[DebuggerDisplay("{Label}")]
	public class Movie
	{
		public int MovieId { get; set; }
		public string Label { get; set; }
		public string ImdbNumber { get; set; }
	}
}
