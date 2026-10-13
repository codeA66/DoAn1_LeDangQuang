# BreakWall — prompts used

## 2026-10-13: preview only

User override: rejected stacked-rock/root design from README, chose a single stone monolith with asymmetrical spiral veins and a few rusty bands; kept256x1344 wall frame, sinking mechanic, identical crack silhouettes. User approved the new base wall with "ok". This deliverable is the four-sample full preview; all remaining frame production awaits approval.

Tool: built-in image_gen with transparent_background=true. Final references: Platforms_Stone.png for buried material; native wall for crack edit; intact device for dead-device edit.

## Wall_00 new approved design

```text
Use case: stylized-concept. ONE new Unity pixel art upright breakable stone-wall preview sprite. User-approved NEW design overrides stacked-platform look: a SINGLE SOLID DARK STONE MONOLITH, asymmetrical spiral geological veins carved through its face, just a few rusty industrial metal bands clamping the monolith. Reference platform is MATERIAL/COLOR ONLY, do NOT copy its stacked boulders, root clutter, wooden supports. Very tall narrow pillar logical256x1344px, about5.25times taller than wide, centered completely visible on portrait transparent canvas with generous side margins. Strong distinctive silhouette, flat straight TOP ceiling contact and flat straight BOTTOM floor contact, slightly jagged sides. One huge continuous stone slab with large sweeping crooked spiral/S-shaped mineral grain and offset winding grooves, irregular asymmetrical stone whorls and subtle sparse fine hairline fractures. NO regular glyph, face/skull, glowing rune, vortex light or magic ornament. Material dark charcoal-purple slate with muted gray-violet stone planes; veins matte mineral/earth browns and lighter neutral stone, not emissive. Few rusty dull brown metal compression bands/rivets interrupting grain at different heights. Sparse tiny root fibers only in crevices, avoid vines covering monolith. Bold readable large shape, restrained tiny sharp pixel texture clusters native1â€“2px, dark #0C0A14 contour, pixel density128px pergame tile. Art should feel unusual but part of old buried industrial earth world. Completely transparent background, solid hard pixel edges, no blur, antialias, gradientglow, softshadow, floor or scenery. No turquoise, no red, no yellow/orange LIGHT, no text. Normal material orangebrown rust is matte. Intact state only, no huge cracks yet. Narrow tall COLUMN not wide tower, top/bottom flat stone caps integrated into monolith itself.
```

## WallCrack_02 strongest damage

```text
Use case: precise-object-edit. Edit reference1 Wall_00 sprite into ONE strongest damaged state WallCrack_02. EXACTLY preserve original OUTER SILHOUETTE, width/height, flat top/bottom, monolithic spiral stone grain, roots, rusty metal bands, rivet positions and color texture. Change ONLY ADDING a network of dark jagged stone cracks: deep irregular branching fissures spanning the column vertically and diagonally, spread across spiral grain, several large ruptures joined by thinner branching cracks, about to collapse. Cracks are #0C0A14 dark hard1â€“4nativepixel jagged lines, NOT glowing, no red/mint. Metal bands can show thin dark fractures but must not move or bend/lose shape. Do not shift/rotate any stone or root. Do NOT break away edges, detach fragments, create rubble or erase alpha inside fissures. Solid visible dark stone stays behind cracks. Same native256x1344frame; source canvas may be larger but pillar pose/layout identical. Pixel-precise edit, no other lighting or recolor. Background true transparent, hard alpha0/255, no glow/blur/antialias/shadow/text/ground. ONE COLUMN ONE STATIC FRAME ONLY.
```

## DeviceIdle_00

