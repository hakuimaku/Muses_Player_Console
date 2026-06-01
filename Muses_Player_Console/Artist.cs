using System;
using System.Collections.Generic;
using System.Text;

namespace Muses_Player_Console
{
    public class Artist
    {
        public string ArtistID { get; set; }
        public string ArtistName { get; set; }
        public string Bio { get; set; }
        public string AvatarURL { get; set; }
        public string UserID { get; set; }
        public List<Song> MySongs { get; set; }
        
        const int artistIdWidth = 15;
        const int artistNameWidth = 30;
        const int bioWidth = 40;
        
        public Artist()
        {
            // Default constructor
        }
        public Artist(string artistID, string artistName, string bio, string avatarURL, string userID, List<Song> mySongs = null)
        {
            ArtistID = artistID;
            ArtistName = artistName;
            Bio = bio;
            AvatarURL = avatarURL;
            UserID = userID;
            MySongs = mySongs;
        }

        public void PrintArtist()
        {
            Console.WriteLine(
                $"{ConsoleTableFormatter.PadRightDisplay(ArtistID, artistIdWidth)}" +
                $"{ConsoleTableFormatter.PadRightDisplay(ArtistName, artistNameWidth)}" +
                $"{ConsoleTableFormatter.PadRightDisplay(Bio, bioWidth)}"
            );
        }

        public void PrintHeader()
        {
            // Header
            Console.WriteLine(
                $"{ConsoleTableFormatter.PadRightDisplay("ArtistID", artistIdWidth)}" +
                $"{ConsoleTableFormatter.PadRightDisplay("ArtistName", artistNameWidth)}" +
                $"{ConsoleTableFormatter.PadRightDisplay("Bio", bioWidth)}"
            );

            Console.WriteLine(new string('-', artistIdWidth + artistNameWidth + bioWidth));
        }
    }
}