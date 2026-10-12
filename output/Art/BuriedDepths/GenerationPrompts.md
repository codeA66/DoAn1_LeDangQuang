# Bộ prompt — vùng đất (bản vẽ duyệt)

Công cụ: built-in imagegen. Ảnh tham chiếu: concept đã được chủ project duyệt. Tên BuriedDepths là tên thư mục làm việc, chưa phải tên màn đã chốt.

## Foreground bản hạ sáng cuối

Use case: precise-object-edit. Edit this SAME SIX-object foreground sheet. Preserve exact six silhouettes, object layout, all roots, stones, broken mine support, pixel art material detail, and true transparent background. Change ONLY lighting/value hierarchy to make these proper NEAR foreground silhouettes matching the approved underground concept: dominant deep black-brown/purple shadow, very faint warm earth/ochre edge accents and faint stone relief. Reduce overall brightness substantially, preserving shape read and rich texture within shadows. No bright orange surfaces, no lamps, no glow halos. Do NOT flatten into completely solid black icons; retain subtle readable root grooves and stone cracks. No extra objects, no text, no scenery. Foreground sits at screen edges in front of warm-brown terrain and distant purple background. True alpha outside the six objects, remove any surrounding halo.

## Hướng chung cuối cùng

Use case: stylized-concept. Input image 1 is the APPROVED art direction and COLOR reference, not an edit target. Extract the requested ORIGINAL side-scrolling game art, tightly matching its warm earthy colors and pixel art aesthetic. Crucial: the reference has rich warm brown/ochre earth and roots, amber-lit warm rock faces, deep charcoal/purple shadows, distant violet fog. Match these EXACT visual roles: warm brown-orange earth and roots in nearer layers; purple ONLY distant mist and deep shadow. Do NOT paint all terrain/root bodies purple. This is an underground EARTH biome with a few buried old industrial mine remnants. Crisp deliberate pixel clusters, original silhouettes. Hostile accents dark red and ember orange; no turquoise on enemies or spikes. No text, watermark, UI, labels, checkerboard. All cutouts genuine alpha outside objects. Orthographic side view, no 3D render. Follow reference material/color/style; produce only the specifically requested deliverable, without copying complete reference scene or other objects.
Latest user direction: add rich coherent material detail matching approved concept: layered shale fractures, soil strata, rootlets, chipped rust and rivets on industrial remnants, weathering and strong form separation. Enemy armor has readable joints and detailed material texture. Keep silhouettes crisp, avoid uniform noisy texture, preserve clear landing tops and danger points. This is a visually rich polished production asset, not a simplified icon.

## BG_Far

Asset: one wide landscape distant background ONLY. Full-bleed night-violet cavern depths, receding huge cavern arches and indistinct buried mine silhouettes behind layered smoky purple haze. Low contrast, no walkable floor, no near foreground, no enemies, no platforms. Fill entire rectangular canvas with distant background. Broad atmosphere and readable negative space, no bright hotspots, no lamps. Intended farthest parallax layer.

## BG_Mid

Asset: one wide transparent middle-distance background layer ONLY. Soft muted purple-gray stratified earth walls around three large irregular tunnel arches, immense receding roots, a few buried ruined factory pipes/mine supports. Holes through arches and outside silhouettes genuinely transparent so far background shows. Contrast below gameplay terrain. No floor strip, no standalone floating platforms, no enemies. Flat side view, landscape composition.

## BG_Near

Asset: one wide transparent nearer background decoration layer ONLY, with huge dry twisted roots draping from upper edge and side edges, broken buried pipes and old mine beams. Keep central lower-middle large open space for gameplay visibility. Muted warm brown-gray earth/stone with subdued amber glints on one old mine lantern. This is behind terrain, not foreground. No floor, no enemies, no platform, no solid backplate, true alpha.

## FG_RootsStone

Asset: transparent modular foreground prop sheet with SIX separated silhouette objects, arranged 3 columns 2 rows with generous empty margins; objects never touch: thick root curtain hanging from ceiling; twisted vertical root pillar; jagged near-rock column; low rubble/root mound; diagonal broken mine brace with root wraps; thin dangling roots. Near-black purple #0C0A14 with very restrained purple edge texture. Each object complete, no crop, no supporting floor or backdrop. These sit at screen edges, modular silhouettes not a complete scene.

