# Thang máy — màn 2 (Level3, phòng R16)

## Cơ chế (để vẽ đúng)

- Giếng thang máy rộng 6 ô. **Cabin** chạy dọc giếng giữa đáy (sàn R10) và lối lên R17.
- Cabin có **sàn**, **mái chặn phía trên** và **hai bên để trống** (lối bước vào từ trái/phải). **Khi thang chạy, 2 cửa song chắn hai bên đóng lại** (chặn cả 4 phía), dừng thì mở. Trong lúc chạy có quái bay tấn công (đánh nhau trên thang). Người chơi đứng trên sàn cabin, chém **cần điều khiển** gắn trong cabin để chạy.
- Ở mỗi đầu giếng có **cần gọi** đứng trên sàn: chém để gọi cabin về.
- Phần chạy lên/xuống, lật cần do code làm; art chỉ cần các hình tĩnh + vài frame đèn.

## Phong cách

- Cảm hứng cabin thang máy cổ trong Hollow Knight: khung kim loại thanh mảnh, mái vòm/mái nhọn trang trí, nhiều **họa tiết hoa văn uốn lượn, đường viền chạm khắc, tay vịn, đèn lồng nhỏ**. Tạo hình riêng của game, không chép nguyên mẫu.
- **Màu chủ đạo trắng + xanh dương** (chủ project duyệt riêng cho thang máy):
  - Trắng ngà `#F2F4F8`, xanh dương nhạt `#BFD8F2`, xanh dương `#4A7FC1`, xanh dương đậm `#1E3557` (bóng), viền tối `#0C0A14`.
  - Đèn phát sáng xanh dương nhạt `#BFD8F2` / lõi trắng. **Không dùng xanh ngọc `#45DCC4`** (dành cho người chơi), không cam/vàng phát sáng, không đỏ.
- **Không dùng chung tông màu với tilemap đất** (nâu cam `RT_Earth`): thang máy phải nổi hẳn khỏi nền đất.
- Pixel art sắc nét, viền tối, không blur, không glow mềm, nền **trong suốt hoàn toàn**.
- Mật độ: **128 px = 1 ô** (giống bộ tường sập; Unity đặt PPU 128). Nhân vật cao ≈ 1,7 ô (≈ 218 px).

## Danh sách

| File | Khung (px) | Frame | Mô tả |
|---|---|---|---|
| `Cabin/Cabin_00.png` | 720×512 | 1 | Cabin nhìn ngang: **sàn** dày ≈ 64 px ở đáy (mặt sàn phẳng, cao đúng 64 px tính từ đáy khung), **mái** trang trí ở trên (đáy mái cách mặt sàn ≥ 320 px = 2,5 ô để người chơi đứng lọt), **hai cột/thanh mảnh ở 2 mép nối sàn với mái**; giữa hai bên **để trống** để thấy lối đi vào. Có thể có tấm lưới/hoa văn mờ phía sau (lớp nền của cabin) nhưng không che lối vào |
| `Cabin/CabinLight_00..03.png` | 720×512 | 4 (lặp) | Chỉ vẽ **phần đèn sáng** (đèn lồng trên mái, viền hoa văn phát sáng) trên nền trong suốt, đặt đúng vị trí so với Cabin_00; độ sáng nhấp nháy nhẹ khi cabin đang chạy |
| `Cabin/CabinGates_00.png` | 720×512 | 1 | Chỉ vẽ **2 cửa song chắn ở hai bên** cabin (đóng lại khi thang đang chạy), đúng vị trí so với Cabin_00, cao từ mặt sàn tới đáy mái, nền trong suốt. Code bật/tắt hình này |
| `Lever/LeverBase_00.png` | 128×192 | 1 | Đế cần điều khiển (cột trụ nhỏ trắng-xanh, có đèn báo tròn ở đỉnh), đáy phẳng |
| `Lever/LeverHandle_00.png` | 64×160 | 1 | Tay gạt riêng (code xoay quanh **chân tay gạt ở giữa cạnh dưới**), đầu tay gạt có núm tròn |

- Lưu: `Assets/Art/Generated/Level/Man2/Elevator/<Thư mục>/<Tên>.png` như bảng.

## Quy trình

1. Vẽ trước `Preview_Elevator.png`: Cabin_00 (kèm CabinLight_00 chồng lên), LeverBase + LeverHandle ghép, đặt cạnh nhân vật `Generated/Player/NewSkills/Frames/Cast/Cast_04.png` phóng ×1,6 và 1 ô đất `Environment/BuriedDepths/Tiles/Tiles32/Earth_r0_c1.png` phóng ×4 để so tỉ lệ; vẽ thêm 1 hình người chơi đứng TRONG cabin để thấy độ cao mái. Gửi chủ project duyệt, **chưa duyệt thì không vẽ tiếp**.
2. Duyệt xong mới vẽ đủ. Ghi prompt vào `PROMPTS_USED.md`.
3. Tự kiểm tra trên file thật: đủ frame, đúng khung, nền trong suốt, mặt sàn cabin đúng 64 px từ đáy, khoảng đứng dưới mái ≥ 320 px; báo bảng đạt / chưa.
4. Không sửa scene, prefab, script, file `.meta` hay art có sẵn.
