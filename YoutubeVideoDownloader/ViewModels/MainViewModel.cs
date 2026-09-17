using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Runtime.InteropServices;

namespace YoutubeVideoDownloader.ViewModels;

public partial class MainViewModel : ViewModelBase
{


    void LogToFile(string message, int fileType)
    {
        if (fileType == 1)
        {
            try
            {
                string logPath = Path.Combine(AppContext.BaseDirectory, "YoutubeVideoDownloaderERRORS.txt");
                System.IO.File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n");
            }
            catch { /* Ignora errori di log */ }
        }
        else if (fileType == 2)
        {
            try
            {
                string logPath = Path.Combine(AppContext.BaseDirectory, "YoutubeVideoDownloaderErrorLog.txt");
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

    private async Task ControllaDipendenzeLinuxAsync()
    {
        // Se siamo su Windows, usiamo l'exe e saltiamo tutto questo!
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

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

        // {idcounterVideo:{Link,Formato,MessaggioErrore}}
        Dictionary<int, Tuple<string, string, string>> videoInfo = new Dictionary<int, Tuple<string, string, string>>();

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

        // Aggiungi questa riga! Aspetterà in automatico che l'eventuale installazione finisca
        await ControllaDipendenzeLinuxAsync();

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

                            lock (videoInfo)
                            {
                                videoInfo[ErroriDownload] = Tuple.Create(linkErr, formatoErr, msgErr);
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

                            lock (videoInfo)
                            {
                                videoInfo[ErroriDownload] = Tuple.Create(linkErr, formatoErr, msgErr);
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
            LogToFile($"- Link: {item.Value.Item1} | Formato: {item.Value.Item2} | Errore: {item.Value.Item3}", 2);
        }
    }
}


