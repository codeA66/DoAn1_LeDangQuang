# PROMPTS_USED — Player New Skills

Tất cả strip dưới đây được tạo sau khi `Preview_NewSkills.png` được chủ project duyệt. Mỗi strip dùng ảnh tham chiếu nêu ở đầu nhóm; frame mẫu được giữ làm mốc và không thay thế bằng ảnh sinh mới. Các file cuối được cắt theo từng vật thể có alpha, căn vào khung mục 2 của README và lưu PNG.

## Frame mẫu tham chiếu đã duyệt

- `Frames/WallSlide/WallSlide_00.png` — mẫu bám tường; tham chiếu thêm `TravelerPreview/traveler_sheet.png` và `TravelerActions/actions_sheet.png`.
- `Frames/Cast/Cast_04.png` — mẫu Cast, người chơi quay phải, tay phát sáng, không có Orb; tham chiếu hai sheet nhân vật.
- `Projectile/Orb/Orb_00.png` và `Projectile/OrbShadow/OrbShadow_00.png` — mẫu đạn thường và Shadow, đạn bay phải; tham chiếu strip hai loại đạn.
- `Checkpoint/BenchLit/BenchLit_00.png` — mẫu ghế kích hoạt, tông rêu đá ẩm; tham chiếu sheet ghế.
- `Frames/Sit/Sit_00.png` — mẫu nhân vật ngồi, mông ở mặt ghế; tham chiếu sheet nhân vật và ghế.

Các frame mẫu này đã được tạo trong bước duyệt trước đó và được giữ nguyên làm frame gốc (ngoại trừ đổi một vài pixel màu lạc sang màu bảng cho phép). Phần dưới ghi nguyên prompt của các strip animation được tạo trong lượt hoàn thiện này.

## WallSlide — 8 frame

Tham chiếu: `WallSlide_00.png`, `traveler_sheet.png`, `actions_sheet.png`.

> Create ONE clean horizontal pixel-art sprite strip for the same exact Traveler character and WallSlide animation reference. Strip contains exactly 8 equal adjacent square cells in one row, no gutters, no borders, no labels, no extra sprites, transparent background. Each cell is a 256x256 frame (overall strip 2048x256). Use the provided approved WallSlide_00 as the exact character design, palette, scale, pixel density, and pose anchor reference; use traveler source sheets only to preserve identity. Traveler faces LEFT, pressed against an invisible wall at the RIGHT edge of each cell (one hand and one boot touch x=252, about 4 px inset); do not draw any wall. Hood, face, cloak, colors and hard pixel edges match the reference exactly. Keep the alpha silhouette vertically in the same 148px height / same center and baseline as WallSlide_00. Make eight genuinely distinct consecutive slow downward slide poses: subtle 1-2 pixel descent and alternating slight elbow/knee/cloak movement, cloak lifted left/up, tiny contact dust at the hand. End frame loops smoothly to frame 0. No antialiasing, no blur, no floor, no shadows beyond the existing pixel style. Preserve the character's original turquoise eye only. Do not alter the design.

## WallJump — 6 frame

Tham chiếu: `WallSlide_00.png`, `traveler_sheet.png`, `actions_sheet.png`.

> Create one horizontal sprite sheet strip containing exactly 6 equally spaced side-view pixel art frames, in one single row, transparent background, no labels or borders. It is a non-looping WallJump for the exact same Traveler, matching the supplied WallSlide_00 character identity, cloak/hood silhouette, face, pixel outline, palette and scale. Traveler is facing LEFT and a wall is on the RIGHT edge, but do not draw a wall. Sequence: frames 0-1 compress against the wall with one boot braced, hands close, body crouched; frames 2-3 push off and leap diagonally up-left, cloak flares to the right; frames 4-5 stretch into the airborne pose matching the supplied game's jump_00 pose. Clearly different poses and readable action. Keep character about 148px tall at final 256x256 frame scale, anchor centered horizontally and at same ground baseline as other player frames, with only the actual leap rising naturally. Transparent alpha only, sharp pixel edges, no blur, no floor or cast shadow. Keep eye turquoise exactly like reference; preserve dark violet traveler design.

## Cast — 8 frame

Tham chiếu: `Cast_04.png`, `traveler_sheet.png`, `actions_sheet.png`.

> Create one horizontal transparent pixel-art sprite sheet with exactly 8 distinct side-view frames in one row for the same Traveler, facing RIGHT. Match the supplied approved Cast_04 exactly for identity, silhouette, colors, pixel outline and 148px-tall scale. No labels, no cell borders, no extra characters, no environment. Sequence Cast standing: frames 0-2 pull hand toward chest and palm glow ramps up in turquoise; 3-4 thrust hand forward, glow bursts at fingertips; 5-7 retract arm, cloak gives a small recoil, return to the supplied game's idle_00 pose. All eight poses genuinely differ, readable silhouette, anchored to same baseline with feet 8px above bottom in a 256x256 frame. Keep turquoise eye. Hand emission is #45DCC4/#B8FFF0 only. Sharp hard pixel edges, no blur or floor shadow.

## CastAir — 8 frame

Tham chiếu: `Cast_04.png`, `traveler_sheet.png`, `actions_sheet.png`.

> Create one horizontal transparent pixel-art sprite sheet with exactly 8 distinct side-view frames in one row for the same Traveler, facing RIGHT. Match approved Cast_04 and the supplied Traveler sheets exactly for identity, silhouette, palette and hard pixel outline. No labels, no borders, no background or extra sprites. CastAir while airborne: knees stay bent and feet tucked, cloak lifted as if falling/jumping; 0-2 draw arm back and palm light grows turquoise, 3-4 thrust forward with a compact turquoise burst at fingertips, 5-7 retract and settle into the game's fall_00 airborne pose. All frames distinct, same character scale and centered anchor within each 256x256 frame; character alpha height about 148px. Turquoise #45DCC4/#B8FFF0 only for the casting light, eye as in reference. No orange/red, no blur, crisp pixel art.

## SitDown — 6 frame

Tham chiếu: `Sit_00.png`, `BenchLit_00.png`, `traveler_sheet.png`, `actions_sheet.png`.

> Create one horizontal pixel-art strip with exactly 6 distinct frames in a single row, transparent background, no labels, no borders, no background. Same exact Traveler from Sit_00 and source sheets, facing RIGHT, same colors, proportions, pixels and outline. Same bench baseline and seat height as provided BenchLit_00; the bench is only a reference and must not appear in the character cells. Non-looping SitDown transition: frame 0 matches standing idle_00; frames 1-2 bend knees and lower hips; frames 3-4 settle onto the 56px-from-bottom seat level with cloak falling; frame 5 matches the supplied Sit_00 pose, seated with hands on thighs. Keep same scale as the approved character (~148px visible height when standing), aligned so seated rump lands at same seat height as Sit_00; final 256x256 frame. Crisp hard pixels, turquoise eye only, no blur or floor.

