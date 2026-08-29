using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Runtime.InteropServices;

namespace YoutubeVideoDownloader.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    // Creando questo campo privato con [ObservableProperty], 
    // il toolkit genera in automatico la proprietà pubblica 'YoutubeUrl' collegabile alla grafica.
    [ObservableProperty]
    private string _estensione = "1";
    [ObservableProperty]
    private string _numeroFile = "s";
    [ObservableProperty]
    private string _pathCartella = "";
    [ObservableProperty]
    private string _youtubeUrl = "";
    // Genera automaticamente la proprietà pubblica 'StatusText'
    [ObservableProperty]
    private string _statusText = "Pronto per scaricare";

    [ObservableProperty]
    private bool _isDownloading = false;

    [ObservableProperty]
    private string _buttonText = "Scarica Video";

    private Process? _currentProcess;

    public bool IsMp4Selected
    {
        get=>_estensione == "1";
        set
        {
            if (value)
            {
                _estensione = "1";
            }
        }
    }
    public bool IsMp3Selected
    {
        get=>_estensione == "2";
        set
        {
            if (value)
            {
                _estensione = "2";
            }
        }
    }
    public bool IsWebmSelected
    {
        get => _estensione == "3";
        set
        {
            if (value)
            {
                _estensione ="3";
            }
        }
    }
    
    public bool IsSingleSelected
    {
        get=>_numeroFile == "s";
        set
        {
            if (value)
            {
                _numeroFile = "s";
            }
        }
    }
    public bool IsPlaylistSelected
    {
        get=>_numeroFile == "p";
        set
        {
            if (value)
            {
                _numeroFile = "p";
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

            using (Process checkProcess = Process.Start(checkInfo))
            {
                await checkProcess.WaitForExitAsync();
                if (checkProcess.ExitCode == 0)
                {
                    // ExitCode 0 significa che non ci sono stati errori. yt-dlp esiste!
                    return; 
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

            using (Process installProcess = Process.Start(installInfo))
            {
                await installProcess.WaitForExitAsync();
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

        // C# chiede: "Siamo su Windows?"
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // LOGICA WINDOWS: Usiamo l'eseguibile compilato
            string scriptPath = Path.Combine(cartellaBase, "Python", "PythonYoutubeVideoDownloader.exe");
            motoreAvvio = scriptPath; 
            argomentiFinali = $"\"{_estensione}\" \"{_numeroFile}\" \"{PathCartella}\" \"{YoutubeUrl}\"";
        }
        else
        {
            // LOGICA LINUX / macOS: Usiamo python3 e il file di testo .py
            // (Assicurati di mettere il VERO nome del tuo script qui sotto)
            string scriptPath = Path.Combine(cartellaBase, "Python", "PythonYoutubeVideoDownloader.py");
            motoreAvvio = "python3"; 
            // Attenzione all'ordine su Linux: prima lo script, poi le variabili!
            argomentiFinali = $"\"{scriptPath}\" \"{_estensione}\" \"{_numeroFile}\" \"{PathCartella}\" \"{YoutubeUrl}\"";
        }

        void LogToFile(string message)
        {
            try
            {
                string logPath = Path.Combine(AppContext.BaseDirectory, "YoutubeVideoDownloaderLog.txt");
                System.IO.File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n");
            }
            catch { /* Ignora errori di log */ }
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
            LogToFile($"Avvio processo: {motoreAvvio} con argomenti: {argomentiFinali}");
            using (Process process = new Process{ StartInfo = avvioPython })
            {
                _currentProcess = process;
                // 3. Creiamo le "spie" che ascoltano Python in tempo reale
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        LogToFile($"[PYTHON STDOUT]: {e.Data}");
                        // Aggiorna il testo sulla grafica con la nuova riga
                        StatusText = e.Data;
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        LogToFile($"[PYTHON STDERR]: {e.Data}");
                        StatusText = $"Errore: {e.Data}";
                    }
                };

                // 4. Avviamo il processo e l'ascolto
                LogToFile("Processo avviato, in attesa dell'output...");
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // 5. Aspettiamo che finisca, MA senza bloccare la grafica!
                await process.WaitForExitAsync();
                
                LogToFile($"Processo terminato con codice: {process.ExitCode}");
                StatusText = process.ExitCode == 0 ? "Download terminato!" : "Download fermato / Errore!";
            }
        }
        catch (System.Exception ex)
        {
            LogToFile($"ECCEZIONE C#: {ex.Message}");
            StatusText = $"Errore nell'avvio di Python.\nErrore: {ex.Message}";
        }
        finally
        {
            _currentProcess = null;
            IsDownloading = false;
            ButtonText = "Scarica Video";
        }
    }
}


