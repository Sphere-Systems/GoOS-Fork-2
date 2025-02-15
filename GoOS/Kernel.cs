
/////// ekeleze ///////
// I hate xrc2 code. // 
///////////////////////

using Cosmos.HAL;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.TCP;
using Cosmos.System.Network.IPv4.UDP.DHCP;
using System;
using System.Collections.Generic;
using Sys = Cosmos.System;
using System.IO;
using System.Text;
using GoOS.Themes;
using GoOS.Commands;
using Console = BetterConsole;
using ConsoleColor = PrismAPI.Graphics.Color;
using static GoOS.Core;
using System.Threading;
using PrismAPI.Graphics;
using IL2CPU.API.Attribs;
using PrismAPI.Hardware.GPU;
using GoOS.GUI;
using GoOS.GUI.Apps;
using GoOS.Networking;

// Goplex Studios - GoOS
// Copyright (C) 2022  Owen2k6

namespace GoOS
{
    public class Kernel : Sys.Kernel
    {

        public static Dictionary<string, string> InstalledPrograms = new Dictionary<string, string>() { };

        public static bool isGCIenabled = false;

        //Vars for OS
        public static string version = "1.5";
        public static string BuildType = "Beta";
        public static string olddir = @"0:\";

        public static Sys.FileSystem.CosmosVFS FS;

        public static string username = null;
        public static string computername = null;

        [ManifestResourceStream(ResourceName = "GoOS.Resources.GoOS_Intro.bmp")] public static byte[] rawBootLogo;

