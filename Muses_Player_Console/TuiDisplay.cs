using System.Data;
using Terminal.Gui.Views;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Drawing;
using Terminal.Gui.App;
using Terminal.Gui.Input;
using System.Collections.ObjectModel;

namespace Muses_Player_Console
{
    public class TuiDisplay : Window
    {
        private readonly MusesService _musesService;
        private readonly IApplication _app; 
        
        private Window _loginWindow;
        
        private Window _userDashboard;
        private Window _userMenuFrame;
        private Window _userMainContentFrame;
        private Window _userFooterFrame;
        
        private Window _artistDashboard;
        private Window _artistMenuFrame;
        private Window _artistMainContentFrame;
        private Window _artistFooterFrame;
        
        private Window _guestDashboard;
        private Window _guestMainContentFrame;
        private Window _guestMenuFrame;
        
        // User Footer Frame (Control Panel)
        public Label LblNowPlaying;
        public Button BtnPlayPause;
        public Button BtnPrev;
        public Button BtnNext;
        public Label LblTime;

        public TuiDisplay(IApplication app, MusesService musesService)
        {
            _app = app;
            _musesService = musesService;
            
            Title = "Muses System Container";
            X = 0; Y = 0;
            Width = Dim.Fill();
            Height = Dim.Fill();
            BorderStyle = LineStyle.None;

            _userMenuFrame = new Window()
            {
                Title = "Menu",
                X = 0, Y = 0,
                Width = 20,
                Height = Dim.Fill() -5,
            };
            
            _userMainContentFrame = new Window()
            {
                Title = "Main Content",
                X = 20, Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill() -5,
                BorderStyle = LineStyle.None
            };
            
            _userFooterFrame = new Window()
            {
                X = 0, Y = Pos.AnchorEnd(5),
                Width = Dim.Fill(),
                Height = 5,
                BorderStyle = LineStyle.Dashed,
            };

            _artistMenuFrame = new Window()
            {
                Title = "Menu",
                X = 0, Y = 0,
                Width = 20,
                Height = Dim.Fill() -4,
            };

            _artistMainContentFrame = new Window()
            {
                Title = "Main Content",
                X = 20, Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill() -4,
            };

            _guestMainContentFrame = new Window()
            {
                Title = "Main Content",
                X = 0, Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                BorderStyle = LineStyle.None
            };
            
            _guestMenuFrame = new Window()
            {
                X = 0, Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                BorderStyle = LineStyle.None
            };
            
            
            InitLoginWindow();
            
            InitGuestWindow();
            
            InitUserWindow();
            
            InitArtistWindow();
            
            this.Add(_loginWindow, _guestDashboard, _userDashboard, _artistDashboard);
            
            WireUpMusicEngineEvents();
        }
        
