using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Watched.Plex
{
	[DebuggerDisplay("{Title}")]
	public class TvShow
	{
		public TvShow()
		{
			Episodes=new List<Episode>();
		}

		public int RatingKey { get; set; }
		public string Title { get; set; }
		public string ImdbNumber { get; set; }
		public IList<Episode> Episodes { get; set; }
	}

	[DebuggerDisplay("{Title}")]
	public class Episode
	{
		public int RatingKey { get; set; }
		public int EpisodeNumber { get; set; }
		public int Season { get; set; }
		public string Title { get; set; }
		public bool Watched { get; set; }
	}
}