        protected override void BeforeRun()
        {
            WindowManager.Canvas = Display.GetDisplay(800, 600); //TODO: Not have this hard coded >:^(
            Console.Init(800, 600);

            var loadingDialogue = new LoadingDialogue("GoOS is starting\nPlease wait...");
            WindowManager.AddWindow(loadingDialogue);

            ThemeManager.SetTheme(Theme.Fallback);
            log(ThemeManager.WindowText, "GoOS - Starting GoOS...");
            try
            {
                FS = new Sys.FileSystem.CosmosVFS();
                Sys.FileSystem.VFS.VFSManager.RegisterVFS(FS);
                FS.Initialize(true);
                var total_space = FS.GetTotalSize(@"0:\");
            }
            catch
            {
                log(ThemeManager.ErrorText, "GoOS - Failed to initialize filesystem.\n");
                log(ThemeManager.ErrorText, "GoOS - GoOS Needs a HDD installed to save user settings, application data and more.\n");
                log(ThemeManager.ErrorText, "GoOS - Please verify that your hard disk is plugged in correctly.");
                while (true) { }
            }
            

            if (!File.Exists(@"0:\content\sys\setup.gms"))
            {
                Console.ConsoleMode = true;
                WindowManager.AddWindow(new GTerm());
                Console.WriteLine("First boot... This may take awhile...");
                OOBE.Launch();
            }

            if (!Directory.Exists(@"0:\content\GCI\"))
            {
                try
                {
                    Directory.CreateDirectory(@"0:\content\GCI\");
                }
                catch (Exception)
                {
                    if (File.Exists(@"0:\content\sys\GCI.gms"))
                    {
                        File.Delete(@"0:\content\sys\GCI.gms");
                    }
                }
            }

            try
            {
                var systemsetup = File.ReadAllLines(@"0:\content\sys\user.gms");
                foreach (string line in systemsetup)
                {
                    if (line.StartsWith("username: "))
                    {
                        username = line.Replace("username: ", "");
                    }

                    if (line.StartsWith("computername: "))
                    {
                        computername = line.Replace("computername: ", "");
                    }
                }

                foreach (string line in File.ReadAllLines(@"0:\content\sys\theme.gms"))
                {
                    if (line.StartsWith("ThemeFile = "))
                    {
                        ThemeManager.SetTheme(line.Split("ThemeFile = ")[1]);
                    }
                }

                //byte videoMode = File.ReadAllBytes(@"0:\content\sys\resolution.gms")[0];
                //Console.Init(ControlPanel.videoModes[videoMode].Item2.Width, ControlPanel.videoModes[videoMode].Item2.Height);
            }
            catch
            {
                log(ThemeManager.Other1, "GoOS - Failed to load settings, continuing with default values...");
            }

            if (username == null || username == "")
            {
                username = "user";
            }

            if (computername == null || computername == "")
            {
                computername = "GoOS";
            }

            loadingDialogue.Closing = true;
            WindowManager.Canvas = Display.GetDisplay(1280, 720);
            WindowManager.AddWindow(new Taskbar());
            WindowManager.AddWindow(new DesktopIcons());
            WindowManager.AddWindow(new Welcome());

            Console.Clear();

            Canvas cv = Image.FromBitmap(rawBootLogo, false);
            Console.Canvas.DrawImage(0, 0, cv, false);
            Console.SetCursorPosition(0, 13);

            Directory.SetCurrentDirectory(@"0:\");
        }

        public static string currentdirfix = string.Empty;

        public static void DrawPrompt()
        {
            textcolour(ThemeManager.WindowText);
            string currentdir = Directory.GetCurrentDirectory() + @"\";
            currentdirfix = @"0:\";
            if (currentdir.Contains(@"0:\\"))
            {
                currentdirfix = currentdir.Replace(@"0:\\", @"0:\");
            }
            else if (currentdir.Contains(@"0:\\\"))
            {
                currentdirfix = currentdir.Replace(@"0:\\\", @"0:\");
            }

            write($"{username}");
            textcolour(ThemeManager.Other1);
            write("@");
            textcolour(ThemeManager.WindowText);
            write($"{computername} ");
            textcolour(ThemeManager.WindowBorder);
            write(currentdirfix);
            textcolour(ThemeManager.Default);
        }

        protected override void Run()
        {
            isGCIenabled = File.Exists(@"0:\content\sys\GCI.gms");

            if (isGCIenabled)
            {
                GoCodeInstaller.CheckForInstalledPrograms();
            }

            DrawPrompt();

            // Commands section

            string[] args = Console.ReadLine().Trim().Split(' ');

            uint TotalRamUINT = Cosmos.Core.CPU.GetAmountOfRAM();
            int TotalRam = Convert.ToInt32(TotalRamUINT);

            switch (args[0])
            {
                case "gui":
                    Console.ConsoleMode = false;
                    WindowManager.Canvas = Display.GetDisplay(1280, 720);
                    WindowManager.AddWindow(new Taskbar());
                    WindowManager.AddWindow(new DesktopIcons());
                    WindowManager.AddWindow(new Welcome());
                    break;
                case "exit":
                    Console.Visible = false;
                    break;
                case "ping":
                    Ping.Run();
                    break;
                case "install":
                    if (args.Length < 2)
                    {
                        log(ThemeManager.ErrorText, "Missing arguments!");
                        break;
                    }
                    if (args.Length > 2)
                    {
                        log(ThemeManager.ErrorText, "Too many arguments!");
                        break;
                    }

                    GoCodeInstaller.Install(args[1]);
                    break;
                case "uninstall":
                    if (args.Length < 2)
                    {
                        log(ThemeManager.ErrorText, "Missing arguments!");
                        break;
                    }
                    if (args.Length > 2)
                    {
                        log(ThemeManager.ErrorText, "Too many arguments!");
                        break;
                    }

                    GoCodeInstaller.Uninstall(args[1]);
                    break;
                case "movefile":
                    if (args.Length < 3)
                    {
                        log(ThemeManager.ErrorText, "Missing arguments!");
                        break;
                    }
                    if (args.Length > 3)
                    {
                        log(ThemeManager.ErrorText, "Too many arguments!");
                        break;
                    }
                    try
                    {
                        ExtendedFilesystem.MoveFile(args[1], args[2]);
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine("Error whilst trying to move file: " + e);
                        break;
                    }
                    break;
                case "copyfile":
                    if (args.Length < 3)
                    {
                        log(ThemeManager.ErrorText, "Missing arguments!");
                        break;
                    }
                    if (args.Length > 3)
                    {
                        log(ThemeManager.ErrorText, "Too many arguments!");
                        break;
                    }
                    try
                    {
                        ExtendedFilesystem.CopyFile(args[1], args[2]);
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine("Error whilst trying to copy file: " + e);
                        break;
                    }
                    break;
                case "help":
                    if (args.Length > 1)
                    {
                        log(ThemeManager.ErrorText, "Too many arguments");
                    }

                    Commands.Help.Main();
                    break;
                case "goget":
                    if (args.Length != 2)
                    {
                        log(ThemeManager.ErrorText, "Invalid Params");
                        break;
                    }

                    String filetoget = args[1];
                    try
                    {
                        log(ConsoleColor.Red, "1");
                        using (var xClient = new DHCPClient())
                        {
                            /** Send a DHCP Discover packet **/
                            //This will automatically set the IP config after DHCP response
                            xClient.SendDiscoverPacket();
                            log(ConsoleColor.Blue, NetworkConfiguration.CurrentAddress.ToString());
                        }

                        using (var xClient = new TcpClient(39482))
                        {
                            log(ConsoleColor.Red, "2");
                            try
                            {
                                xClient.Connect(NetworkConfiguration.CurrentAddress, 500, 1000);
                                //5.39.84.58
                            }
                            catch (Exception e)
                            {
                                log(ConsoleColor.Red, e.ToString());
                            }

                            log(ConsoleColor.Red, "3");
                            /** Send data **/
                            xClient.Send(Encoding.ASCII.GetBytes("GET /" + filetoget + ".goexe HTTP/1.1\nHost: apps.goos.owen2k6.com\n\n"));

                            /** Receive data **/
                            log(ConsoleColor.Red, "4");
                            var endpoint = new EndPoint(Address.Zero, 0);
                            log(ConsoleColor.Red, "5");
                            var data = xClient.Receive(ref endpoint); //set endpoint to remote machine IP:port
                            log(ConsoleColor.Red, "6");
                            var data2 = xClient
                                .NonBlockingReceive(ref endpoint); //retrieve receive buffer without waiting
                            log(ConsoleColor.Red, "7");
                            string bitString = BitConverter.ToString(data2);
                            log(ConsoleColor.Red, "8");
                            File.Create(@"0:\" + filetoget + ".goexe");
                            log(ConsoleColor.Red, "9");
                            File.WriteAllText(@"0:\" + filetoget + ".goexe", bitString);
                            log(ConsoleColor.Red, "10");
                            print(bitString);
                            log(ConsoleColor.Red, "TASK END");
                        }
                    }
                    catch (Exception e)
                    {
                        log(ConsoleColor.Red, "Internal Error:");
                        log(ConsoleColor.White, e.ToString());
                    }

                    break;
                case "run":
                    if (args.Length > 2)
                    {
                        log(ThemeManager.ErrorText, "Too many arguments");
                        break;
                    }

                    if (args.Length == 1)
                    {
                        log(ThemeManager.ErrorText, "Missing arguments");
                        break;
                    }

                    Commands.Run.Main(args[1]);
                    break;
                case "mkdir":
                    if (args.Length > 2)
                    {
                        log(ThemeManager.ErrorText, "Too many arguments");
                        break;
                    }

                    if (args.Length == 1)
                    {
                        log(ThemeManager.ErrorText, "Missing arguments");
                        break;
                    }

                    Commands.Make.MakeDirectory(args[1]);
                    break;
                case "mkfile":
                    if (args.Length > 2)
                    {
                        log(ThemeManager.ErrorText, "Too many arguments");
                        break;
                    }

                    if (args.Length == 1)
                    {
                        log(ThemeManager.ErrorText, "Missing arguments");
                        break;
                    }

                    Commands.Make.MakeFile(args[1]);
                    break;
                case "deldir":
                    if (args.Length > 2)
                    {
                        log(ThemeManager.ErrorText, "Too many arguments");
                        break;
                    }

                    if (args.Length == 1)
                    {
                        log(ThemeManager.ErrorText, "Missing arguments");
                        break;
                    }

                    Commands.Delete.DeleteDirectory(args[1]);
                    break;
                case "delfile":
                    if (args.Length > 2)
                    {
                        log(ThemeManager.ErrorText, "Too many arguments");
                        break;
                    }

                    if (args.Length == 1)
                    {
                        log(ThemeManager.ErrorText, "Missing arguments");
                        break;
                    }

                    Commands.Delete.DeleteFile(args[1]);
                    break;
                case "del":
                    if (args.Length > 2)
                    {
                        log(ThemeManager.ErrorText, "Too many arguments");
                        break;
                    }

                    if (args.Length == 1)
                    {
                        log(ThemeManager.ErrorText, "Missing arguments");
                        break;
                    }

                    Delete.UniveralDelete(args[1]);
                    break;
                case "cd":
                    if (args.Length > 2)
                    {
                        log(ThemeManager.ErrorText, "Too many arguments");
                        break;
                    }

                    if (args.Length == 1)
                    {
                        log(ThemeManager.ErrorText, "Missing arguments");
                        break;
                    }

                    Commands.Cd.Run(args[1]);
                    break;
                case "cd..":
                    if (args.Length > 1)
                    {
                        log(ThemeManager.ErrorText, "Too many arguments");
                        break;
                    }

                    // do it the ChaOS way, it works so dont touch else you're gay
                    try
                    {
                        Directory.SetCurrentDirectory(Directory.GetCurrentDirectory().TrimEnd('\\').Remove(Directory.GetCurrentDirectory().LastIndexOf('\\') + 1));
                        Directory.SetCurrentDirectory(Directory.GetCurrentDirectory().Remove(Directory.GetCurrentDirectory().Length - 1));
                    }
                    catch { }
                    if (!Directory.GetCurrentDirectory().StartsWith(@"0:\"))
                    {
                        Directory.SetCurrentDirectory(@"0:\"); // Directory error correction
                    }
                    break;
                case "cdr":
                    if (args.Length > 1)
                    {
                        log(ThemeManager.ErrorText, "Too many arguments");
                        break;
                    }

                    string roota = @"0:\";
                    Directory.SetCurrentDirectory(roota);
                    break;
                case "dir":
                    if (args.Length > 1)
                    {
                        log(ThemeManager.ErrorText, "Too many arguments");
                        break;
                    }

                    Commands.Dir.Run();
                    break;
                case "ls":
                    if (args.Length > 1)
                    {
                        log(ThemeManager.ErrorText, "Too many arguments");
                        break;
                    }

                    Commands.Dir.Run();
                    break;
                case "notepad":
                    if (TotalRam < 1000)
                    {
                        log(ThemeManager.ErrorText, "This program has been disabled due to low ram.");
                        break;
                    }

                    if (args.Length > 2)
                    {
                        log(ThemeManager.ErrorText, "Too many arguments");
                        break;
                    }

                    if (args.Length == 1)
                    {
                        log(ThemeManager.ErrorText, "Missing arguments");
                        break;
                    }

                    if (args[1].EndsWith(".gms"))
                    {
                        log(ThemeManager.ErrorText,
                            "Files that end with .gms cannot be opened. they are protected files.");
                        break;
                    }

                    textcolour(ThemeManager.Default);
                    var editor = new TextEditor(Util.Paths.JoinPaths(currentdirfix, args[1]));
                    editor.Start();
                    break;
                case "settings":
                    ControlPanel.Launch();
                    break;
                case "vm":
                    if (args.Length > 2)
                    {
                        log(ThemeManager.ErrorText, "Too many arguments");
                        break;
                    }

                    if (args.Length == 1)
                    {
                        log(ThemeManager.ErrorText, "Missing arguments");
                        break;
                    }

                    VM.Run(args[1]);
                    break;
                case "clear":
                    Console.Clear();
                    break;
                case "settheme":
                    if (args.Length > 2)
                    {
                        log(ThemeManager.ErrorText, "Too many arguments");
                        break;
                    }

                    if (args.Length == 1)
                    {
                        log(ThemeManager.ErrorText, "Missing arguments");
                        break;
                    }

                    ThemeManager.SetTheme(@"0:\content\themes\" + args[1]);
                    break;
                case "systest":
                    systest.run();
                    break;
                case "whoami":
                    log(ThemeManager.ErrorText, "Showing Internet Information");
                    log(ThemeManager.ErrorText, NetworkConfiguration.CurrentAddress.ToString());
                    break;
                case "lr":
                    if (args[1] == "get")
                    {
                        string app = new GoOS.Util.localRepo().GetFile(args[2]);
                        log(ThemeManager.WindowText, app);
                    }
                    else
                    {
                        log(ThemeManager.ErrorText, "Unknown order.");
                    }
                    break;
                case "mode":
                    if (args.Length > 3)
                    {
                        log(ThemeManager.ErrorText, "Too many arguments");
                        break;
                    }
                    if (args.Length == 1)
                    {
                        log(ThemeManager.ErrorText, "Missing arguments");
                        break;
                    }
                    Console.Init(Convert.ToUInt16(args[1]), Convert.ToUInt16(args[2]));
                    break;
                case "floppy":
                    log(ThemeManager.WindowText, "1");
                    PrismAPI.Network.NetworkManager.Init();
                    log(ThemeManager.WindowText, "2");
                    PrismAPI.Network.HTTP.HTTPClient client = new("http://apps.goos.owen2k6.com/test.goexe");
                    //log(ThemeManager.WindowText, "3");
                    //client.URL = new();
                    log(ThemeManager.WindowText, "3");
                    byte[] test = client.Get();
                    log(ThemeManager.WindowText, "4");
                    Console.WriteLine(Encoding.ASCII.GetString(test));
                    break;
                case "dtest":
                    Dialogue.Show("Message", "Hello world!!!");
                    break;
                default:
                    if (isGCIenabled)
                    {
                        GoCodeInstaller.CheckForInstalledPrograms();
                    }

                    if (InstalledPrograms.ContainsKey(args[0]))
                    {

                        string rootass = @"0:\";

                        string currentDIRRRRRR = Directory.GetCurrentDirectory();

                        Directory.SetCurrentDirectory(rootass);

                        InstalledPrograms.TryGetValue(args[0], out string locat);

                        string TrueLocat = locat;

                        if (locat.Contains(@"0:\"))
                        {
                            TrueLocat = TrueLocat.Replace(@"0:\", "");
                        }

                        Commands.Run.Main(TrueLocat);

                        Directory.SetCurrentDirectory(currentDIRRRRRR);
                        break;
                    }

                    Console.WriteLine("Invalid command.");
                    break;
            }
        }
    }
}