# Bức tường đá sập + Thiết bị phá được — màn 2 (vùng đất, Level3)

## Cơ chế (để vẽ cho đúng)

- Ở phòng R8 có **bức tường đá dựng đứng** chặn lối đi tiếp sang phải (cao từ sàn tới trần ≈ 10,3 ô, rộng 2 ô).
- Người chơi phải đi vòng xuống R13, **phá thiết bị** (chém / bắn 3 đòn). Thiết bị vỡ → camera lia sang R8 → bức tường **rung, nứt rồi lún thẳng xuống sàn** kèm bụi và đá vụn → mở đường.
- Phần lún do code dịch chuyển sprite xuống; art chỉ cần các trạng thái tĩnh + nứt.

## Phong cách

- Cùng bộ với `Assets/Art/Environment/BuriedDepths/` (đọc `ASSET_GUIDE.md`): đất và rễ nâu cam, đá tối, nhiều vết nứt, lớp đất, rễ nhỏ, kim loại gỉ của công nghiệp bị chôn.
- **Bức tường giống phong cách platform** `Platforms/Platforms_Stone.png` (đá + rễ + đồ chống hầm), nhưng dựng đứng thành một khối cột.
- Pixel art sắc nét, có viền tối `#0C0A14`, không blur, không glow mềm, nền **trong suốt hoàn toàn**.
- **Không dùng ánh sáng màu cam / vàng phát sáng.** Thiết bị là thứ có lợi khi phá → lõi dùng xanh ngọc `#45DCC4` / sáng `#B8FFF0` (quy tắc màu AGENTS.md). Không dùng đỏ (đỏ = nguy hiểm).

## Danh sách

Mật độ chung: **128 px = 1 ô** trong game (Unity sẽ đặt PPU 128).

| File | Khung (px) | Frame | Mô tả |
|---|---|---|---|
| `Wall/Wall_00.png` | 256×1344 | 1 | Tường nguyên vẹn: cột đá xếp chồng + rễ quấn + thanh chống hầm gỉ, đáy phẳng (chạm sàn), đỉnh phẳng (chạm trần). Hai cạnh trái/phải hơi gồ ghề |
| `Wall/WallCrack_00..02.png` | 256×1344 | 3 | Cùng hình Wall_00, vết nứt tăng dần: 00 vài đường nứt nhỏ, 01 nứt lan, có mảnh bong, 02 nứt toác khắp thân, sắp vỡ. **Hình dáng ngoài giữ nguyên** từng frame (chỉ thêm nứt) |
| `Debris/Debris_00..03.png` | 64×64 | 4 | 4 mảnh đá vụn khác nhau (dùng cho particle văng ra) |
| `Device/DeviceIdle_00..05.png` | 384×448 | 6 (lặp) | Thiết bị khi chưa phá: khối máy cổ bằng đá + kim loại gỉ, rễ cắm sâu vào nền, giữa có **lõi tinh thể xanh ngọc** đập nhịp (sáng lên rồi dịu xuống). Có vài rễ / dây dẫn chạy từ thiết bị xuống đất (gợi ý nó "giữ" bức tường ở xa) |
| `Device/DeviceBreak_00..05.png` | 384×448 | 6 | Bị phá: 00–01 lõi chớp trắng, nứt; 02–03 lõi vỡ tung, mảnh văng; 04–05 vỏ máy nứt vỡ còn lại, lõi tắt tối hẳn (frame 05 giữ nguyên khi đã phá) |

- Đáy thiết bị phẳng, đứng trên bệ đá; chiều cao phần hình ≈ 2,5–3 ô (320–384 px), không vượt khung.
- Lưu: `Assets/Art/Generated/Level/Man2/BreakWall/<Thư mục>/<Tên>_00.png ...` như bảng trên.

## Quy trình

1. Vẽ trước `Preview_BreakWall.png`: Wall_00, WallCrack_02, DeviceIdle_00, DeviceBreak_05 đặt cạnh nhau, kèm 1 ô đất `Environment/BuriedDepths/Tiles/Tiles32/Earth_r0_c1.png` phóng ×4 (= 1 ô) và nhân vật `Generated/Player/NewSkills/Frames/Cast/Cast_04.png` phóng ×1,6 để so tỉ lệ. Gửi chủ project duyệt, **chưa duyệt thì không vẽ tiếp**.
2. Duyệt xong mới vẽ đủ. Ghi prompt vào `PROMPTS_USED.md` cùng thư mục.
3. Tự kiểm tra trên file thật: đủ frame, đúng khung, nền trong suốt, hình dáng tường giữ nguyên giữa các frame nứt; báo bảng đạt / chưa.
4. Không sửa scene, prefab, script, file `.meta` hay art có sẵn.
