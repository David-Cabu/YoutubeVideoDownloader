<p align="center">
  <img width="256" height="256" alt="favicon-6" src="https://github.com/user-attachments/assets/63a74903-0fa3-4333-9ac9-8a6ce1411833" />
</p>

# Youtube Video Downloader

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4.svg?style=flat&logo=dotnet)
![Avalonia UI](https://img.shields.io/badge/UI-Avalonia%20C%23-purple.svg?style=flat)
![Python](https://img.shields.io/badge/Motore-Python%20%2B%20yt--dlp-blue.svg?style=flat&logo=python)
![Platform](https://img.shields.io/badge/Piattaforma-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey.svg?style=flat)

<br/>

Un'applicazione desktop moderna e intuitiva sviluppata in **C# (Avalonia UI)** e **Python (yt-dlp)** che permette di scaricare video e brani musicali da YouTube con estrema facilità.

## ✨ Funzionalità e Opzioni

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
- **Logica Intelligente per le Dipendenze (Linux):** Sotto al cofano, il motore Python controllerà autonomamente la presenza di `yt-dlp` aggiornato, `pip` e `ffmpeg`. In caso di componenti mancanti, il programma aprirà un popup grafico nativo chiedendo la password e installerà le dipendenze in modo completamente trasparente.

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

Per creare in autonomia una release Linux (un singolo file eseguibile "Self-Contained" che include tutto), apri un terminale, entra nella cartella `YoutubeVideoDownloader` e lancia il comando:
```bash
dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true -o ../Release-Linux-SingleFile
```

