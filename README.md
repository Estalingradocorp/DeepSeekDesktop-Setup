# DeepSeek Desktop — launcher de Estalingrado Corp

Aplicación de escritorio para Windows de **Estalingrado Corp** que abre
[chat.deepseek.com](https://chat.deepseek.com/) como una app nativa, sin necesidad
de abrir el navegador. Usa **WebView2** (ya incluido en Windows 10/11) y es
**autocontenida** (no requiere .NET instalado en el equipo destino).

## Características

- Ventana nativa de Windows con el icono de DeepSeek.
- Menú integrado con opciones:
  - **Archivo** → Recargar página, Abrir en el navegador, Salir.
  - **Ayuda** → Acerca de Estalingrado Corp (qué hace el launcher y qué es DeepSeek).
- Se adapta al **modo claro/oscuro** de Windows (fondo + barra de título).
- Las **letras del menú superior** ("Archivo", "Ayuda") se adaptan al tema
  (claras en modo oscuro, oscuras en modo claro); los menús desplegables
  conservan su estilo nativo de Windows.
- **Recuerda el tamaño y la posición** de la ventana entre sesiones.
- Tamaño de ventana redimensionable y limitado al área útil de la pantalla
  (la barra superior nunca se sale de la vista).

## Requisitos en el equipo destino

- **Windows 10/11** (64 bits). El WebView2 Runtime viene incluido con Windows 10/11
  y Edge; si faltara, el instalador puede pedirlo.
- No necesita .NET instalado: el ejecutable es autocontenido.

## Artefactos generados

| Archivo | Qué es |
|---|---|
| `DeepSeekDesktop-Setup.exe` | Instalador distribuible (≈64 MB) |
| `dist\app\DeepSeekDesktop.exe` | Versión portable, sin instalar (≈72 MB) |

## Instalación

```powershell
# Manual: doble clic sobre DeepSeekDesktop-Setup.exe y seguir el asistente.
# Silenciosa (para desplegar en varios equipos):
& "\\servidor\software\DeepSeekDesktop-Setup.exe" /VERYSILENT /SUPPRESSMSGBOXES /NORESTART
```

Ver más opciones de distribución (SMB, script, GPO) en [`DISTRIBUCION.md`](DISTRIBUCION.md).

## Compilar desde el código fuente

Requisitos previos: SDK de .NET 8 y [Inno Setup 6](https://jrsoftware.org/isinfo.php).

```powershell
# 1. Publicar el ejecutable autocontenido de un solo archivo
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true -o .\dist\app

# 2. Generar el instalador
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer.iss
```

> Nota: si Inno Setup se instaló por usuario, ISCC.exe está en
> `%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe`.

## Publicar una Release (automática)

El repo incluye un flujo de GitHub Actions (`.github/workflows/release.yml`) que
compila el instalador y lo sube como Release al marcar una versión con una tag `v*`:

```bash
git tag v1.0.0
git push origin v1.0.0
```

El instalador `DeepSeekDesktop-Setup.exe` quedará adjunto a la Release en
https://github.com/Estalingradocorp/DeepSeekDesktop-Setup/releases, listo para
distribuir a los equipos.

## Versiones publicadas

| Versión | Cambios | Descarga |
|---|---|---|
| `v1.0.0` | Versión inicial: WebView2, menú, tema claro/oscuro, persistencia de ventana, instalador Inno Setup. | [DeepSeekDesktop-Setup.exe](https://github.com/Estalingradocorp/DeepSeekDesktop-Setup/releases/download/v1.0.0/DeepSeekDesktop-Setup.exe) |
| `v1.0.1` | Visibilidad del menú en modo oscuro. | [DeepSeekDesktop-Setup.exe](https://github.com/Estalingradocorp/DeepSeekDesktop-Setup/releases/download/v1.0.1/DeepSeekDesktop-Setup.exe) |
| `v1.0.2` | **Actual:** solo el color de las letras del menú sigue el tema; los desplegables mantienen su estilo nativo. | [DeepSeekDesktop-Setup.exe](https://github.com/Estalingradocorp/DeepSeekDesktop-Setup/releases/download/v1.0.2/DeepSeekDesktop-Setup.exe) |

## Estructura del proyecto

```
agalodo/
├── App.xaml / App.xaml.cs        # Arranque de la aplicación
├── MainWindow.xaml(.cs)          # Ventana principal + menú + WebView2
├── AboutWindow.xaml(.cs)         # Ventana "Acerca de" (info de la empresa)
├── DeepSeekDesktop.csproj        # Proyecto .NET 8 WPF
├── installer.iss                 # Script de Inno Setup
├── Assets/
│   ├── deepseek.ico              # Icono de la aplicación
│   └── logo-estalingrado.jpg     # Logo usado en la ventana "Acerca de"
└── DISTRIBUCION.md               # Guía de despliegue a varios equipos
```

## Notas

- El binario no está firmado con certificado de código: Windows SmartScreen puede
  mostrar una advertencia al instalarlo. Para evitarla habría que firmarlo con un
  certificado de una CA comercial.
- DeepSeek es una empresa y servicio independientes; Estalingrado Corp solo
  distribuye este launcher de acceso.