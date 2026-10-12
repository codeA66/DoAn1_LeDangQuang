# PROMPTS_USED — Hushmole

## Walk — 8 frame

Công cụ: ImageGen tích hợp. Tạo 1 sheet ngang rồi cắt thành 8 PNG.

Ảnh tham chiếu khi tạo: `D:\DoAn1_LeDangQuang\Assets\Art\Generated\Enemies\BuriedDepthsV2\Hushmole\Frames\Idle\Idle_00.png` (mẫu đã duyệt trước khi xuất animation). Bản lưu nguyên mẫu: `C:\Users\DANG QUANG\.codex\visualizations\2026\10\10\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\Approved\Hushmole.png`.

Prompt nguyên văn:

```text
Reference attached approved sprite: preserve EXACT character anatomy and appearance. Detailed handcrafted pixel art, side view facing RIGHT. Near black purple body #0C0A14 to #292734, subtle details, thin lavender rim #D9D4FF top/back only. ONLY warm color red-orange #EA6547 core/eyes with darkred #8F393D edge, soft glow. NO teal, blue fringe, magenta fringe, text, ground, scene, shadow, checkerboard. Transparent alpha. One horizontal sheet, exactly 8 DISTINCT sequential poses in ONE row. Each entire pose MUST fit only 55% of its slot width, leave generous 22.5% clear empty gutters on BOTH SIDES. No overlapping neighbors, no connecting effects. All poses same body size, stable body root at same horizontal point inside each slot. Fixed common foot baseline for grounded poses; intended jump and fall motion allowed. Never randomly rescale. Full character and debris visible. Animation Hushmole Walk LOOP: slow cautious blind round mole walking RIGHT, EARS ALWAYS DROOPED like approved Idle_00, FOREHEAD SLIT DIM, eyes absent. Front digging claws slowly feel and sweep ahead to find footing, alternating short deliberate steps, gentle body rocking. ALL eight poses distinct, fixed foot baseline, no airborne pounce. Frame7 smoothly leads to frame0, nearly same pose. CLEAR difference from Run: no raised ears, no bright slit, no energetic fast stride. Same head/body size and pixel style as approved Idle.
```

Sheet: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-fd7c432b-4a08-4acc-99ae-ec3859135a37.png`

## Kết quả đo trên PNG đã đặt trong project

- 8 frame khác nhau, 512×512, PNG RGBA; biên ngoài alpha = 0.
- Cạnh dưới vùng ảnh: y=504 cho cả 8 frame (chừa 8 px).
- Pixel gần #45DCC4 (sai số mỗi kênh <24): 0.
- Tổng năng lượng đỏ từng frame (sum(max(0,R-max(G,B))*alpha/255)): 112037, 116799, 107978, 108949, 110233, 109386, 114523, 114430.
- Căn bằng một hệ số tỷ lệ chung cho cả sheet; không đổi kích thước riêng từng frame.
- Chưa kiểm tra playback trong Unity.


# Bổ sung animation độn thổ — Burrow / Tunnel / Emerge

Công cụ: ImageGen tích hợp; mỗi animation được vẽ thành một sheet ngang rồi cắt 8 frame. Tham chiếu khi tạo/chỉnh: `Frames/Idle/Idle_00.png` và `Frames/Run/Run_00.png` của chính Hushmole. Không thay đổi hai file tham chiếu.

## Burrow — 8 frame

Sheet cuối: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-32298f63-e04a-41fd-a9fe-dbd3c8ac82ef.png`.

### Prompt tạo sheet

