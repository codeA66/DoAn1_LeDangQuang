$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;using System.Drawing;using System.Collections.Generic;using System.Linq;
public class AlphaPose {
 public Bitmap Image;public int Left,Top,Right,Bottom;public int OriginalWidth,OriginalHeight;public bool SourceEdge;
}
public class AlphaAtlas {
 class C {public int Id,Area,L,T,R,B;public double X,Y;}
 public static AlphaPose[] Extract(string path) {using(var b=new Bitmap(path)){
 int w=b.Width,h=b.Height,n=w*h;Color[] pixels=new Color[n];int[] component=new int[n];
 for(int y=0;y<h;y++)for(int x=0;x<w;x++)pixels[y*w+x]=b.GetPixel(x,y);
 var all=new List<C>();var q=new Queue<int>();int next=0;
 for(int i=0;i<n;i++){if(component[i]!=0||pixels[i].A<128)continue;var c=new C{Id=++next,L=w,T=h};component[i]=next;q.Enqueue(i);double sx=0,sy=0;
 while(q.Count>0){int k=q.Dequeue(),x=k%w,y=k/w;c.Area++;sx+=x;sy+=y;c.L=Math.Min(c.L,x);c.R=Math.Max(c.R,x);c.T=Math.Min(c.T,y);c.B=Math.Max(c.B,y);
 for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int nx=x+dx,ny=y+dy;if(nx<0||ny<0||nx>=w||ny>=h)continue;int a=ny*w+nx;if(component[a]==0&&pixels[a].A>=128){component[a]=next;q.Enqueue(a);}}}
 c.X=sx/c.Area;c.Y=sy/c.Area;all.Add(c);}
 var main=all.Where(c=>c.Area>1000).OrderByDescending(c=>c.Area).Take(12).ToArray();if(main.Length!=12)throw new Exception("Expected 12 bodies: "+path);
 var ordered=main.OrderBy(c=>Math.Min(2,(int)(c.Y*3/h))).ThenBy(c=>c.X).ToArray();
 for(int row=0;row<3;row++)if(ordered.Count(c=>Math.Min(2,(int)(c.Y*3/h))==row)!=4)throw new Exception("Row count not four: "+path);
 int[] owners=new int[next+1];for(int i=0;i<12;i++)owners[ordered[i].Id]=i+1;
 foreach(var c in all){if(owners[c.Id]!=0||c.Area<4)continue;double best=double.MaxValue;int who=0;for(int i=0;i<12;i++){var a=ordered[i];double dx=c.X-a.X,dy=c.Y-a.Y;double d=dx*dx+dy*dy;if(d<best){best=d;who=i+1;}}var target=ordered[who-1];double gx=Math.Max(0,Math.Max(target.L-c.R,c.L-target.R)),gy=Math.Max(0,Math.Max(target.T-c.B,c.T-target.B));if(gx*gx+gy*gy<=64*64)owners[c.Id]=who;}
 int[] label=new int[n],distance=new int[n];for(int i=0;i<n;i++){if(component[i]>0&&owners[component[i]]>0){label[i]=owners[component[i]];q.Enqueue(i);}}
 while(q.Count>0){int k=q.Dequeue();if(distance[k]>=3)continue;int x=k%w,y=k/w;
 for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int nx=x+dx,ny=y+dy;if(nx<0||ny<0||nx>=w||ny>=h)continue;int a=ny*w+nx;if(label[a]==0&&component[a]==0&&pixels[a].A>0){label[a]=label[k];distance[a]=distance[k]+1;q.Enqueue(a);}}}
 var poses=new AlphaPose[12];for(int i=0;i<12;i++)poses[i]=new AlphaPose{Left=w,Top=h,Right=-1,Bottom=-1,OriginalWidth=w,OriginalHeight=h};
 for(int k=0;k<n;k++)if(label[k]>0){var p=poses[label[k]-1];int x=k%w,y=k/w;p.Left=Math.Min(p.Left,x);p.Top=Math.Min(p.Top,y);p.Right=Math.Max(p.Right,x);p.Bottom=Math.Max(p.Bottom,y);if(pixels[k].A>=128&&(x==0||y==0||x==w-1||y==h-1))p.SourceEdge=true;}
 for(int i=0;i<12;i++){var p=poses[i];p.Image=new Bitmap(p.Right-p.Left+1,p.Bottom-p.Top+1,System.Drawing.Imaging.PixelFormat.Format32bppArgb);}
 for(int k=0;k<n;k++)if(label[k]>0){var p=poses[label[k]-1];p.Image.SetPixel(k%w-p.Left,k/w-p.Top,pixels[k]);}
 return poses;}}
}
'@
$root='Assets/Art/Generated/Enemies/BuriedDepths'
$jobs=Get-Content 'output/Art/BuriedDepths/EnemyAnimations/ManifestFinal.json' -Raw|ConvertFrom-Json
$jobs|Where-Object{$_.id -eq 'EarthGolem' -and $_.state -eq 'Attack'}|ForEach-Object{$_.source=(Resolve-Path 'output/Art/BuriedDepths/EnemyAnimations/AttackPreview/EarthGolem_Attack_MarginPreview.png').Path}
$jobs|Where-Object{$_.id -eq 'Beetle' -and $_.state -eq 'Idle'}|ForEach-Object{$_.source=(Resolve-Path 'output/Art/BuriedDepths/EnemyAnimations/AttackPreview/Beetle_Idle_MarginPreview.png').Path}
$records=@()
foreach($j in $jobs){$poses=[AlphaAtlas]::Extract($j.source);for($i=0;$i -lt 12;$i++){$records+=[pscustomobject]@{Job=$j;Frame=$i;Pose=$poses[$i]}};Write-Output ('Measured '+$j.id+'/'+$j.state)}
$report=@()
foreach($group in ($records|Group-Object {$_.Job.id})){
 $cw=([int](($group.Group|ForEach-Object{$_.Pose.Image.Width}|Measure-Object -Maximum).Maximum))+16
 $ch=([int](($group.Group|ForEach-Object{$_.Pose.Image.Height}|Measure-Object -Maximum).Maximum))+16
 foreach($r in $group.Group){$j=$r.Job;$p=$r.Pose;$folder=Join-Path $root ($j.id+'/Frames/'+$j.state);New-Item -ItemType Directory -Force -Path $folder|Out-Null
 $canvas=New-Object System.Drawing.Bitmap($cw,$ch,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $g=[System.Drawing.Graphics]::FromImage($canvas);$g.CompositingMode=[System.Drawing.Drawing2D.CompositingMode]::SourceCopy
 $ox=[int][Math]::Floor(($cw-$p.Image.Width)/2);$oy=$ch-8-$p.Image.Height;$g.DrawImageUnscaled($p.Image,$ox,$oy);$g.Dispose()
 $path=Join-Path $folder ('{0}_{1:00}.png' -f $j.state,$r.Frame);$canvas.Save($path,[System.Drawing.Imaging.ImageFormat]::Png)
 $report+=[pscustomobject]@{Enemy=$j.id;State=$j.state;Frame=$r.Frame;SheetWidth=$p.OriginalWidth;SheetHeight=$p.OriginalHeight;X=$p.Left;Y=$p.Top;CropWidth=$p.Image.Width;CropHeight=$p.Image.Height;CanvasWidth=$cw;CanvasHeight=$ch;OffsetX=$ox;OffsetY=$oy;SourceEdge=$p.SourceEdge;CornerAlpha=$canvas.GetPixel(0,0).A;SHA256=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash;Path=(Resolve-Path $path).Path}
 $canvas.Dispose();$p.Image.Dispose()}
 Write-Output ('Exported '+$group.Name+' canvas '+$cw+'x'+$ch)
}
foreach($j in $jobs){Copy-Item -LiteralPath $j.source -Destination (Join-Path $root ($j.id+'/Source/'+$j.state+'.png'))}
$report|Export-Csv 'output/Art/BuriedDepths/EnemyAnimations/FrameMeasurements.csv' -NoTypeInformation -Encoding UTF8
$report|Group-Object Enemy,State|ForEach-Object{if(@($_.Group.SHA256|Sort-Object -Unique).Count -ne 12){throw ('Duplicate frames '+$_.Name)}}
Write-Output ('TOTAL '+$report.Count+' frames; unique hashes within each state; source-edge frames='+@($report|Where-Object SourceEdge -eq $true).Count)
