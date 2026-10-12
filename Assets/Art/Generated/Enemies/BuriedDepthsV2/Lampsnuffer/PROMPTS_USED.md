# PROMPTS_USED — Lampsnuffer

## Dive — 8 frame

Công cụ: ImageGen tích hợp. Tạo 1 sheet ngang rồi cắt thành 8 PNG.

Ảnh tham chiếu khi tạo: `D:\DoAn1_LeDangQuang\Assets\Art\Generated\Enemies\BuriedDepthsV2\Lampsnuffer\Frames\Fly\Fly_00.png` (mẫu đã duyệt trước khi xuất animation). Bản lưu nguyên mẫu: `C:\Users\DANG QUANG\.codex\visualizations\2026\10\10\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\Approved\Lampsnuffer.png`.

Prompt nguyên văn:

```text
Reference attached approved sprite: preserve EXACT character anatomy and appearance. Detailed handcrafted pixel art, side view facing RIGHT. Near black purple body #0C0A14 to #292734, subtle details, thin lavender rim #D9D4FF top/back only. ONLY warm color red-orange #EA6547 core/eyes with darkred #8F393D edge, soft glow. NO teal, blue fringe, magenta fringe, text, ground, scene, shadow, checkerboard. Transparent alpha. One horizontal sheet, exactly 8 DISTINCT sequential poses in ONE row. Each entire pose MUST fit only 55% of its slot width, leave generous 22.5% clear empty gutters on BOTH SIDES. No overlapping neighbors, no connecting effects. All poses same body size, stable body root at same horizontal point inside each slot. Fixed common foot baseline for grounded poses; intended jump and fall motion allowed. Never randomly rescale. Full character and debris visible. Animation Lampsnuffer Dive NONLOOP: eight distinct sequential moth poses. 0-2 pause midair, wings gradually close, ABDOMEN RED CORE visibly gets progressively BRIGHTER as telegraph. 3-5 dive diagonally DOWN and RIGHT, wings pressed flush to body, ash trail behind up-left. 6-7 flare wings to brake, recover to SAME horizontal spread-wing flying posture as attached approved Fly_00. Keep torso size and same canvas root/baseline throughout, draw action pose in place with diagonal angle, no camera motion. Proboscis curled, dark moth anatomy exactly reference.
```

Sheet: `C:\Users\DANG QUANG\.codex\generated_images\01a1243a-44c0-7aa0-b8a8-efdabc3667e9\exec-5cd7e24f-9116-473f-974b-ce86519f35a9.png`

## Kết quả đo trên PNG đã đặt trong project

- 8 frame khác nhau, 512×512, PNG RGBA; biên ngoài alpha = 0.
- Cạnh dưới vùng ảnh: y=504 cho cả 8 frame (chừa 8 px).
- Pixel gần #45DCC4 (sai số mỗi kênh <24): 0.
- Tổng năng lượng đỏ từng frame (sum(max(0,R-max(G,B))*alpha/255)): 222423, 336978, 339619, 270710, 249927, 186582, 178900, 224730.
- Căn bằng một hệ số tỷ lệ chung cho cả sheet; không đổi kích thước riêng từng frame.
- Chưa kiểm tra playback trong Unity.
