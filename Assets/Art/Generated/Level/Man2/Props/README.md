# Đạo cụ trang trí + nguồn sáng — màn 2 (vùng đất, Level3)

## Mục đích

Level3 đang trống, phòng nhìn như hộp. Cần một bộ đạo cụ **chỉ để trang trí** (không va chạm) và vài **nguồn sáng** đặt rải trong phòng để có không khí hang đất có di tích công nghiệp bị chôn.

## Phong cách

- Cùng bộ `Assets/Art/Environment/BuriedDepths/` (đọc `ASSET_GUIDE.md`): rễ, đá tối, đất, kim loại gỉ của công nghiệp bị chôn. Pixel art sắc nét, viền tối `#0C0A14`, không blur, nền **trong suốt**.
- **Không dùng chung tông màu với tilemap đất / hang** (chủ project yêu cầu): đạo cụ và đèn phải nổi hẳn khỏi nền đất nâu cam và ánh sáng xanh rêu của hang.
- **Tông màu chủ đạo trắng + xanh dương, đồng bộ với thang máy** (`Generated/Level/Man2/Elevator`): trắng ngà `#F2F4F8`, xanh dương nhạt `#BFD8F2`, xanh dương `#4A7FC1`, bóng `#1E3557`, viền `#0C0A14`. Phần đá / rễ / gỉ của đạo cụ có thể giữ chất liệu vùng đất nhưng **chi tiết nhấn (đai, khung, đèn, tinh thể) phải trắng-xanh dương** để tách khỏi đất.
- **Nguồn sáng**: lõi trắng `#F2F4F8`, quầng xanh dương nhạt `#BFD8F2`. **Không cam / vàng phát sáng**, không đỏ (đỏ = nguy hiểm), không xanh ngọc `#45DCC4` (dành cho người chơi), không xanh rêu (trùng ánh sáng hang).
- Đạo cụ không lấn mép sàn / bệ, không che khe nhảy (AGENTS.md quy tắc 4).
- Mật độ: **128 px = 1 ô** (Unity PPU 128, giống tường sập và thang máy). Nhân vật cao ≈ 218 px.

## Danh sách

| File | Khung (px) | Frame | Mô tả |
|---|---|---|---|
| `Light/GlowMushroom_00..03.png` | 128×128 | 4 (lặp) | Cụm nấm phát sáng mọc chân tường / mép sàn, mũ nấm sáng trắng lõi xanh dương nhạt, nhấp nháy rất nhẹ. Đáy phẳng |
| `Light/HangingLamp_00..03.png` | 128×320 | 4 (lặp) | Đèn mỏ cũ treo bằng xích/rễ từ trần, khung đèn trắng-xanh dương, bóng đèn trắng lõi xanh dương nhạt, đung đưa rất nhẹ. Đỉnh xích chạm cạnh trên khung |
| `Light/CrystalCluster_00.png` | 192×192 | 1 | Cụm tinh thể trắng - xanh dương mọc từ đá (không phải xanh ngọc), đáy phẳng |
| `Deco/RootHang_S.png` / `_M.png` / `_L.png` | 128×256 / 128×448 / 192×640 | 1 mỗi cái | Rễ cây buông từ trần, 3 độ dài (≈2 / 3,5 / 5 ô), đầu trên chạm cạnh trên khung |
| `Deco/RootWall_00..01.png` | 256×256 | 2 biến thể | Rễ bò trên vách tường (đặt sát tường, mép trái là phần bám tường) |
| `Deco/Chain_Hang.png` | 64×512 | 1 | Xích gỉ treo từ trần (≈4 ô) |
| `Deco/Pipe_Broken_00..01.png` | 384×192 | 2 biến thể | Ống kim loại gỉ gãy, nửa chìm trong đất, đặt ở chân tường / sàn |
| `Deco/Support_Beam.png` | 192×512 | 1 | Thanh chống hầm gỗ + đai sắt gỉ đứng (≈4 ô), đặt sát vách |
| `Deco/Minecart_Wrecked.png` | 320×192 | 1 | Xe goòng gỉ lật nghiêng, một phần chôn đất |
| `Deco/Stalactite_00..02.png` | 64×128 / 96×192 / 128×256 | 3 cỡ | Nhũ đá nhỏ trên trần (chỉ trang trí, KHÔNG nhọn như gai nguy hiểm, bo tròn đầu) |
| `Deco/Rubble_00..02.png` | 192×96 | 3 biến thể | Đống đá vụn + rêu ở chân tường, đáy phẳng |
| `Deco/MossTuft_00..03.png` | 128×64 | 4 biến thể | Túm rêu/cỏ nhỏ mọc mép sàn (thấp, không che chân nhân vật) |

## Quy trình

1. Vẽ trước `Preview_Props.png`: mỗi món 1 mẫu, sắp trong một **khung phòng giả** (sàn + tường + trần bằng `Environment/BuriedDepths/Tiles/Tiles32/Earth_*.png` phóng ×4) kèm nhân vật `Generated/Player/NewSkills/Frames/Cast/Cast_04.png` phóng ×1,6 để thấy tỉ lệ và độ tương phản với đường đi. Gửi chủ project duyệt, **chưa duyệt thì không vẽ tiếp**.
2. Duyệt xong mới vẽ đủ. Ghi prompt vào `PROMPTS_USED.md`.
3. Tự kiểm tra trên file thật: đủ frame, đúng khung, nền trong suốt, không dùng màu cấm; báo bảng đạt / chưa.
4. Không sửa scene, prefab, script, file `.meta` hay art có sẵn.