```text
Use case: stylized-concept. Asset: Hushmole Unity 2D pixel-art enemy animation. Image 1 is the EXISTING Idle_00 character reference, image 2 EXISTING Run_00 reference for raised ears/bright slit. Preserve EXACT identity, anatomy, torso size, root-membrane fan ears, digging claws, near-black purple body #0C0A14–#292734, thin pale lavender top/back rim #D9D4FF, only forehead slit warm #EA6547 darkedge #8F393D. Loose earth #454052–#292734 gray-purple. No teal, no brown/yellow earth, no text, no floor, no scenery, no ground shadow, no painted background or checkerboard. Fully transparent alpha. Detailed handcrafted pixel art matching references. Facing RIGHT. Generate ONE HORIZONTAL SHEET with exactly EIGHT DISTINCT chronological frames in ONE ROW, evenly spaced identical slots with generous EMPTY gutters, all full poses and particles isolated from neighbors. Each cell same camera scale and same invisible horizontal GROUND CLIPPING PLANE, fixed body root centered. Ground near bottom of each slot. Everything below that plane must be TRANSPARENT: cut buried portions horizontally, NEVER draw an underground body or vertical soil cross-section. Draw loose dirt mound ONLY where requested, not a solid floor strip. Do not move ground up/down between frames. All original full body proportions unchanged. BURROW 8 frames NONLOOP. Frame0 bends head and extends both foreclaws downward; frame1 claws dig harder with dirt thrown left and right; frame2 head dives nose-first with scooping claws and a bigger dirt spray. Frame3 head is entirely below invisible ground, only back rump/ears visible above it; frame4 back and rump sink further with small earth covering; frame5 only upper back and drooped membrane ear tips peek above low dirt mound; frame6 only a tiny ear tip and small sinking mound; frame7 NEARLY EMPTY, NO creature, NO ear, NO mound, only a FEW isolated falling gray-purple dirt grains just above ground. Progressively SUBMERGE actual body through fixed ground plane, DO NOT shrink the creature. Never show any body below ground. Dim red slit visible only on exposed forehead at beginning.
```

### Prompt chỉnh lần 1

```text
Edit THIS Burrow 8-frame horizontal animation sheet. Keep SAME 8 slots, character identity, palette, dimensions, camera, clipping ground, last five frames unchanged. CHANGE ONLY frames0,1,2: visibly bend the head DOWN and both front digging claws SCOOP DOWNWARD into ground, with loose gray-purple dirt thrown to BOTH sides. Frame0 must already be digging, not an unchanged Idle. Frame1 stronger claw excavation, frame2 nose enters ground with foreclaws pressing down; keep drooped ears and exact body proportions. Preserve frames3-7 gradual submersion ending isolated grains only. All body below invisible fixed ground is transparent. No floor, no underground view, no brown/teal, no labels.
```

### Prompt chỉnh lần 2

```text
Create a corrected Burrow animation from attached Idle and Run CHARACTER REFERENCES. Preserve their exact anatomy and palette. TRANSPARENT horizontal sheet EXACTLY8 sequential poses ONE row. CRITICAL: wide transparent GUTTERS between every cell, all body and ALL dirt confined to central 55% of EACH evenly spaced cell width, no shared dirt between frames, no bridging/overlap. Reduce art scale on the entire sheet uniformly to ensure large empty gutters; will be uniformly enlarged after cutting. Same identical body size, same invisible clipping-ground baseline in all8cells. Body near black purple #0C0A14–#292734, thin lavender #D9D4FF top rim, ONLY warm color dim forehead slit #EA6547 darkedge #8F393D. Graypurple clods #454052–#292734, NO brown/teal, no floor/crosssection/background/shadow/text. RIGHT facing. 0 crouch nose DOWN and both claws scoop downward with a few flying dirt clods on BOTH sides;1 dig harder headlower;2 nose-first through fixed groundplane, ear membranes drooped not raised.3 head/claws hidden BELOW clippingplane TRANSPARENT, only rump/back/earstips above mound;4 less exposed rump above dirt;5 small back/ear tips;6 TINY ear tip atop low sinking mound;7 ONLY THREE OR FOUR isolated dirt grains, NO creature, NO ears, NO mound. Cut off every buried body portion horizontally at fixed invisible ground. Do not shrink character during submersion: progressively hide actual body. Dirt contained locally. Eight clearly distinct frames.
```

### Prompt chỉnh cuối

```text
Edit attached Burrow8frame sheet, keep all8poses/layout/scale/transparent background and frames1-5 and7 unchanged. ONLY change frame0: lower head/nose a little and show BOTH foreclaws angled DOWN in the dirt, excavating, not resting in Idle. ONLY change frame6 (seventh pose): add a TINY drooped root-membrane EAR TIP rising from the small remaining dirt clods, at most one quarter of mound height; no body. Frame7 remains only few grains. Remove any thin horizontal ground/floor line beneath frames; loose separated earth clods only. All buried portions transparent. Preserve palette graypurple earth, lavender creature rim, dim orange forehead, no teal/brown, don't bridge neighbors.
```

## Tunnel — 8 frame

