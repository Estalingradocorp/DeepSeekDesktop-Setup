# DeepSeek Desktop

Aplicación de escritorio para Windows que abre **chat.deepseek.com** como app nativa
(ventana propia con el icono de DeepSeek), usando WebView2 (ya incluido en Windows 10/11).

## Artefactos

| Archivo | Qué es |
|---|---|
| `DeepSeekDesktop-Setup.exe` | Instalador que se reparte a los equipos (≈64 MB) |
| `dist\app\DeepSeekDesktop.exe` | Versión portable (≈72 MB), sin instalar, doble clic y listo |

Requisito en cada equipo: **WebView2 Runtime** (ya viene con Windows 10/11 y Edge).
No necesita .NET instalado (la app es autocontenida).

## Instalar en un equipo (manual)

1. Copia `DeepSeekDesktop-Setup.exe` al equipo.
2. Doble clic y sigue el asistente (instala para el usuario actual, sin permisos de admin).
3. Aparece un acceso directo "DeepSeek Desktop" en el menú Inicio (y opcional en el escritorio).

## Distribuir a varios equipos (opciones)

### A) Carpeta compartida (SMB) — más simple
1. Copia `DeepSeekDesktop-Setup.exe` a un recurso compartido, p. ej. `\\servidor\software\`.
2. Cada usuario hace doble clic desde esa carpeta y se instala.

### B) Instalación silenciosa por script (PowerShell)
Copia el instalador y ejecuta en cada equipo:

```powershell
# Instalar en segundo plano para el usuario actual, sin mostrar asistente
& "\\servidor\software\DeepSeekDesktop-Setup.exe" /VERYSILENT /SUPPRESSMSGBOXES /NORESTART
# Crear acceso directo en el escritorio (opcional):
# añade /MERGETASKS="desktopicon"
```

### C) Despliegue por GPO (Microsoft Entra / Active Directory)
1. Copia el instalador a un recurso compartido de red.
2. En `Computer Configuration > Software Installation`, añade un nuevo paquete apuntando al `.exe`.
3. En "Deployment Method" elige *Assigned*. Se instala automáticamente al iniciar sesión o por `gpupdate /force`.

## Desinstalar
Panel de control → Programas → **DeepSeek Desktop** → Desinstalar.
Borra la app y su caché local.

## Reconstruir (desde el código fuente)
```powershell
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true -o .\dist\app
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer.iss
```

> Nota: el binario no está firmado con certificado de código, por lo que Windows SmartScreen
> puede mostrar una advertencia en los equipos destino. Para eliminarla habría que firmar
> el exe con un certificado (p. ej. de una CA comercial).