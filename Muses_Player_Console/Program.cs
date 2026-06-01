namespace Muses_Player_Console;

class Program
{

    public static void Main(string[] args)
    {
        Console.WriteLine("Muses Player Console Application");
        Console.WriteLine("This is a placeholder for testing database connectivity and other backend features.");
        
        MusesService musesService = new MusesService();
        musesService = LoginMenu(musesService);
        if (musesService.IsLoggedIn == false)
        {
            Console.WriteLine("You are continuing as a Guest. Some features may be limited.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Program program = new Program();
            program.ProcessAsGuest(musesService);
        }
        else
        {
            Program program = new Program();
            program.ProcessAsUser(musesService);
        }
    }

    public static MusesService LoginMenu(MusesService musesService)
    {
        Console.WriteLine("1. Continue as Guest");
        Console.WriteLine("2. Login");
        Console.Write("Enter your choice: ");

        string input;
        do
        {
            input = Console.ReadLine() ?? string.Empty;
        
            switch (input)
            {
                case "1":
                    Console.WriteLine("Continuing as Guest...");
                    return musesService;
                
                case "2":
                    bool loginSuccess = false;
                    while (!loginSuccess)
                    {
                        Console.WriteLine("Please enter your username and password to log in.");
                        Console.Write("Username: ");
                        string username = Console.ReadLine() ?? string.Empty;
                        Console.Write("Password: ");
                        string password = Console.ReadLine() ?? string.Empty;

                        if (musesService.Login(username, password))
                        {
                            Console.WriteLine("Welcome to the Muses Player Console Application!");
                            loginSuccess = true;
                        }
                        else
                        {
                            Console.WriteLine("Invalid username or password. Please try again.");
                        }
                    }

                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                    return musesService;
                
                default:
                    Console.WriteLine("Invalid choice. Please select 1 or 2.");
                    Console.Write("Enter your choice: ");
                    continue;
            }
        } while (true);
    }

    public static void UserMenu()
    {
        Console.WriteLine("Welcome to the Muses Player Console Application!");
        Console.WriteLine("Please select an option:");
        Console.WriteLine("1. View All Songs");
        Console.WriteLine("2. View All Playlists");
        Console.WriteLine("3. View All Artists");
        Console.WriteLine("4. Select playlist to play");
        Console.WriteLine("5. Create new playlist");
        Console.WriteLine("6. Add song to playlist");
        Console.WriteLine("7. Remove song from playlist");
        Console.WriteLine("8. Change song order in playlist");
        Console.WriteLine("9. Delete playlist");
        Console.WriteLine("10. Login as a Artist");
        Console.WriteLine("---------------------------------------------");
        Console.WriteLine("0. Exit");
    }
    public static void ArtistMenu()
    {
        Console.WriteLine("Welcome to the Muses Player Console Application!");
        Console.WriteLine("Please select an option:");
        Console.WriteLine("1. View My Songs");
        Console.WriteLine("2. Create new song");
        Console.WriteLine("3. Delete song");
        Console.WriteLine("4. Update song");
        Console.WriteLine("------------------------------------------------------");
        Console.WriteLine("0. Switch back to User Mode");
    }

    public void ProcessAsGuest(MusesService musesService)
    {
        int choice = -1;
        while (choice != 0)
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("Welcome to the Muses Player Console Application!");
            Console.WriteLine("1. View All Songs");
            Console.WriteLine("2. View All Artists");
            Console.WriteLine("0. Exit");
            Console.WriteLine("---------------------------------------------");
            Console.Write("Enter your choice: ");
            string input = Console.ReadLine() ?? string.Empty;
            if (!int.TryParse(input, out choice))
            {
                Console.WriteLine("Invalid input. Please enter a number.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                continue;
            }

            switch (choice)
            {
                case 0:
                    Console.WriteLine("Exiting the application.");
                    break;
                case 1:
                    musesService.GetAllSongs();
                    musesService.PrintAllSongs();
                    break;
                case 2:
                    musesService.GetAllArtists();
                    musesService.PrintAllArtists();
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please select a valid option.");
                    break;
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    public void ProcessAsArtist(MusesService musesService)
    {
        int choice = -1;
        while (choice != 0)
        {
            Console.Clear();
            musesService.PrintArtistInfo();
            Console.WriteLine();
            ArtistMenu();
            Console.Write("Enter your choice: ");
            string input = Console.ReadLine() ?? string.Empty;
            if (!int.TryParse(input, out choice))
            {
                Console.WriteLine("Invalid input. Please enter a number.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                continue;
            }
            switch (choice)
            {
                case 0:
                    musesService.SwitchToUserMode();
                    return;
                case 1:
                    musesService.GetArtistSongs(musesService.Artist.ArtistID);
                    musesService.PrintArtistSongs();
                    break;
                case 2:
                    musesService.CreateNewSongInteractive();
                    break;
                case 3:
                    musesService.DeleteSongInteractive();
                    break;
                case 4:
                    musesService.UpdateSongInteractive();
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please select a valid option.");
                    break;
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }
    }
    public void ProcessAsUser(MusesService musesService)
    {
        int choice = -1;
        while (choice != 0)
        {
            Console.Clear();
            musesService.PrintUserInfo();
            Console.WriteLine();
            UserMenu();
            Console.Write("Enter your choice: ");
            string input = Console.ReadLine() ?? string.Empty;
            if (!int.TryParse(input, out choice))
            {
                Console.WriteLine("Invalid input. Please enter a number.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                continue;
            }

            switch (choice)
            {
                case 0:
                    Console.WriteLine("Exiting the application.");
                    break;
                case 1:
                    musesService.GetAllSongs();
                    musesService.PrintAllSongs();
                    break;
                case 2:
                    musesService.GetPlaylists();
                    musesService.PrintPlaylists();
                    break;
                case 3:
                    musesService.GetAllArtists();
                    musesService.PrintAllArtists();
                    break;
                case 4:
                    musesService.GetPlaylists();
                    musesService.SelectPlaylistToPlay();
                    break;
                case 5:
                    musesService.CreateNewPlaylistInteractive();
                    break;
                case 6:
                    musesService.AddSongToPlaylistInteractive();
                    break;
                case 7:
                    musesService.RemoveSongFromPlaylistInteractive();
                    break;
                case 8:
                    musesService.SwapSongsInPlaylistInteractive();
                    break;  
                case 9: 
                    musesService.DeletePlaylistInteractive();
                    break;
                case 10:
                    if (musesService.SwitchToArtistMode())
                    {
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        ProcessAsArtist(musesService);
                    }
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please select a valid option.");
                    break;
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }
    }
}