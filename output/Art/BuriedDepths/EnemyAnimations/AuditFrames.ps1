$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System; using System.Drawing;
public class ArtFrameAudit {
 public static int[] Read(string path) {
  using(var b=new Bitmap(path)) {
   int visible=0,edge=0,minX=b.Width,minY=b.Height,maxX=-1,maxY=-1;
   for(int y=0;y<b.Height;y++) for(int x=0;x<b.Width;x++) {
    if(b.GetPixel(x,y).A>=128) {visible++;minX=Math.Min(x,minX);maxX=Math.Max(x,maxX);minY=Math.Min(y,minY);maxY=Math.Max(y,maxY);if(x==0||y==0||x==b.Width-1||y==b.Height-1) edge++;}
   } return new int[]{visible,edge,minX,minY,maxX,maxY};
  }
 }
}
'@
$rows=Import-Csv 'output/Art/BuriedDepths/EnemyAnimations/FrameMeasurements.csv'
foreach($r in $rows){$s=[ArtFrameAudit]::Read($r.Path);$r|Add-Member VisiblePixels $s[0];$r|Add-Member EdgePixels $s[1];$r|Add-Member MinX $s[2];$r|Add-Member MinY $s[3];$r|Add-Member MaxX $s[4];$r|Add-Member MaxY $s[5]}
$rows|Export-Csv 'output/Art/BuriedDepths/EnemyAnimations/FrameAudit.csv' -NoTypeInformation -Encoding UTF8
Write-Output ('Empty='+@($rows|Where-Object{[int]$_.VisiblePixels -eq 0}).Count+' EdgeContact='+@($rows|Where-Object{[int]$_.EdgePixels -gt 0}).Count)
$rows|Where-Object{[int]$_.EdgePixels -gt 0}|Group-Object Enemy,State|Select-Object Name,Count