```text
Use case: stylized-concept. ONE intact breakable remote-control device preview sprite DeviceIdle_00 for buried-earth Unity pixelart level. Reference1 platform supplies gritty dark rock, matte orangebrown roots/earth and rusty old buried mining machinery material. Reference2 new monolithic spiral wall supplies related stone grain details, use as material motif not device silhouette. Draw an ancient squat MACHINE built from dark stone housing and rusted industrial metal: a central recessed socket holds a jagged TURQUOISE CRYSTAL energy core, #45DCC4 with #B8FFF0 hard pixel highlights, low steady starting idle brightness. Chunky worn riveted metal braces/plates and mechanical fittings integrated in stone; sparse small roots and old conduits wind down into a flat stone plinth, suggesting it controls a distant wall. Readable strong compact side-view game object, front side elevation, no perspective ground plane. Native frame384x448; ART visible HEIGHT320â€“384px (2.5â€“3tiles), width fitsframe withmargin, center horizontally, flat bottom resting on stone plinth, feet/roots do not protrude below bottom baseline. Root/rust earthy matte browns, rock charcoal-purple, contour #0C0A14, no glowing orange/yellow surfaces, NO red. Only luminous component is mint/turquoise crystal, hard flat highlight clusters, no softglow spill. Fine sharp 1â€“2nativepixel detail density at128px/tile matchingworld reference; no cartoon bigblocks/no smooth illustration. Actually transparent alpha background, no floor/scene, no blur/antialias/softshadows, no letters, runes, labels, UI. Only ONE intact sprite, no animation, no second object.
```

## DeviceBreak_05

```text
Use case: precise-object-edit. Edit reference1 native DeviceIdle_00 into ONE final destroyed state DeviceBreak_05, not an animation sheet. Preserve device overall centered location, size, main outer stone/metal framework, root/conduit layout, flat bottom plinth and floor baseline, exact384x448logicalframe. Crystal core has SHATTERED and gone COMPLETELY DARK: central socket now empty near-black charcoal-purple cavity with a few dull broken gray-violet mineral crystal stumps, NO turquoise/mint pixels, NO light, no active core, no red. Add major jagged dark cracks across stone housing, split metal braces and fractured socket rim; several small dull settled chips inside cavity/on plinth. No airborne flying fragments or motion effects in this final held state. Frame05 is static inert destroyed device, recognizable SAME machine after damage. Preserve flat base and root positions, no moving machine downward or shrinking. Matte orangebrown roots/rust, dark purplecharcoal stone, contour #0C0A14. Native sharp1â€“2pixelart detail at128px/tile. Background completelytransparent, hard alpha0/255, no glow/blur/gradient/antialias/softshadow/warmLIGHTS, text/scene/ground. Only this one final dead machine.
```

## Mechanical preparation and measured results

Nearest-neighbour crop/resize only. Sprite alpha threshold128, final alpha0/255 and transparent RGB0. One native-pixel inner dark contour#0C0A14. Crack ink extracted from generated edit and composited onto unchanged base RGB; no rearrangement/recoloring of base stone texture, identical wall alpha masks. Device crystals normalized to#45DCC4/#B8FFF0; broken core is dark. Both device samples use the same337x384 native art rectangle within384x448, foot baseline448.

| Sample | Measured frame size | Visible height | PNG/RGBA/alpha |
|---|---|---:|---|
| Wall_00 | 256x1344 | 1344 | Pass |
| WallCrack_02 | 256x1344 | 1344 | Pass |
| DeviceIdle_00 | 384x448 | 384 | Pass |
| DeviceBreak_05 | 384x448 | 384 | Pass |

Wall_00/WallCrack_02 alpha masks equal pixel-for-pixel; 51577RGB pixels differ, every changed pixel uses dark fissure ink#0C0A14. Preview2098x1536. Earth32x32 ->128x128 exactly4x; Cast256x256 ->410x410 nearest-neighbour (1.6x rounded to integer raster dimensions). All objects align alpha bottoms at previewy1440. Reference PNGs and README/meta hash unchanged.

