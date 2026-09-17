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

    private async Task ControllaDipendenzeWindowsAsync()
    {
        string cartellaPython = Path.Combine(AppContext.BaseDirectory, "Python");
        string ffmpegExe = Path.Combine(cartellaPython, "ffmpeg.exe");

        // Se ffmpeg esiste già nella cartella Python o nel PATH di sistema, non serve scaricare nulla
        if (File.Exists(ffmpegExe) || IsFfmpegInPath())
        {
            return;
        }

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

    private async Task ControllaDipendenzeAsync()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            await ControllaDipendenzeWindowsAsync();
            return;
        }

        try
        {
            // 1. Controlliamo se yt-dlp è già presente
            ProcessStartInfo checkInfo = new ProcessStartInfo
            {
                FileName = "python3",
                Arguments = "-c \"import yt_dlp\"", // Tenta di importare la libreria
                UseShellExecute = false,
                CreateNoWindow = true
            };
            LogToFile($"Controllo dipendenze Linux: {checkInfo.FileName} {checkInfo.Arguments}", 1);

            using (Process? checkProcess = Process.Start(checkInfo))
            {
                if (checkProcess != null)
                {
                    await checkProcess.WaitForExitAsync();
                    if (checkProcess.ExitCode == 0)
                    {
                        // ExitCode 0 significa che non ci sono stati errori. yt-dlp esiste!
                        return;
                    }
                }
            }

            // 2. Se arriviamo qui, yt-dlp manca. Avvisiamo l'utente e lo installiamo.
            StatusText = "Primo avvio: Installazione moduli necessari in corso...";

            ProcessStartInfo installInfo = new ProcessStartInfo
            {
                FileName = "python3",
                // Usiamo --user per non chiedere i permessi di amministratore (root) a Linux
                Arguments = "-m pip install --user yt-dlp",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process? installProcess = Process.Start(installInfo))
            {
                if (installProcess != null)
                {
                    await installProcess.WaitForExitAsync();
                }
            }
        }
        catch (System.Exception)
        {
            StatusText = "Impossibile verificare o installare le dipendenze Linux.";
        }
    }


    // Con [RelayCommand], viene generato automaticamente il comando 'DownloadCommand' collegabile al Button
    // AllowConcurrentExecutions permette di cliccare il bottone anche mentre il task è in esecuzione (per fermarlo)
    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task Download()
    {
        int ErroriDownload = 0;
        string currentPlaylistIndex = "";
        string currentVideoTitle = "";

        // {idcounterVideo:{Link, Formato, MessaggioErrore, IndicePlaylist, TitoloVideo}}
        Dictionary<int, Tuple<string, string, string, string, string>> videoInfo = new Dictionary<int, Tuple<string, string, string, string, string>>();

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
            return;
        }

        IsDownloading = true;
        ButtonText = "Ferma Download";

        // Aspetterà in automatico che l'eventuale download/installazione delle dipendenze finisca
        await ControllaDipendenzeAsync();

        string cartellaBase = AppContext.BaseDirectory;
        string motoreAvvio = "";
        string argomentiFinali = "";

        string pathCartellaSicuro = PathCartella.Trim().TrimEnd('\\', '/');

        // C# chiede: "Siamo su Windows?"
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // LOGICA WINDOWS: Usiamo l'eseguibile compilato
            string scriptPath = Path.Combine(cartellaBase, "Python", "PythonYoutubeVideoDownloader.exe");
            motoreAvvio = scriptPath;
            argomentiFinali = $"\"{Estensione}\" \"{NumeroFile}\" \"{pathCartellaSicuro}\" \"{YoutubeUrl}\"";
        }
        else
        {
            // LOGICA LINUX / macOS: Usiamo python3 e il file di testo .py
            // (Assicurati di mettere il VERO nome del tuo script qui sotto)
            string scriptPath = Path.Combine(cartellaBase, "Python", "PythonYoutubeVideoDownloader.py");
            motoreAvvio = "python3";
            // Attenzione all'ordine su Linux: prima lo script, poi le variabili!
            argomentiFinali = $"\"{scriptPath}\" \"{Estensione}\" \"{NumeroFile}\" \"{pathCartellaSicuro}\" \"{YoutubeUrl}\"";
        }



        ProcessStartInfo avvioPython = new ProcessStartInfo
        {
            FileName = motoreAvvio,
            Arguments = argomentiFinali,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        try
        {
            LogToFile($"Avvio processo: {motoreAvvio} con argomenti: {argomentiFinali}", 1);
            using (Process process = new Process { StartInfo = avvioPython })
            {
                _currentProcess = process;
                // 3. Creiamo le "spie" che ascoltano Python in tempo reale
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        LogToFile($"[PYTHON STDOUT]: {e.Data}", 1);

                        // Tracciamo l'indice del video nella playlist (es. Downloading item 3 of 10 o Downloading video 3 of 10)
                        Match playlistIdxMatch = Regex.Match(e.Data, @"Downloading (?:item|video)\s+(\d+)\s+of\s+(\d+)", RegexOptions.IgnoreCase);
                        if (playlistIdxMatch.Success)
                        {
                            currentPlaylistIndex = $"{playlistIdxMatch.Groups[1].Value}/{playlistIdxMatch.Groups[2].Value}";
                            currentVideoTitle = ""; // reset per il nuovo elemento
                        }

                        // Tracciamo il titolo se yt-dlp inizia il download o l'estrazione
                        Match destMatch = Regex.Match(e.Data, @"Destination:\s+.*?[\\/](.+?)\.(?:f\d+\.)?[a-zA-Z0-9]+$", RegexOptions.IgnoreCase);
                        if (destMatch.Success)
                        {
                            currentVideoTitle = destMatch.Groups[1].Value;
                        }

                        // C# CERCA IL TAG SPECIALE
                        if (e.Data.Contains("[VIDEO_ERRORE]"))
                        {
                            ErroriDownload++; // Aumenta il contatore

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

                            string titoloFinale = !string.IsNullOrWhiteSpace(currentVideoTitle) ? currentVideoTitle : "[Titolo non disponibile o Privato]";
                            string indiceFinale = !string.IsNullOrWhiteSpace(currentPlaylistIndex) ? currentPlaylistIndex : "N/D";

                            lock (videoInfo)
                            {
                                videoInfo[ErroriDownload] = Tuple.Create(linkErr, formatoErr, msgErr, indiceFinale, titoloFinale);
                            }

                            StatusText = $"Errore su un video (Totali: {ErroriDownload}). Passo al prossimo...";
                        }
                        else
                        {
                            // Se è output normale (es: percentuale di download)
                            StatusText = e.Data;
                        }
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        LogToFile($"[PYTHON STDERR]: {e.Data}", 2);
                        if (e.Data.Contains("[VIDEO_ERRORE]"))
                        {
                            ErroriDownload++;
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

                            string titoloFinale = !string.IsNullOrWhiteSpace(currentVideoTitle) ? currentVideoTitle : "[Titolo non disponibile o Privato]";
                            string indiceFinale = !string.IsNullOrWhiteSpace(currentPlaylistIndex) ? currentPlaylistIndex : "N/D";

                            lock (videoInfo)
                            {
                                videoInfo[ErroriDownload] = Tuple.Create(linkErr, formatoErr, msgErr, indiceFinale, titoloFinale);
                            }
                        }
                    }
                };

                // 4. Avviamo il processo e l'ascolto
                LogToFile("Processo avviato, in attesa dell'output...", 1);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // 5. Aspettiamo che finisca, MA senza bloccare la grafica!
                await process.WaitForExitAsync();

                LogToFile($"Processo terminato con codice: {process.ExitCode}", 1);
                StatusText = process.ExitCode == 0 ? "Download terminato!" : "Download fermato / Errore!";
            }
        }
        catch (System.Exception ex)
        {
            LogToFile($"ECCEZIONE C#: {ex.Message}", 2);
            StatusText = $"Errore nell'avvio di Python.\nErrore: {ex.Message}";
        }
        finally
        {
            _currentProcess = null;
            IsDownloading = false;
            ButtonText = "Scarica Video";
        }
        LogToFile("Video falliti: ", 2);
        foreach (var item in videoInfo)
        {
            LogToFile($"[Traccia: {item.Value.Item4}] Titolo: {item.Value.Item5} | Link: {item.Value.Item1} | Formato: {item.Value.Item2} | Errore: {item.Value.Item3}", 2);
        }
    }
}