## Sit — 8 frame

Tham chiếu: `Sit_00.png`, `BenchLit_00.png`, `traveler_sheet.png`, `actions_sheet.png`.

> Create one horizontal pixel-art strip with exactly 8 distinct seated idle frames in one row, transparent background, no labels, no borders, no background or bench. Match the exact approved Sit_00 Traveler, facing RIGHT, same pose, proportions, outline, palette, and 256x256 scale. The traveler is seated at a fixed seat baseline (rump stays at same 56px-from-bottom seat height), hands resting on thighs. A slow breathing loop: shoulder/chest shifts only 1-2 pixels, cloak hem gently shifts a few pixels, slight head movement, return smoothly from frame 7 to frame 0. Eight frames visibly differ subtly but retain the same seated silhouette. Preserve eye turquoise. Sharp pixel edges, no blur or shadow.

## StandUp — 6 frame

Tham chiếu: `Sit_00.png`, `BenchLit_00.png`, `traveler_sheet.png`, `actions_sheet.png`.

> Create one horizontal pixel-art strip containing exactly 6 distinct frames in one row, transparent background, no labels, no borders, no background. Same exact Traveler identity from supplied references, facing RIGHT. BenchLit_00 is a positioning reference only and must not appear in frames. Non-looping StandUp transition: frame 0 matches approved Sit_00 seated pose at the correct seat baseline; frames 1-2 lift hips and rise, cloak trails naturally; frames 3-4 straighten and settle; frame 5 returns to the game's idle_00 standing pose with feet 8px above bottom. Keep scale, colors, line weight and pixels matching source art, about 148px standing height, align across all cells. Crisp pixel art and transparent alpha.

## Orb — 8 frame

Tham chiếu: `Orb_00.png`.

> Make one horizontal pixel-art sprite strip of exactly 8 distinct equal frames in one row, transparent background, no labels or borders. Match the supplied Orb_00 exactly: turquoise player energy orb, white-mint core #B8FFF0, body #45DCC4, dark teal outline #1F6F66, approximately 40px core and 40px trailing tail. Projectile travels toward RIGHT so tail always trails LEFT. Subtle seamless loop: core brightness pulses slightly, tail ripples and changes length by a few pixels, tiny orbiting sparks shift around it; final frame connects smoothly to first. Center the orb assembly in each 128x128 frame. Hard crisp pixel edges, no blur, no red/orange/yellow, no extra objects.

## OrbShadow — 8 frame

Tham chiếu: `OrbShadow_00.png`.

> Make one horizontal pixel-art sprite strip of exactly 8 distinct equal frames in one row, transparent background, no labels or borders. Match the supplied OrbShadow_00 exactly: shadow player orb core #B8FFF0/#45DCC4 around 80px diameter, dark purple smoke #594580/#2A1F40 curling around it, dark outline #0C0A14, tail about 80px trailing LEFT as projectile travels RIGHT. Fit complete effect within each 128x128 frame. Seamless loop with gentle smoke curl and core pulse; frame 7 connects smoothly to 0. Sharp pixel art, no blur, no red/orange/yellow.

## OrbHit — 8 frame

Tham chiếu: `Orb_00.png`.

> Create one horizontal transparent pixel-art strip of exactly 8 distinct frames in one row for OrbHit, matching supplied Orb_00 palette and design. 128x128 cell size, effect centered. Projectile hits a target on the RIGHT: frames 0-2 compress the turquoise orb and flash brighter, 3-5 break it into a turquoise ring and small pixel shards radiating outward, 6-7 fade to only a few tiny mint particles. Keep all colors within #B8FFF0, #45DCC4, #1F6F66 and transparent. No red/orange/yellow, no target or background, crisp pixels, visibly distinct progression.

## OrbShadowHit — 8 frame

Tham chiếu: `OrbShadow_00.png`.

> Create one horizontal transparent pixel-art strip of exactly 8 distinct frames in one row for OrbShadowHit, matching supplied OrbShadow_00. 128x128 cells with the whole larger shadow effect centered and contained. Projectile impacts to the RIGHT: 0-2 large core compresses and flashes mint, 3-5 bursts into a ring, fragments and curling purple-black smoke, 6-7 dissipates to a few mint particles and wisps. Palette only #B8FFF0/#45DCC4 and dark shadow #594580/#2A1F40/#0C0A14; no red/orange/yellow. Crisp hard pixel edges, transparent background, no target.

## BenchIdle — 8 frame

Tham chiếu: `BenchLit_00.png`.

> Create one horizontal transparent pixel-art animation strip with exactly 8 distinct equal frames in one row for BenchIdle. Match the supplied approved BenchLit_00 checkpoint bench exactly: same front-facing damp mossy stone bench, proportions, roots, lantern column, hard pixel edges and position; no other objects or background. Canvas intent per cell is 256x192, bench footprint about 200x160, bottom at y=184 (8px bottom margin), seat height about 56px from bottom, lamp about 160px high. This is inactive idle: turquoise crystal is dim and blinks very slowly, a few tiny mint particles rise; only small light changes, bench structure remains identical. Seamless loop with frame 7 returning smoothly to 0. Colors: stone #3E4A40/#22302A, edge #A9C2A8, moss #4E5A50, roots #1A1626, crystal #45DCC4/#B8FFF0 only. No orange/yellow/red, no blur, no labels or borders.

## BenchActive — 8 frame

Tham chiếu: `BenchLit_00.png`.

> Create one horizontal transparent pixel-art animation strip with exactly 8 distinct equal frames in one row for BenchActive. Match the supplied approved BenchLit_00 exactly in bench design, proportions, front view, roots, moss, palette and placement; only light/particles change. Cell intent 256x192, bench width about 200, bottom y=184, seat top 56px above bottom, lantern column about 160px tall. Activation: frames 0-3 turquoise crystal ramps from dim to brilliant and a thin mint ring spreads from the bench; frames 4-7 a few particles rise and the crystal eases to steady active brightness. Colors only stone #3E4A40/#22302A, edge #A9C2A8, moss #4E5A50, roots #1A1626 and crystal #45DCC4/#B8FFF0. Transparent background, sharp pixel edges, no orange/yellow/red, no text, no borders.

## BenchLit — 8 frame

Tham chiếu: `BenchLit_00.png`.

