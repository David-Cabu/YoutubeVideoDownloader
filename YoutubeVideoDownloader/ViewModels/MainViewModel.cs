using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace YoutubeVideoDownloader.ViewModels;

public partial class MainViewModel : ViewModelBase
{

    void LogToFile(string message, int fileType)
    {
        if (fileType == 1)
        {
            try
            {
                string logPath = Path.Combine(AppContext.BaseDirectory, "YoutubeVideoDownloaderLog.log");
                System.IO.File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n");
            }
            catch { /* Ignora errori di log */ }
        }
        else if (fileType == 2)
        {
            try
            {
                string logPath = Path.Combine(AppContext.BaseDirectory, "YoutubeVideoDownloaderERRORS.log");
                System.IO.File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n");
            }
            catch { /* Ignora errori di log */ }
        }
    }

    [RelayCommand]
    private void OpenLogDirectory()
    {
        if(System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                string logPath = Path.Combine(AppContext.BaseDirectory, "YoutubeVideoDownloaderLog.log");
                Process.Start("explorer.exe", $"/select,\"{logPath}\"");
            }
        catch { /* Ignora errori di log */ }
        }
        else if(System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                string logPath = Path.Combine(AppContext.BaseDirectory, "YoutubeVideoDownloaderLog.log");
                Process.Start("xdg-open", logPath);
            }
        catch { /* Ignora errori di log */ }
        }
        else if(System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            try
            {
                string logPath = Path.Combine(AppContext.BaseDirectory, "YoutubeVideoDownloaderLog.log");
                Process.Start("open", logPath);
            }
        catch { /* Ignora errori di log */ }
        }
    }
    
    [RelayCommand]
    private void OpenErrorLogDirectory()
    {
        if(System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                string logPath = Path.Combine(AppContext.BaseDirectory, "YoutubeVideoDownloaderERRORS.log");
                Process.Start("explorer.exe", $"/select,\"{logPath}\"");
            }
        catch { /* Ignora errori di log */ }
        }
        else if(System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                string logPath = Path.Combine(AppContext.BaseDirectory, "YoutubeVideoDownloaderERRORS.log");
                Process.Start("xdg-open", logPath);
            }
        catch { /* Ignora errori di log */ }
        }
        else if(System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            try
            {
                string logPath = Path.Combine(AppContext.BaseDirectory, "YoutubeVideoDownloaderERRORS.log");
                Process.Start("open", logPath);
            }
        catch { /* Ignora errori di log */ }
        }
    }
    #region UI Bindings
    private string _estensione = "1";
    public string Estensione
    {
        get => _estensione;
        set
        {
            if (SetProperty(ref _estensione, value))
            {
                OnPropertyChanged(nameof(IsMp4Selected));
                OnPropertyChanged(nameof(IsMp3Selected));
                OnPropertyChanged(nameof(IsWebmSelected));
            }
        }
    }
    private string _numeroFile = "s";
    public string NumeroFile
    {
        get => _numeroFile;
        set
        {
            if (SetProperty(ref _numeroFile, value))
            {
                OnPropertyChanged(nameof(IsSingleSelected));
                OnPropertyChanged(nameof(IsPlaylistSelected));
            }
        }
    }
    private string _pathCartella = "";
    public string PathCartella
    {
        get => _pathCartella;
        set => SetProperty(ref _pathCartella, value);
    }
    private string _youtubeUrl = "";
    public string YoutubeUrl
    {
        get => _youtubeUrl;
        set => SetProperty(ref _youtubeUrl, value);
    }
    private string _statusText = "Pronto per scaricare";
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }
    private string _statusColor = "Gray";
    public string StatusColor
    {
        get => _statusColor;
        set => SetProperty(ref _statusColor, value);
    }
    private string _selectedBrowser = "Nessuno";
    public string SelectedBrowser
    {
        get => _selectedBrowser;
        set => SetProperty(ref _selectedBrowser, value);
    }
    public string[] AvailableBrowsers { get; } = new[] { "Nessuno", "Chrome", "Edge", "Firefox", "Brave", "Opera", "Safari", "Vivaldi" };
    private bool _isDownloading = false;
    public bool IsDownloading
    {
        get => _isDownloading;
        set => SetProperty(ref _isDownloading, value);
    }
    private string _buttonText = "Scarica Video";
    public string ButtonText
    {
        get => _buttonText;
        set => SetProperty(ref _buttonText, value);
    }
    private Process? _currentProcess;
    private int _erroriDownload = 0;
    private string _currentPlaylistIndex = "";
    private string _currentVideoTitle = "";
    private readonly Dictionary<int, Tuple<string, string, string, string, string>> _videoInfo = new();
    public bool IsMp4Selected
    {
        get => Estensione == "1";
        set
        {
            if (value)
            {
                Estensione = "1";
                LogToFile($"Estensione selezionata: {Estensione}", 1);
            }
        }
    }
    public bool IsMp3Selected
    {
        get => Estensione == "2";
        set
        {
            if (value)
            {
                Estensione = "2";
                LogToFile($"Estensione selezionata: {Estensione}", 1);
            }
        }
    }
    public bool IsWebmSelected
    {
        get => Estensione == "3";
        set
        {
            if (value)
            {
                Estensione = "3";
                LogToFile($"Estensione selezionata: {Estensione}", 1);
            }
        }
    }
    public bool IsSingleSelected
    {
        get => NumeroFile == "s";
        set
        {
            if (value)
            {
                NumeroFile = "s";
                LogToFile($"Numero file selezionato: {NumeroFile}", 1);
            }
        }
    }
    public bool IsPlaylistSelected
    {
        get => NumeroFile == "p";
        set
        {
            if (value)
            {
                NumeroFile = "p";
                LogToFile($"Numero file selezionato: {NumeroFile}", 1);
            }
        }
    }
    #endregion
    private bool IsFfmpegInPath()
    {
        try
        {
            ProcessStartInfo check = new ProcessStartInfo
            {
                FileName = "where",
                Arguments = "ffmpeg",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };
            using var p = Process.Start(check);
            p?.WaitForExit();
            return p != null && p.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
    private bool IsNodeInPath()
    {
        try
        {
            ProcessStartInfo check = new ProcessStartInfo
            {
                FileName = "where",
                Arguments = "node",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };
            using var p = Process.Start(check);
            p?.WaitForExit();
            return p != null && p.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task ControllaDipendenzeWindowsAsync()
    {
        string cartellaPython = Path.Combine(AppContext.BaseDirectory, "Python");
        string ffmpegExe = Path.Combine(cartellaPython, "ffmpeg.exe");

        if (!File.Exists(ffmpegExe) && !IsFfmpegInPath())
        {
            try
            {
                StatusText = "Primo avvio: download automatico di FFmpeg in corso...\nL'operazione potrebbe richiedere qualche minuto.";
                LogToFile("Inizio download automatico di FFmpeg per Windows...", 1);

                if (!Directory.Exists(cartellaPython))
                {
                    Directory.CreateDirectory(cartellaPython);
                }

                string zipPath = Path.Combine(cartellaPython, "ffmpeg_temp.zip");
                string downloadUrl = "https://github.com/yt-dlp/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip";

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(5);
                    using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await stream.CopyToAsync(fs);
                        }
                    }
                }

                StatusText = "Estrazione di FFmpeg in corso...";
                LogToFile("Estrazione ffmpeg.exe dallo zip...", 1);

                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.Name.Equals("ffmpeg.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            entry.ExtractToFile(ffmpegExe, overwrite: true);
                            break;
                        }
                    }
                }

                try { File.Delete(zipPath); } catch { }

                LogToFile("FFmpeg installato con successo in " + ffmpegExe, 1);
                StatusText = "FFmpeg installato con successo!";
            }
            catch (Exception ex)
            {
                LogToFile($"Errore durante il download/estrazione di FFmpeg: {ex.Message}", 2);
                StatusText = "Avviso: Impossibile scaricare FFmpeg automaticamente.";
            }
        }
        
        string nodeExe = Path.Combine(cartellaPython, "node.exe");
        if (!File.Exists(nodeExe))
        {
            try
            {
                StatusText = "Primo avvio: download automatico di Node.js in corso...\nL'operazione potrebbe richiedere un minuto.";
                LogToFile("Inizio download automatico di Node.js per Windows...", 1);

                if (!Directory.Exists(cartellaPython))
                {
                    Directory.CreateDirectory(cartellaPython);
                }

                string nodeUrl = "https://nodejs.org/dist/v22.17.1/win-x64/node.exe";

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(5);
                    using (var response = await client.GetAsync(nodeUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fs = new FileStream(nodeExe, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await stream.CopyToAsync(fs);
                        }
                    }
                }
                
                LogToFile("Node.js installato con successo in " + nodeExe, 1);
                StatusText = "Node.js installato con successo!";
            }
            catch (Exception ex)
            {
                LogToFile($"Errore durante il download di Node.js: {ex.Message}", 2);
                StatusText = "Avviso: Impossibile scaricare Node.js automaticamente.";
            }
        }
    }

    private async Task ControllaDipendenzeLinuxNodeAsync()
    {
        string cartellaPython = Path.Combine(AppContext.BaseDirectory, "Python");
        string nodeExe = Path.Combine(cartellaPython, "node");

        if (File.Exists(nodeExe)) return; // già presente

        // Controlla se è già nel PATH di sistema con versione sufficiente
        try
        {
            var checkVer = new ProcessStartInfo
            {
                FileName = "node",
                Arguments = "--version",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };
            using var p = Process.Start(checkVer);
            if (p != null)
            {
                string ver = await p.StandardOutput.ReadToEndAsync();
                await p.WaitForExitAsync();
                if (p.ExitCode == 0)
                {
                    // Estrai il major: "v22.1.0" -> 22
                    var match = System.Text.RegularExpressions.Regex.Match(ver.Trim(), @"v(\d+)");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int major) && major >= 22)
                        return; // Node.js >= 22 già presente nel sistema
                }
            }
        }
        catch { }

        // Node.js assente o troppo vecchio → lo scarichiamo come binario
        try
        {
            StatusText = "Primo avvio: download automatico di Node.js per Linux in corso...";
            LogToFile("Inizio download Node.js binario per Linux...", 1);

            if (!Directory.Exists(cartellaPython))
                Directory.CreateDirectory(cartellaPython);

            // Archivio tar.gz del binario ufficiale
            string nodeUrl = "https://nodejs.org/dist/v22.17.1/node-v22.17.1-linux-x64.tar.gz";
            string tarPath = Path.Combine(cartellaPython, "node_linux_temp.tar.gz");

            using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) })
            using (var response = await client.GetAsync(nodeUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                using var stream = await response.Content.ReadAsStreamAsync();
                using var fs = new FileStream(tarPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await stream.CopyToAsync(fs);
            }

            StatusText = "Estrazione Node.js in corso...";
            LogToFile("Estrazione node dal tar.gz...", 1);

            // Estraiamo solo il binario node con tar (disponibile su qualsiasi Linux)
            string extractDir = Path.Combine(cartellaPython, "node_extract_tmp");
            Directory.CreateDirectory(extractDir);

            var tar = new ProcessStartInfo
            {
                FileName = "tar",
                Arguments = $"-xzf \"{tarPath}\" -C \"{extractDir}\" --wildcards \"*/bin/node\" --strip-components=2",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var p = Process.Start(tar))
                await p!.WaitForExitAsync();

            string extractedNode = Path.Combine(extractDir, "node");
            if (File.Exists(extractedNode))
            {
                File.Move(extractedNode, nodeExe, overwrite: true);
                // Rendi eseguibile
                var chmod = new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"+x \"{nodeExe}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var cp = Process.Start(chmod);
                await cp!.WaitForExitAsync();
            }

            // Pulizia
            try { File.Delete(tarPath); } catch { }
            try { Directory.Delete(extractDir, true); } catch { }

            LogToFile("Node.js installato con successo in " + nodeExe, 1);
            StatusText = "Node.js installato con successo!";
        }
        catch (Exception ex)
        {
            LogToFile($"Errore download Node.js Linux: {ex.Message}", 2);
            StatusText = "Avviso: impossibile scaricare Node.js automaticamente.";
        }
    }

    private async Task ControllaDipendenzeAsync()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            await ControllaDipendenzeWindowsAsync();
            return;
        }

        try
        {
            ProcessStartInfo checkInfo = new ProcessStartInfo
            {
                FileName = "python3",
                Arguments = "-c \"import yt_dlp\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            LogToFile($"Controllo dipendenze Linux: {checkInfo.FileName} {checkInfo.Arguments}", 1);

            using (Process? checkProcess = Process.Start(checkInfo))
            {
                if (checkProcess != null)
                {
                    await checkProcess.WaitForExitAsync();
                    if (checkProcess.ExitCode != 0)
                    {
                        // yt-dlp manca, lo installiamo
                        StatusText = "Primo avvio: Installazione moduli necessari in corso...";
                        ProcessStartInfo installInfo = new ProcessStartInfo
                        {
                            FileName = "python3",
                            Arguments = "-m pip install --user yt-dlp",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        using (Process? installProcess = Process.Start(installInfo))
                        {
                            if (installProcess != null)
                                await installProcess.WaitForExitAsync();
                        }
                    }
                    // yt-dlp OK (o appena installato) → si prosegue SEMPRE
                }
            }
        }
        catch (System.Exception)
        {
            StatusText = "Impossibile verificare o installare le dipendenze Linux.";
        }

        // ← Chiamato SEMPRE, indipendentemente dallo stato di yt-dlp
        await ControllaDipendenzeLinuxNodeAsync();
    }

    private (string MotoreAvvio, string ArgomentiFinali) GetScriptOS(string cartellaBase, string pathCartellaSicuro, string YoutubeUrl, string cookieArg)
    {
        string motoreAvvio = "";
        string argomentiFinali = "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // LOGICA WINDOWS: Usiamo l'eseguibile compilato
            string scriptPath = Path.Combine(cartellaBase, "Python", "PythonYoutubeVideoDownloader.exe");
            motoreAvvio = scriptPath;
            argomentiFinali = $"\"{Estensione}\" \"{NumeroFile}\" \"{pathCartellaSicuro}\" \"{YoutubeUrl}\" \"{cookieArg}\"";
        }
        else
        {
            // LOGICA LINUX / macOS: Usiamo python3 e il file di testo .py
            // (Assicurati di mettere il VERO nome del tuo script qui sotto)
            string scriptPath = Path.Combine(cartellaBase, "Python", "PythonYoutubeVideoDownloader.py");
            motoreAvvio = "python3";
            // Attenzione all'ordine su Linux: prima lo script, poi le variabili!
            argomentiFinali = $"\"{scriptPath}\" \"{Estensione}\" \"{NumeroFile}\" \"{pathCartellaSicuro}\" \"{YoutubeUrl}\" \"{cookieArg}\"";
        }
        return (motoreAvvio, argomentiFinali);
    }

    private void GestioneSTDOUT(Process process)
    {
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                LogToFile($"[PYTHON STDOUT]: {e.Data}", 1);

                // Tracciamo l'indice del video nella playlist (es. Downloading item 3 of 10 o Downloading video 3 of 10)
                Match playlistIdxMatch = Regex.Match(e.Data, @"Downloading (?:item|video)\s+(\d+)\s+of\s+(\d+)", RegexOptions.IgnoreCase);
                if (playlistIdxMatch.Success)
                {
                    _currentPlaylistIndex = $"{playlistIdxMatch.Groups[1].Value}/{playlistIdxMatch.Groups[2].Value}";
                    _currentVideoTitle = ""; // reset per il nuovo elemento
                }

                // Tracciamo il titolo se yt-dlp inizia il download o l'estrazione
                Match destMatch = Regex.Match(e.Data, @"Destination:\s+.*?[\\/](.+?)\.(?:f\d+\.)?[a-zA-Z0-9]+$", RegexOptions.IgnoreCase);
                if (destMatch.Success)
                {
                    _currentVideoTitle = destMatch.Groups[1].Value;
                }

                // C# CERCA IL TAG SPECIALE
                if (e.Data.Contains("[VIDEO_ERRORE]"))
                {
                    _erroriDownload++; // Aumenta il contatore

                    // Rimuove la parola "[VIDEO_ERRORE]" per tenere solo i dati dell'errore
                    string motivoErrore = e.Data.Replace("[VIDEO_ERRORE]", "").Trim();

                    string[] errrerParti = motivoErrore.Split(' ', 3);
                    string linkErr = errrerParti.Length > 0 ? errrerParti[0] : YoutubeUrl;
                    string formatoErr = errrerParti.Length > 1 ? errrerParti[1] : Estensione;
                    string msgErr = errrerParti.Length > 2 ? errrerParti[2] : motivoErrore;

                    // Se è presente l'ID del singolo video nell'errore (es. [youtube] XVyDuUCGKSU: Private video), ricava il link del singolo video
                    Match idMatch = Regex.Match(msgErr, @"\[youtube\]\s+([a-zA-Z0-9_-]{11})");
                    if (idMatch.Success)
                    {
                        linkErr = $"https://www.youtube.com/watch?v={idMatch.Groups[1].Value}";
                    }

                    string titoloFinale = !string.IsNullOrWhiteSpace(_currentVideoTitle) ? _currentVideoTitle : "[Titolo non disponibile o Privato]";
                    string indiceFinale = !string.IsNullOrWhiteSpace(_currentPlaylistIndex) ? _currentPlaylistIndex : "N/D";

                    lock (_videoInfo)
                    {
                        _videoInfo[_erroriDownload] = Tuple.Create(linkErr, formatoErr, msgErr, indiceFinale, titoloFinale);
                    }

                    StatusText = $"Errore su un video (Totali: {_erroriDownload}). Passo al prossimo...";
                    StatusColor = "Orange";
                }
                else
                {
                    // Se è output normale (es: percentuale di download)
                    StatusText = e.Data;
                    StatusColor = "Gray";
                }
            }
        };
    }
    private void GestioneSTDERR(Process process)
    {
        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                LogToFile($"[PYTHON STDERR]: {e.Data}", 2);
                if (e.Data.Contains("[VIDEO_ERRORE]"))
                {
                    _erroriDownload++;
                    string motivoErrore = e.Data.Replace("[VIDEO_ERRORE]", "").Trim();
                    string[] errrerParti = motivoErrore.Split(' ', 3);
                    string linkErr = errrerParti.Length > 0 ? errrerParti[0] : YoutubeUrl;
                    string formatoErr = errrerParti.Length > 1 ? errrerParti[1] : Estensione;
                    string msgErr = errrerParti.Length > 2 ? errrerParti[2] : motivoErrore;

                    Match idMatch = Regex.Match(msgErr, @"\[youtube\]\s+([a-zA-Z0-9_-]{11})");
                    if (idMatch.Success)
                    {
                        linkErr = $"https://www.youtube.com/watch?v={idMatch.Groups[1].Value}";
                    }

                    string titoloFinale = !string.IsNullOrWhiteSpace(_currentVideoTitle) ? _currentVideoTitle : "[Titolo non disponibile o Privato]";
                    string indiceFinale = !string.IsNullOrWhiteSpace(_currentPlaylistIndex) ? _currentPlaylistIndex : "N/D";

                    lock (_videoInfo)
                    {
                        _videoInfo[_erroriDownload] = Tuple.Create(linkErr, formatoErr, msgErr, indiceFinale, titoloFinale);
                    }
                }
            }
        };
    }

    private void setStatustext(string totVideo, int errori, int exitcode)
    {
        if (exitcode != 0)
        {
            StatusText = "Download fermato / Errore!";
            StatusColor = "Red";
            return;
        }

        if (string.IsNullOrWhiteSpace(totVideo) || !totVideo.Contains('/'))
        {
            if (errori > 0)
            {
                StatusText = $"Download completato con {errori} errori.";
                StatusColor = "Orange";
            }
            else
            {
                StatusText = "Download terminato!";
                StatusColor = "Green";
            }
            return;
        }

        string[] index = totVideo.Split('/');
        if (int.TryParse(index[1], out int total))
        {
            if (errori >= total)
            {
                StatusText = "Download fermato / Errore!";
                StatusColor = "Red";
            }
            else if (errori > 0)
            {
                StatusText = $"Download terminato! Video scaricati con successo: {total - errori}/{total}";
                StatusColor = "Orange";
            }
            else
            {
                StatusText = $"Download terminato! Video scaricati con successo: {total}/{total}";
                StatusColor = "Green";
            }
        }
        else
        {
            StatusText = errori == 0 ? "Download terminato!" : "Download terminato con errori!";
            StatusColor = errori == 0 ? "Green" : "Orange";
        }
    }

    // Con [RelayCommand], viene generato automaticamente il comando 'DownloadCommand' collegabile al Button
    // AllowConcurrentExecutions permette di cliccare il bottone anche mentre il task è in esecuzione (per fermarlo)
    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task Download()
    {
        if (IsDownloading)
        {
            if (_currentProcess != null && !_currentProcess.HasExited)
            {
                try { _currentProcess.Kill(true); } catch { }
            }
            return;
        }

        if (string.IsNullOrWhiteSpace(YoutubeUrl) || string.IsNullOrWhiteSpace(PathCartella))
        {
            StatusText = "Inserisci un URL e una cartella validi!";
            StatusColor = "Red";
            return;
        }

        IsDownloading = true;
        ButtonText = "Ferma Download";
        StatusColor = "Gray";

        _erroriDownload = 0;
        _currentPlaylistIndex = "";
        _currentVideoTitle = "";
        lock (_videoInfo)
        {
            _videoInfo.Clear();
        }

        // Aspetterà in automatico che l'eventuale download/installazione delle dipendenze finisca
        await ControllaDipendenzeAsync();

        string cartellaBase = AppContext.BaseDirectory;
        string pathCartellaSicuro = PathCartella.Trim().TrimEnd('\\', '/');
        string cookieArg = SelectedBrowser == "Nessuno" ? "none" : SelectedBrowser;

        // Avvio di python in base al S.O.
        var (motoreAvvio, argomentiFinali) = GetScriptOS(cartellaBase, pathCartellaSicuro, YoutubeUrl, cookieArg);
        ProcessStartInfo avvioPython = new ProcessStartInfo
        {
            FileName = motoreAvvio,
            Arguments = argomentiFinali,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        // Aggiungo la cartella Python al PATH in modo che yt-dlp trovi node.exe e ffmpeg.exe
        string cartellaPython = Path.Combine(cartellaBase, "Python");
        if (Directory.Exists(cartellaPython))
        {
            avvioPython.EnvironmentVariables["PATH"] = cartellaPython + ";" + Environment.GetEnvironmentVariable("PATH");
        }

        try
        {
            LogToFile($"Avvio processo: {motoreAvvio} con argomenti: {argomentiFinali}", 1);
            using (Process process = new Process { StartInfo = avvioPython })
            {
                _currentProcess = process;

                // 3. Colleghiamo le spie che ascoltano Python in tempo reale
                GestioneSTDOUT(process);
                GestioneSTDERR(process);

                // 4. Avviamo il processo e l'ascolto
                LogToFile("Processo avviato, in attesa dell'output...", 1);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // 5. Aspettiamo che finisca, MA senza bloccare la grafica!
                await process.WaitForExitAsync();

                LogToFile($"Processo terminato con codice: {process.ExitCode}", process.ExitCode == 0 ? 1 : 2);
                string playlistInfo = !string.IsNullOrWhiteSpace(_currentPlaylistIndex)
                    ? _currentPlaylistIndex
                    : (_videoInfo.Count > 0 && _videoInfo.TryGetValue(0, out var info) ? info.Item4 : "");
                setStatustext(playlistInfo, _erroriDownload, process.ExitCode);
            }
        }
        catch (System.Exception ex)
        {
            LogToFile($"ECCEZIONE C#: {ex.Message}", 2);
            StatusText = $"Errore nell'avvio di Python.\nErrore: {ex.Message}";
            StatusColor = "Red";
        }
        finally
        {
            _currentProcess = null;
            IsDownloading = false;
            ButtonText = "Scarica Video";
        }

        lock (_videoInfo)
        {
            if (_videoInfo.Count > 0)
            {
                LogToFile("Video falliti: ", 2);
                foreach (var item in _videoInfo)
                {
                    LogToFile($"[Traccia: {item.Value.Item4}] Titolo: {item.Value.Item5} | Link: {item.Value.Item1} | Formato: {item.Value.Item2} | Errore: {item.Value.Item3}", 2);
                }
            }
        }
    }
}


