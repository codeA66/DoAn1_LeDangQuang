# Tiền Shards — art

## Cơ chế

- Quái chết bung ra vài **mảnh Shard** nảy ra rồi bay hút vào người chơi (mỗi mảnh = 1 Shard). Mỗi con quái chỉ rơi 1 lần.
- Người chơi chết: toàn bộ Shards rơi lại thành **một túi Shards** lơ lửng nhấp nhô ở chỗ đứng an toàn gần nhất; chạm vào để lấy lại (kiểu Hollow Knight).
- Bộ đếm ở **góc trên bên phải** màn hình: icon + số.
- Sau này dùng Shards để nâng kỹ năng.

## Phong cách

- Pixel art sắc nét, viền tối `#0C0A14`, nền **trong suốt**, không blur / glow mềm.
- **Trắng + xanh dương** (AGENTS.md quy tắc 9 — không dùng chung màu đất / hang): trắng ngà `#F2F4F8`, xanh dương nhạt `#BFD8F2`, xanh dương `#4A7FC1`, bóng `#1E3557`. Không cam/vàng, không đỏ, không xanh ngọc `#45DCC4` (năng lượng của người chơi đã dùng xanh ngọc — Shards phải khác hẳn để không lẫn với hạt năng lượng).
- Tạo hình: **mảnh tinh thể sắc cạnh** (không phải đồng xu tròn), lấp lánh.

## Danh sách

| File | Khung (px) | Frame | Mô tả |
|---|---|---|---|
| `Shard_00..03.png` | 32×32 | 4 (lặp) | Mảnh tinh thể nhỏ xoay / lấp lánh khi bay; hình chiếm ~20 px |
| `ShardCache_00..05.png` | 96×96 | 6 (lặp) | Túi Shards rơi lúc chết: cụm nhiều mảnh tinh thể quấn quanh một lõi sáng, xoay nhẹ, dễ thấy từ xa |
| `ShardIcon.png` | 64×64 | 1 | Icon cho bộ đếm HUD (góc trên phải), đọc rõ ở cỡ nhỏ |

Mật độ: mảnh Shard và túi dùng trong thế giới (Unity đặt PPU 64 → Shard ≈ 0,5 ô, túi ≈ 1,5 ô); icon dùng cho UI.

## Quy trình

1. Vẽ trước `Preview_Shards.png`: Shard_00, ShardCache_00, ShardIcon đặt cạnh nhân vật `Generated/Player/NewSkills/Frames/Cast/Cast_04.png` (thu về ×0,5) và 1 ô đất `Environment/BuriedDepths/Tiles/Tiles32/Earth_r0_c1.png` (×2) để so tỉ lệ và độ nổi. Gửi chủ project duyệt, **chưa duyệt thì không vẽ tiếp**.
2. Duyệt xong mới vẽ đủ, ghi prompt vào `PROMPTS_USED.md`.
3. Tự kiểm tra trên file thật: đủ frame, đúng khung, nền trong suốt, không màu cấm; báo bảng đạt / chưa.
4. Không sửa scene, prefab, script, file `.meta` hay art có sẵn.
