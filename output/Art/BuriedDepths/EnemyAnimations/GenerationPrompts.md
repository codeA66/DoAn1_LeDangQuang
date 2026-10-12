# Prompt tạo hoạt ảnh

Công cụ: built-in imagegen, transparent_background=true, mỗi job tham chiếu art tạo hình đã duyệt. 12 frame mỗi sheet, layout 4×3. Attack dùng bản v2 đã duyệt. Hurt bọ cạp được vẽ lại để sửa hướng; prompt bổ sung yêu cầu đầu bên phải, đuôi bên trái.

## Worm / Idle

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_Worm.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-c10d4213-c83c-45a1-8dd0-cb4563d89761.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors unique frames of restrained breathing/idle: A neutral alert; B body begins lowering, joints bend slightly; C deeper exhale lowers head and loosens appendages; D lowest body with tail/root appendages settling; E begins inhale lifting torso; F raises head and separates plates subtly; G highest inhale with appendages lifted; H settles partway toward neutral, still a different pose from anchor A. No swelling into balloon. Loop-friendly, anchor H NOT identical to anchor A.
```

## Worm / Move

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_Worm.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-f7c67f8e-626c-4b1d-be93-b3a75703e536.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors for worm locomotion through muscular segment undulation, NO legs: A neck bent slightly forward and rear segments gathered; B contraction begins at tail and travels upward; C body coils compactly and head lowers; D neck extends forward while tail compresses; E longest forward extension with head lifted; F wave passes through middle plates and tail begins releasing; G neck pulls back while tail extends; H returning curved alert pose still different from A. Keep same tail-foot anchor, actual segment/body silhouette changes in every pose, no simple translation of one unchanged worm.
```

## Worm / Attack

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_Worm.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-11cee911-a4e8-484f-9045-b0ece6b126cf.png

```text
Create a detailed pixelart 2D sideview enemy ATTACK SPRITESHEET matching supplied approved DESIGN reference, warm orange-brown earth and dark slate armor, crimson danger eyes. Preserve anatomy, do not redesign. Exactly 12 UNIQUE DRAWN POSES, 4 equal columns and 3 equal rows, rowmajor chronological. Transparent background TRUE alpha. No text/grid/scene/opaque background, no atmospheric glow behind sprites. All face RIGHT. Entire creature within each cell with generous 15% empty margins; fit BIGGEST attack pose to cell, same body scale all12, do NOT enlarge neutral poses to fill cell. Motion must be FORCEFUL, extreme anticipation and unmistakable contact keyframes, redrawing articulation not rotating/scaling/translating idle copies. Stable footbaseline and body anchor across cells. Pose sequence: 0 upright guard; 1 neck pulls BACK; 2 bends into deep backward S curve; 3 head rises above normal; 4 maximum tall backward arch, jaw WIDE; 5 neck uncoils diagonally down; 6 mouth lunges forward/down, body near horizontal; 7 JAWS CLAMP AT GROUND far ahead, flattened neck; 8 biting follow-through pressed low; 9 jaw opens and neck pulls up; 10 rises halfway; 11 returns toward upright. Midattack silhouette MUST radically differ from idle upright worm.
```

## Worm / Hurt

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_Worm.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-51254e09-1154-4334-bc80-ef3075030788.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors distinct frames of taking one hit then recovering: A sees impact and tenses; B flinches with head jerk back; C bends limbs and recoils strongly; D reaches greatest recoil, loose roots/plates bend; E starts regaining footing; F raises head while joints straighten; G torso returns partway; H alert recovery stance different from0. No white flash overlay, no attack pose, no new wounds/gore, no huge size inflation.
```

## Worm / Die

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_Worm.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-f9ec9d1d-ed93-495c-860b-316c4e7e1087.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors distinct NON-looping frames of death: A legs begin giving way and head drops; B front joints buckle; C body leans downward; D collapses toward ground; E impacts ground with a few tiny loose rock/soil fragments; F plate/root limbs settle unevenly; G body slumps flatter and face darkens; H final clearly dead inert collapsed remains with red eye/core extinguished, never return to idle. All frames complete within cell with SAME baseline, no disappearance or empty frame. No blood or gore.
```

