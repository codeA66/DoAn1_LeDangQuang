"""Cut generated sheets into anchored frames. No procedural drawing or pose synthesis."""
from pathlib import Path
import json, shutil
import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parent
ASSET = ROOT / 'Assets/Art/Generated/Enemies/Rootwraith/HandDrawn_v2'
SIZE = (320, 288)
BASELINE = 252

def remove_cut_debris(cell):
    """Remove disconnected fragments from neighbouring grid cells, not body details."""
    arr=np.array(cell); mask=arr[:,:,3]>8
    visited=np.zeros(mask.shape,dtype=bool); components=[]
    for y,x in zip(*np.where(mask)):
        if visited[y,x]: continue
        pending=[(int(y),int(x))]; visited[y,x]=True; points=[]
        while pending:
            py,px=pending.pop(); points.append((py,px))
            for ny,nx in ((py-1,px),(py+1,px),(py,px-1),(py,px+1)):
                if 0<=ny<mask.shape[0] and 0<=nx<mask.shape[1] and mask[ny,nx] and not visited[ny,nx]:
                    visited[ny,nx]=True;pending.append((ny,nx))
        components.append(points)
    if not components:return cell
    largest=max(map(len,components))
    for points in components:
        if len(points)<largest*.015:
            yy,xx=zip(*points);arr[yy,xx]=0
    return Image.fromarray(arr)

