
namespace Muses_Player_Console
{
    public class Artist
    {
        public string? ArtistId { get; set; }
        public string? ArtistName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? UserId { get; set; }
        public List<Song>? MySongs { get; set; }
        
        
        public Artist()
        {
            // Default constructor
        }
        public Artist(string artistId, string artistName, string bio, string avatarUrl, string userId, List<Song>? mySongs)
        {
            ArtistId = artistId;
            ArtistName = artistName;
            Bio = bio;
            AvatarUrl = avatarUrl;
            UserId = userId;
            MySongs = mySongs;
        }
    }
}