## Beetle / Idle

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_Beetle.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-4b7a08b4-0daa-4976-9bd6-89da6a7bdfa0.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors unique frames of restrained breathing/idle: A neutral alert; B body begins lowering, joints bend slightly; C deeper exhale lowers head and loosens appendages; D lowest body with tail/root appendages settling; E begins inhale lifting torso; F raises head and separates plates subtly; G highest inhale with appendages lifted; H settles partway toward neutral, still a different pose from anchor A. No swelling into balloon. Loop-friendly, anchor H NOT identical to anchor A.
```

## Beetle / Move

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_Beetle.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-61fc1036-9b19-47d1-afde-8d7327135a8c.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors distinct frames of grounded locomotion cycle: A reach front appendage forward; B plant front foot and lift rear; C shift weight forward and compress torso; D push rear foot back while front joints load; E opposite limb pair extends; F opposite pair plants; G body rises through push-off with rear limbs drawing forward; H transit to start with changed foot and head positions. Consistent anchor to animate movement through game code, do not progressively slide across cells. Use anatomically correct legs/root limbs for this reference enemy.
```

## Beetle / Attack

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_Beetle.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-dd99d1a3-94ca-49ae-8f18-dd5cb43f4951.png

```text
Create a detailed pixelart 2D sideview enemy ATTACK SPRITESHEET matching supplied approved DESIGN reference, warm orange-brown earth and dark slate armor, crimson danger eyes. Preserve anatomy, do not redesign. Exactly 12 UNIQUE DRAWN POSES, 4 equal columns and 3 equal rows, rowmajor chronological. Transparent background TRUE alpha. No text/grid/scene/opaque background, no atmospheric glow behind sprites. All face RIGHT. Entire creature within each cell with generous 15% empty margins; fit BIGGEST attack pose to cell, same body scale all12, do NOT enlarge neutral poses to fill cell. Motion must be FORCEFUL, extreme anticipation and unmistakable contact keyframes, redrawing articulation not rotating/scaling/translating idle copies. Stable footbaseline and body anchor across cells. Pose sequence: 0 guard; 1 head tucks; 2 front legs fold; 3 rear legs bend deeply and body leans BACK; 4 extreme low compact charged crouch; 5 rear legs drive and front legs leave ground; 6 completely EXTENDED horizontal RAM, head thrust forward, rear legs stretched backward; 7 full lunging impact pose head low/front, hind legs airborne; 8 forelegs brake wide, carapace tilts forward; 9 body recoils and rear legs land; 10 head lifts; 11 settles. Show actual articulated legs and tilted carapace, not idle variations.
```

## Beetle / Hurt

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_Beetle.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-3acde37e-d72f-4e80-a0fb-cc204528b02b.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors distinct frames of taking one hit then recovering: A sees impact and tenses; B flinches with head jerk back; C bends limbs and recoils strongly; D reaches greatest recoil, loose roots/plates bend; E starts regaining footing; F raises head while joints straighten; G torso returns partway; H alert recovery stance different from0. No white flash overlay, no attack pose, no new wounds/gore, no huge size inflation.
```

## Beetle / Die

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_Beetle.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-e806853c-48ec-4dda-907a-f8fbc3a812c0.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors distinct NON-looping frames of death: A legs begin giving way and head drops; B front joints buckle; C body leans downward; D collapses toward ground; E impacts ground with a few tiny loose rock/soil fragments; F plate/root limbs settle unevenly; G body slumps flatter and face darkens; H final clearly dead inert collapsed remains with red eye/core extinguished, never return to idle. All frames complete within cell with SAME baseline, no disappearance or empty frame. No blood or gore.
```