def cut(name, source, cols, rows):
    im = Image.open(source).convert('RGBA')
    a = np.asarray(im)[:,:,3]
    # Find empty gutter nearest expected grid boundary; never crop to opaque pixels.
    xs=[0]
    for i in range(1, cols):
        expected=round(im.width*i/cols); radius=round(im.width/cols*.07)
        scores=(a[:,expected-radius:expected+radius+1]>24).sum(axis=0)
        xs.append(expected-radius+int(np.argmin(scores)))
    xs.append(im.width)
    ys=[0]
    for i in range(1,rows):
        expected=round(im.height*i/rows); radius=round(im.height/rows*.25)
        scores=(a[expected-radius:expected+radius+1,:]>24).sum(axis=1)
        candidates=np.flatnonzero(scores==scores.min())+expected-radius
        ys.append(int(candidates[np.argmin(abs(candidates-expected))]))
    ys.append(im.height)
    cells=[]
    for r in range(rows):
        for c in range(cols):
            cell=im.crop((xs[c],ys[r],xs[c+1],ys[r+1]))
            arr=np.array(cell)
            # Discard only near-transparent export residue; retain antialiased contours.
            arr[arr[:,:,3]<8]=0
            cell=Image.fromarray(arr)
            if name not in ('Teleport_Out','Death'):cell=remove_cut_debris(cell)
            bb=cell.getchannel('A').point(lambda v:255 if v>24 else 0).getbbox()
            cells.append((cell,bb))
    firstbb=cells[0][1]
    scale=(215 if name=='Idle' else 216)/(firstbb[3]-firstbb[1])
    xscale=scale
    if name not in ('Dive','Volley_Fire'):
        xscale=130/(firstbb[2]-firstbb[0])
    elif name=='Volley_Fire':
        xscale=168/(firstbb[2]-firstbb[0])
    if name=='Dive':
        # Coiled first drawing is much shorter than the extended body; match windup scale.
        scale*=.74; xscale*=.74
    # All frames within a clip share one scale; never enlarge disappearing/death frames.
    maxw=max((b[2]-b[0])*xscale for _,b in cells if b)
    maxh=max((b[3]-b[1])*scale for _,b in cells if b)
    fit=min(1,296/maxw,244/maxh)
    scale*=fit; xscale*=fit
    dest=ASSET/'Frames'/name; dest.mkdir(parents=True,exist_ok=True)
    records=[]; frames=[]
    for i,(cell,bb) in enumerate(cells):
        canvas=Image.new('RGBA',SIZE)
        if bb:
            # Use complete alpha bounds so semitransparent edge pixels are retained.
            full=cell.getbbox(); body=cell.crop(full)
            body=body.resize((max(1,round(body.width*xscale)),max(1,round(body.height*scale))),Image.Resampling.LANCZOS)
            x=round((SIZE[0]-body.width)/2)
            y=BASELINE-body.height
            canvas.alpha_composite(body,(x,y))
        target=dest/f'{name}_{i:02}.png';canvas.save(target)
        bbout=canvas.getchannel('A').point(lambda v:255 if v>24 else 0).getbbox()
        records.append({'file':target.relative_to(ROOT).as_posix(),'bbox_alpha24':bbout,'source_cell_bbox':bb,'source_rect':[xs[i%cols],ys[i//cols],xs[i%cols+1],ys[i//cols+1]],'touches_canvas':bool(bbout and (bbout[0]<=0 or bbout[1]<=0 or bbout[2]>=SIZE[0] or bbout[3]>=SIZE[1]))})
        frames.append(canvas)
    if name=='Teleport_Out':
        # Disappearance must end genuinely empty; appearance reuses exact reversed drawings.
        blank=Image.new('RGBA',SIZE);blank.save(dest/'Teleport_Out_06.png');frames.append(blank)
        indir=ASSET/'Frames/Teleport_In';indir.mkdir(parents=True,exist_ok=True)
        for i,frame in enumerate(reversed(frames)):frame.save(indir/f'Teleport_In_{i:02}.png')
    if name=='Death':
        blank=Image.new('RGBA',SIZE);blank.save(dest/'Death_06.png');frames.append(blank)
    preview=Image.new('RGBA',(SIZE[0]*cols,SIZE[1]*rows),(25,33,28,255))
    for i,frame in enumerate(frames[:cols*rows]):preview.alpha_composite(frame,((i%cols)*SIZE[0],(i//cols)*SIZE[1]))
    preview.convert('RGB').save(OUT/f'{name}_cut_preview.png')
    # GIF is a review aid, clips in Unity use original RGBA frames.
    bg=[]
    for frame in frames:
        p=Image.new('RGBA',SIZE,(25,33,28,255));p.alpha_composite(frame);bg.append(p.convert('RGB'))
    bg[0].save(OUT/f'{name}.gif',save_all=True,append_images=bg[1:],duration=83,loop=0)
    shutil.copy2(source,OUT/'Sources'/f'{name}.png')
    return {'name':name,'scale_y':scale,'scale_x':xscale,'frames':records,'count':len(frames)}

if __name__=='__main__':
    manifest=json.loads((OUT/'sources.json').read_text(encoding='utf-8'))
    (OUT/'Sources').mkdir(exist_ok=True)
    report=[cut(n,s['path'],s['cols'],s['rows']) for n,s in manifest.items()]
    # Shared endpoint drawings prevent a design pop at transitions between sheets.
    idle=ASSET/'Frames/Idle/Idle_00.png'
    for name in ('Hurt','Teleport_Out','Dive_Windup','Volley_Windup','Pillar_Windup','Phase_Shift','Death'):
        shutil.copy2(idle,ASSET/f'Frames/{name}/{name}_00.png')
    for name in ('Hurt','Volley_Fire','Pillar_Slam'):
        shutil.copy2(idle,ASSET/f'Frames/{name}/{name}_05.png')
    shutil.copy2(ASSET/'Frames/Dive_Windup/Dive_Windup_05.png',ASSET/'Frames/Dive/Dive_00.png')
    # Rebuild reverse after canonical endpoint replacement.
    for i,f in enumerate(reversed(sorted((ASSET/'Frames/Teleport_Out').glob('*.png')))):
        shutil.copy2(f,ASSET/f'Frames/Teleport_In/Teleport_In_{i:02}.png')
    (OUT/'frame_report.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
    print(json.dumps([{'clip':r['name'],'count':r['count'],'clipped':sum(f['touches_canvas'] for f in r['frames'])} for r in report]))