> Create one horizontal transparent pixel-art animation strip with exactly 8 distinct equal frames in one row for BenchLit. Match the supplied approved BenchLit_00 exactly: same front-facing damp mossy stone checkpoint bench, roots, lantern column, scale, shape, pixels and placement; no background and no other object. Per frame intent 256x192; bench about 200x160, bottom at y=184 (8px margin), seat at 56px from bottom, lamp column about 160px. The crystal is steadily activated and bright, with only gentle small brightness fluctuation and a few evenly rising mint particles. Make a calm seamless loop, frame 7 joins frame 0. Palette: stone #3E4A40/#22302A, edge #A9C2A8, moss #4E5A50, roots #1A1626, crystal #45DCC4/#B8FFF0 only; no orange, yellow or red. Hard crisp pixel art, no blur, no labels or border.

### Các pose nối đầu/cuối lấy trực tiếp từ ảnh tham chiếu

- `SitDown_00`, `StandUp_05`, `Cast_07`: dùng đúng crop `idle_00` từ `traveler_sheet.png` (rect đo trong `.meta`: x=26, y=902, 132×148 px; `.meta` chỉ được đọc).
- `SitDown_05`, `StandUp_00`: dùng frame `Sit_00` đã duyệt.
- `CastAir_07`: dùng crop `fall_00` từ `traveler_sheet.png` (x=41, y=173, 128×149 px).
- `WallJump_05`: dùng pose `jump_00` từ sheet tham chiếu, lật ngang để giữ hướng sang trái theo quy ước bám tường.
- Crop các frame tham chiếu được căn giữa và đặt đáy ở hàng 248 trong 256×256; ảnh tham chiếu gốc không bị sửa.

## Ghi chú đo sau khi cắt

- `idle_00` đo từ sheet tham chiếu là 132×148 px; các pose đứng cuối/đầu được đặt theo chiều cao thật 148 px này. README ghi chiều cao xấp xỉ 136 px, nên số đo nguồn được ưu tiên để giữ cùng tỉ lệ với nhân vật có sẵn.
- Foot baseline của frame nhân vật nằm ở hàng 248/256 (lề dưới 8 px). `WallSlide` chạm x=252 (lùi 4 px từ mép phải).
- Ghế có footprint 200×160 trong frame 256×192, đáy alpha đến hàng 183 (lề dưới 8 px). Mặt ghế đo theo hàng bề mặt: BenchIdle y=136 (56 px từ đáy); BenchActive y=138–139 (53–54 px); BenchLit y=137–138 (54–55 px).
- Pixel màu lạc đỏ/xanh lá ở một vài frame mẫu được đổi về màu thân/turquoise; alpha glow/bờ pixel đã có ở mẫu duyệt được giữ. Tất cả frame có bốn góc alpha=0 và PNG RGBA.
- Không mở Unity hoặc phát animation trong runtime; vòng lặp được so trực quan trên contact sheet và file frame.


## 2026-10-13 — FX preview, README section 10 (awaiting approval)

Tool: built-in image_gen, transparent_background=true. References for all three calls: Frames/Cast/Cast_04.png and Projectile/Orb/Orb_00.png. No full animation generated.

### CastBurst peak sample

```text
Use case: stylized-concept. Asset type: one native-resolution pixel art game VFX sprite, CastBurst sample peak frame (frame 2 of eventual 6, generate ONLY this one sample). Reference image 1 is the game's existing Cast_04 character: match its crisp fine pixel density and 1–2 native pixel edge thickness; reference image 2 Orb_00 is the energy projectile, use its turquoise visual identity. Do not draw either reference subject. Draw only a powerful jagged CASTING BURST exploding toward RIGHT from the canvas midpoint. Logical canvas 256 by 256 with transparent background, origin at (128,128): several long pointed torn rays fan into a cone toward the right, bright small central white flash at origin, a few short backward recoil shards to left; strong forceful Hollow Knight Vengeful Spirit-like impact feel translated into game's turquoise identity. Not a circular star icon: asymmetrical directional spear shards, torn featherlike energy, sharp negative-space cuts. Keep all rays inside frame with transparent margin; no text, no character, no orb, no floor. Strict flat four-color palette ONLY #FFFFFF #B8FFF0 #45DCC4 #1F6F66, no other colors. White core, pale mint interior, turquoise outer jagged surfaces, dark teal thin selective edge. Hard stair-step PIXEL ART at logical 256x256 resolution, tiny pixel clusters like character reference, avoid chunky 8-bit blocks. Absolutely no smooth gradients, no fuzzy glow, no blur, no antialias, no partial-opacity edges or shadows. Actual alpha transparency.
```

### CastWave peak sample

```text
Use case: stylized-concept. Asset type: one native-resolution pixel art game VFX sprite, CastWave sample peak frame (frame 2 of eventual 6, generate ONLY this one sample). Reference image 1 Cast_04 character: match its fine crisp pixel density and 1–2 native-pixel line thickness; reference image 2 Orb_00: turquoise energy identity. Do not draw character or orb. Logical canvas 128 wide by 256 high, fully transparent. Draw ONLY a powerful VERTICAL energy wave centered at (64,128), tall standing light slash, thick bright core with ripped featherlike jagged edges and uneven projecting sharp spines, a few separated angular splinters. The motion pushes to RIGHT. Wave reads as vertical torn energy blade, not closed circle, not symmetric generic star. Leave transparent margins on all sides, approx 210px tall inside native frame. Forceful Hollow Knight Vengeful Spirit-like feel expressed in turquoise pixel art. Strict flat palette ONLY #FFFFFF #B8FFF0 #45DCC4 #1F6F66, no other colors. White brightest core, mint blade, turquoise torn edges, thin selective dark teal edge. Native pixel stair-steps and fine small clusters matching reference's density, not large chunky 8-bit blocks. No text, character, floor, no gradient, no glow blur, no antialias, no soft shadows, no partial opacity. Actual alpha transparent background.
```

### OrbImpactBig peak sample

```text
Use case: stylized-concept. Asset type: one native-resolution pixel art game VFX sprite, OrbImpactBig sample peak frame (frame 3 of eventual 8, generate ONLY this one sample). Reference image 1 Cast_04 is character pixel density/style reference, reference image 2 Orb_00 is projectile energy identity. Do not draw either reference subject. Logical canvas 256 by 256, midpoint (128,128) is impact origin, fully transparent backdrop. Draw ONLY explosive projectile impact: bright fragmented white flash, a broken angular expanding ring with long uneven jagged spikes and torn edges, scattered pointed fragments flung outward, violent rightward impact momentum. Forceful readable Hollow Knight Vengeful Spirit-like explosion in game's turquoise identity. Keep center somewhat open between bright fragments, broken ring irregular and asymmetrical; NOT perfectly round smooth circle, not geometric regular sun icon. Native footprint within 232x232 and transparent margin. Strict flat palette ONLY #FFFFFF #B8FFF0 #45DCC4 #1F6F66, no other colors. White flashes, pale mint ring interior, turquoise jagged torn outer rays, thin selective dark teal contour. Sharp native pixel stair-step art, fine 1–2pixel edges at logical resolution matching character, no chunky oversized pixels. No text, no orb sprite, no character, no floor, no gradients, no soft glows, no blur, no antialias, no partial transparency. True alpha background.
```

