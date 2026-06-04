using Microsoft.Data.SqlClient;
using LibVLCSharp.Shared;


namespace Muses_Player_Console;

public class MusesService
{
    public Queue<Song> PlayQueue = new Queue<Song>();
    public Playlist? CurrentPlaylist;

    private LibVLC _libVlc;
    private MediaPlayer _mediaPlayer;
    
    private string _connectionStringGuest = "Server=localhost,1433;Database=Muses_DB;User Id=login_guest;Password=GuestPass@123;TrustServerCertificate=True;Encrypt=False;";
    private string _connectionStringUser = "Server=localhost,1433;Database=Muses_DB;User Id=login_user;Password=UserPass@123;TrustServerCertificate=True;Encrypt=False;";
    private string _connectionStringArtist = "Server=localhost,1433;Database=Muses_DB;User Id=login_artist;Password=ArtistPass@123;TrustServerCertificate=True;Encrypt=False;";
    private string _connectionStringAdmin = "Server=localhost,1433;Database=Muses_DB;User Id=login_admin;Password=AdminPass@123;TrustServerCertificate=True;Encrypt=False;";

        
    public string ConnectionString { get; private set; }
    public User User = new User();
    public Artist Artist = new Artist();
    
    public bool IsLoggedIn;
    private bool IsAdmin;
    
    public List<Playlist> Playlists = new List<Playlist>();
    public List<Song> Songs = new List<Song>();
    public List<Artist> Artists = new List<Artist>();
    public List<Category> Categories = new List<Category>();
    
    public MusesService()
    {
        ConnectionString = _connectionStringGuest; // default to guest connection
        if (OperatingSystem.IsWindows())
        {
            Core.Initialize(@"C:\Program Files\VideoLAN\VLC"); // For Window
        }
        else
        {
            Core.Initialize(); // For Linux
        }
        _libVlc = new LibVLC();
        _mediaPlayer = new MediaPlayer(_libVlc);
    }
    
