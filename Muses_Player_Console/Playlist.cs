using System;

namespace Muses_Player_Console
{
    public class Playlist
    {
        public string PlaylistID { get; set; }
        public string PlaylistName { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsFavorite { get; set; }
        public string UserID { get; set; }
        
        const int playlistIdWidth = 15;
        const int playlistNameWidth = 30;
        const int createdDateWidth = 20;
        const int isFavoriteWidth = 10;

        public Playlist()
        {
            // Default constructor
        }

        public Playlist(string playlistID, string playlistName, DateTime createdDate, bool isFavorite, string userID)
        {
            PlaylistID = playlistID;
            PlaylistName = playlistName;
            CreatedDate = createdDate;
            IsFavorite = isFavorite;
            UserID = userID;
        }

        public void PrintPlaylist()
        {
            Console.WriteLine(
                $"{ConsoleTableFormatter.PadRightDisplay(PlaylistID, playlistIdWidth)}" +
                $"{ConsoleTableFormatter.PadRightDisplay(PlaylistName, playlistNameWidth)}" +
                $"{ConsoleTableFormatter.PadRightDisplay(CreatedDate.ToShortDateString(), createdDateWidth)}" +
                $"{ConsoleTableFormatter.PadRightDisplay(IsFavorite ? "Yes" : "No", isFavoriteWidth)}"
            );
        }
        
        public void PrintHeader()
        {
            Console.WriteLine(
                $"{ConsoleTableFormatter.PadRightDisplay("PlaylistID", playlistIdWidth)}" +
                $"{ConsoleTableFormatter.PadRightDisplay("PlaylistName", playlistNameWidth)}" +
                $"{ConsoleTableFormatter.PadRightDisplay("CreatedDate", createdDateWidth)}" +
                $"{ConsoleTableFormatter.PadRightDisplay("IsFavorite", isFavoriteWidth)}"
            );
        
            Console.WriteLine(new string('-', playlistIdWidth + playlistNameWidth + createdDateWidth + isFavoriteWidth));
        }
    }
}