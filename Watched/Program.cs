using System;
using System.Linq;
using Watched.Kodi;
using Watched.Plex;

namespace Watched
{
	class Program
	{
		static void Main()
		{
			Console.WriteLine("Plex username:");
			string plexUsername = Console.ReadLine();
			Console.WriteLine("Plex password:");
			string plexPassword = Console.ReadLine();

			Console.WriteLine("Kodi host name:");
			string kodiHost = Console.ReadLine();
			Console.WriteLine("Kodi port:");
			int kodiPort = int.Parse(Console.ReadLine());
			Console.WriteLine("Kodi username:");
			string kodiUserName = Console.ReadLine();
			Console.WriteLine("Kodi password");
			string kodiPassword = Console.ReadLine();

			Console.Write("Connecting to Plex...");
			var plexConnector = new PlexConnector(plexUsername, plexPassword);
			plexConnector.SignIn();
			Console.WriteLine(" Connected");
			
			var kodiConnector = new KodiConnector(kodiHost, kodiPort, kodiUserName, kodiPassword);

			Console.Write("Getting watched episodes from Kodi...");
			var watchedTvShows = kodiConnector.GetWatchedEpisodes();
			Console.WriteLine(" Got {0} watched episodes in {1} tv shows", watchedTvShows.SelectMany(x => x.Episodes).Count(), watchedTvShows.Count());

			Console.Write("Getting tvshows from Plex...");
			var plexTvShows = plexConnector.GetTvShows();
			Console.WriteLine(" Got {0} tv shows", plexTvShows.Count());

			Console.WriteLine("Checking for matches...");
			foreach (var plexTvShow in plexTvShows)
			{

				Console.WriteLine("Checking {0}", plexTvShow.Title);
				var kodiTvShow = watchedTvShows.FirstOrDefault(t => t.ImdbNumber == plexTvShow.ImdbNumber);
				if (kodiTvShow == null)
				{
					Console.WriteLine("No match found for {0}", plexTvShow.Title);
					continue;
				}

				foreach (var plexEpisode in plexTvShow.Episodes)
				{
					Console.Write("\tChecking {0}-{1} {2}", plexEpisode.Season, plexEpisode.EpisodeNumber, plexEpisode.Title);
					if (plexEpisode.Watched)
					{
						Console.WriteLine(" Already marked as watched");
						continue;
					}

					var watched = kodiTvShow.Episodes.Any(x => x.Season == plexEpisode.Season && x.EpisodeNumber == plexEpisode.EpisodeNumber);
					if (watched)
					{
						Console.Write("\t\tMarking {0}-{1} {2} as watched... ", plexEpisode.Season, plexEpisode.EpisodeNumber, plexEpisode.Title);
						plexConnector.MarkAsWatched(plexEpisode.RatingKey);
					}
					Console.WriteLine(" Done");
				}
			}


			Console.Write("Getting watched movies from Kodi...");
			var watchedMovies = kodiConnector.GetWatchedMovies();
			Console.WriteLine(" Got {0} watched movies", watchedMovies.Count());


			Console.Write("Getting movies from Plex...");
			var plexMovies = plexConnector.GetMovies();
			Console.WriteLine(" Got {0} movies", plexMovies.Count());

			Console.WriteLine("Checking for matches...");
			foreach (var plexMovie in plexMovies)
			{
				Console.Write("Checking {0}", plexMovie.Title);
				if (plexMovie.Watched)
				{
					Console.WriteLine(" Already marked as watched");
					continue;
				}
				
				var watched = watchedMovies.Any(x => x.ImdbNumber == plexMovie.ImdbNumber);
				if (watched)
				{
					Console.Write("\t\tMarking {0} as watched... ", plexMovie.Title);
					plexConnector.MarkAsWatched(plexMovie.RatingKey);
					Console.WriteLine(" Done");
				}
			}

			Console.WriteLine("Press any key to quit");
			Console.ReadKey();
		}
	}
}
