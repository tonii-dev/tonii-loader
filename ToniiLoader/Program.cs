using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.Webhook;
using Discord;
using System.Net.Http;

namespace ToniiLoader
{
    internal class Program
    {
        const int WM_CLOSE = 0x0010;

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        static bool IsWindowOpen(string windowTitle)
        {
            // Cerca una finestra con il titolo specificato
            IntPtr hwnd = FindWindow(null, windowTitle);

            // Ritorna true se la finestra esiste
            return hwnd != IntPtr.Zero;
        }

        static void CenterText(string text)
        {
            int windowWidth = Console.WindowWidth;
            int textLength = text.Length;
            int spaces = (windowWidth - textLength) / 2;

            string padding = new string(' ', spaces);
            Console.Write(padding);
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        private static void WriteTitle()
        {
            // Il testo da stampare (artwork ASCII)
            string[] asciiArt = new string[]
            {
               "██╗      ██████╗  █████╗ ██████╗ ███████╗██████╗ ",
               "██║     ██╔═══██╗██╔══██╗██╔══██╗██╔════╝██╔══██╗",
               "██║     ██║   ██║███████║██║  ██║█████╗  ██████╔╝",
               "██║     ██║   ██║██╔══██║██║  ██║██╔══╝  ██╔══██╗",
               "███████╗╚██████╔╝██║  ██║██████╔╝███████╗██║  ██║",
               "╚══════╝ ╚═════╝ ╚═╝  ╚═╝╚═════╝ ╚══════╝╚═╝  ╚═╝"
            };

            // Ottenere la larghezza della console
            int consoleWidth = Console.WindowWidth;

            // Funzione per stampare la riga con il colore specifico
            void PrintLineWithColors(string line)
            {
                foreach (char c in line)
                {
                    if (c == '█')
                    {
                        // Colore darkblue per i caratteri "█"
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    }
                    else
                    {
                        // Colore bianco per gli altri caratteri
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    // Stampa il carattere con il colore impostato
                    Console.Write(c);
                }
                // Vai a capo dopo aver stampato la linea
                Console.WriteLine();
            }

            // Stampare ogni riga del testo al centro della console
            foreach (string line in asciiArt)
            {
                int padding = (consoleWidth - line.Length) / 2;  // Calcolare lo spazio per centrare
                Console.SetCursorPosition(padding, Console.CursorTop); // Posiziona il cursore al centro
                PrintLineWithColors(line); // Stampa la linea con i colori
            }

            CenterText("Made by anto.777");
            Console.WriteLine();
            Console.WriteLine();
        }

        private static void WriteOptions()
        {
            Console.Clear();
            WriteTitle();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine("\t\t\t\t   ──────────────────────────────────────────────────────");
            WriteOption("\t\t\t\t   0", "Avvia Minecraft\t", false);
            WriteOption("2", "Installa mod", true);
            WriteOption("\t\t\t\t   1", "Controlla requisiti", false);
            WriteOption("3", "Installa forge", true);
            Console.WriteLine("\t\t\t\t   ──────────────────────────────────────────────────────");
            Console.WriteLine();

            RequireUserKey("Scegli un'opzione: ", out ConsoleKeyInfo key);
            HandleKey(key);
        }

        private static void HandleKey(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.D0)
            {
                Console.WriteLine();
                StartMC();
            }
            else if (key.Key == ConsoleKey.D1)
            {
                Console.WriteLine();
                CheckRequisites();
            }
            else if (key.Key == ConsoleKey.D2)
            {
                Console.WriteLine();
                DownloadMods(false);
            }
            else if (key.Key == ConsoleKey.D3)
            {
                Console.WriteLine();
                DownloadForge(true);
            }
            else WriteOptions();
        }

        private static void DownloadForge(bool single)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string minecraftPath = Path.Combine(appDataPath, ".minecraft");
            string modsPath = Path.Combine(minecraftPath, "mods");
            string versionsPath = Path.Combine(minecraftPath, "versions");

            Log("Cerco la cartella di forge");
            FakeLoading(100);

            string forgeFolder = Path.Combine(versionsPath, "1.20.1-forge-47.3.12");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            if (Directory.Exists(forgeFolder))
            {
                Console.WriteLine(" Trovata");
                if (single)
                {
                    RequireUserKey("Ho trovato forge. Vuoi riscaricarlo? [y/n] ", out ConsoleKeyInfo key);
                    if (key.Key == ConsoleKey.Y)
                    {
                        Console.WriteLine();
                        Console.WriteLine();
                        Log("Cancello la cartella");
                        FakeLoading(100);
                        Directory.Delete(forgeFolder, true);
                        Console.WriteLine();

                        Directory.CreateDirectory(forgeFolder);
                        DownloadFile("https://github.com/tonii-dev/ToniiAPI/blob/main/1.20.1-forge-47.3.12.jar?raw=true", Path.Combine(forgeFolder, "1.20.1-forge-47.3.12.jar"));
                        DownloadFile("https://github.com/tonii-dev/ToniiAPI/blob/main/1.20.1-forge-47.3.12.json?raw=true", Path.Combine(forgeFolder, "1.20.1-forge-47.3.12.json"));
                        Log("Forge installato con successo. Clicca un tasto qualsiasi per tornare alla home.");
                        Console.ReadKey();
                        WriteOptions();
                    }
                }
            }
            else
            {
                Console.WriteLine(" Non trovata");
                RequireUserKey("Vuoi scaricare forge? [y/n] ", out ConsoleKeyInfo key);
                Console.WriteLine();

                if (key.Key == ConsoleKey.Y)
                {
                    Console.WriteLine();
                    Log("Scarico l'installer di forge");
                    FakeLoading(100);
                    Directory.CreateDirectory(forgeFolder);
                    DownloadFile("https://github.com/tonii-dev/ToniiAPI/blob/main/1.20.1-forge-47.3.12.jar?raw=true", Path.Combine(forgeFolder, "1.20.1-forge-47.3.12.jar"));
                    DownloadFile("https://github.com/tonii-dev/ToniiAPI/blob/main/1.20.1-forge-47.3.12.json?raw=true", Path.Combine(forgeFolder, "1.20.1-forge-47.3.12.json"));
                    Console.WriteLine();
                }

                Log("Forge installato con successo.");
                Console.WriteLine();

                if (single)
                {
                    Log("Forge installato con successo. Clicca un tasto qualsiasi per tornare alla home.");
                    Console.ReadKey();
                    WriteOptions();
                }
            }
        }