Sheet cuối: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-6f746a6f-ab56-43a8-9331-1a155cc01af6.png`.

### Prompt tạo sheet

```text
Use case: stylized-concept. Asset: Hushmole Unity 2D pixel-art enemy animation. Image 1 is the EXISTING Idle_00 character reference, image 2 EXISTING Run_00 reference for raised ears/bright slit. Preserve EXACT identity, anatomy, torso size, root-membrane fan ears, digging claws, near-black purple body #0C0A14–#292734, thin pale lavender top/back rim #D9D4FF, only forehead slit warm #EA6547 darkedge #8F393D. Loose earth #454052–#292734 gray-purple. No teal, no brown/yellow earth, no text, no floor, no scenery, no ground shadow, no painted background or checkerboard. Fully transparent alpha. Detailed handcrafted pixel art matching references. Facing RIGHT. Generate ONE HORIZONTAL SHEET with exactly EIGHT DISTINCT chronological frames in ONE ROW, evenly spaced identical slots with generous EMPTY gutters, all full poses and particles isolated from neighbors. Each cell same camera scale and same invisible horizontal GROUND CLIPPING PLANE, fixed body root centered. Ground near bottom of each slot. Everything below that plane must be TRANSPARENT: cut buried portions horizontally, NEVER draw an underground body or vertical soil cross-section. Draw loose dirt mound ONLY where requested, not a solid floor strip. Do not move ground up/down between frames. All original full body proportions unchanged. TUNNEL 8 frames LOOP, underground movement to RIGHT rendered IN PLACE. Only ONE low rounded gray-purple loose dirt mound, NO exposed torso, head, face, legs or claws. Mound width about the original Hushmole torso width, height only 0.4 times standing Idle Hushmole body height. Mound remains same scale and same fixed ground plane. Root-membrane EAR TIP briefly peeks out through dirt in frames2,3,5, then disappears; never a full ear or full head. Small dirt grains flick lightly ahead to the RIGHT. Mound gently rumbles alternating up/down and rolls tiny clods. Eight genuinely distinct subtle poses, frame7 very close to frame0 for smooth repeating motion. No red glow because forehead entirely buried. No underground body, no floor.
```

### Prompt chỉnh lần 1

```text
Edit THIS Tunnel 8-frame horizontal sheet. Keep exactly8slots, same mound scale and ground baseline, same dark purple palette, all background transparent. CHANGE mound MATERIAL from fuzzy creature-back to loose GRAY-PURPLE EARTH #454052–#292734, chunky angular clods with small cracks, no furry torso silhouette, no lavender rim around soil (lavender only very thin on exposed ear tips). NO head/face/torso/legs/claws. Replace exposed ears with TINY MEMBRANE EAR TIPS ONLY: ear tips can stick at most 15% of mound height above dirt in frames2,3,5. Otherframes no ears. One low mound about0.4times Idle body height, width one torso. Small dirt grains right, subtle rattling distinct frameposes, frame7 closely returns to0. No teal, brown, red, floor strip, underground body, text.
```

### Prompt chỉnh cuối

```text
Edit attached Tunnel8frame sheet, keep exact8poses, body/ears absent except occasional TINY tips, transparent background, same graypurple EARTH material, same height, sameground and subtleloop. Make every low soil mound WIDER and flatter: width-to-height of the MOUND ITSELF (excluding floating particles and ear tips) should be 3.5:1. Mound has the width of Idle reference torso and height0.4times Idle whole-body height. Do not add floor-strip. Maintain generous empty gaps by uniformly reducing overall artwork on sheet if necessary, no overlapping dirt particles acrossneighbors. Just one low mound percell, a few small grains to right, ear tips below15% of moundheight, no full ears, no torso, no red/teal/brown. Frame7 close0,8distinctframes.
```

## Emerge — 8 frame

Sheet cuối: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-8b49e5d7-2da3-447d-9783-a24d20142fc4.png`.

### Prompt tạo sheet