Preparation: nearest-neighbour scaling to the specified native frame, hard alpha threshold 128 to alpha 0/255, nearest-color mapping to the four allowed FX colors. CastBurst origin is the densest 65x65 white-pixel window in the generated source; remaining FX centered within native frames. No smooth resampling, gradient or glow added. The preview uses unchanged Cast_04 and Orb_00 sources, shown with every frame at the same 2x nearest-neighbour display scale; both references have PPU 80.

Measured sample files: CastBurst 256x256; CastWave 128x256; OrbImpactBig 256x256; all PNG RGBA, alpha values only 0 and 255, visible RGB values exactly #FFFFFF, #B8FFF0, #45DCC4, #1F6F66. Preview_Fx.png is 2336x704. Color/alpha checks apply to the three new FX samples; the two existing reference sprites are included unchanged. Native sample files remain in the task scratch directory until preview approval; only Preview_Fx.png is installed in the project at this stage.


## 2026-10-13 — Full pixel FX after preview approval, including OrbBig additions

Authorizations: FX preview approved with "okay"; OrbBig/OrbBigShadow additions approved with "duyệt" during generation. Tool: built-in image_gen, true transparent requests. Each animation was generated as one horizontal sheet, then cut; HealShard is four different static shapes.

### OrbBig preview

```text
Use case: stylized-concept. Asset type: ONLY ONE preview sprite of OrbBig, NOT animation. Reference1 Cast_04 is the game's precise native1â€“2pixel line thickness and density; reference2 old Orb_00 establishes turquoise energy identity but is TOO SMALL, draw a substantially bigger NEW energy projectile for scale1 PPU80. Logical frame192x128 transparent; main projectile CORE BODY center exactly(120,64), main energy body height110nativepixels, about3/4 of actual148px character. Head faces RIGHT, broadly rounded but slightly flattened at right tip, silhouette sharply torn flame spikes, NOT a perfect geometric circle. White central broken energy blaze, flat pale mint then turquoise bands; thin selective dark teal edge. Short sharp torn flame tail trails LEFT, entirely inside frame, no long trail offscreen. ALL body/tail inside192x128, enough transparent margin. Native sharp1â€“2pixel step edges and fine clusters like character, no giant chunky pixels, strong forceful VengefulSpirit-like energy. STRICT ONLY four colors #FFFFFF #B8FFF0 #45DCC4 #1F6F66, flat solid fills, true transparent alpha0/255. No gradient, blurry glow, antialias, soft shadow, warmcolors, text, character, floor, frames or scene. Generate just one sprite at its intended native pixel detail, upscaled display allowed but structure stays native192x128.
```

### OrbBig preview correction

```text
Use case: precise-object-edit. Correct this OrbBig draft (reference1 edit target), character Cast_04 reference2 only gives sharp pixel density. KEEP large bright rounded-but-flattened RIGHT-facing head with jagged flame perimeter and white/mint/turquoise identity. REMOVE absolutely ALL blurred outer glow and halo, replace backdrop around sprite with truly alpha0 transparent pixels. Shorten trailing left tail to HALF current length; keep energetic sharp torn tongues. Re-render at fine native pixel density intended for192x128frame, core head height110px with center(120,64), body widthabout110px, short tail reachingx8to65. Everything fits the frame with transparent margin. Pixel edges one native pixel thick, no giantpixel art. STRICT flat solid four-color fills #FFFFFF #B8FFF0 #45DCC4 #1F6F66 ONLY. Sharp stepped perimeter, flat white core shapes not gradient; NO antialiasing, no smooth glow or blur anywhere, alpha ONLY0 or255. Tail must be SHORT. No text, frame, character, added particles outsideframe, red/orange/yellow. Single preview sprite, no animation.
```

### CastBurst

Source sheet: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-bd96cb38-be1d-47b2-a230-49b29c516a31.png`

```text
Use case: stylized-concept. Production Unity pixel VFX horizontal animation sprite sheet CastBurst. Reference1 is approved CastBurst peak sprite, preserve its sharp shapes, turquoise identity and fine native1â€“2 pixel edge thickness. Reference2 Cast_04 character is pixel-density guide; reference3 Orb_00 energy identity. Draw only VFX. EXACTLY 6 consecutive different frames in ONE HORIZONTAL ROW from left to right, NO second row, NO text or grid borders, NO character. Logical canvas 1536x256, each cell 256x256; evenly spaced fixed cell centers; whole canvas may be upscaled but native pixels stay consistent. ALL frames fit entirely inside their own cell, no touching neighbors, large transparent gutters. Use the SAME scale and anchor for all frames: do NOT enlarge the first/last small frames to fill cells. Frames 0â€“1: small white blink from center; frame 1 stronger. Frames 2â€“3: long jagged rays explode in a cone toward RIGHT like approved reference, frame 3 splits spear tips further. Frames 4â€“5: rays break apart into flying angular splinters, sparse last fragments. No circle. Anchor casting origin at center (128,128) in every cell. Strong fast attack then slow dissolution, genuine redraw per frame not repeated scaled stamps. Match approved reference at peak. ONLY flat palette #FFFFFF #B8FFF0 #45DCC4 #1F6F66. True transparent background, alpha only0 or255, hard stair-step pixels, no smooth gradients, no blurred glows, no antialias, no soft shadow, no warm colors.
```

### CastWave

Source sheet: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-491ada7d-a28c-4427-b295-08d91202915e.png`

```text
Use case: stylized-concept. Unity pixel VFX horizontal sprite sheet CastWave. Reference1 is APPROVED CastWave peak: preserve fine native1â€“2pixel edges, shapes and palette. Reference2 character and reference3 orb are style guides, DO NOT draw them. EXACTLY 6 different sequential frames in ONE HORIZONTAL ROW, no second row, no text, no labels, no cell borders. Logical sheet 768x256 with identical 128x256 cells, evenly spaced fixed center at each cell midpoint. Same scale and center EVERY frame. Small early/late phases STAY SMALL, no magnifying them. Frame0 narrow upright white slash. Frame1 quickly grows wider. Frames2â€“3 thick tall standing light wave, torn featherlike edges, sharp asymmetric spines matching approved reference. Frames4â€“5 splits into thin interrupted vertical streaks and disappearing shards. Vertical midpoint fixed, facing/pushing RIGHT. All art inside individual cells with transparent separation gutters. Match approved peak shape at frames2â€“3. ONLY flat #FFFFFF #B8FFF0 #45DCC4 #1F6F66. Hard sharp pixel stair-steps, alpha0/255, fully transparent background. No gradients, blur, glow, antialias, warm colors, character, floor.
```

