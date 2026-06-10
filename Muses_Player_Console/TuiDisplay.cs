using System.Data;
using Terminal.Gui.Views;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Drawing;
using Terminal.Gui.App;
using Terminal.Gui.Input;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.JavaScript;

namespace Muses_Player_Console
{
    public class TuiDisplay : Window
    {
        private readonly MusesService _musesService;
        private readonly IApplication _app; 
        
        private Window _loginWindow;
        private Window _registerWindow;
        private Window _createPlaylistWindow;
        private Window _createSongWindow;
        private Window _registerArtistWindow;
        
        private Window _userDashboard;
        private Window _userMenuFrame;
        private Window _userMainContentFrame;
        private Window _userFooterFrame;
        
        private Window _artistDashboard;
        private Window _artistMenuFrame;
        private Window _artistMainContentFrame;
        
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
                Height = Dim.Fill(),
            };

            _artistMainContentFrame = new Window()
            {
                Title = "Main Content",
                X = 20, Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
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
            
            InitRegisterWindow();
            
            InitGuestWindow();
            
            InitUserWindow();
            
            InitCreatePlaylistWindow();
            
            InitRegisterArtistWindow();
            
            InitArtistWindow();
            
            InitCreateSongWindow();
            
            this.Add(_loginWindow, _registerWindow, _createPlaylistWindow, _registerArtistWindow, _createSongWindow, _guestDashboard, _userDashboard, _artistDashboard);
            
            _registerWindow.Visible = false;
            _createPlaylistWindow.Visible = false;
            _guestDashboard.Visible = false;
            _userDashboard.Visible = false;
            _artistDashboard.Visible = false;
            _createSongWindow.Visible = false;
            _registerArtistWindow.Visible = false;
            
            WireUpMusicEngineEvents();
        }
        
        private void InitLoginWindow()
        {
            _loginWindow = new Window () {
                Title = "Welcome to Muses Player! (Esc to exit)",
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
            var txtUser = new TextField () { Text = "", X = 12, Y = 3, Width = 22 };

            var lblPass = new Label () { Text = "Password:", X = 2, Y = 5 };
            var txtPass = new TextField () { Text = "", X = 12, Y = 5, Width = 22, Secret = true };

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
                    _registerWindow.Visible = false;
                    _userDashboard.Visible = true;
                    _userMenuFrame.SetFocus();
                } else {
                    MessageBox.Query(_app, "Failed", "Invalid username or password!", "Try again");
                }
            };
            
