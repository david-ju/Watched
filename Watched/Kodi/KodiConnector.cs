using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace Watched.Kodi
{
    public class KodiConnector
    {
	    private readonly string _host;
	    private readonly int _port;
	    private readonly string _username;
	    private readonly string _password;

	    public KodiConnector(string host, int port)
	    {
		    _host = host;
		    _port = port;
	    }

	    public KodiConnector(string host, int port, string username, string password) : this(host, port)
	    {
		    _username = username;
		    _password = password;
	    }

	    public IEnumerable<Movie> GetWatchedMovies()
	    {
		    var url = string.Format("http://{0}:{1}/jsonrpc", _host, _port);
		    var request = JsonConvert.SerializeObject(new
		    {
			    jsonrpc = "2.0",
			    method = "VideoLibrary.GetMovies",
			    id = 1,
			    @params = new
			    {
				    filter = new
				    {
					    field = "playcount",
					    @operator = "isnot",
					    value = "0"
				    },
					properties = new[] { "imdbnumber", "art", "genre", "plot", "title", "originaltitle", "year", "rating", "thumbnail", "playcount", "file", "fanart" }
			    }
		    });

		    using (var wc = new WebClient())
		    {
			    if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
			    {
				    string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(_username + ":" + _password));
					wc.Headers.Add("Authorization", "Basic " + encoded);
			    }
				var response = (GetMoviesResponse)JsonConvert.DeserializeObject(wc.UploadString(url, request), typeof(GetMoviesResponse));
			    return response.Result.Movies;
		    }
	    }

	    public IEnumerable<TvShow> GetWatchedEpisodes()
	    {
			var url = string.Format("http://{0}:{1}/jsonrpc", _host, _port);
			var tvShowRequest = JsonConvert.SerializeObject(new
			{
				jsonrpc = "2.0",
				method = "VideoLibrary.GetTVShows",
				id = 1,
				@params = new
				{
					properties = new[] { "imdbnumber" }
				}
			});

			var episodesRequest = JsonConvert.SerializeObject(new
			{
				jsonrpc = "2.0",
				method = "VideoLibrary.GetEpisodes",
				id = 1,
				@params = new
				{
					filter = new
					{
						field = "playcount",
						@operator = "isnot",
						value = "0"
					},
					properties = new[] { "tvshowid", "episode", "season" }
				}
			});

			using (var wc = new WebClient())
			{
				if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
				{
					string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(_username + ":" + _password));
					wc.Headers.Add("Authorization", "Basic " + encoded);
				}
				var tvShowsResponse = (GetTvShowsResponse)JsonConvert.DeserializeObject(wc.UploadString(url, tvShowRequest), typeof(GetTvShowsResponse));

				var episodesResponse = (GetEpisodesResponse)JsonConvert.DeserializeObject(wc.UploadString(url, episodesRequest), typeof(GetEpisodesResponse));

				foreach (var tvShow in tvShowsResponse.Result.TvShows)
				{
					tvShow.Episodes = episodesResponse.Result.Episodes.Where(e => e.TvShowId == tvShow.TvShowId);
				}

				return tvShowsResponse.Result.TvShows;
			}
	    }

    }
}