### OrbTrail

Source sheet: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-5afe19b1-0ed1-4eae-9b82-a27427372928.png`

```text
Use case: stylized-concept. Production Unity pixel VFX sprite sheet OrbTrail. Reference1 approved burst gives sharp energy tongues, reference2 character gives fine pixel density, reference3 orb gives energy identity. Draw ONLY tail VFX. EXACTLY SIX DIFFERENT looping frames in ONE HORIZONTAL ROW, no second row, no labels/text/borders. Logical sheet768x64,6 identical128x64cells, same stable pixel scale, tail-head at RIGHT EDGE of EACH cell. Six genuinely different LOOPING frames of energy tail ONLY, no orb body. Head of tail anchored at RIGHT edge (x127,y32); tapered flame trails LEFT with jagged torn tongues and little ripped bits. Irregular horizontal tails flicker around stable head, smoothly returning from frame5 to frame0. Loop phases must have similar overall footprint. Exact 128x64 per cell, thin 1â€“2px pixels. Loop sequence: frame0 split left flame tips, frame1 upper tips rise slightly, frame2 lower tongues extend, frame3 upper tongues tuck while lower tear, frame4 tips taper, frame5 tips near first pose but genuinely different; maintain similar white/turquoise mass for seamless loop into frame0. No pulse from small to big. Every tail extends left from right attachment at y32, no upward vertical attack, no round orb/head. Sharp asymmetrical fire-tail spines with little separated fragments inside cell margins. Use fine1â€“2pixel native edges like reference. ONLY flat #FFFFFF #B8FFF0 #45DCC4 #1F6F66. True transparent background with alpha0/255. No gradients, no smooth glow, no antialias/blur, no warmcolors, no character, no bench.
```

### OrbImpactBig

Source sheet: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-ae395991-e1f9-494f-9fcb-40464dcfdbc0.png`

```text
Use case: stylized-concept. Production Unity OrbImpactBig pixel VFX animation sprite sheet. Ref1 is approved peak explosion, preserve jagged ring shape and fine native1â€“2pixel detail. Ref2 Cast_04 density guide, Ref3 Orb_00 identity; do not draw character/orb. EXACTLY EIGHT SEQUENTIAL DIFFERENT FRAMES in ONE HORIZONTAL ROW, no second row, no labels/text/cellborders. Logical sheet2048x256, eight equal256x256cells, every impact origin at cell midpoint, identical scale across frames. Frames0â€“1 sudden tight white impact flash at midpoint. Frames2â€“3 jagged broken irregular ring explodes outward, long uneven spears and shards matching approved peak reference. Frame4 splits ring further. Frames5â€“6 flying sharp fragments drift outward and dissolve. Frame7 sparse final small fragments, almost empty. Momentum to RIGHT. Midpoint at (128,128) in every cell. Make attack happen fast in first1â€“2frames and fade slowly over remaining frames by fragment breakup/shrinking/count, not smooth alpha. Small first and last frames must stay small; match approved sample at peak. Transparent blank separation gutters, each frame fully inside cell. Palette ONLY #FFFFFF #B8FFF0 #45DCC4 #1F6F66. True alpha0/255 transparency, crisp pixel stair-steps, no gradients, antialias, blur or soft glow; no warmcolors, character, scene or floor. CRITICAL LAYOUT CORRECTION: every sprite INCLUDING scattered particles must be smaller than 65 percent of its cell width and 65percent of cell height. Leave at least 35percent horizontal blank gap between adjoining sprite silhouettes. Never let rays overlap a neighboring frame. Use tiny first flash, two medium flashes, then peak broken ring, then declining pieces. A long wide single row of eight distinctly SEPARATED islands, all centers evenly spaced. The peak ring is NOT bigger than its allotted cell. If rendered in a larger canvas keep large blank margins vertically. Count exactly 8 distinct islands.
```

### BenchActivate

Source sheet: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-65cafc24-ad42-4bfa-8c41-ac62723d7418.png`

```text
Use case: stylized-concept. Production pixel art BenchActivate animation sheet with EXACTLY EIGHT DIFFERENT frames ONE horizontal row. Reference bench crystal defines turquoise identity and native fine pixel density; DO NOT draw bench or crystal. Each logical cell256x256, sheet2048x256, center(128,128) is light origin. Frames0â€“1: tiny mintwhite twinkle. Frame2: thin jagged mint ring starts expanding. Frame3: larger sharp irregular expanding energy ring with torn edges, delicate turquoise pointed rays, hollow center. Frame4: broken expanded ring with SIX SMALL bright diamond particles starting to float UP. Frame5: NO RING, only five SMALL diamond spark particles drifting up, each3â€“8nativepixels. Frame6: only three tiny upward drifting specks. Frame7: only TWO tiny specks near top center. Keep last THREE frames almost empty, no large energy slash/blade, no large chunks. SAME scale for every frame, small late specks MUST NOT be magnified. Keep complete art within65percent cellwidth and cells clearly separated by blank transparent gutters. ONLY four flatcolors #FFFFFF #B8FFF0 #45DCC4 #1F6F66, actual transparent alpha0/255background. Fine1â€“2px native stepped edges. No glow, gradients, blurred halos, antialias, warmcolors, text/borders/character/floor.
```

### HealShard

Source sheet: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-fe72e3fd-2a15-4ac2-b7ba-f6054adac86d.png`

```text
Use case: stylized-concept. Unity pixel art health-mask HealShard production sprite sheet. Reference1 Cast_04 is native fine pixel-density guide, reference2 approved OrbImpactBig is mintwhite palette and sharp edge guide. DO NOT draw a character or explosion. FOUR DIFFERENT STATIC MASK-FRAGMENT SHAPES in ONE HORIZONTAL ROW of four equal32x32nativecells (logical128x32sheet); these are four unique pieces, not four states of one piece. FOUR DIFFERENT STATIC SHARD SHAPES, NOT an animation of same shard. White mask health fragments flashing during reassembly: frame0 pointed triangular broken curved mask corner, frame1 hooked concave mask cheek fragment, frame2 thin long tapered mask splinter, frame3 chunky irregular notched mask brow piece. Each alone centered in32x32 with margin, cream-white represented by allowed #FFFFFF/#B8FFF0, turquoise thin edge and tiny glint. No complete face, no red. Each irregular white/mint mask ceramic piece spans20â€“26 native pixels max, centered with at least3px transparent margin. Single1pixel dark teal selective outline, white and pale mint flat highlights, turquoise tiny edge or2pixel glint. Looks like broken mask shard, angular fracture edges and subtly curved mask surface, no full maskface/eyes. Fine precise sharp32x32pixel art, not realistic shaded render, no multiscale giant pixels. ONLY #FFFFFF #B8FFF0 #45DCC4 #1F6F66. Background actually transparent, hard alpha0/255, no antialiasing/glow/gradients/blur, no red/orange/yellow, no text/labels/borders.
```