            // Event handler for Register button (register)
            btnRegister.Accepted += (s, e) =>
            {
                _registerWindow.Visible = true;
                _loginWindow.Visible = false;
                _registerWindow.SetFocus();
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

        private void InitRegisterWindow()
        {
            _registerWindow = new Window()
            {
                Title = "Register",
                X = Pos.Center(), Y = Pos.Center(),
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                BorderStyle = LineStyle.Double,
            };

            var lblRegister = new Label() { Text = "REGISTER", X = Pos.Center(), Y = 1 };
            
            var lblUser = new Label () { Text = "Username:", X = 2, Y = 3 };
            var txtUser = new TextField () { Text = "", X = 20, Y = 3, Width = 20 };
                
            var lblEmail = new Label() { Text = "Email:", X = 2, Y = 5 };
            var txtEmail = new TextField() { Text = "", X = 20, Y = 5, Width = 20 };

            var lblPass = new Label () { Text = "Password:", X = 2, Y = 7 };
            var txtPass = new TextField () { Text = "", X = 20, Y = 7, Width = 20, Secret = true };
            
            var lblConfirmPass = new Label() { Text = "Confirm Password:", X = 2, Y = 9 };
            var txtConfirmPass = new TextField() { Text = "", X = 20, Y = 9, Width = 20, Secret = true };

            var btnRegister = new Button () { Title = "Register", X = Pos.Center(), Y = 12 };
            
            var registerFrame = new Window()
            {
                X = Pos.Center(),
                Y = Pos.Center(),
                Width = 45,
                Height = 17,
                BorderStyle = LineStyle.Double,
            };
            
            
            btnRegister.Accepted += (s, e) => {
                string uName = txtUser.Text.ToString().Trim();
                string eMail = txtEmail.Text.ToString().Trim();
                string pWord = txtPass.Text.ToString().Trim();

                if (string.IsNullOrEmpty(uName) || string.IsNullOrEmpty(pWord) || string.IsNullOrEmpty(eMail)) {
                    MessageBox.Query(_app, "Error", "Username, email, password cannot be empty!", "OK");
                    return;
                }

                if (pWord != txtConfirmPass.Text.ToString().Trim())
                {
                    MessageBox.Query(_app, "Error", "Password and confirm password do not match!", "OK");
                    return;
                }

                if (_musesService.Register(uName, eMail, pWord)) {
                    txtUser.Text = "";
                    txtEmail.Text = "";
                    txtPass.Text = "";
                    txtConfirmPass.Text = "";
                    
                    txtUser.SetFocus(); 

                    MessageBox.Query(_app, "Success", "Register successfully!, Please login to continue", "OK");
                    _registerWindow.Visible = false;
                    _loginWindow.Visible = true;
                    _loginWindow.SetFocus();
                } else {
                    MessageBox.Query(_app, "Failed", "Invalid username, email or password!", "Try again");
                }
            };
            
            registerFrame.Add(lblRegister, lblUser, txtUser, lblEmail, txtEmail, lblPass, txtPass, lblConfirmPass, txtConfirmPass, btnRegister);
            _registerWindow.Add(registerFrame);
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
                "5. View Categories",
                "6. Switch to Artist",
                "7. Register Artist"
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
                        _userMainContentFrame.RemoveAll();
                        _loginWindow.SetFocus();
                        break;
                    case 1:
                        _musesService.GetAllSongs();
                        RenderSongsUser(_musesService.Songs, _userMainContentFrame);
                        break;
                    case 2:
                        RenderSongs_PlayQueue(_musesService.PlayQueue, _userMainContentFrame);
                        break;
                    case 3:
                        _musesService.GetPlaylists();
                        RenderPlaylists(_musesService.Playlists, _userMainContentFrame);
                        break;
                    case 4:
                        _musesService.GetAllArtists();
                        RenderArtists(_musesService.Artists, _userMainContentFrame);
                        break;
                    case 5:
                        _musesService.GetAllCategories();
                        RenderCategories(_musesService.Categories, _userMainContentFrame);
                        break;
                    case 6:
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
                    case 7:
                        if (_musesService.IsArtist())
                        {
                            MessageBox.Query(_app, "Error", "You are already an Artist!", "OK");
                        }
                        else
                        {
                            _registerArtistWindow.Visible = true;
                            _registerArtistWindow.SetFocus();
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

        private void InitCreatePlaylistWindow()
        {
            _createPlaylistWindow = new Window()
            {
                Title = "Create Playlist",
                X = Pos.Center(), Y = Pos.Center(),
                Width = 40,
                Height = 10,
                BorderStyle = LineStyle.Double,
            };

            var lblName = new Label() { Text = "Playlist Name:", X = 2, Y = 1 };
            var txtName = new TextField() { Text = "", X = 2, Y = 2, Width = 33 };
            
            var checkFav = new CheckBox() { Title = "Favorite", X = 2, Y = 4, Value = CheckState.UnChecked,};

            var btnAccept = new Button() { Title = "Accept", X = 26, Y = 6 };
            
            var btnCancel = new Button() { Title = "Cancel", X = 2, Y = 6 };

            btnAccept.Accepted += (s, e) =>
            {
                string pName = txtName.Text.ToString().Trim();
                bool isFav = checkFav.Value == CheckState.Checked;
                
                if (_musesService.CreateNewPlaylist(pName, _musesService.User.UserID, isFav))
                {
                    txtName.Text = "";
                    checkFav.Value = CheckState.UnChecked;
                    MessageBox.Query(_app, "Success", "Create Playlist successfully!", "OK");
                    _createPlaylistWindow.Visible = false;
                }
                else
                {
                    MessageBox.Query(_app, "Failed", "Create Playlist failed!", "Try again");
                }
            };

            btnCancel.Accepted += (s, e) =>
            {
                txtName.Text = "";
                checkFav.Value = CheckState.UnChecked;
                MessageBox.Query(_app, "Cancel", "Create Playlist canceled!", "OK");
                _createPlaylistWindow.Visible = false;
            };
            
            _createPlaylistWindow.Add(lblName, txtName, checkFav, btnAccept, btnCancel);
        }

        private void InitRegisterArtistWindow()
        {
            _registerArtistWindow = new Window()
            {
                Title = "Register as Artist",
                X = Pos.Center(), Y = Pos.Center(),
                Width = 40,
                Height = 12,
                BorderStyle = LineStyle.Double,
            };
            
            var lblName = new Label() { Text = "Artist Name:", X = 2, Y = 1 };
            var txtName = new TextField() { Text = "", X = 2, Y = 2, Width = 33 };
            
            var lblBio = new Label() { Text = "Bio:", X = 2, Y = 4 };
            var txtBio = new TextField() { Text = "", X = 2, Y = 5, Width = 33 };

            var btnAccept = new Button() { Title = "Accept", X = 26, Y = 8 };
            
            var btnCancel = new Button() { Title = "Cancel", X = 2, Y = 8 };

            btnAccept.Accepted += (s, e) =>
            {
                string aName = txtName.Text.ToString().Trim();
                string aBio = txtBio.Text.ToString().Trim();
                
                if(_musesService.AddNewArtist(aName, aBio, ""))
                {
                    txtName.Text = "";
                    txtBio.Text = "";
                    MessageBox.Query(_app, "Success", "Register as Artist successfully!", "OK");
                    _registerArtistWindow.Visible = false;
                }
                else
                {
                    MessageBox.Query(_app, "Failed", "Register as Artist failed!", "Try again");
                }
            };

            btnCancel.Accepted += (s, e) =>
            {
                txtName.Text = "";
                txtBio.Text = "";
                MessageBox.Query(_app, "Cancel", "Register as Artist canceled!", "OK");
                _registerArtistWindow.Visible = false;
            };
            
            _registerArtistWindow.Add(lblName, txtName, lblBio, txtBio, btnAccept, btnCancel);
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
                 "3. View Categories",
                 "4. View Artists",
                 "5. Add a new Song"
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
                        RenderSongs_DeleteSong(_musesService.Artist.MySongs, _artistMainContentFrame);
                        break;
                    case 2:
                        RenderArtistInfo(_musesService.Artist, _artistMainContentFrame);
                        break;
                    case 3:
                        _musesService.GetAllCategories();
                        RenderCategories(_musesService.Categories, _artistMainContentFrame);
                        break;
                    case 4:
                        _musesService.GetAllArtists();
                        RenderArtists(_musesService.Artists, _artistMainContentFrame);
                        break;
                    case 5:
                        _createSongWindow.Visible = true;
                        _createSongWindow.SetFocus();
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

        private void InitCreateSongWindow()
        {
            _createSongWindow = new Window()
            {
                Title = "Add a new Song",
                X = 20,
                Y = Pos.AnchorEnd(15),
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                BorderStyle = LineStyle.RoundedDotted
            };

            var lblTitle = new Label() { Text = "Title:", X = 5, Y = 1 };
            var txtTitle = new TextField() { Text = "", X = 5, Y = 2, Width = 50 };
            
            var lblAltTitle = new Label() { Text = "Alt Title (optional)", X = 5, Y = 3 };
            var txtAltTitle = new TextField() { Text = "", X = 5, Y = 4, Width = 50 };
            
            var lblDuration = new Label() { Text = "Duration (sec):", X = 5, Y = 5 };
            var txtDuration = new TextField() { Text = "", X = 5, Y = 6, Width = 50 };
            
            var lblReleaseDate = new Label() { Text = "Release Date (yyyy-mm-dd):", X = 60, Y = 1 };
            var txtReleaseDate = new TextField() { Text = "", X = 60, Y = 2, Width = 50 };
            
            var lblArtistId = new Label() { Text = "Artist IDs (optional) (comma separated):", X = 60, Y = 3 };
            var txtArtistId = new TextField() { Text = "", X = 60, Y = 4, Width = 50 };
            
            var lblCategoryId = new Label() { Text = "Category IDs (comma separated):", X = 60, Y = 5 };
            var txtCategoryId = new TextField() { Text = "", X = 60, Y = 6, Width = 50 };

            var lblAudioUrl = new Label() { Text = "Audio URL:", X = 5, Y = 8 };
            var txtAudioUrl = new TextField() {Text = "", X = 5, Y = 9, Width = 105 };
            
            var btnAccept = new Button() { Title = "Accept", X = Pos.Center(), Y = 11 };

            btnAccept.Accepted += (s, e) =>
            {
                try
                {
                    string sTitle = txtTitle.Text.ToString().Trim();
                    string sAltTitle = txtAltTitle.Text.ToString().Trim();
                    int iDuration = Int32.Parse(txtDuration.Text.ToString().Trim());
                    DateTime dReleaseDate = DateTime.Parse(txtReleaseDate.Text.ToString().Trim());
                    string sArtistId;
                    if (string.IsNullOrEmpty(txtArtistId.Text.ToString().Trim()))
                    {
                        sArtistId = _musesService.Artist.ArtistID;
                    }
                    else
                    {
                        sArtistId = _musesService.Artist.ArtistID + ',' + txtArtistId.Text.ToString().Trim();
                    }

                    string sCategoryID = txtCategoryId.Text.ToString().Trim();
                    string sAudioUrl = txtAudioUrl.Text.ToString().Trim();

                    MessageBox.Query(_app, "Warning", $"Title: {sTitle}\nAltTitle: {sAltTitle}\n" +
                                                      $"Duration: {iDuration} seconds\nRelease Date: {dReleaseDate.ToShortDateString()}\n" +
                                                      $"ArtistID: {sArtistId}\nCategoryID: {sCategoryID}\n" +
                                                      $"AudioURL: {sAudioUrl}", "OK");

                    if (_musesService.CreateNewSong(sTitle, sAltTitle, iDuration, dReleaseDate, sArtistId, sCategoryID,
                            sAudioUrl))
                    {
                        txtTitle.Text = "";
                        txtAltTitle.Text = "";
                        txtDuration.Text = "";
                        txtReleaseDate.Text = "";
                        txtArtistId.Text = "";
                        txtCategoryId.Text = "";
                        txtAudioUrl.Text = "";
                        MessageBox.Query(_app, "Success", "Create Song successfully!", "OK");
                        _createSongWindow.Visible = false;
                    }
                    else
                    {
                        MessageBox.Query(_app, "Failed", "Create Song failed!", "Try again");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Query(_app, "Failed", "Create Song failed!", ex.Message);
                }
            };
            
            _createSongWindow.Add(lblTitle, txtTitle, lblAltTitle, txtAltTitle, lblDuration,
                txtDuration, lblReleaseDate, txtReleaseDate, lblArtistId, txtArtistId, lblCategoryId,
                txtCategoryId, lblAudioUrl, txtAudioUrl, btnAccept);
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
        
        private DataTable UpdateTableCategoryData(IEnumerable<Category> categoriesToRender, TableView tableView)
        {
            var tableData = new DataTable();
            tableData.Columns.Add("CategoryID", typeof(string));
            tableData.Columns.Add("CategoryName", typeof(string));

            foreach (var category in categoriesToRender)
            {
                tableData.Rows.Add(
                    category.CategoryID,
                    category.CategoryName
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

            tableContainer.Add(tableView);
            targetContentFrame.Add(lblSearch, txtSearch, tableContainer);
        }
        private void RenderSongsUser(IEnumerable<Song> initialSongs, Window targetContentFrame)
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
        private void RenderSongs_PlayQueue(IEnumerable<Song> initialSongs, Window targetContentFrame)
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
                    _currentTable = UpdateTableSongData(initialSongs, tableView);
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

        private void RenderSongs_DeleteSong(IEnumerable<Song> initialSongs, Window targetContentFrame)
        {
            targetContentFrame.RemoveAll(); 
            
            var tableContainer = new Window() {
                X = 0, Y = 0, 
                Width = Dim.Fill(), Height = Dim.Fill(),
                BorderStyle = LineStyle.Single,
                Title = "My Songs - Delete (Backspace) to delete"
            };
            
            var tableView = new TableView() {
                X = 0, Y = 0,
                Width = Dim.Fill(), Height = Dim.Fill(),
                FullRowSelect = true
            };
            
            DataTable _currentTable = UpdateTableSongData(initialSongs, tableView);
            
            // Delete (Backspace) to delete
            tableView.KeyDown += (s, e) =>
            {
                var selectedCells = tableView.GetAllSelectedCells();
                if (!selectedCells.Any() || _currentTable == null) return;

                int row = selectedCells.First().Y;
                if (row < 0 || row >= _currentTable.Rows.Count) return;

                string songId = _currentTable.Rows[row]["SongID"]?.ToString();
                var selectedSong = _musesService.Artist.MySongs.FirstOrDefault(x => x.SongID == songId);

                if (selectedSong == null || selectedSong.SongID != songId) return;

                if (e.KeyCode == Key.DeleteChar || e.KeyCode == Key.Backspace)
                {
                    _musesService.DeleteSong(selectedSong.SongID, _musesService.Artist.ArtistID);
                    _currentTable = UpdateTableSongData(initialSongs, tableView);
                    MessageBox.Query(_app, "Success", $"Deleted: {selectedSong.Title}", "OK");
                    e.Handled = true;
                }
            };

            tableContainer.Add(tableView);
            targetContentFrame.Add(tableContainer);
        }

        
        private void RenderArtists(IEnumerable<Artist> artists, Window targetContentFrame)
        {
            targetContentFrame.RemoveAll();
            
             var tableView = new TableView()
             {
                 X = 0, Y = 0,
                 Width = Dim.Fill(),
                 Height = Dim.Fill(),
                 FullRowSelect = true
             };
             
             UpdateTableArtistData(artists, tableView);
             targetContentFrame.Add(tableView);
        }
        private void RenderArtistInfo(Artist artist, Window targetContentFrame)
        {
            MessageBox.Query(_app, "Warning", "This feature is not available yet.", "OK");
        }


        private void RenderPlaylists(IEnumerable<Playlist> playlists, Window targetContentFrame)
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
                X = 0, Y = 2,
                Width = Dim.Fill(), Height = Dim.Fill(),
                FullRowSelect = true
            };
            
            
            var btnCreatePlaylist = new Button()
            {
                Title = "Create Playlist",
                X = 0,
                Y = 1,
                Width = 30,
                ShadowStyle = ShadowStyles.None
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
                                RenderPlaylistSongs(selectedPlaylist, targetContentFrame);
                                e.Handled = true;
                            }
                        }
                    }
                }
            };
            
            // A to create playlist
            btnCreatePlaylist.Accepted += (s, e) =>
            {
                _createPlaylistWindow.Visible = true;
                _createPlaylistWindow.SetFocus();
                _currentTable = UpdateTablePlaylistData(_musesService.Playlists, tableView);
            };

            tableContainer.Add(tableView, btnCreatePlaylist);
            targetContentFrame.Add(tableContainer);
        }
    

        private void RenderPlaylistSongs(Playlist playlist, Window targetContentFrame)
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
                Y = 2,
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
                RenderPlaylists(_musesService.Playlists, targetContentFrame);
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

        private void RenderCategories(IEnumerable<Category> categories, Window targetContentFrame)
        {
            targetContentFrame.RemoveAll();

            var lblSearch = new Label() { Text = "Search:", X = 1, Y = 2 };
            var txtSearch = new TextField() { Text = "", X = 10, Y = 2, Width = Dim.Fill() - 2 };

            var tableContainer = new Window()
            {
                X = 0,
                Y = 4,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                BorderStyle = LineStyle.Single,
                Title = "Categories"
            };

            var tableView = new TableView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                FullRowSelect = true
            };

            UpdateTableCategoryData(categories, tableView);

            txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Key.Enter)
                {
                    string keyword = txtSearch.Text.ToString().Trim();

                    if (string.IsNullOrWhiteSpace(keyword))
                    {
                        UpdateTableCategoryData(categories, tableView);
                    }
                    else
                    {
                        List<Category> filteredCategories = _musesService.FindCategory(keyword);
                        UpdateTableCategoryData(filteredCategories, tableView);
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

            tableContainer.Add(tableView);
            targetContentFrame.Add(lblSearch, txtSearch, tableContainer);
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
