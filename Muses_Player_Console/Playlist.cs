
namespace Muses_Player_Console
{
    public class Playlist
    {
        public string? PlaylistId { get; set; }
        public string? PlaylistName { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsFavorite { get; set; }

        public Playlist()
        {
            // Default constructor
        }

        public Playlist(string playlistId, string playlistName, DateTime createdDate, bool isFavorite)
        {
            PlaylistId = playlistId;
            PlaylistName = playlistName;
            CreatedDate = createdDate;
            IsFavorite = isFavorite;
        }
    }
}