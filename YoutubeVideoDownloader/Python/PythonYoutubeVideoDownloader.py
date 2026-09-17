
import importlib
import site
import os
"""https://youtu.be/sedGP9VAxEE?si=qpxi3_3OpVB7eRZR"""
import subprocess
import shutil
import sys

class MyLogger:
    def debug(self, msg):
        # yt-dlp manda i progressi del download qui. Li stampiamo normalmente.
        print(msg, flush=True)

    def warning(self, msg):
        print(f"[ATTENZIONE] {msg}", flush=True)

    def error(self, msg):
        # Quando un video fallisce, yt-dlp chiama questa funzione.
        # Aggiungiamo un "TAG" speciale che C# potrà riconoscere!
        url_input = sys.argv[4] if len(sys.argv) > 4 else "N/D"
        format_input = sys.argv[1] if len(sys.argv) > 1 else "N/D"
        format_map = {"1": "mp4", "2": "mp3", "3": "webm"}
        format_str = format_map.get(format_input, format_input)
        print(f"[VIDEO_ERRORE] {url_input} {format_str} {msg}", flush=True)

def installDependencies():
    # 1. CONTROLLO E INSTALLAZIONE DI PIP (Il gestore pacchetti)
    # Verifichiamo se il modulo 'pip' è disponibile per Python
    try:
        import pip
    except ImportError:
        print("PIP non trovato. Tentativo di installazione automatica...")
        try:
            # Eseguiamo l'installazione tramite pkexec (apre un popup grafico per la password)
            subprocess.run(['pkexec', 'sh', '-c', 'apt update && apt install -y python3-pip'], check=True)
            print("PIP installato con successo.")
        except Exception as e:
            print(f"Errore critico: Impossibile installare PIP ({e})")
            print("Esegui manualmente: sudo apt install python3-pip")
            return  # Esci perché senza pip non possiamo fare il resto

    # 2. AGGIORNAMENTO SEMPRE ATTIVO DI YT-DLP 
    print("Verifica aggiornamenti di yt-dlp...")
    try:
        subprocess.run([
            sys.executable, "-m", "pip",
            "install", "-U", "yt-dlp", "--break-system-packages"
        ], check=True)
        print("yt-dlp è aggiornato.")
    except Exception as e:
        raise Exception(f'Imposibile aggiornare yt-dlp automaticamente. Errore: {e}')

    # 3. CONTROLLO FFMPEG
    if shutil.which('ffmpeg') is None:
        print('FFMPEG non trovato. Insallazione in corso')
        try:
            subprocess.run(['pkexec', 'sh', '-c', 'apt update && apt install -y ffmpeg'], check=True)
            print('FFMPEG Installato con successo!')
        except subprocess.CalledProcessError:
            raise Exception('Imposibile installare ffmpeg. Inserisci la password corretta o installalo manualmente')


def ytopt(extension, fileNumber) -> dict[str, str | bool]:
    ytdplopt = {
        "format": "bestvideo[ext=webm]+bestaudio[ext=webm]/best[ext=webm]",
        'merge_output_format': 'webm',
        'no_warnings': True,
        'nocheckcertificate': True,
        'ignoreerrors':True,
        'logger':MyLogger()
    }

    if path != "":
        ytdplopt['paths'] = {'home': path}

    if extension == "1":
        ytdplopt['format'] = "bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best"
        ytdplopt['merge_output_format'] = 'mp4'

    if extension == "2":
        ytdplopt['format'] = 'bestaudio[ext=m4a]/bestaudio/best'
        ytdplopt['postprocessors'] = [{
            'key': 'FFmpegExtractAudio',
            'preferredcodec': 'mp3',
            'preferredquality': '320',
        }]

    if fileNumber == "p":
        ytdplopt['outtmpl'] = '%(playlist_title)s/%(title)s.%(ext)s'
        ytdplopt['noplaylist'] = False
    else:
        ytdplopt['outtmpl'] = '%(title)s.%(ext)s'
        ytdplopt['noplaylist'] = True
        
#    ytdplopt['extractor_args'] = {
#        'youtube': {
#            "player_client": ["android", "ios"],
#            "player_skip": ["web"],
#        }
#    }

    return ytdplopt

if __name__ == '__main__':
    import multiprocessing
    multiprocessing.freeze_support()
    if len(sys.argv) < 5:
        print(f"[VIDEO_ERRORE] N/D N/D Argomenti insufficienti per avviare il download (ricevuti {len(sys.argv)-1}, attesi 4)", flush=True)
        sys.exit(1)

    extension = sys.argv[1]
    fileNumber = sys.argv[2]
    path = sys.argv[3]
    url = sys.argv[4]
    try:
        if sys.platform != 'win32' and not getattr(sys, 'frozen', False):
            installDependencies()
            user_site = site.getusersitepackages()
            if user_site not in sys.path:
                sys.path.insert(0, user_site)

            # Forza il ricaricamento delle librerie installate
            importlib.reload(site)
            importlib.invalidate_caches()

        options = ytopt(extension, fileNumber)
        import yt_dlp
        from yt_dlp.utils import DownloadError

        print(f"Dati passati:{ytopt(extension, fileNumber)}")
        yt_dlp.YoutubeDL(options).download([url])
    #    break  # Esce dal ciclo se il download va a buon fine
    except DownloadError:
        print("Invalid link")
        raise Exception("Invalid link.")

    except Exception as e:
        print(f"Unexpected error: {e}")
        raise Exception(f"Unexpected error: {e}")

    # while answer.lower()!="o" and answer.lower()!="d":
    #    answer=input("Chose an action:\n d Download\n o Options\nYour answer: ")
    # if answer.lower()=="o":
    #    while answer.lower()!="q" and answer.lower()!="c":
    #        answer = input("Chose an action:\n c Change directory for the files\n q Exit \nYour answer: ")
    #        if answer.lower() =="c":
    #            path=input("Paste the path you want to download: ")


    # while extension!="1" and extension!="2":
    #    extension = input("Select your extension:\n 1 mp4\n 2 mp3\n 3 webm\nYour option: ")
    # while fileNumber.lower()!="s" and fileNumber.lower()!="p":
    #    fileNumber = input("Type of download:\n s Single video\n p Playlist\nYour option: ")

    # while True:
    #    print()
    #    url = input("\nInsert a Youtube Link (or 'q' to exit): ")
    #    if url.lower() == 'q': break

