<p align="center">
  <img width="256" height="256" alt="favicon-6" src="https://github.com/user-attachments/assets/63a74903-0fa3-4333-9ac9-8a6ce1411833" />
</p>
<div align="center">
  <h1>Youtube Video Downloader</h1>
  
  ![.NET](https://img.shields.io/badge/.NET-8.0-512BD4.svg?style=flat&logo=dotnet)
  ![Avalonia UI](https://img.shields.io/badge/UI-Avalonia%20C%23-purple.svg?style=flat)
  ![Python](https://img.shields.io/badge/Engine-Python%20%2B%20yt--dlp-blue?style=flat&logo=python&logoColor=FFD43B)
  ![Platform](https://img.shields.io/badge/Platforms-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey.svg?style=flat)
</div>
<br/>

Un'applicazione desktop moderna e intuitiva sviluppata in **C# (Avalonia UI)** e **Python (yt-dlp)** che permette di scaricare video e brani musicali da YouTube con estrema facilità.

## 🌟 Funzionalità e Opzioni

Questa applicazione è stata progettata per rendere il processo di download accessibile a chiunque tramite un'interfaccia grafica pulita, rimuovendo la necessità di interagire con riga di comando.

### Opzioni disponibili:
- **Scelta del Formato:**
  - **.mp4:** Scarica la migliore qualità video ed estrae automaticamente anche il miglior audio, unendoli in un unico file `.mp4`.
  - **.mp3:** Estrae solo l'audio dal video convertendolo nel classico formato `.mp3` (alla massima qualità disponibile, 320kbps).
  - **.webm:** Scarica il video nel formato libero `.webm`.
- **Numero di File:**
  - **Singolo:** Scarica esclusivamente il video specifico indicato dall'URL.
  - **Playlist:** Se l'URL fa parte di una playlist o è un link a una playlist intera, l'applicazione scaricherà in blocco tutti i video contenuti, inserendoli in una cartella apposita.
- **Interruzione in tempo reale:** Se hai avviato un download per errore o vuoi fermarlo, il tasto di scaricamento diventerà rosso. Cliccandolo interromperai immediatamente il processo di download in modo sicuro.
- **Logica Intelligente per le Dipendenze:** 
  - **Windows:** Scarica ed estrae automaticamente l'eseguibile di `ffmpeg` da GitHub al primo avvio, se mancante, in modo del tutto invisibile.
  - **Linux:** Il motore Python controlla autonomamente la presenza di `yt-dlp` aggiornato, `pip` e `ffmpeg`. In caso di componenti mancanti, il programma aprirà un popup chiedendo i permessi per installare le dipendenze in modo trasparente.
- **Gestione Avanzata degli Errori (Novità!):**
  - Tracciamento accurato degli indici (es. `1/10`) e del titolo del video durante i download di playlist.
  - Se un video all'interno di una playlist risulta non scaricabile (es. video privato o non disponibile), l'errore viene catturato in modo pulito nei file di log (`YoutubeVideoDownloaderERRORS.log`) ricostruendo il link del video esatto per facilitare l'ispezione dell'utente, senza interrompere la playlist.

## 🚀 Come avviare il programma

Se stai usando i file sorgenti, puoi lanciare il programma dal terminale nella cartella del progetto con:
```bash
dotnet run
```

### 🐧 Note per gli Utenti Linux (Versione Pre-Compilata)

Se hai scaricato o creato la **Release eseguibile** del programma (il file senza estensione chiamato `YoutubeVideoDownloader`), il tuo sistema operativo per motivi di sicurezza potrebbe non farti avviare il programma se non gli concedi prima i permessi.

Per avviarlo con il doppio clic, fai così:
1. Fai **clic col tasto destro** sul file `YoutubeVideoDownloader`.
2. Seleziona **Proprietà** (Properties) dal menu a tendina.
3. Spostati nella scheda **Permessi** (Permissions).
4. Spunta la casella **"Consentire l'esecuzione del file come programma"** (in inglese: _"Execute: allow executing file as a program"_).
5. Chiudi la finestra delle proprietà.
6. Fai **doppio clic** sul file per aprirlo!

*(Se preferisci il terminale, ti basta aprire il terminale in quella cartella e lanciare `chmod +x YoutubeVideoDownloader` e poi `./YoutubeVideoDownloader`).*

## 🛠️ Come Compilare dal Sorgente

Per creare in autonomia una release singola (un eseguibile "Self-Contained" che include tutto senza dipendenze aggiuntive), apri un terminale, entra nella cartella `YoutubeVideoDownloader` e lancia il comando appropriato per il tuo sistema:

### Windows
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false
```

### Linux
```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false
```
oppure:

```bash
~/.dotnet/dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true
```