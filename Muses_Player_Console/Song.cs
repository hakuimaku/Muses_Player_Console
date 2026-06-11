
namespace Muses_Player_Console
{
    public class Song
    {
        public string? SongId { get; set; }
        public string? Title { get; set; }
        public string? AltTitle { get; set; }
        public int Duration { get; set; }
        public string? AudioUrl { get; set; }
        public int PlayCount { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string? Categories { get; set; }
        public string? ArtistNames { get; set; }
        public int OrderNo { get; set; }

        public Song()
        {
        }

        public Song(string songId, string title, string altTitle, int duration,
            string audioUrl, int playCount, DateTime releaseDate, string categories, string artistNames,
            int orderNo = 0)
        {
            SongId = songId;
            Title = title;
            AltTitle = altTitle;
            Duration = duration;
            AudioUrl = audioUrl;
            PlayCount = playCount;
            ReleaseDate = releaseDate;
            Categories = categories;
            ArtistNames = artistNames;
            OrderNo = orderNo;
        }
    }
}