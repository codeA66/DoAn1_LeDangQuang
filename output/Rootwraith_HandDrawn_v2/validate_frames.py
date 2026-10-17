from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import json, hashlib

ROOT=Path(__file__).resolve().parents[2]
OUT=Path(__file__).resolve().parent
ASSET=ROOT/'Assets/Art/Generated/Enemies/Rootwraith/HandDrawn_v2'
NAMES=['Idle','Hurt','Teleport_Out','Teleport_In','Dive_Windup','Dive','Volley_Windup','Volley_Fire','Pillar_Windup','Pillar_Slam','Phase_Shift','Death']
font=ImageFont.truetype(str(ROOT/'Assets/Art/Fonts/beholden-book-font/BeholdenBook-Regular.ttf'),22)
overview=Image.new('RGB',(1280,12*174),(19,26,21))
draw=ImageDraw.Draw(overview)
report=[]
for row,name in enumerate(NAMES):
    files=sorted((ASSET/'Frames'/name).glob('*.png'));frames=[];records=[]
    draw.text((8,row*174+2),name,font=font,fill=(230,235,227))
    strip=Image.new('RGBA',(320*len(files),288),(25,33,28,255))
    for i,file in enumerate(files):
        im=Image.open(file).convert('RGBA'); a=im.getchannel('A')
        bb=a.point(lambda v:255 if v>24 else 0).getbbox()
        assert im.size==(320,288),file
        assert a.getpixel((0,0))==0,file
        assert not bb or (bb[0]>0 and bb[1]>0 and bb[2]<320 and bb[3]<288),file
        records.append({'name':file.name,'bbox_alpha24':bb,'sha256':hashlib.sha256(file.read_bytes()).hexdigest()})
        small=im.resize((160,144),Image.Resampling.NEAREST)
        overview.paste(small,(i*160,row*174+30),small)
        strip.alpha_composite(im,(i*320,0))
        bg=Image.new('RGBA',im.size,(25,33,28,255));bg.alpha_composite(im);frames.append(bg.convert('RGB'))
    frames[0].save(OUT/f'{name}.gif',save_all=True,append_images=frames[1:],duration=83,loop=0)
    strip.convert('RGB').save(OUT/f'{name}_final_strip.png')
    report.append({'name':name,'count':len(files),'frames':records})
assert Image.open(ASSET/'Frames/Teleport_Out/Teleport_Out_06.png').getbbox() is None
assert Image.open(ASSET/'Frames/Death/Death_06.png').getbbox() is None
for i in range(7):
    assert (ASSET/f'Frames/Teleport_Out/Teleport_Out_{i:02}.png').read_bytes()==(ASSET/f'Frames/Teleport_In/Teleport_In_{6-i:02}.png').read_bytes()
overview.save(OUT/'All_Animations_Preview.png')
(OUT/'validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
idle=report[0]['frames'][0]['bbox_alpha24'];print('Clips:',len(report),'Frames:',sum(r['count'] for r in report),'Idle:',(idle[2]-idle[0],idle[3]-idle[1]),'No canvas clipping. Teleport reverse exact. Terminal frames empty.')
