using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Muses_Player_Console
{
    public class Song
    {
        public string SongID { get; set; }
        public string Title { get; set; }
        public string AltTitle { get; set; }
        public int Duration { get; set; }
        public string AudioURL { get; set; }
        public int PlayCount { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Categories { get; set; }
        public string ArtistNames { get; set; }
        public int OrderNo { get; set; }
        
        const int songIdWidth = 15;
        const int titleWidth = 40;
        const int durationWidth = 10;
        const int playCountWidth = 15;
        const int releaseDateWidth = 15;
        const int artistWidth = 30;
        const int categoryWidth = 30;

        public Song()
        {
        }
        public Song (string songID, string title, string altTitle, int duration,
            string audioURL, int playCount, DateTime releaseDate, string categories, string artistNames, int orderNo = 0)
        {
            SongID = songID;
            Title = title;
            AltTitle = altTitle;
            Duration = duration;
            AudioURL = audioURL;
            PlayCount = playCount;
            ReleaseDate = releaseDate;
            Categories = categories;
            ArtistNames = artistNames;
            OrderNo = orderNo;
        }

        public void PrintSong()
        {
            Console.WriteLine(
                $"| {ConsoleTableFormatter.PadRightDisplay(SongID, songIdWidth)}" +
                $"| {ConsoleTableFormatter.PadRightDisplay(Title, titleWidth)}" +
                $"| {ConsoleTableFormatter.PadLeftDisplay(Duration.ToString(), durationWidth)}" +
                $"| {ConsoleTableFormatter.PadLeftDisplay(PlayCount.ToString(), playCountWidth)}" +
                $"| {ConsoleTableFormatter.PadRightDisplay(ReleaseDate.ToShortDateString(), releaseDateWidth)}" +
                $"| {ConsoleTableFormatter.PadRightDisplay(ArtistNames, artistWidth)}" +
                $"| {ConsoleTableFormatter.PadRightDisplay(Categories, categoryWidth)}"
            );
        }

        public void PrintHeader()
        {
            Console.WriteLine(
                $"| {ConsoleTableFormatter.PadRightDisplay("SongID", songIdWidth)}" +
                $"| {ConsoleTableFormatter.PadRightDisplay("Title", titleWidth)}" +
                $"| {ConsoleTableFormatter.PadLeftDisplay("Duration", durationWidth)}" +
                $"| {ConsoleTableFormatter.PadLeftDisplay("PlayCount", playCountWidth)}" + 
                $"| {ConsoleTableFormatter.PadRightDisplay("ReleaseDate", releaseDateWidth)}" +
                $"| {ConsoleTableFormatter.PadRightDisplay("ArtistNames", artistWidth)}" +
                $"| {ConsoleTableFormatter.PadRightDisplay("Categories", categoryWidth)}"
            );
            Console.WriteLine(new string('-', songIdWidth + titleWidth + durationWidth + playCountWidth + releaseDateWidth + artistWidth + categoryWidth));
        }
    }
}