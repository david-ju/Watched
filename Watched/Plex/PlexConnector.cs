using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Watched.Plex
{
    public class PlexConnector
    {
	    private const string _baseUrl = "https://plex.tv/";
	    private const string _pmsUrl = _baseUrl + "pms/";

	    private readonly string _username;
	    private readonly string _password;
	    private string _authenticationToken;

	    public PlexConnector(string username, string password)
	    {
		    _username = username;
		    _password = password;
	    }

	    public void SignIn()
	    {
		    using (var wc = new WebClient())
			{
				AddPlexHeaders(wc);
				
				var authBytes = Encoding.UTF8.GetBytes(_username + ":" + _password);
				wc.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(authBytes));
			    var response = wc.UploadString(_baseUrl + "users/sign_in.xml", "");
			    var xdoc = XDocument.Parse(response);
				_authenticationToken = xdoc.Descendants("authentication-token").First().Value;
		    }
	    }

	    public IEnumerable<Movie> GetMovies()
	    {
		    using (var wc = new WebClient())
			{
				AddPlexHeaders(wc);

			    var response = wc.DownloadString(_pmsUrl + "servers.xml");
			    var xdoc = XDocument.Parse(response);
			    var serverNode = xdoc.Descendants("Server").First();
			    var serverIp = serverNode.Attribute("address").Value;
			    var serverPort = serverNode.Attribute("port").Value;

			    response = wc.DownloadString(string.Format("http://{0}:{1}/library/sections", serverIp, serverPort));
			    
			    xdoc = XDocument.Parse(response);
				var moviesNode = xdoc.Descendants("Directory").FirstOrDefault(x => x.Attribute("type").Value == "movie");
			    if (moviesNode == null)
				    return new Movie[0];

			    var sectionId = moviesNode.Attribute("key").Value;
			    response =
				    wc.DownloadString(string.Format("http://{0}:{1}/library/sections/{2}/all", serverIp, serverPort, sectionId));
			    xdoc = XDocument.Parse(response);
			    var movies = xdoc.Descendants("Video").Select(m => new Movie
			    {
				    RatingKey = Convert.ToInt32(m.Attribute("ratingKey").Value),
				    Title = m.Attribute("title").Value
			    }).ToList();

			    foreach (var movie in movies)
			    {
				    GetMovieDetails(movie);
			    }
			    return movies;
		    }
	    }

	    private void GetMovieDetails(Movie movie)
	    {
		    using (var wc = new WebClient())
			{
				AddPlexHeaders(wc);

				var response = wc.DownloadString(_pmsUrl + "servers.xml");
				var xdoc = XDocument.Parse(response);
				var serverNode = xdoc.Descendants("Server").First();
				var serverIp = serverNode.Attribute("address").Value;
				var serverPort = serverNode.Attribute("port").Value;

				response = wc.DownloadString(string.Format("http://{0}:{1}/library/metadata/{2}", serverIp, serverPort, movie.RatingKey));
				xdoc = XDocument.Parse(response);
			    var guid = xdoc.Descendants("Video").First().Attribute("guid").Value;
				var imdbNumber = Regex.Match(guid, @"imdb:\/\/(.*)\?").Groups[1].Value;
			    movie.ImdbNumber = imdbNumber;
			    var viewCountAttribute = xdoc.Descendants("Video").First().Attribute("viewCount");
			    if (viewCountAttribute != null)
			    {
				    int viewCount;
				    if (int.TryParse(viewCountAttribute.Value, out viewCount))
				    {
						if (viewCount > 0)
							movie.Watched = true;
				    }
			    }
		    }
	    }

	    public void MarkAsWatched(int key)
	    {
		    using (var wc = new WebClient())
			{
				AddPlexHeaders(wc);

			    var response = wc.DownloadString(_pmsUrl + "servers.xml");
			    var xdoc = XDocument.Parse(response);
			    var serverNode = xdoc.Descendants("Server").First();
			    var serverIp = serverNode.Attribute("address").Value;
			    var serverPort = serverNode.Attribute("port").Value;

			    var wr =
				    (HttpWebRequest)
					    WebRequest.Create(string.Format("http://{0}:{1}/:/scrobble?key={2}&identifier=com.plexapp.plugins.library",
						    serverIp, serverPort, key));
			    wr.Method = "OPTIONS";
			    foreach (var plexHeader in CreatePlexHeaders())
			    {
				    wr.Headers.Add(plexHeader.Key, plexHeader.Value);
			    }
			    var webResponse = wr.GetResponse();
				
		    }
	    }

	    public IEnumerable<TvShow> GetTvShows()
	    {
			using (var wc = new WebClient())
			{
				AddPlexHeaders(wc);

				var response = wc.DownloadString(_pmsUrl + "servers.xml");
				var xdoc = XDocument.Parse(response);
				var serverNode = xdoc.Descendants("Server").First();
				var serverIp = serverNode.Attribute("address").Value;
				var serverPort = serverNode.Attribute("port").Value;

				response = wc.DownloadString(string.Format("http://{0}:{1}/library/sections", serverIp, serverPort));
				
				xdoc = XDocument.Parse(response);
				var seriesNode = xdoc.Descendants("Directory").FirstOrDefault(x => x.Attribute("type").Value == "show");
				if (seriesNode == null)
					return new TvShow[0];

				var sectionId = seriesNode.Attribute("key").Value;
				response =
					wc.DownloadString(string.Format("http://{0}:{1}/library/sections/{2}/all", serverIp, serverPort, sectionId));
				xdoc = XDocument.Parse(response);
				var tvShows = xdoc.Descendants("Directory").Select(m => new TvShow
				{
					RatingKey = Convert.ToInt32(m.Attribute("ratingKey").Value),
					Title = m.Attribute("title").Value
				}).ToList();

				foreach (var tvShow in tvShows)
				{
					response = wc.DownloadString(string.Format("http://{0}:{1}/library/metadata/{2}", serverIp, serverPort, tvShow.RatingKey));
					xdoc = XDocument.Parse(response);
					var guid = xdoc.Descendants("Directory").First().Attribute("guid").Value;
					var imdbNumber = Regex.Match(guid, @"thetvdb:\/\/(.*)\?").Groups[1].Value;
					tvShow.ImdbNumber = imdbNumber;

					response = wc.DownloadString(string.Format("http://{0}:{1}/library/metadata/{2}/allLeaves", serverIp, serverPort, tvShow.RatingKey));
					xdoc = XDocument.Parse(response);

					foreach (var episodeNode in xdoc.Descendants("Video"))
					{
						tvShow.Episodes.Add(new Episode
						{
							RatingKey = int.Parse(episodeNode.Attribute("ratingKey").Value),
							EpisodeNumber = int.Parse(episodeNode.Attribute("index").Value),
							Season = int.Parse(episodeNode.Attribute("parentIndex").Value),
							Title = episodeNode.Attribute("title").Value,
							Watched = episodeNode.Attribute("viewCount") != null && int.Parse(episodeNode.Attribute("viewCount").Value) > 0
						});
					}
				}

				return tvShows;
			}
	    }

	    private void AddPlexHeaders(WebClient wc)
	    {
		    foreach (var plexHeader in CreatePlexHeaders())
		    {
			    wc.Headers.Add(plexHeader.Key, plexHeader.Value);
		    }
	    }

	    private IDictionary<string, string> CreatePlexHeaders()
	    {
		    var headers = new Dictionary<string, string>
		    {
			    {"X-Plex-Platform", "Windows"},
			    {"X-Plex-Platform-Version", "NT"},
			    {"X-Plex-Provides", "player"},
			    {"X-Plex-Client-Identifier", "1CFB04B5-5A5E-43D8-A53E-B38BD040EAD1"},
			    {"X-Plex-Device", "KodiToPlex"},
			    {"X-Plex-Product", "KodiToPlex"},
			    {"X-Plex-Version", "0.1"}
		    };

		    if (_authenticationToken != null)
		    {
				headers.Add("X-Plex-Token", _authenticationToken);
		    }

		    return headers;
	    }
    }
}