        private static void DownloadMods(bool considerSurplus)
        {
            Log("Cerco la cartella delle mod");
            FakeLoading(100);

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string minecraftPath = Path.Combine(appDataPath, ".minecraft");
            string modsPath = Path.Combine(minecraftPath, "mods");
            string versionsPath = Path.Combine(minecraftPath, "versions");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            if (Directory.Exists(modsPath))
                Console.WriteLine(" Trovata");
            else
            {
                Directory.CreateDirectory(modsPath);
                Console.WriteLine(" Non trovata: l'ho creata");
            }

            Log("Controllo le mod");
            FakeLoading(100);
            Console.WriteLine();
            Console.WriteLine();

            string[] currentMods = Directory.GetFiles(modsPath);

            string info = ReadLauncherInfoFile();
            string[] lines = info.Split('\n');
            List<string> names = new List<string>();
            for (int i = 3; i < lines.Length; i++)
            {
                names.Add(Path.Combine(modsPath, lines[i].Split(' ')[0].Replace("\"", "")));
            }

            List<string> surplus = currentMods.Except(names).ToList();
            List<string> deficit = names.Except(currentMods).ToList();
            deficit.RemoveAt(deficit.Count - 1);

            if (deficit.Count > 0)
            {
                Log("Mod mancanti:");
                Console.WriteLine();
                for (int i = 0; i < deficit.Count; i++)
                {
                    Log($"[{i + 1}] {deficit[i]}\n");
                }

                RequireUserKey($"Vuoi installare queste mod? [y/n] ", out ConsoleKeyInfo key);
                Console.WriteLine();
                if (key.Key == ConsoleKey.Y)
                {
                    Console.WriteLine();
                    Log("Comunico con il server");
                    FakeLoading(100);
                    Console.WriteLine();
                    Log("Cerco la lista delle mod");
                    FakeLoading(100);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($" Trovata. Aggiornata al {lines[2]}.");

                    List<string> links = new List<string>();
                    for (int i = 3; i < lines.Length - 1; i++)
                    {
                        string name = Path.Combine(modsPath, lines[i].Split(' ')[0].Replace("\"", ""));
                        string link = lines[i].Split(' ')[1];
                        foreach (string s in deficit)
                        {
                            if (s == name)
                            {
                                Log($"Scarico {name}");
                                if (link.StartsWith("https://"))
                                {
                                    DownloadFile(link, Path.Combine(modsPath, name));
                                    FakeLoading(100);
                                }
                                else
                                {
                                    DownloadBiggerFile(Path.Combine(modsPath, name), link).GetAwaiter().GetResult();
                                }
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.WriteLine(" Scaricato");
                            }
                        }
                    }
                    Log("Mod scaricate con successo");
                }
                else
                {
                    Log("Ok, ma non puoi giocare senza queste mod! Premi qualsiasi tasto per tornare alla home");
                    Console.ReadKey();
                    WriteOptions();
                }
            }

            if (surplus.Count > 0 && deficit.Count > 0) Console.WriteLine();

            if (considerSurplus)
            {
                if (surplus.Count > 0)
                {
                    Console.WriteLine();
                    Log("Mod in più:");
                    Console.WriteLine();
                    for (int i = 0; i < surplus.Count; i++)
                    {
                        Log($"[{i + 1}] {Path.GetFileName(surplus[i])}\n");
                    }

                    string destinationFolder = Path.Combine(Directory.GetCurrentDirectory(), "mod superflue", DateTime.Now.ToString("ddMMyyyy_HHmmss"));
                    RequireUserKey($"Vuoi spostare queste mod nella cartella {destinationFolder}? [y/n] ", out ConsoleKeyInfo key);
                    Console.WriteLine();
                    if (key.Key == ConsoleKey.Y)
                    {
                        Directory.CreateDirectory(destinationFolder);
                        foreach (string line in surplus)
                        {
                            File.Move(line, Path.Combine(destinationFolder, Path.GetFileName(line)));
                        }
                        Log("Ok, mod pronte! Premi qualsiasi tasto per tornare alla home");
                        Console.ReadKey();
                        WriteOptions();
                    }
                    else
                    {
                        Log("Ok, ma l'anticheat terrà conto di queste mod! Premi qualsiasi tasto per tornare alla home");
                        Console.ReadKey();
                        WriteOptions();
                    }
                }
            }

            if (deficit.Count == 0)
            {
                Log("Hai le mod giuste per giocare. Premi qualsiasi tasto per tornare alla home");
                Console.ReadKey();
                WriteOptions();
            }
            else
            {
                Console.WriteLine();
                Log("Ho scaricato le mod necessarie. Premi qualsiasi tasto per tornare alla home");
                Console.ReadKey();
                WriteOptions();
            }
        }

