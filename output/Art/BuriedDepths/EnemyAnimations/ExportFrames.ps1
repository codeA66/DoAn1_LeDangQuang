param([string]$ManifestPath, [string]$OutputRoot = 'Assets/Art/Generated/Enemies/BuriedDepths')
Add-Type -AssemblyName System.Drawing
$jobs = Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json
$root = Join-Path (Get-Location) $OutputRoot
$report = @()
foreach ($job in $jobs) {
    $folder = Join-Path $root $job.id
    $sourceFolder = Join-Path $folder 'Source'
    $frameFolder = Join-Path $folder ('Frames/' + $job.state)
    New-Item -ItemType Directory -Force -Path $sourceFolder,$frameFolder | Out-Null
    $sheetPath = Join-Path $sourceFolder ($job.state + '.png')
    Copy-Item -LiteralPath $job.source -Destination $sheetPath
    $sheet = [System.Drawing.Bitmap]::new($sheetPath)
    $hashes = @()
    for ($i=0; $i -lt 12; $i++) {
        $col = $i % 4
        $row = [int][Math]::Floor($i / 4)
        $x0 = [int][Math]::Floor($col * $sheet.Width / 4)
        $x1 = [int][Math]::Floor(($col+1) * $sheet.Width / 4)
        $y0 = [int][Math]::Floor($row * $sheet.Height / 3)
        $y1 = [int][Math]::Floor(($row+1) * $sheet.Height / 3)
        $rect = [System.Drawing.Rectangle]::new($x0,$y0,$x1-$x0,$y1-$y0)
        $frame = [System.Drawing.Bitmap]::new([int][Math]::Ceiling($sheet.Width/4),[int][Math]::Ceiling($sheet.Height/3),[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $g = [System.Drawing.Graphics]::FromImage($frame)
        $g.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
        $g.DrawImage($sheet,[System.Drawing.Rectangle]::new(0,0,$rect.Width,$rect.Height),$rect,[System.Drawing.GraphicsUnit]::Pixel)
        $g.Dispose()
        $path = Join-Path $frameFolder ('{0}_{1:00}.png' -f $job.state,$i)
        $frame.Save($path,[System.Drawing.Imaging.ImageFormat]::Png)
        $hash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
        $hashes += $hash
        $report += [pscustomobject]@{Enemy=$job.id;State=$job.state;Frame=$i;SheetWidth=$sheet.Width;SheetHeight=$sheet.Height;X=$x0;Y=$y0;CropWidth=$rect.Width;CropHeight=$rect.Height;CanvasWidth=$frame.Width;CanvasHeight=$frame.Height;CornerAlpha=$frame.GetPixel(0,0).A;SHA256=$hash;Path=$path}
        $frame.Dispose()
    }
    if (($hashes | Sort-Object -Unique).Count -ne 12) { throw ('Duplicate frames: '+$job.id+'/'+$job.state) }
    $sheet.Dispose()
    Write-Output ($job.id+'/'+$job.state+' : 12 distinct file hashes')
}
$report | Export-Csv -LiteralPath (Join-Path $PSScriptRoot 'FrameMeasurements.csv') -NoTypeInformation -Encoding UTF8
