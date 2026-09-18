
import importlib
import site
import os
"""https://youtu.be/sedGP9VAxEE?si=qpxi3_3OpVB7eRZR"""
import subprocess
import shutil
import sys

# Forza stdout e stderr in UTF-8
if sys.stdout and hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
if sys.stderr and hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding='utf-8', errors='replace')

class MyLogger:
    def __init__(self):
        self.errors = []
        self.current_idx = ""

    def debug(self, msg):
        import re
        m = re.search(r'Downloading (?:item|video)\s+(\d+)\s+of\s+(\d+)', msg, re.IGNORECASE)
        if m:
            self.current_idx = f"Downloading video {m.group(1)} of {m.group(2)}"
        # yt-dlp manda i progressi del download qui. Li stampiamo normalmente.
        print(msg, flush=True)

    def warning(self, msg):
        print(f"[ATTENZIONE] {msg}", flush=True)

    def error(self, msg):
        # Quando un video fallisce, salviamo l'errore per i fallback
        self.errors.append((msg, self.current_idx))

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


def ytopt(extension, fileNumber, logger=None) -> dict[str, str | bool]:
    ytdplopt = {
        "format": "bestvideo[ext=webm]+bestaudio[ext=webm]/best[ext=webm]",
        'merge_output_format': 'webm',
        'no_warnings': True,
        'nocheckcertificate': True,
        'ignoreerrors':True,
        'logger': logger if logger is not None else MyLogger()
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
        print(f"[VIDEO_ERRORE] N/D Argomenti insufficienti per avviare il download (ricevuti {len(sys.argv)-1}, attesi 4)", flush=True)
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

        import yt_dlp
        from yt_dlp.utils import DownloadError

        main_logger = MyLogger()
        options = ytopt(extension, fileNumber, main_logger)

        print(f"Dati passati:{options}")
        
        # Ottieni il titolo della playlist per mantenere i file nella cartella giusta nei fallback
        playlist_title = "NA"
        if fileNumber == "p":
            try:
                with yt_dlp.YoutubeDL({'quiet': True, 'extract_flat': 'in_playlist'}) as ydl:
                    info = ydl.extract_info(url, download=False)
                    if info and 'title' in info:
                        playlist_title = info['title']
            except:
                pass

        yt_dlp.YoutubeDL(options).download([url])

        if main_logger.errors:
            import re
            
            fallback_map = {
                "1": ["2", "3"],
                "2": ["1", "3"],
                "3": ["1", "2"]
            }
            fallbacks = fallback_map.get(extension, [])
                
            for err_tuple in main_logger.errors:
                if isinstance(err_tuple, tuple):
                    err_msg, err_idx = err_tuple
                else:
                    err_msg, err_idx = err_tuple, ""

                m = re.search(r'\[youtube\](?:[:\s]+)?([a-zA-Z0-9_-]{11})', err_msg)
                if m:
                    fail_url = f"https://www.youtube.com/watch?v={m.group(1)}"
                else:
                    fail_url = url
                
                success = False
                for fb_ext in fallbacks:
                    format_name_map = {"1": "mp4", "2": "mp3", "3": "webm"}
                    fb_format_name = format_name_map.get(fb_ext, fb_ext)
                    print(f"Tentativo di fallback con formato {fb_format_name} per: {fail_url}", flush=True)
                    fb_logger = MyLogger()
                    opts_fb = ytopt(fb_ext, fileNumber, fb_logger)
                    
                    if fileNumber == "p" and playlist_title != "NA":
                        # Inietta il titolo della playlist al posto della variabile per mantenere la cartella
                        # Attenzione: i caratteri speciali non saranno sanitizzati come farebbe yt-dlp nativamente,
                        # ma permette di mantenere una struttura sensata.
                        opts_fb['outtmpl'] = f"{playlist_title}/%(title)s.%(ext)s"
                        
                    yt_dlp.YoutubeDL(opts_fb).download([fail_url])
                    
                    if not fb_logger.errors:
                        success = True
                        break
                        
                if not success:
                    format_input = extension
                    format_map = {"1": "mp4", "2": "mp3", "3": "webm"}
                    format_str = format_map.get(format_input, format_input)
                    
                    # Trick C# into updating its internal index tracker before emitting the error
                    if err_idx:
                        print(f"[download] {err_idx}", flush=True)
                    print(f"[VIDEO_ERRORE] {fail_url} {format_str} {err_msg}", flush=True)
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