### CastBurstShadow

Source sheet: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-87cf0d8d-678d-4840-ad77-8aaaa193db81.png`

```text
Use case: stylized-concept. Unity CastBurstShadow pixel VFX horizontal sprite sheet. Ref1 approved CastBurst peak: preserve sharp right-facing cone and fine1â€“2nativepixel edges. Ref2 character style. EXACTLY SIX different sequential frames in ONE HORIZONTAL ROW, no second row/text/borders, equal256x256nativecells logical1536x256. Shadow variant of approved CastBurst: same turquoise white strong RIGHT-directed cone with sharp rays. Frames0â€“1 small white flashes with dark purple curling smoke. Frames2â€“3 long jagged cone rays explode toward RIGHT, black-purple angular smoke curls wrapping behind and around energy; frames4â€“5 broken pointed turquoise fragments and torn purple smoke strips dissolving. Casting origin center fixed. Smoke hard-edged pixel clusters, no glow. ALL frames at SAME SCALE, fixed origin each cell midpoint. Small initial flashes and final particles MUST stay small. Peak cone with purple smoke fits inside70percent of its cell width; leave wide blank transparent separation so cells never overlap. Curled smoke is angular stepped pixel art with torn gaps, stays purple-black with turquoise/white only in energy core. Palette ONLY #FFFFFF #B8FFF0 #45DCC4 #1F6F66 plus #594580 #2A1F40. Strong quick hit then slow shredded fragments/smoke disappearance. Alpha0/255, actual transparent background, no soft glow/gradient/blur/antialias, no warm colors, no character or orb/floor.
```

### OrbImpactBigShadow

Source sheet: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-a4fd1be4-e789-4ee8-a1a8-fd3b2bdf70d2.png`

```text
Use case: stylized-concept. Unity OrbImpactBigShadow pixel VFX horizontal sprite sheet. Reference1 approved OrbImpactBig peak silhouette/shape and fine1â€“2nativepixel edges. Ref2 character style; do NOT draw character or orb. EXACTLY EIGHT DIFFERENT SEQUENTIAL FRAMES, ONE HORIZONTAL ROW no second row/labels/text/cellborders. Logical2048x256, equal256x256cells, fixed impact midpoint per cell, consistent scale. Shadow variant of approved peak OrbImpactBig. Frames0â€“1 tight bright white/turquoise flash at center with dense dark purple curls. Frames2â€“3 jagged broken energy ring explodes with uneven spears; angular dark-purple smoke coils around ring and shards. Frame4 ring breaks; frames5â€“6 drifting splinters and torn smoke dissolve slowly; frame7 sparse final particles. Center origin fixed (128,128), impact momentum RIGHT. Smoke hard-edged, never soft/fuzzy. CRITICAL each entire sprite INCLUDING smoke/particles MUST fit65percent of its cell width/height; very wide BLANK transparent separation gaps between all eight islands. Small initial/last flashes remain small, not magnified. Purple smoke curls are flat hard-edged angular torn pixel shapes, not gradient, only the energy ring white/mint/turquoise. Palette ONLY #FFFFFF #B8FFF0 #45DCC4 #1F6F66 plus #594580 #2A1F40. Actual transparentbackground alpha0/255, no antialiasing/softglow/blur/gradient/shadow/warmcolor, no floor or character.
```

Correction prompt:

```text
Use case: precise-object-edit. This is an EIGHT-frame horizontal OrbImpactBigShadow animation sheet. Preserve the first FIVE frames and their purple/turquoise/white sharp jagged-ring style and native pixel density. Re-draw ONLY last THREE frames so they DISINTEGRATE: frame5 = separated jagged mint/turquoise energy shards and torn purple smoke strips scattered around impact center, NO continuous ring; frame6 = fewer smaller splinters with little thin purple smoke scraps; frame7 = only5â€“7 TINY scattered final particles, almost empty, no ring/flash ball or solid smoke mass. Last particles stay small at same scale, do not resize to fill cell. Keep exactly8frames in one horizontal row, widely spaced separated islands with no overlap, no labels/borders. Every frame stays inside65percentcellwidth. All impact anchors cell midpoint at same fixed native scale. STRICT flat palette #FFFFFF #B8FFF0 #45DCC4 #1F6F66 plus smoke #594580 #2A1F40 ONLY. Actual transparent background alpha0/255, pixel stair-steps, no gradients, glow, blur, antialias, warmcolors, character.
```

### OrbBig

Source sheet: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-25e49d17-af6c-47a3-8aa0-585c3811c851.png`

```text
Use case: stylized-concept. Unity OrbBig energy projectile animation. Reference1 is approved native192x128 OrbBig sprite: keep EXACT right-facing flattened rounded head, jagged flame edge, short torn tail to LEFT, white/mint/turquoise identity and fine1pixel thickness. Reference2 is Cast_04 native pixel-density guide. EXACTLY EIGHT DIFFERENT LOOPING frames in ONE HORIZONTAL ROW, no secondrow/text/borders. Native frame192x128, core-center(120,64), main energy body heightabout110px EVERYframe, right-facing head occupyingx65to175,y9to119; short left tail fromx8to65, completely within frame. Logical sheet1536x128. Stable equal cell centers and SAME SCALE, ALL frames similarly sized; NO grow/shrink explosion or breakup animation. White core tongues rotate/curl visibly over eight phases while jagged perimeter flames flicker1â€“3nativepixels. Frame7 is near frame0 with genuine small differences for seamless loop. Each separated island stays inside itsowncell, wide clear transparent gaps between sprites. Same headposition, motion only internal energy and flame tongues, not moving up/down. Pixel art at fine native1â€“2px step thickness, matching approved sprite/character. STRICT flat palette ONLY #FFFFFF #B8FFF0 #45DCC4 #1F6F66. True transparent alpha0/255 backdrop; no glow, gradient, blur, antialias, softshadow, no warmcolors or character/floor/orbtrailoutsidecell. This is a powerful projectile already big at scale1, NOT tiny orb to enlarge later.
```

Correction prompt:

```text
Use case: stylized-concept. Unity OrbBig energy projectile animation. Reference1 is approved native192x128 OrbBig sprite: keep EXACT right-facing flattened rounded head, jagged flame edge, short torn tail to LEFT, white/mint/turquoise identity and fine1pixel thickness. Reference2 is Cast_04 native pixel-density guide. EXACTLY EIGHT DIFFERENT LOOPING frames in ONE HORIZONTAL ROW, no secondrow/text/borders. Native frame192x128, core-center(120,64), main energy body heightabout110px EVERYframe, right-facing head occupyingx65to175,y9to119; short left tail fromx8to65, completely within frame. Logical sheet1536x128. Stable equal cell centers and SAME SCALE, ALL frames similarly sized; NO grow/shrink explosion or breakup animation. White core tongues rotate/curl visibly over eight phases while jagged perimeter flames flicker1â€“3nativepixels. Frame7 is near frame0 with genuine small differences for seamless loop. Each separated island stays inside itsowncell, wide clear transparent gaps between sprites. Same headposition, motion only internal energy and flame tongues, not moving up/down. Pixel art at fine native1â€“2px step thickness, matching approved sprite/character. STRICT flat palette ONLY #FFFFFF #B8FFF0 #45DCC4 #1F6F66. True transparent alpha0/255 backdrop; no glow, gradient, blur, antialias, softshadow, no warmcolors or character/floor/orbtrailoutsidecell. This is a powerful projectile already big at scale1, NOT tiny orb to enlarge later. IMPORTANT LAYOUT: Draw eight CLEARLY SEPARATED sprite islands with WIDE blank transparent gaps. Each sprite no more than65percent of its horizontal cell width, even including its tail and spikes. Never connect a preceding orb's right head to the next orb's left tail. First/last sprites also need transparent outer margins. Leave35percent cellwidth empty between neighbors. Keep all eight body heights identical. Do not fill entire horizontal sheet with a continuous chain.
```

### OrbBigShadow

Source sheet: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-c813362d-9f4a-4941-b900-a91d2cd93a7a.png`