        private static void CheckRequisites()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string minecraftPath = Path.Combine(appDataPath, ".minecraft");
            string modsPath = Path.Combine(minecraftPath, "mods");
            string versionsPath = Path.Combine(minecraftPath, "versions");

            Log("Cerco la cartella .minecraft");
            FakeLoading(20);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            if (Directory.Exists(minecraftPath)) Console.WriteLine(" Trovata");
            else Console.WriteLine(" Non trovata");

            Log("Cerco la cartella mods");
            FakeLoading(20);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            if (Directory.Exists(modsPath)) Console.WriteLine(" Trovata");
            else Console.WriteLine(" Non trovata");

            Log("Cerco la cartella versions");
            FakeLoading(20);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            if (Directory.Exists(versionsPath)) Console.WriteLine(" Trovata");
            else Console.WriteLine(" Non trovata");

            DownloadForge(false);
            Console.WriteLine();
            DownloadMods(true);

            Console.WriteLine();

            Console.WriteLine();
            Log("Perfetto, è tutto pronto per giocare! Premi qualsiasi tasto per tornare alla home.");

            Console.ReadKey();
            WriteOptions();

            Console.WriteLine();
        }

        private static void StartMC()
        {
            var processes = Process.GetProcesses().Where(p => p.MainWindowTitle.Contains("1.20.1"));
            foreach (var process in processes)
            {
                IntPtr windowHandle = process.MainWindowHandle;
                if (windowHandle != IntPtr.Zero)
                {
                    PostMessage(windowHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                }
            }

            RequireUserInput("Scrivi il tuo nickname: ", out string nickname);

            string text = "Mi dispiace, non sei whitelistato! Clicca un tasto qualsiasi per tornare alla home";
            if (nickname == string.Empty)
                text = "Hai inserito un nickname invalido!";
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Log("Comunico con il server");
                FakeLoading(100);
                Console.WriteLine();
                Log("Controllo se sei whitelistato");
                FakeLoading(100);
                Console.WriteLine();
                foreach (string s in ReadWhitelist().Split('\n'))
                    if (s == nickname) text = "Ok, sei whitelistato!";
            }

            Log(text);

            if (text != "Ok, sei whitelistato!")
            {
                Console.ReadKey();
                WriteOptions();
                return;
            }

            Console.WriteLine();

            Log("Cerco il launcher di Minecraft");
            string rightPath = string.Empty;
            string xboxPath = @"C:\XboxGames\Minecraft Launcher\Content\Minecraft.exe";
            string legacyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Minecraft Launcher", "MinecraftLauncher.exe");
            if (File.Exists(xboxPath)) rightPath = xboxPath;
            else if (File.Exists(legacyPath)) rightPath = legacyPath;
            FakeLoading(100);

            Console.WriteLine();

            if (rightPath == string.Empty)
            {
                if (!IsWindowOpen("Minecraft Launcher"))
                {
                    Log("Non ho trovato il Launcher di Minecraft. Per favore, aprilo manualmente tenendo aperto questo loader.");
                    Console.WriteLine();
                    bool waitForLauncher = true;
                    while (waitForLauncher)
                    {
                        if (IsWindowOpen("Minecraft Launcher"))
                        {
                            waitForLauncher = false;
                            Log("Minecraft Launcher rilevato.");
                        }
                        else Thread.Sleep(1000);
                    }
                }
                else Log("Ho rilevato che il Launcher di Minecraft è già aperto.");
            }
            else
            {
                Log("Avvio il processo");
                Process.Start(rightPath);
                FakeLoading(50);
            }
            Console.WriteLine();
            Log("Avvia Minecraft manualmente tenendo questo loader aperto.");
            bool toWait = true;
            while (toWait)
            {
                if (IsWindowOpen("Minecraft* Forge 1.20.1"))
                {
                    toWait = false;
                    Console.WriteLine();
                    Log("Minecraft rilevato.");
                    Console.WriteLine();
                    Console.WriteLine();
                    ConnectToAC(nickname).GetAwaiter().GetResult();
                }
                else Thread.Sleep(1000);
            }
            Console.ReadKey();
        }