## RootCrawler / Idle

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_RootCrawler.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-83f3d757-cfde-4d14-91eb-2c28bde2286f.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors unique frames of restrained breathing/idle: A neutral alert; B body begins lowering, joints bend slightly; C deeper exhale lowers head and loosens appendages; D lowest body with tail/root appendages settling; E begins inhale lifting torso; F raises head and separates plates subtly; G highest inhale with appendages lifted; H settles partway toward neutral, still a different pose from anchor A. No swelling into balloon. Loop-friendly, anchor H NOT identical to anchor A.
```

## RootCrawler / Move

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_RootCrawler.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-48428bb8-a59b-4ed6-8426-7efa928740b7.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors distinct frames of grounded locomotion cycle: A reach front appendage forward; B plant front foot and lift rear; C shift weight forward and compress torso; D push rear foot back while front joints load; E opposite limb pair extends; F opposite pair plants; G body rises through push-off with rear limbs drawing forward; H transit to start with changed foot and head positions. Consistent anchor to animate movement through game code, do not progressively slide across cells. Use anatomically correct legs/root limbs for this reference enemy.
```

## RootCrawler / Attack

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_RootCrawler.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-920b16f7-599f-4677-afec-2fb7bbf658a5.png

```text
Create a detailed pixelart 2D sideview enemy ATTACK SPRITESHEET matching supplied approved DESIGN reference, warm orange-brown earth and dark slate armor, crimson danger eyes. Preserve anatomy, do not redesign. Exactly 12 UNIQUE DRAWN POSES, 4 equal columns and 3 equal rows, rowmajor chronological. Transparent background TRUE alpha. No text/grid/scene/opaque background, no atmospheric glow behind sprites. All face RIGHT. Entire creature within each cell with generous 15% empty margins; fit BIGGEST attack pose to cell, same body scale all12, do NOT enlarge neutral poses to fill cell. Motion must be FORCEFUL, extreme anticipation and unmistakable contact keyframes, redrawing articulation not rotating/scaling/translating idle copies. Stable footbaseline and body anchor across cells. Pose sequence: 0 guard; 1 plants rear feet widely; 2 front ROOT ARMS unfold UP; 3 front torso rises tall balanced on rear legs; 4 BOTH root arms overhead fully raised, body upright forehalf; 5 root arms slash sharply DOWN; 6 long sharp root arms pierce below footline; 7 body bows deeply, roots driven down, shoulders very low; 8 maximum buried-root pulse, soil chips; 9 tears root arms back out; 10 forelegs lower and torso descends; 11 quadruped guard. Strong upright-to-collapsed motion, same root beast not humanoid.
```

## RootCrawler / Hurt

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_RootCrawler.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-bb0dffa8-3577-4f46-aea1-15e811b3e9d2.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors distinct frames of taking one hit then recovering: A sees impact and tenses; B flinches with head jerk back; C bends limbs and recoils strongly; D reaches greatest recoil, loose roots/plates bend; E starts regaining footing; F raises head while joints straighten; G torso returns partway; H alert recovery stance different from0. No white flash overlay, no attack pose, no new wounds/gore, no huge size inflation.
```

## RootCrawler / Die

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_RootCrawler.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-8650946e-9eba-491d-aeb6-4c0824d9e97c.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors distinct NON-looping frames of death: A legs begin giving way and head drops; B front joints buckle; C body leans downward; D collapses toward ground; E impacts ground with a few tiny loose rock/soil fragments; F plate/root limbs settle unevenly; G body slumps flatter and face darkens; H final clearly dead inert collapsed remains with red eye/core extinguished, never return to idle. All frames complete within cell with SAME baseline, no disappearance or empty frame. No blood or gore.
```

## EarthGolem / Idle

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_EarthGolem.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-a2984253-fd90-4aac-b379-3a8283feae2d.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors unique frames of restrained breathing/idle: A neutral alert; B body begins lowering, joints bend slightly; C deeper exhale lowers head and loosens appendages; D lowest body with tail/root appendages settling; E begins inhale lifting torso; F raises head and separates plates subtly; G highest inhale with appendages lifted; H settles partway toward neutral, still a different pose from anchor A. No swelling into balloon. Loop-friendly, anchor H NOT identical to anchor A.
```

## EarthGolem / Move

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_EarthGolem.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-e74793a5-269d-4a8f-bf82-eab544688301.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors distinct frames of grounded locomotion cycle: A reach front appendage forward; B plant front foot and lift rear; C shift weight forward and compress torso; D push rear foot back while front joints load; E opposite limb pair extends; F opposite pair plants; G body rises through push-off with rear limbs drawing forward; H transit to start with changed foot and head positions. Consistent anchor to animate movement through game code, do not progressively slide across cells. Use anatomically correct legs/root limbs for this reference enemy.
```

