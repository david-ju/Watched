using System.Diagnostics;
using Newtonsoft.Json;

namespace Watched.Kodi
{
	public class GetEpisodesResponse
	{
		public int Id { get; set; }
		public EpisodesResult Result { get; set; }
	}

	public class EpisodesResult
	{
		public Episode[] Episodes { get; set; }
	}

	[DebuggerDisplay("{Label}")]
	public class Episode
	{
		public int EpisodeId { get; set; }
		[JsonProperty("Episode")]
		public int EpisodeNumber { get; set; }
		public int Season { get; set; }
		public string Label { get; set; }
		public int TvShowId { get; set; }
	}
}