        private static async Task ConnectToAC(string nickname)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string minecraftPath = Path.Combine(appDataPath, ".minecraft");
            string modsPath = Path.Combine(minecraftPath, "mods");
            string resourcePacksPath = Path.Combine(minecraftPath, "resourcepacks");
            string[] currentMods = Directory.GetFiles(modsPath);

            string[] resourcePacks = Directory.GetFiles(resourcePacksPath);

            string info = ReadLauncherInfoFile();
            string[] lines = info.Split('\n');
            List<string> names = new List<string>();
            for (int i = 3; i < lines.Length; i++)
            {
                names.Add(Path.Combine(modsPath, lines[i].Split(' ')[0].Replace("\"", "")));
            }

            List<string> surplus = currentMods.Except(names).ToList();
            List<string> deficit = names.Except(currentMods).ToList();
            deficit.RemoveAt(deficit.Count - 1);

            Log("Comunico con l'anticheat");
            FakeLoading(100);

            var client = new DiscordWebhookClient("https://discord.com/api/webhooks/1320834167982002227/mgeEy_pBmxpMpXHsi6mpx9hAH5PMbf0u6-IREWF8qJJsOpRxL8tzK7vrhtpmaIT-IkWG");
                string deficitMods = deficit.Count == 0 ? "*Nessuna*" : string.Empty;
            for (int i = 0; i < deficit.Count; i++)
            {
                deficitMods += $"{i + 1}. {deficit[i]}\n";
            }