```text
Use case: stylized-concept. Production Unity OrbBigShadow energy projectile LOOP sprite sheet. Reference1 approved OrbBig sprite defines right-facing rounded flattened head, torn flame edges/short left tail and fine native1pixel density; reference2 Cast_04 is pixel-density guide. Draw EXACTLY EIGHT DIFFERENT LOOPING sprite frames in ONE HORIZONTAL ROW, no secondrow/text/borders. Native EACHframe256x192, main corebody center(160,96), energy head heightabout160px, tail leftinsideframe. Same large scale EVERYframe; head remains at fixedanchor. Eight internal energy phases curl/rotate white/mint flame patches, sharp turquoise perimeter tongues flicker1â€“3pixels. Purple-black hard-edged torn smoke curls around head and trails left, visibly different in each frame but similar mass; frame7 joins frame0 smoothly, no size ramp/explosion/dissipation. Whole sprite including smoke fits frame with8pxmargin, core roughlyx80to240 y16to176, short tailx8to80. Logical horizontal sheet2048x192. CRITICAL8SEPARATE islands: every island max65percent its renderedcellwidth so wide blank transparent gutters, NEVER touch neighbors, first/last also visible margins. Strict flat SIXcolors ONLY #FFFFFF #B8FFF0 #45DCC4 #1F6F66 and smoke #594580 #2A1F40. Fine crisp native1â€“2pixel edges likecharacter, actual alpha transparent background, no gradient/glow/antialias/blur/softshadow, no warmcolors, character, floor.
```

### Mechanical preparation and measured checks

Sheets split at transparent separator valleys; shared X/Y resampling factors for every frame within each animation, nearest-neighbour only. Recenter casting origin; midpoint anchors for impact/wave/checkpoint; trail head at right edge. Alpha threshold128 (250 for OrbBig/OrbBigShadow to remove residual halos), final alpha strictly0/255; nearest-color mapping to four permitted colors, or six for Shadow. No smooth glow/gradient/antialias remains. Character/projectile/bench references and existing art were left unchanged.

| Effect | Frames | Measured frame size | Count/PNG/RGBA/alpha/palette/unique |
|---|---:|---|---|
| CastBurst | 6 | 256x256 | Pass |
| CastWave | 6 | 128x256 | Pass |
| OrbTrail | 6 | 128x64 | Pass |
| OrbImpactBig | 8 | 256x256 | Pass |
| BenchActivate | 8 | 256x256 | Pass |
| HealShard | 4 | 32x32 | Pass |
| CastBurstShadow | 6 | 256x256 | Pass |
| OrbImpactBigShadow | 8 | 256x256 | Pass |
| OrbBig | 8 | 192x128 | Pass |
| OrbBigShadow | 8 | 256x192 | Pass |

Orb energy head height is measured in the head region, excluding left tail; Shadow measurement excludes the two purple smoke colors.
- OrbTrail: changed pixel counts for adjacent transitions including last-to-first: [2044, 2375, 1680, 2115, 2104, 1274]. Visual continuity inspected on contact sheet; not played in Unity.
- OrbBig: energy head height 104–109px; target110/160px.
- OrbBig: changed pixel counts for adjacent transitions including last-to-first: [2626, 2520, 2350, 3253, 3118, 2218, 2591, 1229]. Visual continuity inspected on contact sheet; not played in Unity.
- OrbBigShadow: energy head height 132–143px; target110/160px.
- OrbBigShadow: changed pixel counts for adjacent transitions including last-to-first: [8566, 11244, 10475, 8576, 10535, 11375, 10726, 3486]. Visual continuity inspected on contact sheet; not played in Unity.

PPU80 is the README target and measured reference PPU; new PNGs have no manually edited/generated Unity .meta import configuration. No scene/prefab/script/material changes or Unity runtime verification. Full contact board: Preview_Fx_Full.png. Original approved Preview_Fx.png is preserved; Preview_OrbBig.png shows source comparison at native1:1pixel scale.


### Final correction after measured head-height audit (supersedes earlier OrbBig height ranges)