## EarthGolem / Attack

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_EarthGolem.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-e7514bb7-458a-4190-9edc-62393e559ed8.png

```text
Create a detailed pixelart 2D sideview enemy ATTACK SPRITESHEET matching supplied approved DESIGN reference, warm orange-brown earth and dark slate armor, crimson danger eyes. Preserve anatomy, do not redesign. Exactly 12 UNIQUE DRAWN POSES, 4 equal columns and 3 equal rows, rowmajor chronological. Transparent background TRUE alpha. No text/grid/scene/opaque background, no atmospheric glow behind sprites. All face RIGHT. Entire creature within each cell with generous 15% empty margins; fit BIGGEST attack pose to cell, same body scale all12, do NOT enlarge neutral poses to fill cell. Motion must be FORCEFUL, extreme anticipation and unmistakable contact keyframes, redrawing articulation not rotating/scaling/translating idle copies. Stable footbaseline and body anchor across cells. Pose sequence: 0 fists hanging; 1 bends knees; 2 elbows pull BACK; 3 both fists at shoulder height; 4 both fists ABOVE HEAD fully overhead; 5 maximum high arms, backward arch; 6 swings BOTH giant fists down through chest height; 7 BOTH fists IMPACT ground, torso folded far forward, knees deep; 8 lowest follow-through fists on ground, small stone chips; 9 starts lifting fists; 10 torso rises halfway; 11 fists lower toward guard. Overhead and groundimpact silhouettes must be unmistakable.
```

## EarthGolem / Hurt

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_EarthGolem.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-8608ec83-7fc7-46dc-b5f4-8b2062fe5f08.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors distinct frames of taking one hit then recovering: A sees impact and tenses; B flinches with head jerk back; C bends limbs and recoils strongly; D reaches greatest recoil, loose roots/plates bend; E starts regaining footing; F raises head while joints straighten; G torso returns partway; H alert recovery stance different from0. No white flash overlay, no attack pose, no new wounds/gore, no huge size inflation.
```

## EarthGolem / Die

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_EarthGolem.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-02333f38-211a-4eb4-868c-aeec1f436f8e.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors distinct NON-looping frames of death: A legs begin giving way and head drops; B front joints buckle; C body leans downward; D collapses toward ground; E impacts ground with a few tiny loose rock/soil fragments; F plate/root limbs settle unevenly; G body slumps flatter and face darkens; H final clearly dead inert collapsed remains with red eye/core extinguished, never return to idle. All frames complete within cell with SAME baseline, no disappearance or empty frame. No blood or gore.
```

## Scorpion / Idle

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_Scorpion.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-ef90bbeb-5086-4781-a9df-46995d7bb234.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors unique frames of restrained breathing/idle: A neutral alert; B body begins lowering, joints bend slightly; C deeper exhale lowers head and loosens appendages; D lowest body with tail/root appendages settling; E begins inhale lifting torso; F raises head and separates plates subtly; G highest inhale with appendages lifted; H settles partway toward neutral, still a different pose from anchor A. No swelling into balloon. Loop-friendly, anchor H NOT identical to anchor A.
```

## Scorpion / Move

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_Scorpion.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-68387143-c5bc-44cb-9c46-452dfb8c1f31.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors distinct frames of grounded locomotion cycle: A reach front appendage forward; B plant front foot and lift rear; C shift weight forward and compress torso; D push rear foot back while front joints load; E opposite limb pair extends; F opposite pair plants; G body rises through push-off with rear limbs drawing forward; H transit to start with changed foot and head positions. Consistent anchor to animate movement through game code, do not progressively slide across cells. Use anatomically correct legs/root limbs for this reference enemy.
```