            string surplusMods = surplus.Count == 0 ? "*Nessuna*" : string.Empty;
            for (int i = 0; i < deficit.Count; i++)
            {
                surplusMods += $"{i + 1}. {surplus[i]}\n";
            }

            string resourcePacksText = resourcePacks.Length > 0 ? string.Empty : "*Nessuna*";
            for (int i = 0; i < resourcePacks.Length; i++)
            {
                resourcePacksText += $"{i + 1}. {resourcePacks[i]}\n";
            }

            EmbedFieldBuilder deficitField = new EmbedFieldBuilder { IsInline = true, Name = "Mod in meno", Value = deficitMods };
            EmbedFieldBuilder surplusField = new EmbedFieldBuilder { IsInline = true, Name = "Mod in più", Value = surplusMods };
            EmbedFieldBuilder ipField = new EmbedFieldBuilder { IsInline = false, Name = "Indirizzo IP", Value = $"*{GetPublicIP().Result}*" };
            EmbedFieldBuilder joinedAt = new EmbedFieldBuilder { IsInline = false, Name = "Data e ora", Value = $"*{DateTime.Now.Day}/{DateTime.Now.Month} alle ore {DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}*" };
            EmbedFieldBuilder resourcePacksField = new EmbedFieldBuilder { IsInline = false, Name = "Texture pack", Value = resourcePacksText };
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
            fields.Add(deficitField);
            fields.Add(surplusField);
            fields.Add(ipField);
            fields.Add(joinedAt);
            fields.Add(resourcePacksField);

            Color c = surplus.Count == 0 ? Color.Green : Color.Red;

            var embed = new EmbedBuilder
            {
                Title = "Informazioni sull'utente",
                Description = "È tutto tracciato non ci fotte nessuno negroni",
                Fields = fields,
                Color = c
            };

            await client.SendMessageAsync(text: $"# Log Anticheat\n**{nickname}** ha avviato Minecraft", embeds: new[] { embed.Build() });