        private void InitLoginWindow()
        {
            _loginWindow = new Window () {
                Title = "Welcome to Muses Player!",
                X = 0, Y = 0,
                Width = Dim.Fill(), Height = Dim.Fill()
            };
            
            var loginContainer = new View () {
                X = Pos.Center(),
                Y = Pos.Center(),
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                CanFocus = true
            };
            
            var lblLogin = new Label() { Text = "LOGIN", X = Pos.Center(), Y = 1 };
            
            var lblUser = new Label () { Text = "Username:", X = 2, Y = 3 };
            var txtUser = new TextField () { Text = "", X = 12, Y = 3, Width = 30 };

            var lblPass = new Label () { Text = "Password:", X = 2, Y = 5 };
            var txtPass = new TextField () { Text = "", X = 12, Y = 5, Width = 30, Secret = true };

            var btnLogin = new Button () { Title = "Login", X = Pos.Center(), Y = 7 };
            
            var btnRegister = new Button () { Title = "Register", X = Pos.Center(), Y = 9 };
            
            var btnGuest = new Button () { Title = "Continue as Guest", X = Pos.Center(), Y = 11 };

            var loginFrame = new Window()
            {
                X = Pos.Center() + 30,
                Y = Pos.Center(),
                Width = 40,
                Height = 15,
                BorderStyle = LineStyle.Double,
            };
            
            var lblLogo = new Label () {
                X = Pos.Center() - 30,
                Y = 14,
                Text = """
                       ███╗   ███╗██╗   ██╗███████╗███████╗███████╗
                       ████╗ ████║██║   ██║██╔════╝██╔════╝██╔════╝
                       ██╔████╔██║██║   ██║███████╗█████╗  ███████╗
                       ██║╚██╔╝██║██║   ██║╚════██║██╔══╝  ╚════██║
                       ██║ ╚═╝ ██║╚██████╔╝███████║███████╗███████║
                       ╚═╝     ╚═╝ ╚═════╝ ╚══════╝╚══════╝╚══════╝
                       """
            };
            
            txtUser.KeyDown += (s, e) => {
                if (e.KeyCode == Key.Enter) {
                    txtPass.SetFocus();
                    e.Handled = true;
                }
            };
            
            txtPass.KeyDown += (s, e) => {
                if (e.KeyCode == Key.Enter) {
                    btnLogin.SetFocus();
                    e.Handled = true;
                }
            };
            
            // Event handler for Login button (login)
            btnLogin.Accepted += (s, e) => {
                string uName = txtUser.Text.ToString().Trim();
                string pWord = txtPass.Text.ToString().Trim();

                if (string.IsNullOrEmpty(uName) || string.IsNullOrEmpty(pWord)) {
                    MessageBox.Query(_app, "Error", "Username and password cannot be empty!", "OK");
                    return;
                }

                if (_musesService.Login(uName, pWord)) {
                    txtUser.Text = "";
                    txtPass.Text = "";
                    
                    txtUser.SetFocus(); 

                    _loginWindow.Visible = false;
                    _userDashboard.Visible = true;
                    _userMenuFrame.SetFocus();
                } else {
                    MessageBox.Query(_app, "Failed", "Invalid username or password!", "Try again");
                }
            };
            
            // Event handler for Register button (register)
            btnRegister.Accepted += (s, e) =>
            {
                MessageBox.Query(_app, "Register", "Registration feature is not implemented yet.", "OK");
            };
            
            // Event handler for Guest button (guest)
            btnGuest.Accepted += (s, e) => {
                _loginWindow.Visible = false;
                _guestDashboard.Visible = true;
                _guestMainContentFrame.SetFocus();
                e.Handled = true;
            };
            
            loginFrame.Add(lblLogin, lblUser, txtUser, lblPass, txtPass, btnLogin, btnRegister, btnGuest);
            loginContainer.Add(lblLogo, loginFrame);
            
            _loginWindow.Add(loginContainer);
        }
        
        // User Mode =========================================================================================
        private void InitUserWindow()
        {
            _userDashboard = new Window()
            {
                Title = "Muses User Dashboard",
                X = 0, Y = 0,
                Width = Dim.Fill(), Height = Dim.Fill(),
                BorderStyle = LineStyle.Single,
            };
            
            // Menu lựa chọn cho User
            var menuItems = new ObservableCollection<string>
            {
                "0. Exit",
                "1. Songs",
                "2. Play Queue",
                "3. My Playlists",
                "4. View Artists",
                "5. Switch to Artist"
            };

            var menuListView = new ListView()
            {
                X = 0, Y = 0,
                Width = Dim.Fill(), Height = Dim.Fill()
            };
            menuListView.SetSource(menuItems);

            menuListView.Accepted += (s, e) =>
            {
                int idx = menuListView.SelectedItem ?? -1;
                switch (idx)
                {
                    case 0:
                        _musesService.ClearData();
                        _musesService.StopSong();
                        _loginWindow.Visible = true;
                        _userDashboard.Visible = false;
                        _loginWindow.SetFocus();
                        break;
                    case 1:
                        _musesService.GetAllSongs();
                        RenderSongUser(_musesService.Songs, _userMainContentFrame);
                        break;
                    case 2:
                        RenderSong_PlayQueue(_musesService.PlayQueue, _userMainContentFrame);
                        break;
                    case 3:
                        _musesService.GetPlaylists();
                        RenderPlaylistsToTable(_musesService.Playlists, _userMainContentFrame);
                        break;
                    case 4:
                        _musesService.GetAllArtists();
                        RenderArtistsToTable(_musesService.Artists);
                        break;
                    case 5:
                        if (_musesService.SwitchToArtistMode())
                        {
                            _artistDashboard.Visible = true;
                            _userDashboard.Visible = false;
                            _artistMenuFrame.SetFocus();
                        }
                        else
                        {
                            MessageBox.Query(_app, "Error", "You are not a Artist!", "OK");
                        }
                        break;
                }
            };
            
            _userMainContentFrame.KeyDown += (sender, e) => {
                if (e.KeyCode == Key.CursorLeft)
                {
                    menuListView.SetFocus();
                    e.Handled = true; 
                }
            };
            
            menuListView.KeyDown += (sender, e) => {
                if (e.KeyCode == Key.CursorRight)
                {
                    _userMainContentFrame.SetFocus();
                    e.Handled = true;
                }
            };

            _userFooterFrame.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Key.CursorUp)
                {
                    _userMainContentFrame.SetFocus();
                    e.Handled = true;
                }
            };
            