Initial Shadow energy-head measurements132–143px failed the160px target; the Shadow sheet was regenerated with a larger energy head and thin purple smoke. Final source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-e13940f5-f97a-4e4a-8576-da88df7085ef.png`.

```text
Use case: stylized-concept. Recreate production OrbBigShadow horizontal loop sheet, EXACTLY8distinctframes ONErow, eight256x192logicalcells, overall2048x192sheet. Approved reference1 is OrbBig native sprite. Critical correction: TURQUOISE ENERGY HEAD ITSELF is160nativepixels TALL, NOT just height including smoke. Head center(160,96), energy body topy16 bottomy176, right-facing wide rounded slightlyflattened jagged head. White inner moving flame tongues with mint/turquoise body. Purple smoke must be THIN narrow hardpixel curls mostly along LEFT side and short LEFTtail, NOT big thick black shell above/below green head. Green main silhouette covers ALMOST the entire visible projectile height; purple can exceed it by ONLY3â€“6nativepixels. Body/energy/smoke stayfullyinside256x192frame. Short tailinsidex8to80. SAME160pxcoreheight and sameheadcenter EVERYframe, energy swirls clearly different through8phases, whiteflametongues rotate, rim flickers1â€“2pixels. Frame7 smoothlyjoinsframe0, no size ramp/no explosion/no fade. Eight separate islands with large transparent gutters, no overlapping neighbor; first/last margins, no text/cellborders/secondrow. Keep precise fine1â€“2nativepixel steps matching player density, NO coarse chunky large pixels. Palette ONLY #FFFFFF #B8FFF0 #45DCC4 #1F6F66 plus #594580/#2A1F40 for THIN smoke. Flat fills, truealpha0/255transparent backdrop, no gradient/halo/glow/blur/antialias/softshadow, no red/orange/yellow, no character/floor. The head itself must be at least85percent of total visible height.
```

Nearest-neighbour mechanical height normalization was then applied to the head silhouette within each already-generated frame: OrbBig energy head110px with vertical midpoint64; OrbBigShadow energy head160px with vertical midpoint96. These normalization steps preserve a hard native pixel grid and do not generate animation poses or gradients. Horizontal body anchors remain(120,64)/(160,96) by the specified head geometry; body rightmost extent is175/240px in the native frame.

All68 INSTALLED PNGs were reopened and measured: exact directory/file counts and names, specified native frame sizes, PNG RGBA, alpha only0/255, transparent pixels RGB0, no colors outside permitted four/six-color palettes, no duplicated frames in any set. Every one of the8OrbBig head heights=110px and vertical centers=64; every one of the8Shadow head heights=160px and vertical centers=96. Trail has visible attachment pixels at right edge x127 in all6frames. Existing reference art and .meta hashes unchanged.

- OrbTrail adjacent/wrap changed-pixel counts: [2044, 2303, 1680, 2053, 2072, 1280]. Last-to-first was visually compared; these figures measure pixel changes, not an automatic guarantee of smoothness.
- OrbBig adjacent/wrap changed-pixel counts: [3096, 2905, 2404, 3688, 3154, 2107, 3076, 2010]. Last-to-first was visually compared; these figures measure pixel changes, not an automatic guarantee of smoothness.
- OrbBigShadow adjacent/wrap changed-pixel counts: [7091, 8014, 7932, 8168, 6819, 7552, 6926, 4281]. Last-to-first was visually compared; these figures measure pixel changes, not an automatic guarantee of smoothness.

Pixel density and phase rhythm inspected on final contact board, plus review loop artifact. No Unity import/runtime or scene check performed; PPU80/Point/pivots remain target settings and new .meta files were not edited. Final report: all required art files passed measured file checks.


### Final correction after measured head-height audit (supersedes earlier OrbBig height ranges)

Initial Shadow energy-head measurements132–143px failed the160px target; the Shadow sheet was regenerated with a larger energy head and thin purple smoke. Final source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-e13940f5-f97a-4e4a-8576-da88df7085ef.png`.

```text
Use case: stylized-concept. Recreate production OrbBigShadow horizontal loop sheet, EXACTLY8distinctframes ONErow, eight256x192logicalcells, overall2048x192sheet. Approved reference1 is OrbBig native sprite. Critical correction: TURQUOISE ENERGY HEAD ITSELF is160nativepixels TALL, NOT just height including smoke. Head center(160,96), energy body topy16 bottomy176, right-facing wide rounded slightlyflattened jagged head. White inner moving flame tongues with mint/turquoise body. Purple smoke must be THIN narrow hardpixel curls mostly along LEFT side and short LEFTtail, NOT big thick black shell above/below green head. Green main silhouette covers ALMOST the entire visible projectile height; purple can exceed it by ONLY3â€“6nativepixels. Body/energy/smoke stayfullyinside256x192frame. Short tailinsidex8to80. SAME160pxcoreheight and sameheadcenter EVERYframe, energy swirls clearly different through8phases, whiteflametongues rotate, rim flickers1â€“2pixels. Frame7 smoothlyjoinsframe0, no size ramp/no explosion/no fade. Eight separate islands with large transparent gutters, no overlapping neighbor; first/last margins, no text/cellborders/secondrow. Keep precise fine1â€“2nativepixel steps matching player density, NO coarse chunky large pixels. Palette ONLY #FFFFFF #B8FFF0 #45DCC4 #1F6F66 plus #594580/#2A1F40 for THIN smoke. Flat fills, truealpha0/255transparent backdrop, no gradient/halo/glow/blur/antialias/softshadow, no red/orange/yellow, no character/floor. The head itself must be at least85percent of total visible height.
```

Nearest-neighbour mechanical height normalization was then applied to the head silhouette within each already-generated frame: OrbBig energy head110px with vertical midpoint64; OrbBigShadow energy head160px with vertical midpoint96. These normalization steps preserve a hard native pixel grid and do not generate animation poses or gradients. Horizontal body anchors remain(120,64)/(160,96) by the specified head geometry; body rightmost extent is175/240px in the native frame.

Final Shadow smoke correction: alpha threshold128 is used for OrbBigShadow to retain its opaque purple smoke after binarization; threshold250 remains only for OrbBig. Both export alpha0/255. Every Shadow frame contains permitted purple smoke pixels.

All68 INSTALLED PNGs were reopened and measured: exact directory/file counts and names, specified native frame sizes, PNG RGBA, alpha only0/255, transparent pixels RGB0, no colors outside permitted four/six-color palettes, no duplicated frames in any set. Every one of the8OrbBig head heights=110px and vertical centers=64; every one of the8Shadow head heights=160px and vertical centers=96. Trail has visible attachment pixels at right edge x127 in all6frames. Existing reference art and .meta hashes unchanged.

- OrbTrail adjacent/wrap changed-pixel counts: [2044, 2303, 1680, 2053, 2072, 1280]. Last-to-first was visually compared; these figures measure pixel changes, not an automatic guarantee of smoothness.
- OrbBig adjacent/wrap changed-pixel counts: [3096, 2905, 2404, 3688, 3154, 2107, 3076, 2010]. Last-to-first was visually compared; these figures measure pixel changes, not an automatic guarantee of smoothness.
- OrbBigShadow adjacent/wrap changed-pixel counts: [8356, 9760, 10490, 10022, 8014, 8456, 8904, 5145]. Last-to-first was visually compared; these figures measure pixel changes, not an automatic guarantee of smoothness.

Pixel density and phase rhythm inspected on final contact board, plus review loop artifact. No Unity import/runtime or scene check performed; PPU80/Point/pivots remain target settings and new .meta files were not edited. Final report: all required art files passed measured file checks.