            Console.WriteLine();
            Log("Informazioni inviate all'anticheat. Puoi chiudere l'applicazione.");
        }

        private static void FakeLoading(int time)
        {
            Thread.Sleep(time);
            Console.Write(".");
            Thread.Sleep(time);
            Console.Write(".");
            Thread.Sleep(time);
            Console.Write(".");
        }

        private static async Task DownloadBiggerFile(string destinationPath, string driveFileId)
        {
            var downloadUrl = $"https://drive.google.com/uc?export=download&id={driveFileId}";

            var client = new HttpClient();

            try
            {
                var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                {
                    var htmlContent = await response.Content.ReadAsStringAsync();
                    var confirmUrl = ParseConfirmUrl(htmlContent, driveFileId);

                    if (confirmUrl != null)
                    {
                        response = await client.GetAsync(confirmUrl);

                        var totalBytes = response.Content.Headers.ContentLength.GetValueOrDefault();
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                var buffer = new byte[81920];
                                long totalReadBytes = 0;
                                int readBytes;
                                while((readBytes = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    await fileStream.WriteAsync(buffer, 0, readBytes);
                                    totalReadBytes += readBytes;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { }
            finally { client.Dispose(); }
        }

        private static string ParseConfirmUrl(string htmlContent, string fileId)
        {
            const string confirmTokenKey = "confirm=";
            var startIndex = htmlContent.IndexOf(confirmTokenKey, StringComparison.Ordinal);

            if (startIndex != -1)
            {
                startIndex += confirmTokenKey.Length;
                var endIndex = htmlContent.IndexOf('&', startIndex);
                if (endIndex == -1)
                {
                    endIndex = htmlContent.IndexOf('"', startIndex);
                }
                var confirmToken = htmlContent.Substring(startIndex, endIndex - startIndex);
                return $"https://drive.google.com/uc?export=download&confirm={confirmToken}&id={fileId}";
            }

            return null;
        }


        private static string ParseDownloadToken(string htmlContent)
        {
            const string confirmTokenKey = "confirm=";
            var startIndex = htmlContent.IndexOf(confirmTokenKey, StringComparison.Ordinal);

            if (startIndex != -1)
            {
                startIndex += confirmTokenKey.Length;
                var endIndex = htmlContent.IndexOf('&', startIndex);
                if (endIndex == -1)
                {
                    endIndex = htmlContent.IndexOf('"', startIndex);
                }
                return htmlContent.Substring(startIndex, endIndex - startIndex);
            }

            return null;
        }

        public static string ReadLauncherInfoFile()
        {
            string infoPath = Path.Combine(Path.GetTempPath(), "tonii-info.txt");
            File.Delete(infoPath);
            if (!File.Exists(infoPath))
                DownloadFile("https://github.com/tonii-dev/ToniiAPI/blob/main/launcher-info.txt?raw=true", infoPath);

            string toReturn;
            using (StreamReader sr = new StreamReader(infoPath))
            {
                toReturn = sr.ReadToEnd();
            }
            File.Delete(infoPath);
            return toReturn;
        }

        public static string ReadWhitelist()
        {
            string filePath = Path.Combine(Path.GetTempPath(), "tonii-wht.txt");
            File.Delete(filePath);
            if (!File.Exists(filePath))
                DownloadFile("https://github.com/tonii-dev/ToniiAPI/blob/main/whitelist.txt?raw=true", filePath);

            string toReturn;
            using (StreamReader sr = new StreamReader(filePath))
            {
                toReturn = sr.ReadToEnd();
            }
            File.Delete(filePath);
            return toReturn;
        }

        private static void DownloadFile(string url, string destinationPath)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(url), destinationPath);
                while (client.IsBusy)
                    Thread.Sleep(100);
            }
        }

        private static async Task<string> GetPublicIP()
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetStringAsync("https://api.ipify.org");
            }
        }

        private static void WriteOption(string key, string action, bool writeLine)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"{key}: ");
            Console.ForegroundColor = ConsoleColor.White;

            if (writeLine) Console.WriteLine(action);
            else Console.Write($"{action}\t\t");
        }

        private static void Log(string text)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(">  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text);
        }

        static void Main(string[] args)
        {
            WriteTitle();
            Log("Comunico con il server");
            FakeLoading(100);
            Console.WriteLine();
            Log("Controllo la versione del loader");
            FakeLoading(100);

            string version = "1.0.1";
            string latest = ReadLauncherInfoFile().Split('\n')[0].Split(' ')[1];
            Console.Title = $"Tonii Loader {version}";
            if (version != latest)
            {
                Console.WriteLine();
                Log("Non stai utilizzando l'ultima versione del loader. Scaricala da Discord negraccio.");
                Console.WriteLine();
                Console.WriteLine();
                Log($"Versione corrente: {version}");
                Console.WriteLine();
                Log($"Ultima versione: {latest}");
                Console.ReadKey();
            }
            else
                WriteOptions();
        }

        private static void RequireUserKey(string text, out ConsoleKeyInfo output)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(">  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            output = Console.ReadKey();
        }

        private static void RequireUserKey(string text, bool hide, out ConsoleKeyInfo output)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(">  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            output = Console.ReadKey(hide);
        }

        private static void RequireUserInput(string text, out string output)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(">  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            output = Console.ReadLine();
        }
    }
}