## Scorpion / Attack

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_Scorpion.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-3cab0102-5fe3-4fa0-bd7f-a5a0c36d9238.png

```text
Create a detailed pixelart 2D sideview enemy ATTACK SPRITESHEET matching supplied approved DESIGN reference, warm orange-brown earth and dark slate armor, crimson danger eyes. Preserve anatomy, do not redesign. Exactly 12 UNIQUE DRAWN POSES, 4 equal columns and 3 equal rows, rowmajor chronological. Transparent background TRUE alpha. No text/grid/scene/opaque background, no atmospheric glow behind sprites. All face RIGHT. Entire creature within each cell with generous 15% empty margins; fit BIGGEST attack pose to cell, same body scale all12, do NOT enlarge neutral poses to fill cell. Motion must be FORCEFUL, extreme anticipation and unmistakable contact keyframes, redrawing articulation not rotating/scaling/translating idle copies. Stable footbaseline and body anchor across cells. Pose sequence: 0 guard; 1 front pincers anchor wide; 2 tail recoils BACK; 3 tail coils tightly behind carapace; 4 tail lifts VERY high sting pointing forward; 5 unfurls over head; 6 sting sweeps forward diagonally; 7 full forward STAB low beyond claws, tail extended almost horizontally; 8 maximum stab extension body pressed low; 9 tail sharply retracts upward; 10 tail curls halfway; 11 guarded arch. Tail tip must travel a huge arc relative to body, not static uprighttail.
```

## Scorpion / Hurt

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_Scorpion.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-e9cb7025-6750-4d3c-98cd-1bd0e40a5ec3.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors distinct frames of taking one hit then recovering: A sees impact and tenses; B flinches with head jerk back; C bends limbs and recoils strongly; D reaches greatest recoil, loose roots/plates bend; E starts regaining footing; F raises head while joints straighten; G torso returns partway; H alert recovery stance different from0. No white flash overlay, no attack pose, no new wounds/gore, no huge size inflation.
```

## Scorpion / Die

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_Scorpion.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-45bef4b7-fad8-4393-bda2-39f6638dfd22.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors distinct NON-looping frames of death: A legs begin giving way and head drops; B front joints buckle; C body leans downward; D collapses toward ground; E impacts ground with a few tiny loose rock/soil fragments; F plate/root limbs settle unevenly; G body slumps flatter and face darkens; H final clearly dead inert collapsed remains with red eye/core extinguished, never return to idle. All frames complete within cell with SAME baseline, no disappearance or empty frame. No blood or gore.
```

## Worm / Emerge

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_Worm.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-ec45c6b2-4fd6-450f-9def-360eee940fb2.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors DISTINCT frames of worm EMERGING vertically from below baseline: A only small jaw tip and one rock plate visible with tiny soil flecks; B head begins breaking through; C head emerges to one quarter height; D jaw and two neck segments emerge; E neck rises half height and bends; F upper torso unfurls with mouth beginning to open; G body stretches higher; H full upright worm exposed ready to attack. Visible portion increases every frame, underground part genuinely occluded by transparency below shared baseline. No actual floor strip, no landscape. One worm per cell.
```

## Worm / Burrow

Reference: D:/DoAn1_LeDangQuang/output/Art/BuriedDepths/Enemy_Worm.png

Native output: C:\Users\DANG QUANG\.codex\generated_images\01a0fbee-92d3-7981-8b0b-6dc84739dedc\exec-05a7096b-0e4c-46ea-b179-ffbe37429523.png

```text
Use case: identity-preserve game sprite animation. The input reference is the approved enemy DESIGN. Create exactly TWELVE genuinely DIFFERENT hand-animated keyframe sprites of THIS SAME creature for the specified animation. Preserve its exact characteristic anatomy, head, armored plate design, warm brown-orange earth/root colors, dark gray slate plates, crimson dangerous accents, crisp detailed pixel art. Do NOT redesign it or make an unrelated monster, keep all poses facing RIGHT like reference, no mirrored or back-facing pose. No lighting/color changes pretending to be animation.
Layout STRICT: 4 columns x 3 rows = exactly 12 equal rectangular cells on a wide landscape sprite sheet. Frames 0,1,2,3 top row; 4,5,6,7 middle row; 8,9,10,11 bottom row, read left-to-right. Every frame has ONE complete creature in its cell, generous transparent margin on ALL sides; no sprites touch or overlap neighboring cells; no cropping limbs/tail. Same drawing SCALE and consistent ground baseline at 85% of cell height for all twelve cells, stable center anchor except purposeful animated motion. No drawn grid, no labels/numbers, no text, no watermark, no scenery, no supporting ground, no opaque background, no glow-halo backdrop. TRUE ALPHA outside creatures and specified small action debris. Sprite sheet is intended to be cut by this 4x3 grid, so regular positioning is critical.
Every frame must show a DISTINCT visible silhouette/articulation change in the requested motion. Never copy-paste the same pose. Distinct consecutive anatomical movement rather than whole-image rotation/translation/color changes. Preserve detailed pixel texture but maintain stable design.

