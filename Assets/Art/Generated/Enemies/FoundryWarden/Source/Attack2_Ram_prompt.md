# Attack2 Ram — nguồn và prompt

Tool: built-in image_gen, transparent_background=true.

Tham chiếu: Source/Attack2_Sweep.png. Bản chọn lưu Source/Attack2_Ram.png; sprite nhập Unity trong FramesRam/. Preview: warden-ram-vent-preview.png trong thư mục visualizations của task.

## Prompt tạo chuyển động

Edit target: reference industrial boss sprite sheet. Replace ONLY its attack choreography with a horizontal body charge animation; preserve exact boss identity: orange welding mask, huge orange piston gauntlets, heavy boots, muted blue torn coat and burgundy/purple clothing, pixel art outlines. Deliver exactly TEN separate full-body frames on TRUE transparent alpha, FIVE columns TWO rows, large clear transparent gaps, no labels, no floor, no particles, no glow or backdrop. All frames face LEFT, consistent camera and size; feet stay at baseline. Frame 0 neutral, 1 lowers torso, 2 plants rear boot, 3 coils body backward, 4 deep windup; 5 strong forward shoulder/gauntlet charge with BOTH boots visible, 6 and 7 two running charge stride poses, 8 planted front boot braking, 9 upright recovery. Actual body moves forward as whole, not swinging a fist horizontally. Maintain same handedness and mask in all ten frames, arm and shoulder forward shielding head, readable silhouette, no spin, no cropped limbs. Intended Unity industrial night-purple 2D boss, same apparent mass and proportions as reference. Separate every character with transparent whitespace so components can be cut. No extra characters.

## Prompt chỉnh nền bản chọn

Precisely extract this existing ten-frame boss sheet onto genuine transparent background. Remove all brown/orange/black diffuse backdrop haze between and around characters. Preserve exactly these TEN characters, their poses, positions, sizes, pixel outlines and all details, five columns two rows. No redraw, no new motion, no text. Transparency starts immediately outside each character silhouette, including spaces between legs and arms. No soft shadows, no glow, no backdrop gradients. Only boss sprite pixels remain; keep the muted blue coats and orange gauntlets unchanged. Ensure each body is fully isolated with transparent gaps.

Unity xác định 10 vùng thân liên thông alpha ≥128; mỗi sprite chứa vùng thân tương ứng, padding 8 pixel. Đây là frame được trích từ atlas; ảnh nguồn imagegen giữ nguyên trong Source. PPU và hệ số ngang được tính từ vùng alpha frame 00 so với collider boss thật; không lấy số ô lưới để cắt mù.
