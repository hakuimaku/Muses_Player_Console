using System.Globalization;
using Microsoft.Data.SqlClient;
using LibVLCSharp.Shared;
using Timer = System.Timers.Timer;

namespace Muses_Player_Console;

public class MusesService
{
    // ============ Audio Player ================
    public List<Song> PlayQueue = new List<Song>();
    public Playlist CurrentPlaylist { get; set; }
    public Song CurrentSong { get; set; }
    public int CurrentSongIndex { get; set; }
    public bool IsPlaying { get; private set; }


    
    // MediaPlayer
    private LibVLC _libVlc;
    private MediaPlayer _mediaPlayer;

    public event Action<Song> OnSongChanged;
    public event Action<string> OnTimeUpdated;
    public event Action<bool> OnPlayStateChanged;

    private DateTime _startedAt;
    private TimeSpan _playedTime;
    private bool _playCountIncremented;
    
    

    
    // ==============================================

    private string _connectionStringGuest =
        "Server=localhost,1433;Database=Muses_DB;User Id=login_guest;Password=GuestPass@123;TrustServerCertificate=True;Encrypt=False;";

    private string _connectionStringUser =
        "Server=localhost,1433;Database=Muses_DB;User Id=login_user;Password=UserPass@123;TrustServerCertificate=True;Encrypt=False;";

    private string _connectionStringArtist =
        "Server=localhost,1433;Database=Muses_DB;User Id=login_artist;Password=ArtistPass@123;TrustServerCertificate=True;Encrypt=False;";


    public string ConnectionString { get; private set; }
    public User User = new User();
    public Artist Artist = new Artist();

    public bool IsLoggedIn;
    public bool IsAdmin;

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
        
        CurrentSong = new Song();
        CurrentPlaylist = new Playlist();
        CurrentSongIndex = 0;