The described motion below contains eight story anchors A..H; render TWELVE frames by adding truly drawn intermediate poses: frame0=A,1=B,2=betweenB/C,3=C,4=D,5=betweenD/E,6=E,7=F,8=betweenF/G,9=G,10=betweenG/H,11=H. These are twelve DISTINCT anatomical poses, no frame copying. Do not print anchor labels.
Eight motion anchors DISTINCT frames of worm BURROWING downward, NOT reversed Emerge: A upright turns jaw toward baseline; B neck coils over with head lowered; C jaw presses into baseline, mouth closes; D head disappears down while upper segments fold strongly forward; E half torso descends with tapering tail raised; F lower segments twist down; G last plated tail tip sinks; H only tiny final tail scale and settling few soil particles remain above baseline. One sprite per cell, never entirely empty frame. No ground strip or scenery.
```


## Sửa cuối EarthGolem / Attack — đã áp dụng

Công cụ: built-in imagegen, transparent_background=true; reference: sheet Attack v2 đã duyệt. Ảnh dùng trong project: Source/Attack.png; bản duyệt: AttackPreview/EarthGolem_Attack_MarginPreview.png.

```text
Redraw this approved 12 frame golem ATTACK sheet as a SPARSE THUMBNAIL ATLAS, with HUGE clear transparent gutters. Keep SAME 12 distinct poses chronological: knees bend, arms rise abovehead, bothfists swing down, both fists impact ground, recover. Four equal columns three equal rows exactly. Creatures occupy ONLY ONE THIRD of everycell width and HALF cellheight, much smaller than input. Every pose strictly inside itscell, no neighboring fists or debris touch or overlap. All tiny stonechips stay directly beside fists insideowncell. No brown fog or background; all emptyspace TRUE alpha0. Preserve approved warm earthbrown roots/slate armor/crimson core style and creature identity. Stable body scale and baseline eachrow. No letters or lines. Prioritize COMPLETE separate sprites over filling image with detail.
```

## Sửa cuối Beetle / Idle — đã áp dụng

Công cụ: built-in imagegen, transparent_background=true; reference: sheet Idle gốc. Ảnh dùng trong project: Source/Idle.png; bản duyệt: AttackPreview/Beetle_Idle_MarginPreview.png.

```text
Repair approved BEETLE IDLE sprite sheet. Preserve same12 DISTINCT idle breathing poses and warm brownroot/slateplate/red eye identity. Exactly4columns3rows. Last-column claws are cut off in source: reconstruct COMPLETE claws and entire creature in all12poses. Make sparse atlas with huge transparent gutters, every complete sprite fully inside its own equalcell, no contact with outside sheet edge. Whole beetle artwork occupies only HALF eachcell width. Same body scale all12, all faceRIGHT, head and clawsRIGHT. Each frame different idle articulation knee/head/back not copies. Truealpha0 background, no brown haze, ground or shadow. No text or grid. Sourceimage is pose reference; make more room for complete claws, don't preserve sourcecropping.
```