The wall visual design intentionally differs from README stacked-stone/platform layout per user override. Preview_BreakWall.png is the only image installed in the project at this stage; four native samples are retained in task scratch pending approval. No debris or full animation generated yet. No scene, prefab, script, existing .meta or existing art modified; no Unity runtime/import check. PPU128 is specified target, not applied through .meta.


## 2026-10-13 — Full20PNG set after Preview_BreakWall approval

User explicitly approved full preview and requested all files installed in project. Approved new monolith design supersedes README stacked-column appearance. Native baseWall_00, maximumWallCrack_02, idleDeviceIdle_00, finalDeviceBreak_05 are retained from approved sample pixels. No Unity scene/prefab/script/importer changes.

### wall source sheet

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-743a6014-3538-4168-b76a-c921c36cb2d7.png`

```text
Use case: stylized-concept. Unity WallCrack STATIC DAMAGE STATE sprite sheet, EXACTLY THREE VERTICAL COLUMNS side by side in ONE HORIZONTAL ROW, logical768x1344, three256x1344cells. Ref1 intact Wall_00 is ABSOLUTE appearance/layout lock; ref2 approved maximum-damage WallCrack_02 gives final fissure positions/network. User approved new MONOLITH asymmetrical spiral grain and rusty bands, do NOT use stackedrockpile. All3columns MUST preserve EXACT outer silhouette, height, width, same grain, root positions, flat top/base, bands/rivets. ONLY dark crack ink changes. Left frame00: a FEW narrow1â€“2nativepixel branching fissures, start of breaking, mostface intact. Middle frame01: same cracks spread farther down across spiral grain and bands, medium network, wider2â€“3pixel branch lines; faces/edges do not detach. Right frame02: final strong widespread network matching approvedref2, deep jagged dark fractures throughoutface. Progressive3states, clearly different, cannot return to intact. Cracks #0C0A14, NO translucent holes, glowingcracks, red, turquoise, floating pieces, collapse/shifting/rotation. Native1â€“2pixel sharpdensity128px/tile; materialmatte darkcharcoal-purple slate, soil/root/rustbrowns same refs, no warm LIGHT. ALLcolumns fullyvisible fromflatceilingcontact toflatfloorcontact, fixedcenter ineachcell, widelyseparated by blanktransparent gutter, never cropped or overlapneighbors. No text/labels/gridlines. Truealpha transparentbackground, opaque hardedges, no antialias/blur/glow/softshadow/scene. Source canvas can haveblankmarginsbut each VERY TALL narrowcolumn aspect256:1344.
```

### debris source sheet

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-fd58aff4-612f-4534-81e0-2310c7f5ea07.png`

```text
Use case: stylized-concept. Unity pixelart Debris source sheet: FOUR DIFFERENT STONE FRAGMENTS ONLY, ONE horizontal row, four64x64logicalcells (sheet256x64). Reference1 approved spiral monolith wall is material and native density guide. Pieces are rubble breaking from this wall, dark charcoal-purple stone with muted matte tan/brown geological veins and a few fine fractures, contour #0C0A14. Left to right: irregular triangular angular wedge; chunky uneven trapezoid boulder; long thin chipped sliver; asymmetrical notched polygon chunk. Each is one distinct centered piece, span35â€“56nativepixels, leave4pxtransparentmargin minimum. Vary fracture shape, grain and orientation naturally, NOT four rotations of sameimage, not pebblecircleicons. Native hard1â€“2pixel clusters at128px/tile, sharp flat pixel planes and gritty fine texture compatible withref. Each piece fullyinsidecell, blank wide transparent gutters, never overlapping neighbors. Actual transparent background alpha0/255, no blur/antialias/glow/softshadow. No light/emission, red/turquoise/core, metal/wood/character/scene/text/borders. Just four separate matte STONE fragments.
```

