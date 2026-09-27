# -*- mode: python ; coding: utf-8 -*-


a = Analysis(
    ['Python/PythonYoutubeVideoDownloader.py'],
    pathex=[],
    binaries=[],
    datas=[],
    hiddenimports=['yt_dlp', 'pycryptodomex', 'mutagen', 'websockets', 'certifi', 'brotli', 'yt_dlp_ejs', 'yt_dlp_ejs.yt.solver', 'yt_dlp.extractor.youtube.jsc._builtin.node', 'yt_dlp.extractor.youtube.jsc._builtin.ejs', 'yt_dlp.extractor.youtube.jsc._builtin.bun', 'yt_dlp.extractor.youtube.jsc._builtin.deno', 'yt_dlp.extractor.youtube.jsc._builtin.quickjs'],
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=[],
    noarchive=False,
    optimize=0,
)
pyz = PYZ(a.pure)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.datas,
    [],
    name='PythonYoutubeVideoDownloader',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    upx_exclude=[],
    runtime_tmpdir=None,
    console=True,
    disable_windowed_traceback=False,
    argv_emulation=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
)