## Tileset_Earth_Source

Asset: a strictly regular 8 by 6 atlas of FORTY-EIGHT square terrain tiles, no gutters, no margin, no drawn grid. All 48 cells same dimensions. Style designed for 32x32 pixel tiles, large pixel clusters not fine illustration. Earth and shale, warm brown-gray earth/stone. Include a complete connected 3x3 solid-earth patch occupying top-left 3x3 cells: four convex outer corners, top/bottom/left/right exposed edge tiles, center solid fill. Exposed boundary tiles have jagged silhouette edging limited to their exposed edge and transparent outside; other connections continuous at cell boundaries. Remaining cells include FOUR concave inner corners, alternate top floor edges, left/right walls, ceiling edges, alternate seamless opaque soil fills, standalone single-earth blocks, narrow vertical columns with end caps, root/stone decorative overlay tiles. Neutral lighting from above; exposed walkable top edges clear light warm brown-gray earth/stone, inner earth dark. No grass, no turquoise, no enemy, no spike, no floating-platform illustration, no text. Orthographic modular tile atlas, exact square-cell geometry. Full rectangular atlas, no perspective. Produce base tile pixels with crisp discrete pixel clusters.

## Platforms_Stone

Asset: THREE complete separated solid chunky rock platform sprites on transparent background, stacked in three rows with ample transparent gaps. All face orthographic side view, clear horizontal flat walkable TOP edge, substantial earth/stone depth below, never paper-thin. First platform layered shale with roots below; second cracked boulder ledge with buried pipe passing through body; third rugged earth slab with broken mine support fragments embedded below. Muted warm brown-gray earth/stone stone, restrained warm brown roots, top rim lighter, undersides dark purple. Different silhouettes matching same biome. No grass, no lamps, no crystals, no spikes, no tile atlas, no text, no ground backdrop. Each sprite is standalone intact with empty space all around.

## Spike_Ground

Asset: one complete ground spike cluster sprite on genuine transparent background, orthographic side view. Five short uneven jagged dark shale rock needles emerging from a shared small earth base, pointed UP, exposed tips sharply readable, warm brown-gray earth/stone shading and tiny dark orange-red danger fissures. Compact coherent object, not a room, no soil floor beyond its own base, no other prop, no glow halo. Crisp pixel art, visible individual dangerous points.

## Spike_Ceiling

Asset: EXACTLY ONE isolated hanging stone stalactite on transparent background, orthographic side view. Single broad crumbling top attachment tapering to ONE sharp downward point, heavy layered shale/earth rock, small crack near top where it could break free. Gray-purple body, light restrained edge for readability, tiny dark reddish danger fissure. Complete standalone cutout, not a cluster; no ceiling strip, no extra loose spikes or floating debris, no motion effect, no backdrop. Intended later to shake then fall as gameplay, but draw only the intact resting asset now.

## Enemy_Worm

Asset: ONE full original earthworm enemy character design on transparent background, static concept pose, no animation sequence. Giant thick segmented shale-armored worm curving up in a side-view biting posture; entire visible body contained in canvas including tapered tail, no soil hiding silhouette. Concentric angular stone-tooth jaw with red-orange mouth interior; plates charcoal warm brown-gray earth/stone, dry root-like seams, dense muscular silhouette. Clearly earth creature not robot. No glowing turquoise, no attached scenery, no ground, no flames, no extra characters. Design consistent with buried depths biome; powerful memorable mouth and readable segments.

## Enemy_Beetle

Asset: ONE full original ground-dwelling burrowing beetle enemy character design on transparent background, static orthographic side view. Broad low oval layered slate-warm brown-gray earth/stone rocky carapace, short strong digging forelegs, small blunt shovel-like head, six articulated legs, tiny dark ember-red eyes. NO long scorpion tail and no worm mouth, distinct insect silhouette. Earth-dusted armored beetle with ancient root cracks, compact powerful body. No scene, no floor, no other character, no animation sheet, no turquoise.

## Enemy_RootCrawler

Asset: ONE full original crawling root creature enemy character design on transparent background, static orthographic side view. Low long tangled dry-root body with four hooked root limbs, knotted wooden head and a single ominous dark-red fissure in face, shale stones embedded on back. Distinct twisted organic silhouette, no beetle shell, no wings, no scorpion tail. Muted purple-brown wood and dark warm brown-gray earth/stone rocks, red restrained, no glowing green. Entire creature visible isolated, no ground, no extra props, no animation sheet.