### idle source sheet

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-9207cd92-4f23-48c3-866c-73994a47602c.png`

```text
Use case: stylized-concept. Production Unity DeviceIdle animation sprite sheet. Ref1 is APPROVED native384x448 DeviceIdle_00; preserve EXACT machine stone/root/rust/rivet positions, same squarefront elevation chassis, same flat plinth. EXACTLY SIX DISTINCT FRAMES in ONE HORIZONTAL ROW, logical2304x448, six identical384x448cells. No secondrow/text/borders. ONLY turquoise crystal lighting changes, entire machine shape/location/static surface MUST remain fixed, no shaking/shrinking. Crystal pulse phases: frame0 low mint highlight area like approvedref; frame1 noticeably brighter with more pale mint faceted highlights; frame2 brightest broad pale mint core planes; frame3 still bright but moving/curling shard-edge light patterns distinctly different; frame4 relaxes to medium highlight area; frame5 dimmer approachingframe0, smooth loop last-to-first but genuinelydifferent crystal pixels. Show strong readable pulse by area of solid color bands, NOT smooth alpha/glow overlays. Crystal EXACT palette #45DCC4 and #B8FFF0 with unchanged darksocketedge; NO white flash in idle, no red/orange/yellow emission. Machine matte darkpurplecharcoal stone, mutedearth/rust/rootbrowns, #0C0A14 contour asref. Each whole device fits individual cell; visible device height384nativepx fromflatbase, same size EVERYframe, widelyseparated islands withlarge blanktransparentgutters, first/last also blankmargins and nocropping. Native1â€“2pixel fine density128px/tile; actualtransparentbackground alpha0/255, no gradients, blur, softglow, antialias, shadows, character/scene/ground. Produce sixfullmachineframes, not six coreicons.
```

### break source sheet

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-120bd716-9959-4d9a-a75b-76dd95135e12.png`

```text
Use case: stylized-concept. Production Unity DeviceBreak destruction animation sprite sheet, EXACTLY SIX DIFFERENT consecutive frames ONE HORIZONTAL ROW, six384x448nativecells logical2304x448. Reference1 approvedDeviceIdle_00 locks intact chassis/root/rust/stone/foot size, reference2 approvedDeviceBreak_05 locks final broken dark machine shape. Same device SAME SCALE/LOCATION/floor baseline EVERYframe, flat base unchanged, visible machinebody384nativepx high. Phase0: crystal abruptly flashes white/mint and fine jagged fissures form across core andsocket. Phase1: stronger angular pure WHITE flash through cracked core, a few moredarkcracks, crystal still seated. Phase2: crystal splits into several discrete angular white/mint/turquoise chunks springing outward aroundsocket, housing beginsfracturing, smallparticles. Phase3: fragments flying further outsidecore, socket darkening, metal/stonehousing major cracks, fewerenergychunks. Phase4: cracked broken shell remains, core totally extinguished nearblack cavity withgraypurple brokenstumps, NO luminous mint/turquoise left; settled stone chips. Phase5: final held inert machine EXACTLY likeapprovedreference2, NO glow/mint/turquoise/whiteenergy. Do not shrink machine, movefloor or turnperspective; preserveouterbody/base/rootposition andpose. Fine crisp1â€“2nativepixelart density128px/tile, dark #0C0A14 outline, darkcharcoalpurple rock, matteorangebrown rust/rootearth. Crystalenergy colors ONLY #FFFFFF #B8FFF0 #45DCC4, neverred/orange/yellowEMISSION. ALLcomplete frames includingcorefragments fit theircells withtransparentgutter, wide separated islands, no clippingfirst/last, no neighboroverlap; keep flyingbits insideframe. Actualtransparentalpha0/255background, no blur/gradientglow/antialias/softshadow, text/borders/secondrow/ground/character. Strong fastflash then gradualshatter and fullydark finalheldstate.
```

### DeviceBreak first-two-flash correction

