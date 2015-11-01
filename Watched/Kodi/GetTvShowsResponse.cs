using System.Collections.Generic;
using System.Diagnostics;

namespace Watched.Kodi
{
	public class GetTvShowsResponse
	{
		public int Id { get; set; }
		public TvShowResult Result { get; set; }
	}

	public class TvShowResult
	{
		public TvShow[] TvShows { get; set; }
	}

	[DebuggerDisplay("{Label}")]
	public class TvShow
	{
		public int TvShowId { get; set; }
		public string ImdbNumber { get; set; }
		public string Label { get; set; }
		public IEnumerable<Episode> Episodes { get; set; }
	}
}
