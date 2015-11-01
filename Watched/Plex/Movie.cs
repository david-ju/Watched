using System.Diagnostics;

namespace Watched.Plex
{
	[DebuggerDisplay("{Title}")]
	public class Movie
	{
		public int RatingKey { get; set; }
		public string Title { get; set; }
		public string ImdbNumber { get; set; }
		public bool Watched { get; set; }
	}
}