        _playedTime = TimeSpan.Zero;
        _playCountIncremented = false;
    }

    public void ClearData()
    {
        Playlists.Clear();
        Songs.Clear();
        Artists.Clear();
        Categories.Clear();
        
        CurrentPlaylist = null;
        PlayQueue.Clear();
        
        User = new User();
        Artist = new Artist();
        IsLoggedIn = false;
        IsAdmin = false;
        
        ConnectionString = _connectionStringGuest;
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

                    System.IO.File.AppendAllText("muses_debug.log", $"[INFO] User {dbUsername} logged in at {DateTime.UtcNow}\n");

                    return true;
                }
                else
                {
                    System.IO.File.AppendAllText("muses_debug.log", $"[WARNING] Failed login attempt for user {dbUsername} at {DateTime.UtcNow}\n");
                    reader.Close();
                    return false;
                }
            }
            else
            {
                System.IO.File.AppendAllText("muses_debug.log", $"[WARNING] Failed login attempt for username {username} at {DateTime.UtcNow}\n");
                return false;
            }
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error during login for username {username} - Exception: {ex.Message}\n{ex.StackTrace}\n");
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
            System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error IncrementPlayCount for SongID {songId} - Exception: {ex.Message}\n{ex.StackTrace}\n");
        }
    }

    public bool Register(string username, string email, string password)
    {
        SqlConnection conn = new SqlConnection(ConnectionString);

        try
        {
            conn.Open();
            string query = "EXEC dbo.sp_AddNewUser @Username = @Username, @Email = @Email, @Password = @Password";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@Password", password);

            cmd.ExecuteNonQuery();
            conn.Close();

            return true;
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText("muses_debug.log", $"[Error] {ex.Message}\n");
            return false;
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
            string query =
                "SELECT * FROM Playlists WHERE UserID = @UserID";
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
            System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error fetching playlists for user {User.Username} - Exception: {ex.Message}\n{ex.StackTrace}\n");
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
                string dbAudioUrl = reader["AudioURL"].ToString() ?? string.Empty;
                
                Song s = new Song(dbSongId, dbTitle, dbAltTitle, dbDuration, dbAudioUrl, dbPlayCount, dbReleaseDate, dbCategories, dbArtistNames);
                Songs.Add(s);
            }
            
            return true;
        }
        catch (Exception e)
        {
            System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error fetching songs - Exception: {e.Message}\n{e.StackTrace}\n");
            return false;
        }
        finally
        {
            conn.Close();
        }
    }

    public bool GetAllArtists()
    {
        Artists.Clear();
        SqlConnection conn = new SqlConnection(ConnectionString);

        try
        {
            conn.Open();
            string query = "SELECT * FROM Artists";
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
            System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error fetching artists - Exception: {e.Message}\n{e.StackTrace}\n");
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

        using (SqlConnection conn = new SqlConnection(ConnectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT * FROM dbo.fn_GetSongsByArtistID(@ArtistID);";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ArtistID", artistId);

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
                    string dbAudioUrl = reader["AudioURL"].ToString() ?? string.Empty;

                    Song s = new Song(dbSongId, dbTitle, dbAltTitle, dbDuration, dbAudioUrl, dbPlayCount, dbReleaseDate,
                        dbCategories, dbArtistNames);
                    Artist.MySongs.Add(s);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + e.Message);
                return false;
            }
        }
        return true;
    }

    public List<Song> GetPlaylistSongs(string playlistId)
    {
        return TryGetPlaylistSongs(playlistId, out var songsInPlaylist)
            ? songsInPlaylist
            : new List<Song>();
    }

    private bool TryGetPlaylistSongs(string playlistId, out List<Song> songs)
    {
        songs = new List<Song>();
        SqlConnection conn = new SqlConnection(ConnectionString);
        try
        {
            conn.Open();

            string query = "EXEC dbo.sp_GetSongsByPlaylistID @PlaylistID";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@PlaylistID", playlistId);

            SqlDataReader reader = cmd.ExecuteReader();

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

                songs.Add(s);
            }

            songs = songs.OrderBy(s => s.OrderNo).ToList();
            return true;
        }
        catch (Exception e)
        {
            System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error fetching songs for playlist ID {playlistId} - Exception: {e.Message}\n{e.StackTrace}\n");
            return false;
        }
        finally
        {
            conn.Close();
        }
    }

    public bool GetSongsInPlaylist(string playlistId)
    {
        CurrentPlaylist = new Playlist();
        if (!TryGetPlaylistSongs(playlistId, out var songsInPlaylist))
        {
            return false;
        }

        foreach (var song in songsInPlaylist)
        {
            PlayQueue.Add(song);
        }

        return true;
    }

    public bool GetAllCategories()
    {
        Categories.Clear();
        SqlConnection conn = new SqlConnection(ConnectionString);
        try
        {
            conn.Open();
            string query = "SELECT * FROM Categories";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.ExecuteNonQuery();

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string dbCategoryId = reader["CategoryID"].ToString() ?? string.Empty;
                string dbCategoryName = reader["CategoryName"].ToString() ?? string.Empty;

                Category c = new Category(dbCategoryId, dbCategoryName);
                Categories.Add(c);
            }

            return true;
        }
        catch (Exception e)
        {
            System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error fetching categories - Exception: {e.Message}\n{e.StackTrace}\n");
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

            Song foundSong = new Song(dbSongId, dbTitle, dbAltTitle, dbDuration, string.Empty, dbPlayCount,
                dbReleaseDate, dbCategories, dbArtistNames);
            foundSongs.Add(foundSong);
        }

        connection.Close();
        return foundSongs;
    }

    public List<Category> FindCategory(string title)
    {
        List<Category> foundCategories = new List<Category>();
        
        using SqlConnection connection = new SqlConnection(ConnectionString);
        string query = "SELECT * FROM dbo.fn_GetCategoryByTitle(@Keyword);";
        using SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Keyword", title);
        
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            string dbCategoryId = reader["CategoryID"].ToString() ?? string.Empty;
            string dbCategoryName = reader["CategoryName"].ToString() ?? string.Empty;
            
            Category foundCategory = new Category(dbCategoryId, dbCategoryName);
            foundCategories.Add(foundCategory);
        }
        connection.Close();
        return foundCategories;
    }

    public bool GetTop10Songs()
    {
        Songs.Clear();
        SqlConnection conn = new SqlConnection(ConnectionString);
        try
        {
            conn.Open();
            string query = "EXEC dbo.sp_GetTop10SongsByPlayCount;";
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
                string dbAudioUrl = reader["AudioURL"].ToString() ?? string.Empty;

                Song s = new Song(dbSongId, dbTitle, dbAltTitle, dbDuration, dbAudioUrl, dbPlayCount, dbReleaseDate,
                    dbCategories, dbArtistNames);
                Songs.Add(s);
            }

            conn.Close();
            return true;
        }
        catch (Exception e)
        {
            System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error fetching top 10 songs - Exception: {e.Message}\n{e.StackTrace}\n");
            return false;
        }
        finally
        {
            conn.Close();
        }
    }

    public void LoadPlaylistToPlayQueue()
    {
        if (CurrentPlaylist == null) return;
        
        ClearQueue();
        PlayQueue = GetSongsInPlaylist(CurrentPlaylist.PlaylistID) ? PlayQueue : new List<Song>();
        CurrentSongIndex = 0;
        CurrentSong = PlayQueue.Count > 0 ? PlayQueue[CurrentSongIndex] : new Song();
    }

    
    
    
    // User Interaction
    public void PlaySong()
    {
        if (CurrentSong == null)
        {
            CurrentSong =  PlayQueue.Count > 0 ? PlayQueue[CurrentSongIndex] : new Song();
        }
        Song song = CurrentSong;
        if (song == null || string.IsNullOrWhiteSpace(song.AudioURL)) return;
        
        Task.Run(async () => {
            try
            {
                if (_mediaPlayer.Media != null)
                {
                    _mediaPlayer.Stop();
                    _mediaPlayer.Media.Dispose();
                    _mediaPlayer.Media = null;
                }
            
                var media = new Media(_libVlc, song.AudioURL, FromType.FromLocation);
                _mediaPlayer.Media = media;
                _mediaPlayer.Play();
            
                IsPlaying = true;
                _playedTime = TimeSpan.Zero;
                _playCountIncremented = false;
                _startedAt = DateTime.UtcNow;
            
                OnSongChanged?.Invoke(song);
                OnPlayStateChanged?.Invoke(true);
                
                System.IO.File.AppendAllText("muses_debug.log", $"[INFO] Started playing song: {song.Title} at {DateTime.UtcNow}\n");
            
                while (true)
                {
                    await Task.Delay(1000);
                
                    if (IsPlaying)
                    {
                        _playedTime = _playedTime.Add(TimeSpan.FromSeconds(1));
                    
                        string durationStr = song.Duration == 0 
                            ? "N/A" 
                            : TimeSpan.FromSeconds(song.Duration).ToString(@"mm\:ss");
                    
                        string timeDisplay = $"{_playedTime:mm\\:ss} / {durationStr}";
                        OnTimeUpdated?.Invoke(timeDisplay);
                    
                        if (!_playCountIncremented && _playedTime.TotalSeconds >= 30)
                        {
                            IncrementPlayCount(song.SongID);
                            _playCountIncremented = true;
                            System.IO.File.AppendAllText("muses_debug.log", $"[INFO] Incremented play count for song: {song.Title} at {DateTime.UtcNow}\n");
                        }
                    }

                    if (CurrentSong.Duration == _playedTime.TotalSeconds)
                    {
                        System.IO.File.AppendAllText("muses_debug.log", $"[INFO] Finished PlaySong: {song.Title} at {DateTime.UtcNow}\n");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("muses_debug.log", $"[CRASH LOG] Error PlaySong - Task.Run: {ex.Message}\n{ex.StackTrace}\n");
            }
        });
    }
    
    public void PauseSong()
    {
        if (_mediaPlayer.Media == null) return;

        if (IsPlaying)
        {
            _mediaPlayer.SetPause(true);
            IsPlaying = false;
            OnPlayStateChanged?.Invoke(false);
            System.IO.File.AppendAllText("muses_debug.log", $"[INFO] Paused song: {CurrentSong.Title} at {DateTime.UtcNow}\n");
        }
        else
        {
            _mediaPlayer.SetPause(false);
            IsPlaying = true;
            OnPlayStateChanged?.Invoke(true);
            System.IO.File.AppendAllText("muses_debug.log", $"[INFO] Resumed song: {CurrentSong.Title} at {DateTime.UtcNow}\n");
        }
    }
    public void StopSong()
    {
        IsPlaying = false;
        
        if (_mediaPlayer.Media != null)
        {
            _mediaPlayer.Stop();
            _mediaPlayer.Media.Dispose();
            _mediaPlayer.Media = null;
        }
        
        _playedTime = TimeSpan.Zero;
        _playCountIncremented = false;
        
        OnPlayStateChanged?.Invoke(false);
        OnTimeUpdated?.Invoke("00:00 / 00:00");
        System.IO.File.AppendAllText("muses_debug.log", $"[INFO] Stopped song: {CurrentSong.Title} at {DateTime.UtcNow}\n");
    }
    public void NextSong()
    {
        if (PlayQueue.Count > 0)
        {
            CurrentSongIndex = (CurrentSongIndex + 1) % PlayQueue.Count;
            CurrentSong = PlayQueue[CurrentSongIndex];
        }
    }
    public void PreviousSong()
    {
        if (PlayQueue.Count > 0)
        {
            CurrentSongIndex = (CurrentSongIndex - 1 + PlayQueue.Count) % PlayQueue.Count;
            CurrentSong = PlayQueue[CurrentSongIndex];
        }
    }
    public void ClearQueue()
    {
        PlayQueue.Clear();
        CurrentSongIndex = 0;
        CurrentSong = new Song();
    }
    public bool HasActiveMedia()
    {
        return _mediaPlayer.Media != null;
    }
    

    public bool AddNewArtist(string artistName, string artistBio, string avatarUrl)
    {
        using SqlConnection conn = new SqlConnection(ConnectionString);
        try
        {
            conn.Open();
            using SqlCommand cmd = new SqlCommand("dbo.sp_AddNewArtist", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ArtistName", artistName);
            cmd.Parameters.AddWithValue("@Bio", artistBio);
            cmd.Parameters.AddWithValue("@AvatarURL", avatarUrl);
            cmd.Parameters.AddWithValue("@UserID", User.UserID);

            cmd.ExecuteNonQuery();
            System.IO.File.AppendAllText("muses_debug.log", $"[INFO] Added new artist: {artistName} at {DateTime.UtcNow}\n");
            return true;
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error AddNewArtist - Exception: {ex.Message}\n{ex.StackTrace}\n");
            return false;
        }
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
            System.IO.File.AppendAllText("muses_debug.log", $"[INFO] Added song (ID: {songId}) to playlist (ID: {playlistId}) at {DateTime.UtcNow}\n");
            return true;
        }
        catch (SqlException ex)
        {
            if (ex.Number == 50050 || ex.Number == 50051)
            {
                System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error AddSongToPlaylist - SqlException: {ex.Message}\n{ex.StackTrace}\n");
            }
            else
            {
                System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error AddSongToPlaylist - SqlException: {ex.Message}\n{ex.StackTrace}\n");
            }

            return false;
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error AddSongToPlaylist - Exception: {ex.Message}\n{ex.StackTrace}\n");
            return false;
        }
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
            System.IO.File.AppendAllText("muses_debug.log", $"[INFO] Created new playlist: {playlistName} at {DateTime.UtcNow}\n");
            return true;
        }
        catch (SqlException ex)
        {
            if (ex.Number == 50030 || ex.Number == 50031)
            {
                System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error CreateNewPlaylist - SqlException: {ex.Message}\n{ex.StackTrace}\n");
            }
            else
            {
                System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error CreateNewPlaylist - Exception: {ex.Message}\n{ex.StackTrace}\n");
            }
        }

        conn.Close();
        return false;
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
                System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error RemoveSongFromPlaylist - SqlException: {ex.Message}\n{ex.StackTrace}\n");
            }
            else
            {
                System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error RemoveSongFromPlaylist - Exception: {ex.Message}\n{ex.StackTrace}\n");
            }
        }

        conn.Close();
        return false;
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
                System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error SwapSongsInPlaylist - SqlException: {ex.Message}\n{ex.StackTrace}\n");
            }
            else
            {
                System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error SwapSongsInPlaylist - Exception: {ex.Message}\n{ex.StackTrace}\n");
            }
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error SwapSongsInPlaylist - Exception: {ex.Message}\n{ex.StackTrace}\n");
        }

        conn.Close();
        return false;
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
                System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error DeletePlaylist - SqlException: {ex.Message}\n{ex.StackTrace}\n");
            }
            else
            {
                System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error DeletePlaylist - Exception: {ex.Message}\n{ex.StackTrace}\n");
            }
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error DeletePlaylist - Exception: {ex.Message}\n{ex.StackTrace}\n");
        }

        conn.Close();
        return false;
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
            System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error IsArtist - Exception: {ex.Message}\n{ex.StackTrace}\n");
        }
        finally
        {
            conn.Close();
        }

        return isArtist;
    }

    public bool SwitchToArtistMode()
    {
        if (IsArtist())
        {
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
                    System.IO.File.AppendAllText("muses_debug.log", $"[INFO] Switched to Artist Mode for user ID: {User.UserID} at {DateTime.UtcNow}\n");
                    return true;
                }
                else
                {
                    System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] No artist profile found for user ID: {User.UserID} at {DateTime.UtcNow}\n");
                }
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error loading artist profile: {ex.Message}\n{ex.StackTrace}\n");
            }
            finally
            {
                conn.Close();
            }
        }
        else
        {
            System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] User ID: {User.UserID} is not an artist, cannot switch to Artist Mode at {DateTime.UtcNow}\n");
        }

        return false;
    }

    public void SwitchToUserMode()
    {
        System.IO.File.AppendAllText("muses_debug.log", $"[INFO] Switching back to User Mode for user ID: {User.UserID} at {DateTime.UtcNow}\n");
        ConnectionString = _connectionStringUser; // switch back to user connection
    }

    public bool CreateNewSong(string title, string altTitle, int duration, DateTime releaseDate, string artistIDs,
        string categoryIDs, string audioUrl)
    {
        SqlConnection conn = new SqlConnection(ConnectionString);

        try
        {
            if (string.IsNullOrWhiteSpace(releaseDate.ToString(CultureInfo.InvariantCulture)))
            {
                releaseDate = DateTime.Now;
            }

            conn.Open();
            using SqlCommand cmd = new SqlCommand("dbo.sp_AddNewSong", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Title", title);
            cmd.Parameters.AddWithValue("@AltTitle", altTitle);
            cmd.Parameters.AddWithValue("@Duration", duration);
            cmd.Parameters.AddWithValue("@ReleaseDate", releaseDate);
            cmd.Parameters.AddWithValue("@ArtistIDs", artistIDs);
            cmd.Parameters.AddWithValue("@CategoryIDs", categoryIDs);
            cmd.Parameters.AddWithValue("@AudioURL", audioUrl);

            cmd.ExecuteNonQuery();
            conn.Close();
            System.IO.File.AppendAllText("muses_debug.log", $"[SUCCESSFUL] Create a new song: {title}\n");
            return true;
        }
        catch (SqlException ex)
        {
            if (ex.Number == 50001 || ex.Number == 50002 || ex.Number == 50003 || ex.Number == 50004)
            {
                System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] {ex.Message}\n");
            }
            else
            {
                System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] While creating a new song: {ex.Message}\n");
            }
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] While creating a new song: {ex.Message}\n");
        }
        finally
        {
            conn.Close();
        }

        return false;
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
            System.IO.File.AppendAllText("muses_debug.log", $"[INFO] Deleted song (ID: {songId}) by artist ID: {artistId} at {DateTime.UtcNow}\n");
            return true;
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error while deleting song (ID: {songId}) - Exception: {ex.Message}\n{ex.StackTrace}\n");
            return false;
        }
        finally
        {
            conn.Close();
        }
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
            System.IO.File.AppendAllText("muses_debug.log", $"[ERROR] Error while updating song audio URL (ID: {songId}) - Exception: {ex.Message}\n{ex.StackTrace}\n");
            return false;
        }
        finally
        {
            conn.Close();
        }
    }
    
}
