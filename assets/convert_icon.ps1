param (
    [string]$InputPath,
    [string]$OutputPath
)

Add-Type -AssemblyName System.Drawing

# Define a small helper for DestroyIcon since it's a PInvoke
$code = @"
using System;
using System.Runtime.InteropServices;

namespace LinksAndMore {
    public static class IconHelper {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool DestroyIcon(IntPtr handle);
    }
}
"@
Add-Type -TypeDefinition $code -ErrorAction SilentlyContinue

$sourceImg = [System.Drawing.Image]::FromFile($InputPath)
$bitmap = New-Object System.Drawing.Bitmap($sourceImg)
$hIcon = $bitmap.GetHicon()
$icon = [System.Drawing.Icon]::FromHandle($hIcon)

$fs = [System.IO.File]::Create($OutputPath)
try {
    $icon.Save($fs)
}
finally {
    $fs.Close()
    $icon.Dispose()
    [void][LinksAndMore.IconHelper]::DestroyIcon($hIcon)
    $bitmap.Dispose()
    $sourceImg.Dispose()
}
param (
    [string]$InputPath,
    [string]$OutputPath
)

Add-Type -AssemblyName System.Drawing

$sourceImg = [System.Drawing.Image]::FromFile($InputPath)
$ms = New-Object System.IO.MemoryStream

# ICO Header
# Reserved (2 bytes), Type (2 bytes, 1 for icon), Count (2 bytes)
$ms.Write([byte[]]@(0, 0, 1, 0, 1, 0), 0, 6)

# Icon Entry
# Width (1 byte), Height (1 byte), Colors (1 byte), Reserved (1 byte), Planes (2 bytes), BitCount (2 bytes), Size (4 bytes), Offset (4 bytes)
$width = if ($sourceImg.Width -ge 256) { 0 } else { [byte]$sourceImg.Width }
$height = if ($sourceImg.Height -ge 256) { 0 } else { [byte]$sourceImg.Height }

$pngStream = New-Object System.IO.MemoryStream
$sourceImg.Save($pngStream, [System.Drawing.Imaging.ImageFormat]::Png)
$pngBytes = $pngStream.ToArray()

# Width, Height, Colors (0), Reserved (0)
$ms.Write([byte[]]@($width, $height, 0, 0), 0, 4)
# Planes (1), BitCount (32 - standard for PNG-in-ICO)
$ms.Write([byte[]]@(1, 0, 32, 0), 0, 4)
# Size of PNG data
$size = $pngBytes.Length
$ms.Write([BitConverter]::GetBytes($size), 0, 4)
# Offset (header 6 bytes + 1 entry 16 bytes = 22)
$ms.Write([BitConverter]::GetBytes(22), 0, 4)

# Data
$ms.Write($pngBytes, 0, $pngBytes.Length)

[System.IO.File]::WriteAllBytes($OutputPath, $ms.ToArray())

$sourceImg.Dispose()
$ms.Dispose()
$pngStream.Dispose()
