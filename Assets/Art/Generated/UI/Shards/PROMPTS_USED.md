# PROMPTS USED — Shards preview

Date: 2026-10-17. Built-in ImageGen. Three static samples only; full 4/6-frame loops await approval.

## Reference and preparation
- Approved Props/Light/CrystalCluster_00.png: crystalline material and five-color palette, simplified for small currency.
- Cropped real generated alpha >=128, scaled proportionally by nearest neighbor: Shard silhouette max22px (~20px per README), Cache80px, Icon52px; centered in requested frames.
- Quantized to #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 #0C0A14; binary alpha0/255. No soft lighting added.
- Native sample PNG files remain in scratch/shards_samples pending approval. Project preview includes Cast_04 at x0.5 and Earth_r0_c1 at x2.
- No scene/prefab/script/meta edits or Unity Game View validation. PPU64 for world sprites is README target; not applied to importers.

## Actual measured files

| Sample | Size | Alpha | Colors | Result |
|---|---|---|---|---|
| Shard_00 | 32x32 | 0/255 | 5 allowed | PASS |
| ShardCache_00 | 96x96 | 0/255 | 5 allowed | PASS |
| ShardIcon | 64x64 | 0/255 | 5 allowed | PASS |

## Exact prompts

### Shard_00
Use case: stylized-concept. Unity2D currency pixel-art SAMPLE Shard_00. ONE tiny sharp irregular splinter of crystal, slants upper right, asymmetrical chipped polygon silhouette with 3 clean faceted planes, one bright sharp specular pixel and a tiny four-point glint; roughly 20 native pixels tall within logical32 frame. Readable at32x32, no excess tiny texture. Reference image supplies ONLY approved crisp WHITE/BLUE crystalline material and palette; simplify to the required much smaller native pixel size, NOT a ground crystal prop. Hard opaque square pixels on actual transparent alpha background. Logical canvas 32x32. Strict palette ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 contour #0C0A14. Faceted color steps, no gradients, antialias, blur, softglow, softshadow. No green/turquoise/teal/mint #45DCC4, red, orange, yellow, brown. Complete sprite visible, no ground or backdrop. One static sample, no animation or multiple variants; no text or UI frame. Sharp broken gem, not round coin.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-d90cabfa-2a6c-4295-a028-823757009ff3.png`

### ShardCache_00
Use case: stylized-concept. Unity2D currency pixel-art SAMPLE ShardCache_00. ONE floating recoverable SHARD CACHE: a compact asymmetric cluster of many sharp crystal splinters orbiting around a luminous ANGULAR white core, a loose spiral/ring with broken negative-space gaps and unequal shard sizes, NO fabric bag, NO coin, NO round smooth orb. Core bright white, stepped pale-blue rim, surrounding crystals blue and white with dark blue crevices. One static resting pose for later6-frame loop. Silhouette about80 native pixels tall, centered in logical96 frame. Clearly a larger collection of the same tiny currency crystals. Reference image supplies ONLY approved crisp WHITE/BLUE crystalline material and palette; simplify to the required much smaller native pixel size, NOT a ground crystal prop. Hard opaque square pixels on actual transparent alpha background. Logical canvas 96x96. Strict palette ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 contour #0C0A14. Faceted color steps, no gradients, antialias, blur, softglow, softshadow. No green/turquoise/teal/mint #45DCC4, red, orange, yellow, brown. Complete sprite visible, no ground or backdrop. One static sample, no animation or multiple variants; no text or UI frame. Sharp broken gem, not round coin.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-ea8b147f-1673-43dd-ae7b-01b3fb269d1d.png`

### ShardIcon
Use case: stylized-concept. Unity2D currency pixel-art SAMPLE ShardIcon. ONE clean bold HUD icon of a sharp asymmetrical crystalline SHARD, related to single currency splinter, one large angular main crystal with one tiny secondary chip and a crisp pixel glint. Strong outline, three broad readable faceted planes, not detailed clutter, clear when shown at24x24. About52 native pixels tall centered in logical64 frame. No circular badge/coin, no letters or number. Reference image supplies ONLY approved crisp WHITE/BLUE crystalline material and palette; simplify to the required much smaller native pixel size, NOT a ground crystal prop. Hard opaque square pixels on actual transparent alpha background. Logical canvas 64x64. Strict palette ONLY #F2F4F8 #BFD8F2 #4A7FC1 #1E3557 contour #0C0A14. Faceted color steps, no gradients, antialias, blur, softglow, softshadow. No green/turquoise/teal/mint #45DCC4, red, orange, yellow, brown. Complete sprite visible, no ground or backdrop. One static sample, no animation or multiple variants; no text or UI frame. Sharp broken gem, not round coin.

Source: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-884cd2f6-e0ce-4b15-b3e5-d73f719ade85.png`