    public bool Login(string username, string password)
    {
        SqlConnection conn = new SqlConnection(ConnectionString);

        try
        {
            conn.Open();
            string query = "EXEC dbo.sp_Auth_GetUserByLogin @Login = @username";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);
            
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                string dbUserId = reader["UserID"].ToString() ?? string.Empty;
                string dbUsername = reader["Username"].ToString() ?? string.Empty;
                string dbEmail = reader["Email"].ToString() ?? string.Empty;
                string dbPassword = reader["Password"].ToString() ?? string.Empty;
                
                if (dbPassword == password)
                {
                    User.UserID = dbUserId;
                    User.Username = dbUsername;
                    User.Email = dbEmail;
                    User.Password = dbPassword;
                    
                    reader.Close();
                    conn.Close();

                    ConnectionString = _connectionStringUser; // switch to user connection
                    IsLoggedIn = true;
                    
                    Console.WriteLine("Login successful");
                    Console.WriteLine($"User {dbUsername} logged in successfully");

                    if (User.UserID == "USR0000001")
                    {
                        IsAdmin = true;
                    }
                    
                    return true;
                }
                else
                {
                    Console.WriteLine("Invalid password");
                    reader.Close();
                    return false;
                }
            }
            else
            {
                Console.WriteLine("User not found");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error during login: " + ex.Message);
            return false;
        }
        finally
        {
            conn.Close();
        }
    }
    public void IncrementPlayCount(string songId)
    {
        SqlConnection conn = new SqlConnection(ConnectionString);
        
        try
        {
            conn.Open();
            string query = "EXEC dbo.sp_IncrementSongPlayCount @SongID = @SongID";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@SongID", songId);
            
            cmd.ExecuteNonQuery();
            conn.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error incrementing play count: " + ex.Message);
        }
    }

    // Get elements from database
    public bool GetPlaylists()
    {
        Playlists.Clear();
        
        SqlConnection conn = new SqlConnection(ConnectionString);
        try
        {
            conn.Open();
            string query = "SELECT PlaylistID, PlaylistName, CreatedDate, IsFavorite, UserID FROM Playlists WHERE UserID = @UserID";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserID", User.UserID);
            
            SqlDataReader reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                string dbPlaylistId = reader["PlaylistID"].ToString() ?? string.Empty;
                string dbPlaylistName = reader["PlaylistName"].ToString() ?? string.Empty;
                DateTime dbCreatedDate = (DateTime)reader["CreatedDate"];
                bool dbIsFavorite = (bool)reader["IsFavorite"];
                string dbUserId = reader["UserID"].ToString() ?? string.Empty;
                
                Playlist playlist = new Playlist(dbPlaylistId, dbPlaylistName, dbCreatedDate, dbIsFavorite, dbUserId);
                Playlists.Add(playlist);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching playlists: " + ex.Message);
            return false;
        }
        finally
        {
            conn.Close();
        }
    }
    public bool GetAllSongs()
    {
        Songs.Clear();
        SqlConnection conn = new SqlConnection(ConnectionString);

        try
        {
            conn.Open();
            
            string query = "EXEC dbo.sp_GetAllSongs";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.ExecuteNonQuery();

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string dbSongId = reader["SongID"].ToString() ?? string.Empty;
                string dbTitle = reader["Title"].ToString() ?? string.Empty;
                string dbAltTitle = reader["AltTitle"].ToString() ?? string.Empty;
                int dbDuration = (int)reader["Duration"];
                DateTime dbReleaseDate = (DateTime)reader["ReleaseDate"];
                int dbPlayCount = (int)reader["PlayCount"];
                string dbArtistNames = reader["ArtistNames"].ToString() ?? string.Empty;
                string dbCategories = reader["CategoryNames"].ToString() ?? string.Empty;
                
                Song s = new Song(dbSongId, dbTitle, dbAltTitle, dbDuration, string.Empty, dbPlayCount, dbReleaseDate, dbCategories, dbArtistNames);
                Songs.Add(s);
            }
            
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error fetching songs: " + e.Message);
            return false;
        }
        finally
        {
            conn.Close();
        }
    }
    public bool GetAllArtists()
    {
        SqlConnection conn = new SqlConnection(ConnectionString);
        
        try
        {
            conn.Open();
            string query = "SELECT ArtistID, ArtistName, Bio, AvatarURL, UserID FROM Artists";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.ExecuteNonQuery();
            
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string dbArtistId = reader["ArtistID"].ToString() ?? string.Empty;
                string dbArtistName = reader["ArtistName"].ToString() ?? string.Empty;
                string dbBio = reader["Bio"].ToString() ?? string.Empty;
                string dbAvatarUrl = reader["AvatarURL"].ToString() ?? string.Empty;
                string dbUserId = reader["UserID"].ToString() ?? string.Empty;
                
                Artist a = new Artist(dbArtistId, dbArtistName, dbBio, dbAvatarUrl, dbUserId);
                Artists.Add(a);
            }
            
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error fetching artists: " + e.Message);
            return false;
        }
        finally
        {
            conn.Close();
        }
    }
    public bool GetArtistSongs(string artistId)
    {
        Artist.MySongs = new List<Song>();
        
        SqlConnection conn = new SqlConnection(ConnectionString);
        try
        {
            conn.Open();
            string query = "SELECT * FROM dbo.fn_GetSongsByArtistID(@ArtistID);";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ArtistID", artistId);
            
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())            {
                string dbSongId = reader["SongID"].ToString() ?? string.Empty;
                string dbTitle = reader["Title"].ToString() ?? string.Empty;
                string dbAltTitle = reader["AltTitle"].ToString() ?? string.Empty;
                int dbDuration = (int)reader["Duration"];
                DateTime dbReleaseDate = (DateTime)reader["ReleaseDate"];
                int dbPlayCount = (int)reader["PlayCount"];
                string dbArtistNames = reader["ArtistNames"].ToString() ?? string.Empty;
                string dbCategories = reader["CategoryNames"].ToString() ?? string.Empty;
                string dbAudioUrl = reader["AudioURL"].ToString() ?? string.Empty;
                
                Song s = new Song(dbSongId, dbTitle, dbAltTitle, dbDuration, dbAudioUrl, dbPlayCount, dbReleaseDate, dbCategories, dbArtistNames);
                Artist.MySongs.Add(s);
            }
            conn.Close();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error fetching artist songs: " + e.Message);
            return false;
        }
        finally
        {
            conn.Close();
        }
    }
    public bool GetSongsInPlaylist(string playlistId)
    {
        PlayQueue.Clear();

        SqlConnection conn = new SqlConnection(ConnectionString);
        try
        {
            conn.Open();

            string query = "EXEC dbo.sp_GetSongsByPlaylistID @PlaylistID";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@PlaylistID", playlistId);

            SqlDataReader reader = cmd.ExecuteReader();

            var songsInPlaylist = new List<Song>();

            while (reader.Read())
            {
                string dbSongId = reader["SongID"].ToString() ?? string.Empty;
                string dbTitle = reader["Title"].ToString() ?? string.Empty;
                string dbAltTitle = reader["AltTitle"].ToString() ?? string.Empty;
                int dbDuration = reader["Duration"] != DBNull.Value ? Convert.ToInt32(reader["Duration"]) : 0;
                DateTime dbReleaseDate = reader["ReleaseDate"] != DBNull.Value
                    ? Convert.ToDateTime(reader["ReleaseDate"])
                    : DateTime.MinValue;
                int dbPlayCount = reader["PlayCount"] != DBNull.Value ? Convert.ToInt32(reader["PlayCount"]) : 0;
                int dbOrderNo = reader["OrderNo"] != DBNull.Value ? Convert.ToInt32(reader["OrderNo"]) : 0;
                string dbArtistNames = reader["ArtistNames"].ToString() ?? string.Empty;
                string dbCategories = reader["CategoryNames"].ToString() ?? string.Empty;
                string dbAudioUrl = reader["AudioURL"].ToString() ?? string.Empty;

                Song s = new Song(
                    dbSongId,
                    dbTitle,
                    dbAltTitle,
                    dbDuration,
                    dbAudioUrl,
                    dbPlayCount,
                    dbReleaseDate,
                    dbCategories,
                    dbArtistNames,
                    dbOrderNo
                );

                songsInPlaylist.Add(s);
            }

            // đảm bảo add vào queue theo OrderNo
            foreach (var song in songsInPlaylist.OrderBy(s => s.OrderNo))
            {
                PlayQueue.Enqueue(song);
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error loading playlist songs: " + e.Message);
            return false;
        }
        finally
        {
            conn.Close();
        }
    }
    public bool GetAllCategories()
    {
        SqlConnection conn = new SqlConnection(ConnectionString);
        try
        {
            conn.Open();
            string query = "SELECT * FROM Categories";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.ExecuteNonQuery();
            
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())            {
                string dbCategoryId = reader["CategoryID"].ToString() ?? string.Empty;
                string dbCategoryName = reader["CategoryName"].ToString() ?? string.Empty;
                
                Category c = new Category(dbCategoryId, dbCategoryName);
                Categories.Add(c);
            }
            
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error fetching categories: " + e.Message);
            return false;
        }
        finally
        {
            conn.Close();
        }
    }
    public List<Song> FindSong(string title)
    {
        List<Song> foundSongs = new List<Song>();

        using SqlConnection connection = new SqlConnection(ConnectionString);
        string query = "SELECT * FROM dbo.fn_SearchSongsByTitle(@Keyword);";
        using SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Keyword", title);

        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            string dbSongId = reader["SongID"].ToString() ?? string.Empty;
            string dbTitle = reader["Title"].ToString() ?? string.Empty;
            string dbAltTitle = reader["AltTitle"].ToString() ?? string.Empty;
            int dbDuration = (int)reader["Duration"];
            DateTime dbReleaseDate = (DateTime)reader["ReleaseDate"];
            int dbPlayCount = (int)reader["PlayCount"];
            string dbArtistNames = reader["ArtistNames"].ToString() ?? string.Empty;
            string dbCategories = reader["CategoryNames"].ToString() ?? string.Empty;
            
            Song foundSong = new Song(dbSongId, dbTitle, dbAltTitle, dbDuration, string.Empty, dbPlayCount, dbReleaseDate, dbCategories, dbArtistNames);
            foundSongs.Add(foundSong);
        }
        connection.Close();
        return foundSongs;
    }

    
    // Print elements to console
    public void PrintUserInfo()
    {
        Console.WriteLine($"UserID: {User.UserID}");
        Console.WriteLine($"Username: {User.Username}");
        Console.WriteLine($"Email: {User.Email}");
    }
    public void PrintArtistInfo()
    {
        Console.WriteLine($"ArtistID: {Artist.ArtistID}");
        Console.WriteLine($"ArtistName: {Artist.ArtistName}");
        Console.WriteLine($"Bio: {Artist.Bio}");
    }
    public void PrintPlaylists()
    {
        new Playlist().PrintHeader();
        foreach (Playlist playlist in Playlists)
        {
            playlist.PrintPlaylist();
        }
    }
    public void PrintAllArtists()
    {
        new Artist().PrintHeader();
        foreach (Artist artist in Artists)
        {
            artist.PrintArtist();
        }
    }
    public void PrintAllSongs()
    {
        new Song().PrintHeader();
        foreach (Song song in Songs)
        {
            song.PrintSong();
        }
    }
    public void PrintAllCategories()
    {
        new Category().PrintHeader();
        foreach (Category category in Categories)
        {
            category.PrintCategory();
        }
    }
    public void PrintPlaylistSongs()
    {
        if (PlayQueue.Count == 0)
        {
            Console.WriteLine("Queue is empty.");
            return;
        }

        new Song().PrintHeader();

        int idx = 1;
        foreach (var song in PlayQueue)
        {
            song.PrintSong();
            idx++;
        }
    }
    public void PrintArtistSongs()
    {
        new Song().PrintHeader();
        foreach (Song song in Artist.MySongs)
        {
            song.PrintSong();
        }
    }
    
    // User Interaction
    private void PlayQueueInteractive()
    {
        // true: chỉ tính thời gian khi đang phát (không tính pause)
        // false: tính theo thời gian thực kể từ lúc start
        bool countOnlyWhilePlaying = false;

        while (PlayQueue.Count > 0)
        {
            var song = PlayQueue.Dequeue();

            if (string.IsNullOrWhiteSpace(song.AudioURL))
            {
                Console.WriteLine($"No AudioURL for song \"{song.Title}\". Skipping.");
                continue;
            }

            using var media = new Media(_libVlc, song.AudioURL, FromType.FromLocation);
            _mediaPlayer.Play(media);

            Console.WriteLine($"Now playing: {song.Title} — {song.ArtistNames}");
            Console.WriteLine("Controls: p = pause/resume, n = next, q = quit");

            bool ended = false;
            void EndHandler(object? s, EventArgs e) => ended = true;
            _mediaPlayer.EndReached += EndHandler;

            bool playCountIncremented = false;

            // 2 chế độ tính 10 giây
            DateTime startedAt = DateTime.UtcNow;
            DateTime lastTick = startedAt;
            TimeSpan playedTime = TimeSpan.Zero;

            while (!ended)
            {
                if (!playCountIncremented)
                {
                    if (countOnlyWhilePlaying)
                    {
                        var now = DateTime.UtcNow;
                        if (_mediaPlayer.IsPlaying)
                        {
                            playedTime += (now - lastTick);
                        }
                        lastTick = now;

                        if (playedTime.TotalSeconds >= 10)
                        {
                            IncrementPlayCount(song.SongID);
                            playCountIncremented = true;
                        }
                    }
                    else
                    {
                        if ((DateTime.UtcNow - startedAt).TotalSeconds >= 10)
                        {
                            IncrementPlayCount(song.SongID);
                            playCountIncremented = true;
                        }
                    }
                }

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).KeyChar;
                    if (key == 'p')
                    {
                        if (_mediaPlayer.IsPlaying) _mediaPlayer.Pause();
                        else _mediaPlayer.Play();
                    }
                    else if (key == 'n')
                    {
                        _mediaPlayer.Stop();
                        break;
                    }
                    else if (key == 'q')
                    {
                        _mediaPlayer.Stop();
                        PlayQueue.Clear();
                        _mediaPlayer.EndReached -= EndHandler;
                        return;
                    }
                }

                Thread.Sleep(200);
            }

            _mediaPlayer.EndReached -= EndHandler;
        }

        Console.WriteLine("Playback queue finished.");
    }
    public void SelectPlaylistToPlay()
    {
        if (Playlists.Count == 0)
        {
            Console.WriteLine("No playlists loaded. Call GetPlaylists(...) first or load playlists from DB.");
            return;
        }

        Console.WriteLine("Available playlists:");
        for (int i = 0; i < Playlists.Count; i++)
        {
            var pl = Playlists[i];
            Console.WriteLine($"{i + 1}. {ConsoleTableFormatter.PadRightDisplay(pl.PlaylistID, 15)} {ConsoleTableFormatter.PadRightDisplay(pl.PlaylistName, 30)}");
        }

        Console.Write("Choose playlist number to enqueue and play: ");
        if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > Playlists.Count)
        {
            Console.WriteLine("Invalid selection.");
            return;
        }

        CurrentPlaylist = Playlists[choice - 1];
        Console.WriteLine($"Selected playlist: {CurrentPlaylist.PlaylistName}");

        // GetSongsInPlaylist đã clear queue rồi
        var loaded = GetSongsInPlaylist(CurrentPlaylist.PlaylistID);
        if (!loaded || PlayQueue.Count == 0)
        {
            Console.WriteLine("No songs found for this playlist or failed to load songs.");
            return;
        }

        PrintPlaylistSongs();
        Console.WriteLine("Starting playback. Controls: p = pause/resume, n = next, s = stop, q = quit player");
        PlayQueueInteractive();
    }
    public bool AddSongToPlaylist(string playlistId, string songId)
    {
        using SqlConnection conn = new SqlConnection(ConnectionString);
        try
        {
            conn.Open();
            using SqlCommand cmd = new SqlCommand("dbo.sp_AddSongToPlaylist", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PlaylistID", playlistId);
            cmd.Parameters.AddWithValue("@SongID", songId);
            cmd.Parameters.AddWithValue("@UserID_WhoAdds", User.UserID);

            cmd.ExecuteNonQuery();
            return true;
        }
        catch (SqlException ex)
        {
            if (ex.Number == 50050 || ex.Number == 50051)
            {
                Console.WriteLine(ex.Message);
            }
            else
            {
                Console.WriteLine("SQL error while adding song: " + ex.Message);
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while adding song: " + ex.Message);
            return false;
        }
    }
    public void AddSongToPlaylistInteractive()
    {
        GetAllSongs();
        GetPlaylists();
        PrintAllSongs();

        // List options
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("1. Find song by Title, And add to playlist");
        Console.WriteLine("2. Type SongID, And add to playlist");
        Console.Write("Choose option: ");
        var opt = Console.ReadLine();

        if (opt == "1")
        {
            Console.WriteLine("Finding song by title...");
            Console.Write("Enter song title: ");
            string title = Console.ReadLine() ?? string.Empty;
            
            List<Song> foundSongs = FindSong(title);
            foreach (var song in foundSongs)
            {
                Console.WriteLine($"Found: {song.Title} by {song.ArtistNames} (SongID: {song.SongID})");
            }
            Console.WriteLine();
        }
        else if (opt != "2")
        {
            Console.WriteLine("Option not valid.");
            return;
        }
        
        // Cơ chế thêm song vào playlist
        Console.Write("Enter SongID: ");
        string songId = Console.ReadLine() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(songId))
        {
            Console.WriteLine("SongID not valid.");
            return;
        }

        // Optional: validate SongID
        var chosenSong = Songs.FirstOrDefault(s => s.SongID.Equals(songId, StringComparison.OrdinalIgnoreCase));
        if (chosenSong == null)
        {
            Console.WriteLine("SongID not found in the list.");
            return;
        }
        
        if (Playlists.Count == 0)
        {
            if (!GetPlaylists())
            {
                Console.WriteLine("Cannot load playlist list.");
                return;
            }
        }

        if (Playlists.Count == 0)
        {
            Console.WriteLine("You don't have any playlists.");
            return;
        }
        
        PrintPlaylists();

        Console.Write("Enter PlaylistID to add to: ");
        string playlistId = Console.ReadLine() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(playlistId))
        {
            Console.WriteLine("PlaylistID not valid.");
            return;
        }

        // Optional: validate PlaylistID
        var chosenPlaylist = Playlists.FirstOrDefault(p => p.PlaylistID.Equals(playlistId, StringComparison.OrdinalIgnoreCase));
        if (chosenPlaylist == null)
        {
            Console.WriteLine("PlaylistID not found in the list.");
            return;
        }

        // 5) Gọi proc thêm
        Console.WriteLine($"Adding \"{chosenSong.Title}\" to \"{chosenPlaylist.PlaylistName}\"...");
        bool ok = AddSongToPlaylist(chosenPlaylist.PlaylistID, chosenSong.SongID);
        Console.WriteLine(ok ? "Song added successfully." : "Failed to add song.");
    }
    public bool CreateNewPlaylist(string playlistName, string userId, bool isFavorite = false)
    {
        SqlConnection conn = new SqlConnection(ConnectionString);
        
        try
        {
            conn.Open();
            using SqlCommand cmd = new SqlCommand("dbo.sp_AddNewPlaylist", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PlaylistName", playlistName);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@IsFavorite", isFavorite);
            
            cmd.ExecuteNonQuery();
            conn.Close();
            return true;
        }
        catch (SqlException ex)
        {
            if (ex.Number == 50030 || ex.Number == 50031)
            {
                Console.WriteLine(ex.Message);
            }
            else            {
                Console.WriteLine("SQL error while creating playlist: " + ex.Message);
            }
        }
        conn.Close();
        return false;
    }
    public void CreateNewPlaylistInteractive()
    {
        Console.Write("Enter playlist name: ");
        string playlistName = Console.ReadLine() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(playlistName))
        {
            Console.WriteLine("Playlist name not valid.");
            return;
        }

        Console.Write("Mark as favorite? (y/n): ");
        string favInput = Console.ReadLine() ?? string.Empty;
        bool isFavorite = favInput.Equals("y", StringComparison.OrdinalIgnoreCase);

        bool created = CreateNewPlaylist(playlistName, User.UserID, isFavorite);
        Console.WriteLine(created ? "Playlist created successfully." : "Failed to create playlist.");
    }
    public bool RemoveSongFromPlaylist(string playlistId, string songId, string userId)
    {
        SqlConnection conn = new SqlConnection(ConnectionString);
        
        try
        {
            conn.Open();
            using SqlCommand cmd = new SqlCommand("dbo.sp_RemoveSongFromPlaylist", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PlaylistID", playlistId);
            cmd.Parameters.AddWithValue("@SongID", songId);
            cmd.Parameters.AddWithValue("@UserID_WhoDeletes", userId);
            cmd.ExecuteNonQuery();
            conn.Close();
            return true;
        }
        catch (SqlException ex)
        {
            if (ex.Number == 50070 || ex.Number == 50071)
            {
                Console.WriteLine(ex.Message);
            }
            else
            {
                Console.WriteLine("SQL error while removing song from playlist: " + ex.Message);
            }
        }
        conn.Close();
        return false;
    }
    public void RemoveSongFromPlaylistInteractive()
    {
        GetPlaylists();
        PrintPlaylists();
        
        Console.Write("Enter PlaylistID: ");
        string playlistId = Console.ReadLine() ?? string.Empty;
        var chosenPlaylist = Playlists.FirstOrDefault(p => p.PlaylistID.Equals(playlistId, StringComparison.OrdinalIgnoreCase));
        if (chosenPlaylist == null)
        {
            Console.WriteLine("PlaylistID not found.");
            return;
        }
        
        GetSongsInPlaylist(chosenPlaylist.PlaylistID);
        PrintPlaylistSongs();
        
        Console.Write("Enter SongID to remove: ");
        string songId = Console.ReadLine() ?? string.Empty;
        var chosenSong = PlayQueue.FirstOrDefault(s => s.SongID.Equals(songId, StringComparison.OrdinalIgnoreCase));
        if (chosenSong == null)
        {
            Console.WriteLine("SongID not found in playlist.");
            return;
        }
        bool removed = RemoveSongFromPlaylist(chosenPlaylist.PlaylistID, chosenSong.SongID, User.UserID);
        Console.WriteLine(removed ? "Song removed from playlist." : "Failed to remove song from playlist.");
    }
    public bool SwapSongsInPlaylist(string playlistId, string songId1, string songId2, string userId)
    {
        SqlConnection conn = new SqlConnection(ConnectionString);

        try
        {
            conn.Open();
            using SqlCommand cmd = new SqlCommand("dbo.sp_SwapPlaylistSongOrder", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PlaylistID", playlistId);
            cmd.Parameters.AddWithValue("@SongID_A", songId1);
            cmd.Parameters.AddWithValue("@SongID_B", songId2);
            cmd.Parameters.AddWithValue("@UserID_WhoUpdates", userId);
            cmd.ExecuteNonQuery();
            conn.Close();
            return true;
        }
        catch (SqlException ex)
        {
            if (ex.Number == 50070 || ex.Number == 50071 || ex.Number == 50072)
            {
                Console.WriteLine(ex.Message);
            }
            else
            {
                Console.WriteLine("SQL error while swapping songs in playlist: " + ex.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while swapping songs in playlist: " + ex.Message);
        }
        conn.Close();
        return false;
    }
    public void SwapSongsInPlaylistInteractive()
    {
        GetPlaylists();
        PrintPlaylists();
        
        Console.Write("Enter PlaylistID: ");
        string playlistId = Console.ReadLine() ?? string.Empty;
        var chosenPlaylist = Playlists.FirstOrDefault(p => p.PlaylistID.Equals(playlistId, StringComparison.OrdinalIgnoreCase));
        if (chosenPlaylist == null)
        {
            Console.WriteLine("PlaylistID not found.");
            return;
        }
        
        GetSongsInPlaylist(chosenPlaylist.PlaylistID);
        PrintPlaylistSongs();
        
        Console.Write("Enter SongID first to swap: ");
        string songId1 = Console.ReadLine() ?? string.Empty;
        var song1 = PlayQueue.FirstOrDefault(s => s.SongID.Equals(songId1, StringComparison.OrdinalIgnoreCase));
        if (song1 == null)
        {
            Console.WriteLine("SongID not found in playlist.");
            return;
        }
        
        Console.Write("Enter SongID second to swap: ");
        string songId2 = Console.ReadLine() ?? string.Empty;
        var song2 = PlayQueue.FirstOrDefault(s => s.SongID.Equals(songId2, StringComparison.OrdinalIgnoreCase));
        if (song2 == null)
        {
            Console.WriteLine("SongID not found in playlist.");
            return;
        }

        bool swapped = SwapSongsInPlaylist(chosenPlaylist.PlaylistID, song1.SongID, song2.SongID, User.UserID);
        Console.WriteLine(swapped ? "Swapped songs in playlist." : "Failed to swap songs in playlist.");
    }
    public bool DeletePlaylist(string playlistId, string userId)
    {
        SqlConnection conn = new SqlConnection(ConnectionString);
        
        try
        {
            conn.Open();
            using SqlCommand cmd = new SqlCommand("dbo.sp_DeletePlaylistByID", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PlaylistID", playlistId);
            cmd.Parameters.AddWithValue("@UserID_WhoDeletes", userId);
            cmd.ExecuteNonQuery();
            conn.Close();
            return true;
        }
        catch (SqlException ex)
        {
            if (ex.Number == 50060)
            {
                Console.WriteLine(ex.Message);
            }
            else
            {
                Console.WriteLine("SQL error while deleting playlist: " + ex.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while deleting playlist: " + ex.Message);
        }
        conn.Close();
        return false;
    }
    public void DeletePlaylistInteractive()
    {
        GetPlaylists();
        PrintPlaylists();
        
        Console.Write("Enter PlaylistID: ");
        string playlistId = Console.ReadLine() ?? string.Empty;
        var chosenPlaylist = Playlists.FirstOrDefault(p => p.PlaylistID.Equals(playlistId, StringComparison.OrdinalIgnoreCase));
        if (chosenPlaylist == null)
        {
            Console.WriteLine("PlaylistID not found.");
            return;
        }

        Console.Write($"Are you sure you want to delete the playlist \"{chosenPlaylist.PlaylistName}\"? (y/n): ");
        string confirm = Console.ReadLine() ?? string.Empty;
        if (!confirm.Equals("y", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Delete operation canceled.");
            return;
        }

        bool deleted = DeletePlaylist(chosenPlaylist.PlaylistID, User.UserID);
        Console.WriteLine(deleted ? "Playlist deleted." : "Failed to delete playlist.");
    }
    
    
    // Artist Interaction
    public bool IsArtist()
    {
        // Check current user is artist
        SqlConnection conn = new SqlConnection(ConnectionString);
        bool isArtist = false;
        string query = "SELECT @IsArtist = dbo.fn_IsUserAnArtist(@UserID)";
        try
        {
            conn.Open();
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserID", User.UserID);
            SqlParameter isArtistParam = new SqlParameter("@IsArtist", System.Data.SqlDbType.Bit)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            cmd.Parameters.Add(isArtistParam);
            cmd.ExecuteNonQuery();

            isArtist = (bool)isArtistParam.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error checking artist status: " + ex.Message);
        }
        finally
        {
            conn.Close();
        }
        return isArtist;
    }
    public bool SwitchToArtistMode()
    {
        if (IsLoggedIn && IsArtist())
        {
            Console.WriteLine("Switching to Artist Mode...");
            SqlConnection conn = new SqlConnection(ConnectionString);
            try
            {
                conn.Open();
                string query = "SELECT * FROM Artists WHERE UserID = @UserID";
                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserID", User.UserID);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string dbArtistId = reader["ArtistID"].ToString() ?? string.Empty;
                    string dbArtistName = reader["ArtistName"].ToString() ?? string.Empty;
                    string dbBio = reader["Bio"].ToString() ?? string.Empty;
                    string dbAvatarUrl = reader["AvatarURL"].ToString() ?? string.Empty;
                    string dbUserId = reader["UserID"].ToString() ?? string.Empty;

                    reader.Close();
                    conn.Close();

                    ConnectionString = _connectionStringArtist; // switch to artist connection
                    
                    Artist = new Artist(dbArtistId, dbArtistName, dbBio, dbAvatarUrl, dbUserId);
                    
                    Console.WriteLine($"Welcome, {Artist.ArtistName}! You are now in Artist Mode.");
                    return true;
                }
                else
                {
                    Console.WriteLine("No artist profile found for current user.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading artist profile: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }
        else
        {
            Console.WriteLine("Current user is not an artist. Cannot switch to Artist Mode.");
        }
        return false;
    }
    public void SwitchToUserMode()
    {
        Console.WriteLine("Switching back to user mode.");
        ConnectionString = _connectionStringUser; // switch back to user connection
    }
    public bool CreateNewSong(string title, string altTitle, int duration, DateTime releaseDate, List<string> artistIDs, List<string> categoryIDs, string audioUrl)
    {
        SqlConnection conn = new SqlConnection(ConnectionString);

        try
        {
            conn.Open();
            using SqlCommand cmd = new SqlCommand("dbo.sp_AddNewSong", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Title", title);
            cmd.Parameters.AddWithValue("@AltTitle", altTitle);
            cmd.Parameters.AddWithValue("@Duration", duration);
            cmd.Parameters.AddWithValue("@ReleaseDate", releaseDate);
            cmd.Parameters.AddWithValue("@ArtistIDs", string.Join(",", artistIDs));
            cmd.Parameters.AddWithValue("@CategoryIDs", string.Join(",", categoryIDs));
            cmd.Parameters.AddWithValue("@AudioURL", audioUrl);

            cmd.ExecuteNonQuery();
            conn.Close();
            return true;
        }
        catch (SqlException ex)
        {
            if (ex.Number == 50001 || ex.Number == 50002 || ex.Number == 50003 || ex.Number == 50004)
            {
                Console.WriteLine(ex.Message);
            }
            else
            {
                Console.WriteLine("SQL error while creating song: " + ex.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while creating new song: " + ex.Message);
        }
        finally
        {
            conn.Close();
        }
        return false;
    }
    public void CreateNewSongInteractive()
    {
        Console.WriteLine("\n");
        GetAllCategories();
        PrintAllCategories();
        Console.WriteLine("------------------------------------------------------------------------------");
        Console.WriteLine("Title | AltTitle | Duration | ReleaseDate | ArtistIDs | CategoryIDs | AudioURL");
        Console.WriteLine("------------------------------------------------------------------------------");
        
        Console.WriteLine(">> Creating new song...");
        Console.WriteLine("------");
        
        // Title
        Console.Write(":: Title: ");
        string title = Console.ReadLine() ?? string.Empty;
        // AltTitle
        Console.Write(":: Alt Title (optional): ");
        string altTitle = Console.ReadLine() ?? string.Empty;
        // Duration
        Console.Write(":: Duration (seconds): ");
        int duration = int.TryParse(Console.ReadLine(), out int dur) ? dur : 0;
        // ReleaseDate
        Console.Write(":: Release Date (yyyy-MM-dd): ");
        DateTime releaseDate = DateTime.TryParse(Console.ReadLine(), out DateTime rd) ? rd : DateTime.MinValue;
        
        // Add artists
        List<string> newArtistIDs = new List<string>(); // List to store new artist IDs
        newArtistIDs.Add(Artist.ArtistID);
        Console.Write(":: ArtistIDs (Press Enter to skip if this song belongs only to you): ");
        string artistInput = Console.ReadLine() ?? string.Empty;
        if (!string.IsNullOrEmpty(artistInput))
        {
            newArtistIDs.AddRange(artistInput.Split(',').Select(s => s.Trim()));   
        }
        
        // Add categories
        List<string> newCategoryIDs = new List<string>(); // List to store new category IDs
        Console.Write(":: CategoryIDs (comma-separated): ");
        string categoryInput = Console.ReadLine() ?? string.Empty;
        if (!string.IsNullOrEmpty(categoryInput))
        {
            newCategoryIDs.AddRange(categoryInput.Split(',').Select(s => s.Trim()));
        }
        
        // AudioURL
        Console.Write(":: Audio URL: ");
        string audioUrl = Console.ReadLine() ?? string.Empty;
        
        Console.WriteLine("\n-------------- New Song Details --------------");
        Console.WriteLine($"Title: {title}");
        Console.WriteLine($"Alt Title: {altTitle}");
        Console.WriteLine($"Duration: {duration} seconds");
        Console.WriteLine($"Release Date: {releaseDate.ToShortDateString()}");
        Console.WriteLine($"ArtistIDs: {string.Join(", ", newArtistIDs)}");
        Console.WriteLine($"CategoryIDs: {string.Join(", ", newCategoryIDs)}");
        Console.WriteLine($"Audio URL: {audioUrl}");

        Console.WriteLine("\n>> Confirm creating this song? (y/n): ");
        string confirm = Console.ReadLine() ?? string.Empty;
        if (confirm.Equals("y", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Creating new song...");
            bool create = CreateNewSong(title, altTitle, duration, releaseDate, newArtistIDs, newCategoryIDs, audioUrl);
            Console.WriteLine(create ? "Song created successfully." : "Failed to create song.");
        }
        else
        {
            Console.WriteLine("Cancelled creating new song.");
        }
    }
    public bool DeleteSong(string songId, string artistId)
    {
        SqlConnection conn = new SqlConnection(ConnectionString);

        try
        {
            conn.Open();
            using SqlCommand cmd = new SqlCommand("dbo.sp_DeleteSongByID", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SongID", songId);
            cmd.Parameters.AddWithValue("@ArtistID_WhoDeletes", artistId);
            cmd.ExecuteNonQuery();
            conn.Close();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while deleting song: " + ex.Message);
            return false;
        }
        finally
        {
            conn.Close();
        }
    }
    public void DeleteSongInteractive()
    {
        GetArtistSongs(Artist.ArtistID);
        PrintArtistSongs();

        Console.Write("Enter SongID: ");
        string songId = Console.ReadLine() ?? string.Empty;
        var chosenSong =
            Artist.MySongs.FirstOrDefault(s => s.SongID.Equals(songId, StringComparison.OrdinalIgnoreCase));
        if (chosenSong == null)
        {
            Console.WriteLine("SongID not found in your song list.");
            return;
        }

        Console.Write($"Are you sure you want to delete the song \"{chosenSong.Title}\"? (y/n): ");
        string confirm = Console.ReadLine() ?? string.Empty;
        if (!confirm.Equals("y", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Delete operation canceled.");
            return;
        }

        bool deleted = DeleteSong(chosenSong.SongID, Artist.ArtistID);
        Console.WriteLine(deleted ? "Song deleted successfully." : "Failed to delete song.");
    }
    public bool UpdateSongAudioUrl(string songId, string newAudioUrl, string artistId)
    {
        SqlConnection conn = new SqlConnection(ConnectionString);

        try
        {
            conn.Open();
            using SqlCommand cmd = new SqlCommand("dbo.sp_UpdateSongAudioURL", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SongID", songId);
            cmd.Parameters.AddWithValue("@NewAudioURL", newAudioUrl);
            cmd.Parameters.AddWithValue("@ArtistID_WhoUpdates", artistId);
            cmd.ExecuteNonQuery();
            conn.Close();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while updating song audio URL: " + ex.Message);
            return false;
        }
        finally
        {
            conn.Close();
        }
    }
    public void UpdateSongInteractive()
    {
        GetArtistSongs(Artist.ArtistID);
        PrintArtistSongs();

        Console.Write("Enter SongID: ");
        string songId = Console.ReadLine() ?? string.Empty;
        Song? chosenSong = Artist.MySongs.FirstOrDefault(s => s.SongID.Equals(songId, StringComparison.OrdinalIgnoreCase));
        if (chosenSong == null)
        {
            Console.WriteLine("SongID not found in your song list.");
            return;
        }

        Console.WriteLine($"Updating song: {chosenSong.Title}");
        // Menu chọn các option update
        Console.WriteLine("---------------------------------------");
        Console.WriteLine("1. Update song audio URL");
        Console.WriteLine("---------------------------------------");
        Console.WriteLine("0. Exit update song mode");
        Console.Write("Enter your choice: ");
        
        string choice = Console.ReadLine() ?? string.Empty;
        switch (choice)
        {
            case "1":
                Console.Write("Enter new Audio URL: ");
                string newAudioUrl = Console.ReadLine() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(newAudioUrl))
                {
                    Console.WriteLine("Audio URL cannot be empty.");
                    return;
                }

                bool updated = UpdateSongAudioUrl(chosenSong.SongID, newAudioUrl, Artist.ArtistID);
                Console.WriteLine(updated ? "Song audio URL updated successfully." : "Failed to update song audio URL.");
                if (updated)
                {
                    chosenSong.AudioURL = newAudioUrl;
                }
                Console.WriteLine($"Updated Audio URL: {chosenSong.AudioURL}");
                break;
            case "0":
                Console.WriteLine("Exiting update song mode.");
                return;
            default:
                Console.WriteLine("Invalid choice. Please try again.");
                break;
        }
    }
}
