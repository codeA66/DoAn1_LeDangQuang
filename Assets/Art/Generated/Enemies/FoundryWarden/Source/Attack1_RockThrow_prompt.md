# Nguồn asset chiêu ném đá

Tool: image_gen built-in; transparent_background=true. Tham chiếu `Attack1_Slam_SyncV2.png`. Nguồn chọn: `Attack1_RockThrow.png`, `RockProjectile.png`; sprite nhập Unity: `FramesThrow/`. Bản đập tay cũ được giữ nguyên.

## Prompt animation

Edit target industrial boss spritesheet; redraw ONLY Attack1 as overhand ROCK THROW. Preserve identical orange welding mask, massive orange piston gauntlets, blue tattered coat, purple/burgundy clothing, heavy boots, outline and pixel art style, same proportions. TEN fullbody animation frames, FIVE columns TWO rows, left-facing consistently. Strong clear transparent gaps and genuine transparent alpha immediately outside sprites, NO backdrop/haze/shadow/particles/text. Frame0 neutral holding one irregular dark violet-gray rubble rock in viewer-left gauntlet at hip; frame1 bring rock to chest; frame2 lift rock to shoulder; frame3 hold rock ABOVE head; frame4 coil backward with same rock above head, boots planted; frame5 snap same viewer-left arm UP and FORWARD to throw rock upward LEFT, hand EMPTY (flying rock is separate Unity projectile, do NOT draw detached rock in sheet); frame6 emptyhand follows high upward release; frame7 extended emptyhand finishing throw; frame8 lower arm; frame9 idle EMPTY. Viewer-right gauntlet stays low throughout, never swaps arms. Held rock has industrial chipped stone texture, muted gray-purple #454052 and #292734, tiny red-orange cracks, approximately 1/3 normal standing boss height. All boots visible, consistent baseline, no cropped limbs; 5 windup frames with rock followed by five release/recovery frames without rock. Clear intent: lob UP into the air, not downward slam or straight punch. Do not add detached objects or dust.

## Prompt sprite đá

Reference image only for style and stone design: create ONE isolated game projectile sprite matching the irregular rock held by this boss. Chipped industrial rubble boulder, gray-violet #454052, steel dark #292734, sharp black-violet contour, tiny red-orange glowing fissures #EA6547. Same crisp pixel art as reference. Single roughly round boulder centered, near circular silhouette, all edges visible, around 128-pixel sprite aesthetic with clean pixel edges. TRUE transparent background immediately outside rock. NO character, hands, gauntlets, floor, detached fragments, shadow, glow halo, haze, text or other objects. This is the rock which the boss throws in a parabolic arc; visual role danger. Keep orange fissures subtle, dominant stone gray-purple, no green or turquoise.

## Nhập Unity

Ảnh nguồn giữ nguyên. Tách mười vùng thân liên thông alpha ≥128, padding 8 pixel; pivot dựa trên hai nhóm pixel giày ở dải dưới. Điểm thả (45,46) trên frame 05 là điểm tác giả chọn, không phải collider mới. Đá trích theo vùng alpha riêng, có PPU đo từ đường kính 1,5 unit. Point filtering, không mipmap, Uncompressed. Preview render trong scene riêng; không chạy Play test.
