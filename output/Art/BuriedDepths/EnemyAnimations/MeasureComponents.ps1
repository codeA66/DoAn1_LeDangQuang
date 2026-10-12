$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;using System.Drawing;using System.Collections.Generic;
public class Components {
public static string[] Measure(string p){using(var b=new Bitmap(p)){
int w=b.Width,h=b.Height;bool[] mask=new bool[w*h];for(int y=0;y<h;y++)for(int x=0;x<w;x++)mask[y*w+x]=b.GetPixel(x,y).A>=128;
var result=new List<string>();var q=new Queue<int>();
for(int i=0;i<mask.Length;i++){if(!mask[i])continue;mask[i]=false;q.Enqueue(i);int area=0,l=w,t=h,r=0,bot=0;
while(q.Count>0){int k=q.Dequeue(),x=k%w,y=k/w;area++;l=Math.Min(l,x);r=Math.Max(r,x);t=Math.Min(t,y);bot=Math.Max(bot,y);for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int nx=x+dx,ny=y+dy;if(nx<0||ny<0||nx>=w||ny>=h)continue;int a=ny*w+nx;if(mask[a]){mask[a]=false;q.Enqueue(a);}}}
if(area>1000)result.Add(area+","+l+","+t+","+r+","+bot);}
return result.ToArray();}}
}
'@
$jobs=Get-Content 'output/Art/BuriedDepths/EnemyAnimations/ManifestFinal.json' -Raw|ConvertFrom-Json
$rows=@()
foreach($j in $jobs){$cs=[Components]::Measure($j.source);Write-Output ($j.id+'/'+$j.state+' components='+$cs.Count);foreach($c in $cs){$v=$c.Split(',');$rows+=[pscustomobject]@{Enemy=$j.id;State=$j.state;Area=[int]$v[0];Left=[int]$v[1];Top=[int]$v[2];Right=[int]$v[3];Bottom=[int]$v[4]}}}
$rows|Export-Csv 'output/Art/BuriedDepths/EnemyAnimations/Components.csv' -NoTypeInformation -Encoding UTF8