```text
Use case: stylized-concept. Asset: Hushmole Unity 2D pixel-art enemy animation. Image 1 is the EXISTING Idle_00 character reference, image 2 EXISTING Run_00 reference for raised ears/bright slit. Preserve EXACT identity, anatomy, torso size, root-membrane fan ears, digging claws, near-black purple body #0C0A14–#292734, thin pale lavender top/back rim #D9D4FF, only forehead slit warm #EA6547 darkedge #8F393D. Loose earth #454052–#292734 gray-purple. No teal, no brown/yellow earth, no text, no floor, no scenery, no ground shadow, no painted background or checkerboard. Fully transparent alpha. Detailed handcrafted pixel art matching references. Facing RIGHT. Generate ONE HORIZONTAL SHEET with exactly EIGHT DISTINCT chronological frames in ONE ROW, evenly spaced identical slots with generous EMPTY gutters, all full poses and particles isolated from neighbors. Each cell same camera scale and same invisible horizontal GROUND CLIPPING PLANE, fixed body root centered. Ground near bottom of each slot. Everything below that plane must be TRANSPARENT: cut buried portions horizontally, NEVER draw an underground body or vertical soil cross-section. Draw loose dirt mound ONLY where requested, not a solid floor strip. Do not move ground up/down between frames. All original full body proportions unchanged. EMERGE 8 frames NONLOOP. Frames0,1,2 ONLY an isolated jagged horizontal cracked narrow patch at invisible ground level, a few gray-purple clods trembling/bouncing above, clearly visible red-orange light leaking UPWARD from crack with progressively brighter glow; NO visible body, face, ears or limbs. Crack is a tiny broken clod opening, not a long floor strip. Frame3 burst halfway above ground, ears raised, two large digging claws clawing UPWARD, bright forehead slit, explosive dirt fragments. Frame4 entire unchanged Hushmole body springs UP above the same ground with claws raised and ears fully UP as Run reference, brightest forehead slit, dirt spray. Frame5 body descending, claws lowering, ears lowering. Frame6 land with dirt falling around feet, droop ears and recover hunch. Frame7 same standing hunch/drooped ears/dim slit pose as Idle_00 reference, feet fixed on original ground level, just a few falling dirt grains below knee height. Preserve torso width and size in entire action, never scale up/down. No body drawn below ground clipping plane.
```

### Prompt chỉnh lần 1

```text
Edit THIS Emerge 8-frame horizontal sheet. Keep exact8slots, SAME character anatomy/scale, purple palette, fixed clipping-ground baseline, frames6/7 near Idle reference unchanged. CHANGE frames0-2 red effect to REDORANGE LIGHT GLOW radiating from short visible jagged crack between clods, NOT flames or fire tongues, glow progressively brightens, ONLY crack and bouncing dirt, NO creature. CHANGE frames3/4: both huge foreclaws raised high toward face/above shoulders with their sharp fingers visibly clawing UPWARD; frame3 half-emerged torso cut horizontally at ground, frame4 full unchanged-size torso airborne and ears UP, bright forehead slit; frame5 descends with claws lowering. Keep dirt fragment spray. Below-ground portion transparent, no floor/soil crosssection. No teal, no brown, no text.
```

### Prompt chỉnh lần 2

```text
Edit ONLY frame4 (fifth cell) and frame5 (sixth cell) of attached Emerge sheet, preserve other6frames UNCHANGED, all8positions and ground baseline unchanged. Character reference2 Idle and reference3 Run preserve exact body size. Frame4 currently torso is too upright/elongated, too tall above the ground. Correct it to SAME COMPACT HUNCHED ROUND TORSO and short hindfeet proportions as Run reference, with head angled RIGHT, ears upright at same height-to-body ratio as Run, foreclaws raised up to head level, bright forehead slit. Spring only a SHORT distance above ground (about 0.1 Idle height). Entire silhouette including ears/claws must stay within1.6 times final Idle height above ground; no vertically stretched belly, no erect humanoid standing posture, no shrinking whole character. Frame5 same compact proportions descending from that short jump, claws lowering. Soil fragments must be mostly near ground and below shoulders, not extend above ear tips. Preserve frame3 half emerged, frames6/7land Idle, frames0-2 crack glowingtelegraph. Transparent, no floor/underground body, no teal/brown,8distinctframes, emptygutters.
```

## Đo PNG tại project

- 24/24 frame: 512×512, PNG RGBA; mỗi animation có 8 hình khác nhau.
- Mặt đất đối chiếu Idle_00: y=504 (chừa 8 px); alpha của mọi pixel y≥504 bằng 0.
- Không có pixel gần xanh ngọc #45DCC4 (sai số mỗi kênh <24, alpha>32).
- Tunnel: chênh lệch RGBA premultiplied trung bình khi nối 7→0 là 1.926/255; các bước trong chu kỳ 1.319–2.925/255.
- Emerge 0–2: năng lượng đỏ đo được tăng 1006993 → 1683634 → 3070158. Đây là tổng chỉ số màu của ảnh, không phải intensity trong Unity.
- Kiểm tra thị giác bằng contact sheet và ảnh chồng Idle_00; chưa chạy playback trong Unity.