Selected source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-f3054278-2e8e-43d7-acbc-02d94205d56e.png`

```text
Use case: precise-object-edit. Reference1 is SIX-frame DeviceBreak sheet, ONE horizontal row. Preserve exactly sixframes and entire existing machine shapes/positions/flatbases and frames2â€“5 destruction states unchanged. Correct ONLY core in first TWO frames: frame0 MUST show clear sudden angular WHITE flash across central crystal, pure #FFFFFF fills on large central planes surrounded by #B8FFF0, thin #45DCC4 atedge, visible sharp jagged fissures. Frame1 MUST be STRONGER WHITE flash, white dominates corebody, cracks darken and branch but crystal remains seated. These flashes must look visibly different/BRIGHTER than normal idle turquoise crystal. NO glow or soft halo, flash made of solid sharp white pixel facets, no extraneous impactrays. Frame2â€“3 maintain shattering/flying turquoisewhite chunks, frame4â€“5 stayfullydarkdeadcore. Allsame scale, onehorizontalrow six384x448logicalcells, no secondrow/text/grid/borders, actualtransparent alpha0/255. Material mattebrownrust/root/darkpurplestone, no warmLIGHTS or red. Finepixelartdensity128px/tile, no antialias/blur/softshadow. Keep eachspritecompleteinsideitscell withoutneighboroverlap; allflatbaselines unchanged.
```

### Cutting/compositing

All new states drawn as horizontal sheets with reference samples. Sheet alpha valleys/fixed equal cells used for cutting; nearest-neighbour resize only. Dark wall fissure ink extracted and intersected with approved maximum fissure network. Crack00 subsetCrack01 subsetCrack02; every wall frame keeps original alpha and unchanged stone/root/rust pixels outside added#0C0A14 crack ink. No procedural lines drawn for damage.

Idle crystal facet patterns are extracted from generated full-machine sheet, mapped to original crystal footprint and two allowed turquoise colors, then composited over exact approved static chassis. Generated phases ordered to rise/fall brightness; RGB outside original core and all alpha remain identical. Break sheet uses shared floor/vertical scale and native body footprint337x384 inside384x448; energy mapped to#45DCC4,#B8FFF0,#FFFFFF. Final05 remains approved dead device. Debris uses one shared scale across four source shapes. Alpha threshold128 -> binary0/255; transparent RGB zeroed; hard1pixel #0C0A14 native contour.

### Measured INSTALLED file results

| Group/file range | Count | Frame size | Result |
|---|---:|---|---|
| Wall/Wall_00.png | 1 | 256x1344 | Pass |
| Wall/WallCrack_00..02.png | 3 | 256x1344 | Pass |
| Debris/Debris_00..03.png | 4 | 64x64 | Pass |
| Device/DeviceIdle_00..05.png | 6 | 384x448 | Pass |
| Device/DeviceBreak_00..05.png | 6 | 384x448 | Pass |

All20 files reopened from project: correct names/counts, PNG RGBA, requested dimensions, alpha only0/255, transparentRGB0, distinct pixel hashes within each series. ApprovedPreview_BreakWall.png and existing .meta files hash unchanged.
- Wall crack changed-pixel counts:[10369, 13815, 51577]; every wall alpha identical, crack masks nested.
- Idle pale-mint pixel counts:[262, 711, 849, 1154, 795, 525] (rises then falls); adjacent/wrap changed-pixel counts:[644, 358, 417, 975, 770, 464]. Whole machine never shifts. Last-to-first visually compared on contact sheet; no Unity playback.
- Break pure-white pixel counts:[1535, 2837, 2023, 934, 0, 0];00–01 visibly flash,02–03 core fragments,04–05 no turquoise pixels;05 is held end sprite.
- Foot/body dimensions: Wall256x1344; device art baseline448; idle/finaldevice384px visible height(3tiles).

PPU128 is README target only; no new .meta configuration applied. PNGs live inWall/,Debris/,Device/. Full visual index:Preview_BreakWall_Full.png. No additional animation, no code-generated glow, no scene or material change.