## Enemy_EarthGolem

Asset: ONE full original earth golem enemy character design on transparent background, static orthographic SIDE VIEW with face pointed sideways. Stocky upright creature made of compacted soil and angular shale blocks, very heavy oversized forearms, short broad legs, small recessed head, one dark-red glowing fractured core inset in chest. Asymmetric rocky silhouette with dry root strands binding joints. Distinct from any industrial furnace boss: no machine parts, no metal armor, no factory body. Gray-purple earthy palette, restrained danger red, no turquoise. Full body isolated, no scenery, no floor, no other character, no animation sheet.

## Enemy_Scorpion

Asset: ONE full original ground scorpion enemy character design on transparent background, static orthographic side view. Low charcoal warm brown-gray earth/stone segmented slate armor, large rocky front pincers, multiple short walking legs, sharply readable arched segmented tail ending in dark-red stone sting. Burrowing earth creature dusted with soil and sparse dry root seams; muted palette except localized danger red at sting/eyes. Full creature and tail contained in frame, no wings, no backdrop, no ground, no other characters, no animation sheet. Distinct from beetle with elongated body, pincers and arched tail.

## Terrain_16_Source (bản terrain bổ sung)

Use case: stylized-concept. Input image 1 is the APPROVED art direction and COLOR reference, not an edit target. Extract the requested ORIGINAL side-scrolling game art, tightly matching its warm earthy colors and pixel art aesthetic. Crucial: the reference has rich warm brown/ochre earth and roots, amber-lit warm rock faces, deep charcoal/purple shadows, distant violet fog. Match these EXACT visual roles: warm brown-orange earth and roots in nearer layers; purple ONLY distant mist and deep shadow. Do NOT paint all terrain/root bodies purple. This is an underground EARTH biome with a few buried old industrial mine remnants. Crisp deliberate pixel clusters, original silhouettes. Hostile accents dark red and ember orange; no turquoise on enemies or spikes. No text, watermark, UI, labels, checkerboard. All cutouts genuine alpha outside objects. Orthographic side view, no 3D render. Follow reference material/color/style; produce only the specifically requested deliverable, without copying complete reference scene or other objects.
Latest user direction: add rich coherent material detail matching approved concept: layered shale fractures, soil strata, rootlets, chipped rust and rivets on industrial remnants, weathering and strong form separation. Enemy armor has readable joints and detailed material texture. Keep silhouettes crisp, avoid uniform noisy texture, preserve clear landing tops and danger points. This is a visually rich polished production asset, not a simplified icon.
Asset: FINAL modular TERRAIN ONLY atlas for Unity RuleTile, sixteen equal square cells in EXACTLY a 4 column x 4 row grid, no gutters, no margins, no outlines separating cells, square canvas. NOT a collection of floating platforms or props. Designed as true 32x32 pixel tile art at enlarged viewing scale.
First three columns first three rows form one CONTIGUOUS assembled 3x3 earth mass: row0 top-left exterior corner / top edge / top-right exterior corner; row1 left wall / opaque seamless solid-earth fill / right wall; row2 bottom-left exterior corner / ceiling bottom edge / bottom-right exterior corner.
Connections between tile cells within this 3x3 mass must be PERFECTLY CONTINUOUS, FULL BLEED soil and layered stone. No transparent seams between these nine cells. Outer silhouette only at outside of mass; top edge clean flatter stone ledge, not large jutting boulders. Bottom is exposed ceiling underside, no long dangling roots.
Last column cells top-to-bottom: concave corner exposing a small transparent quarter-cell at upper-left; concave exposing upper-right; concave exposing lower-left; concave exposing lower-right. These concave tiles are otherwise filled with matching earth.
Bottom row first three columns are THREE fully opaque seamless repeating earthy fill variants.
Warm brown/ochre soil, dark charcoal stone and purple-black deep shadows as approved reference, fine stratified cracks and buried small roots INSIDE soil. No metal props, no lamps, no platforms, no debris sprites, no labels, no grid, no perspective. Restrained pixel clusters suitable to an actual 32x32 tile. Critical: sixteen SQUARE equal-sized tiles on an exactly SQUARE canvas.