            // Control Panel in Footer
            LblNowPlaying = new Label()
            {
                X = 1,
                Y = 1,
                Text = "🎵 Ready: Choose Song to playing...",
                Width = 40
            };

            BtnPlayPause = new Button()
            {
                X = Pos.Center(), 
                Y = 1,
                Title = " > "
            };

            BtnPrev = new Button()
            {
                X = Pos.Left(BtnPlayPause) - 10, 
                Y = 1,
                Title = " |< "
            };

            BtnNext = new Button()
            {
                X = Pos.Right(BtnPlayPause) + 2, 
                Y = 1,
                Title = " >| "
            };

            LblTime = new Label()
            {
                X = Pos.AnchorEnd(15), 
                Y = 1,
                Text = "00:00",
                Width = 15
            };

            BtnPlayPause.Accepted += (s, e) => {
                if (_musesService.CurrentSong == null) return;
                
                if (!_musesService.IsPlaying && !_musesService.HasActiveMedia()) 
                {
                    _musesService.PlaySong();
                }
                else
                {
                    _musesService.PauseSong();
                }

                e.Handled = true;
            };

            BtnNext.Accepted += (s, e) => {
                _musesService.StopSong();
                _musesService.NextSong();
                _musesService.PlaySong();
                e.Handled = true;
            };

            BtnPrev.Accepted += (s, e) => {
                _musesService.StopSong();
                _musesService.PreviousSong();
                _musesService.PlaySong();
                e.Handled = true;
            };
            
            
            
            _userFooterFrame.Add(LblNowPlaying, BtnPrev, BtnPlayPause, BtnNext, LblTime);
            _userMenuFrame.Add(menuListView);
            _userDashboard.Add(_userMenuFrame, _userMainContentFrame, _userFooterFrame);
        }

        
        // Artist Mode =====================================================================================

        private void InitArtistWindow()
        {
            _artistDashboard = new Window()
            {
                Title = "Muses Artist Dashboard",
                X = 0, Y = 0,
                Width = Dim.Fill(), Height = Dim.Fill(),
                BorderStyle = LineStyle.None
            };
            
            
             var menuItems = new ObservableCollection<string>
             {
                 "0. Switch to User",
                 "1. View My Songs",
                 "2. View My Profile",
                 "3. Add a new Song"
             };
              
             var menuListView = new ListView()
             {
                 X = 0, Y = 0,
                 Width = Dim.Fill(), Height = Dim.Fill()
             };
             menuListView.SetSource(menuItems);

             menuListView.Accepted += (s, e) =>
             {
                 int idx = menuListView.SelectedItem ?? -1;
                 switch (idx)
                 {
                    case 0:
                        _musesService.SwitchToUserMode();
                        _userDashboard.Visible = true;
                        _artistDashboard.Visible = false;
                        _userMenuFrame.SetFocus();
                        break;
                    case 1:
                        _musesService.GetArtistSongs(_musesService.Artist.ArtistID);
                        RenderSongs(_musesService.Artist.MySongs, _artistMainContentFrame);
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                 }
             };
             
             _artistMainContentFrame.KeyDown += (sender, e) => {
                 if (e.KeyCode == Key.CursorLeft)
                 {
                     menuListView.SetFocus();
                     e.Handled = true; 
                 }
             };
            
             menuListView.KeyDown += (sender, e) => {
                 if (e.KeyCode == Key.CursorRight)
                 {
                     _artistMainContentFrame.SetFocus();;
                     e.Handled = true;
                 }
             };
             
             _artistMenuFrame.Add(menuListView);
             _artistDashboard.Add(_artistMenuFrame, _artistMainContentFrame);
        }
        
        // Guest Mode ======================================================================================

        private void InitGuestWindow()
        {
            _guestDashboard = new Window()
            {
                Title = "Muses Guest Dashboard",
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                BorderStyle = LineStyle.None
            };

            Button BtnReturnToLogin = new Button()
            {
                Title = "Return to Login",
                X = 0,
                Y = 0,
                Width = 30
            };
            
            _guestMainContentFrame.KeyDown += (sender, e) => {
                if (e.KeyCode == Key.CursorDown)
                {
                    BtnReturnToLogin.SetFocus();
                    e.Handled = true; 
                }
            };

            BtnReturnToLogin.Accepted += (s, e) =>
            {
                _loginWindow.Visible = true;
                _guestDashboard.Visible = false;
                _loginWindow.SetFocus();
            };
                   
            
            _musesService.GetAllSongs();
            RenderSongs(_musesService.Songs, _guestMainContentFrame);
            
            _guestMenuFrame.Add(BtnReturnToLogin);
            _guestDashboard.Add(_guestMenuFrame, _guestMainContentFrame);
        }

        

        // Render display ===================================================================================
        private DataTable UpdateTableSongData(IEnumerable<Song> songsToRender, TableView tableView, bool includeAudioUrl = false)
        {
            var tableData = new DataTable();
            tableData.Columns.Add("SongID", typeof(string));
            tableData.Columns.Add("Title", typeof(string));
            tableData.Columns.Add("AltTitle", typeof(string));
            tableData.Columns.Add("ArtistName", typeof(string));
            tableData.Columns.Add("Categories", typeof(string));
            tableData.Columns.Add("PlayCount", typeof(int));
            tableData.Columns.Add("ReleaseDate", typeof(DateTime));
            tableData.Columns.Add("Duration", typeof(string));

            if (includeAudioUrl)
            {
                tableData.Columns.Add("AudioUrl", typeof(string));
            }

            foreach (var song in songsToRender)
            {
                var duration = song.Duration == 0
                    ? "N/A"
                    : TimeSpan.FromSeconds(song.Duration).ToString(@"mm\:ss");

                if (includeAudioUrl)
                {
                    tableData.Rows.Add(
                        song.SongID,
                        song.Title,
                        song.AltTitle,
                        song.ArtistNames,
                        song.Categories,
                        song.PlayCount,
                        song.ReleaseDate,
                        duration,
                        song.AudioURL
                    );
                }
                else
                {
                    tableData.Rows.Add(
                        song.SongID,
                        song.Title,
                        song.AltTitle,
                        song.ArtistNames,
                        song.Categories,
                        song.PlayCount,
                        song.ReleaseDate,
                        duration
                    );
                }
            }

            tableView.Table = new DataTableSource(tableData);
            tableView.Update();

            return tableData;
        }
        private DataTable UpdateTableArtistData(IEnumerable<Artist> artistsToRender, TableView tableView)
        {
            var tableData = new DataTable();
            tableData.Columns.Add("ArtistID", typeof(string));
            tableData.Columns.Add("Name", typeof(string));
            tableData.Columns.Add("Bio", typeof(string));

            foreach (var artist in artistsToRender)
            {
                tableData.Rows.Add(
                    artist.ArtistID,
                    artist.ArtistName,
                    artist.Bio
                );
            }

            tableView.Table = new DataTableSource(tableData);
            tableView.Update();

            return tableData;
        }
        private DataTable UpdateTablePlaylistData(IEnumerable<Playlist> playlistsToRender, TableView tableView)
        {
            var tableData = new DataTable();
            tableData.Columns.Add("PlaylistID", typeof(string));
            tableData.Columns.Add("Name", typeof(string));
            tableData.Columns.Add("CreatedDate", typeof(DateTime));
            tableData.Columns.Add("IsFavorite", typeof(bool));

            foreach (var playlist in playlistsToRender)
            {
                tableData.Rows.Add(
                    playlist.PlaylistID,
                    playlist.PlaylistName,
                    playlist.CreatedDate,
                    playlist.IsFavorite
                );
            }

            tableView.Table = new DataTableSource(tableData);
            tableView.Update();

            return tableData;
        }

        
        
        private void RenderSongs(IEnumerable<Song> initialSongs, Window targetContentFrame)
        {
            targetContentFrame.RemoveAll(); 
            
            var lblSearch = new Label() { Text = "Search:", X = 1, Y = 2 };
            var txtSearch = new TextField() { Text = "", X = 10, Y = 2, Width = Dim.Fill() - 2 };
            
            var tableContainer = new Window() {
                X = 0, Y = 4, 
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                BorderStyle = LineStyle.Single
            };
            
            var tableView = new TableView() {
                X = 0, Y = 0,
                Width = Dim.Fill(), Height = Dim.Fill(),
                FullRowSelect = true
            };
            
            UpdateTableSongData(initialSongs, tableView);
            
            txtSearch.KeyDown += (s, e) => {
                if (e.KeyCode == Key.Enter)
                {
                    string keyword = txtSearch.Text.ToString().Trim();
                    
                    if (string.IsNullOrWhiteSpace(keyword))
                    {
                        UpdateTableSongData(initialSongs, tableView);
                    }
                    else
                    {
                        List<Song> filteredSongs = _musesService.FindSong(keyword);
                        UpdateTableSongData(filteredSongs, tableView);
                    }
                    
                    tableView.SetFocus(); 
                    e.Handled = true;
                }
                if (e.KeyCode == Key.CursorDown)
                {
                    tableView.SetFocus();
                    e.Handled = true;
                }
            };

            tableView.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Key.CursorUp)
                {
                    txtSearch.SetFocus();
                    e.Handled = true;
                }
            };

            tableContainer.Add(tableView);
            targetContentFrame.Add(lblSearch, txtSearch, tableContainer);
        }
        private void RenderSongUser(IEnumerable<Song> initialSongs, Window targetContentFrame)
        {
            targetContentFrame.RemoveAll(); 
            
            var lblSearch = new Label() { Text = "Search:", X = 1, Y = 0 };
            var txtSearch = new TextField() { Text = "", X = 10, Y = 0, Width = Dim.Fill() - 2 };
            
            var tableContainer = new Window() {
                Title = "Songs - Enter to add to Play Queue, A to add to playlist",
                X = 0, Y = 1, 
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                BorderStyle = LineStyle.Rounded
            };
            
            var tableView = new TableView() {
                X = 0, Y = 0,
                Width = Dim.Fill(), Height = Dim.Fill(),
                FullRowSelect = true
            };
            
            DataTable _currentTable = UpdateTableSongData(initialSongs, tableView, includeAudioUrl: true);
            
            // Find song by keyword
            txtSearch.KeyDown += (s, e) => {
                if (e.KeyCode == Key.Enter)
                {
                    string keyword = txtSearch.Text.ToString().Trim();
                    
                    if (string.IsNullOrWhiteSpace(keyword))
                    {
                        _currentTable = UpdateTableSongData(initialSongs, tableView, includeAudioUrl: true);
                    }
                    else
                    {
                        List<Song> filteredSongs = _musesService.FindSong(keyword);
                        _currentTable = UpdateTableSongData(filteredSongs, tableView, includeAudioUrl: true);
                    }
                    
                    tableView.SetFocus(); 
                    e.Handled = true;
                }
                if (e.KeyCode == Key.CursorDown)
                {
                    tableView.SetFocus();
                    e.Handled = true;
                }
            };
            
            // Add song to queue
            tableView.KeyDown += (s, e) => {
                if (e.KeyCode == Key.Enter)
                {
                    var selectedCells = tableView.GetAllSelectedCells();
                    if (selectedCells.Any() && _currentTable != null)
                    {
                        int row = selectedCells.First().Y; 
                        
                        if (row >= 0 && row < _currentTable.Rows.Count)
                        {
                            string songId = _currentTable.Rows[row]["SongID"]?.ToString();
                            
                            if (!string.IsNullOrEmpty(songId))
                            {
                                var selectedSong = _musesService.Songs.FirstOrDefault(x => x.SongID == songId);

                                if (selectedSong != null)
                                {
                                    _musesService.PlayQueue.Add(selectedSong);
                                    MessageBox.Query(_app, "Success", $"Added {selectedSong.Title} to queue!", "OK");
                                }
                            }
                        }
                    }
                    e.Handled = true;
                }
                if (e.KeyCode == Key.CursorUp)
                {
                    txtSearch.SetFocus();
                    e.Handled = true;
                }
            };
            
            // Add song to playlist
            tableView.KeyDown += (s, e) => {
                if (e.KeyCode == Key.A)
                {
                    var selectedCells = tableView.GetAllSelectedCells();
                    if (selectedCells.Any() && _currentTable != null)
                    {
                        int row = selectedCells.First().Y; 
                        
                        if (row >= 0 && row < _currentTable.Rows.Count)
                        {
                            string songId = _currentTable.Rows[row]["SongID"]?.ToString();
                            
                            if (!string.IsNullOrEmpty(songId))
                            {
                                var selectedSong = _musesService.Songs.FirstOrDefault(x => x.SongID == songId);

                                if (selectedSong != null)
                                {
                                    // Show playlist selection dialog
                                    var playlists = _musesService.Playlists;
                                    var playlistNames = playlists.Select(p => p.PlaylistName).ToArray();

                                    int? selectedIndex = MessageBox.Query(_app, "Add to Playlist", "Select a playlist to add:", playlistNames);
                                    
                                    int selectedIndexInt = selectedIndex ?? -1;
                                    
                                    if (selectedIndexInt >= 0 && selectedIndex < playlists.Count)
                                    {
                                        var selectedPlaylist = playlists[selectedIndexInt];
                                        if (_musesService.AddSongToPlaylist(selectedPlaylist.PlaylistID, selectedSong.SongID))
                                        {
                                            MessageBox.Query(_app, "Success", $"Added {selectedSong.Title} to {selectedPlaylist.PlaylistName}!", "OK");
                                        }
                                        else
                                        {
                                            MessageBox.Query(_app, "Error", $"Failed to add {selectedSong.Title} to {selectedPlaylist.PlaylistName}.", "OK");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    e.Handled = true;
                }
            };
                             

            tableContainer.Add(tableView);
            targetContentFrame.Add(lblSearch, txtSearch, tableContainer);
        }
        private void RenderSong_PlayQueue(IEnumerable<Song> initialSongs, Window targetContentFrame)
        {
            targetContentFrame.RemoveAll(); 
            
            var tableContainer = new Window() {
                X = 0, Y = 0, 
                Width = Dim.Fill(), Height = Dim.Fill(),
                BorderStyle = LineStyle.Single,
                Title = "Play Queue - Enter to play, Delete (Backspace) to remove"
            };
            
            var tableView = new TableView() {
                X = 0, Y = 0,
                Width = Dim.Fill(), Height = Dim.Fill(),
                FullRowSelect = true
            };
            
            DataTable _currentTable = UpdateTableSongData(initialSongs, tableView);
            
            // Table key events for managing i>queue (Delete to remove, Enter to play)
            tableView.KeyDown += (s, e) =>
            {
                var selectedCells = tableView.GetAllSelectedCells();
                if (!selectedCells.Any() || _currentTable == null) return;

                int row = selectedCells.First().Y;
                if (row < 0 || row >= _currentTable.Rows.Count) return;

                string songId = _currentTable.Rows[row]["SongID"]?.ToString();
                var selectedSong = _musesService.PlayQueue.ElementAtOrDefault(row);

                if (selectedSong == null || selectedSong.SongID != songId) return;

                if (e.KeyCode == Key.DeleteChar || e.KeyCode == Key.Backspace)
                {
                    _musesService.PlayQueue.RemoveAt(row);
                    _currentTable = UpdateTableSongData(_musesService.PlayQueue, tableView);
                    MessageBox.Query(_app, "Play Queue", $"Removed: {selectedSong.Title}", "OK");
                    e.Handled = true;
                }

                if (e.KeyCode == Key.Enter)
                {
                    BtnPlayPause.SetFocus();
                    _musesService.CurrentSong = selectedSong;
                    _musesService.CurrentSongIndex = row;
                    e.Handled = true;
                }
            };

            tableContainer.Add(tableView);
            targetContentFrame.Add(tableContainer);
        }

        
        // Còn update
        private void RenderArtistsToTable(IEnumerable<Artist> artists)
        {
            _userMainContentFrame.RemoveAll();
            
             var tableView = new TableView()
             {
                 X = 0, Y = 0,
                 Width = Dim.Fill(),
                 Height = Dim.Fill(),
                 FullRowSelect = true
             };
             
             UpdateTableArtistData(artists, tableView);
             _userMainContentFrame.Add(tableView);
        }

        // Xong
        private void RenderPlaylistsToTable(IEnumerable<Playlist> playlists, Window targetContentFrame)
        {
            targetContentFrame.RemoveAll();

            var tableContainer = new Window()
            {
                X = 0, Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                BorderStyle = LineStyle.Single,
                Title = "Playlists - Enter to play, Delete (Backspace) to remove, Space to see Song List"
            };

            var tableView = new TableView()
            {
                X = 0, Y = 0,
                Width = Dim.Fill(), Height = Dim.Fill(),
                FullRowSelect = true
            };

            DataTable _currentTable = UpdateTablePlaylistData(playlists, tableView);

            // Table key events for managing i>queue (Delete to remove, Enter to play)
            tableView.KeyDown += (s, e) =>
            {
                var selectedCells = tableView.GetAllSelectedCells();
                if (!selectedCells.Any() || _currentTable == null) return;

                int row = selectedCells.First().Y;
                if (row < 0 || row >= _currentTable.Rows.Count) return;

                string playlistId = _currentTable.Rows[row]["PlaylistID"]?.ToString();
                var selectedPlaylist = _musesService.Playlists.FirstOrDefault(x => x.PlaylistID == playlistId);

                if (selectedPlaylist == null) return;

                if (e.KeyCode == Key.DeleteChar || e.KeyCode == Key.Backspace)
                {
                    _musesService.DeletePlaylist(playlistId, _musesService.User.UserID);
                    _currentTable = UpdateTablePlaylistData(_musesService.Playlists, tableView);
                    MessageBox.Query(_app, "Playlists", $"Deleted: {selectedPlaylist.PlaylistName}", "OK");
                    e.Handled = true;
                }

                if (e.KeyCode == Key.Enter)
                {
                    _musesService.CurrentPlaylist = selectedPlaylist;
                    _musesService.LoadPlaylistToPlayQueue();
                    MessageBox.Query(_app, "Playlists", $"Playing playlist: {selectedPlaylist.PlaylistName}", "OK");
                    e.Handled = true;
                }
            };
            
            // Space to see song list
            tableView.KeyDown += (s, e) =>
            {
                if (e.KeyCode != Key.Space)
                {
                    return;
                }
                
                var selectedCells = tableView.GetAllSelectedCells();
                
                if (selectedCells.Any() && _currentTable != null)
                {
                    int row = selectedCells.First().Y; 
                    
                    if (row >= 0 && row < _currentTable.Rows.Count)
                    {
                        string playlistId = _currentTable.Rows[row]["PlaylistID"]?.ToString();
                        
                        if (!string.IsNullOrEmpty(playlistId))
                        {
                            var selectedPlaylist = _musesService.Playlists.FirstOrDefault(x => x.PlaylistID == playlistId);

                            if (selectedPlaylist != null)
                            {
                                RenderPlaylistSongsToTable(selectedPlaylist, targetContentFrame);
                                e.Handled = true;
                            }
                        }
                    }
                }
            };

            tableContainer.Add(tableView);
            targetContentFrame.Add(tableContainer);
        }
    
        // Xong
        private void RenderPlaylistSongsToTable(Playlist playlist, Window targetContentFrame)
        {
            targetContentFrame.RemoveAll();

            var btnReturn = new Button()
            {
                Title = "Return to Playlists",
                X = 0,
                Y = 0,
                Width = 30
            };

            var tableContainer = new Window()
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                BorderStyle = LineStyle.Single,
                Title = $"Songs in {playlist.PlaylistName}"
            };

            var tableView = new TableView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                FullRowSelect = true
            };

            var songs = _musesService.GetPlaylistSongs(playlist.PlaylistID);
            DataTable _currentTable = UpdateTableSongData(songs, tableView);

            btnReturn.Accepted += (s, e) =>
            {
                RenderPlaylistsToTable(_musesService.Playlists, targetContentFrame);
                e.Handled = true;
            };

            btnReturn.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Key.CursorDown)
                {
                    tableView.SetFocus();
                    e.Handled = true;
                }
            };
            
            tableView.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Key.CursorUp)
                {
                    btnReturn.SetFocus();
                    e.Handled = true;
                }
                
                // Delete song from playlist
                if (e.KeyCode == Key.DeleteChar || e.KeyCode == Key.Backspace)
                {
                    var selectedCells = tableView.GetAllSelectedCells();
                    if (!selectedCells.Any() || _currentTable == null) return;

                    int row = selectedCells.First().Y;
                    if (row < 0 || row >= _currentTable.Rows.Count) return;

                    string songId = _currentTable.Rows[row]["SongID"]?.ToString();
                    if (string.IsNullOrEmpty(songId)) return;

                    var selectedSongTitle = _currentTable.Rows[row]["Title"]?.ToString() ?? songId;

                    if (_musesService.RemoveSongFromPlaylist(playlist.PlaylistID, songId, _musesService.User.UserID))
                    {
                        songs = _musesService.GetPlaylistSongs(playlist.PlaylistID);
                        _currentTable = UpdateTableSongData(songs, tableView);
                        MessageBox.Query(_app, "Playlist Songs", $"Removed: {selectedSongTitle}", "OK");
                    }
                    else
                    {
                        MessageBox.Query(_app, "Error", $"Failed to remove: {selectedSongTitle}", "OK");
                    }

                    e.Handled = true;
                }
            };
            

            tableContainer.Add(tableView);
            targetContentFrame.Add(btnReturn, tableContainer);
            tableView.SetFocus();
        }
        
        
        // Cập nhật liên tục hiển thị ở Control Panel (_userFooterFrame)
        private void WireUpMusicEngineEvents()
        {
            _musesService.OnSongChanged += (song) => {
                _app.Invoke(() => {
                    string title = $"🎵 Playing: {song.Title} - {song.ArtistNames}";
                    LblNowPlaying.Text = title.Length > 35 ? title.Substring(0, 32) + "..." : title;
            
                    LblNowPlaying.SetNeedsDraw();
                    LblNowPlaying.SuperView?.SetNeedsDraw();
                });
            };

            _musesService.OnPlayStateChanged += (isPlaying) => {
                _app.Invoke(() => {
                    BtnPlayPause.Title = isPlaying ? " || " : " > ";
            
                    BtnPlayPause.SetNeedsDraw();
                    BtnPlayPause.SuperView?.SetNeedsDraw();
                });
            };
    
            _musesService.OnTimeUpdated += (timeStr) => {
                _app.Invoke(() => {
                    LblTime.Text = timeStr;
            
                    LblTime.SetNeedsDraw();
                    LblTime.SuperView?.SetNeedsDraw();
                });
            };
        }
    }
}
