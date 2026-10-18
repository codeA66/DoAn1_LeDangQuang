# Rootwraith — Vẽ và ghép hiệu ứng skill (giao cho AI)

Đọc `AGENTS.md` ở gốc project trước khi làm. Mọi chữ trong game bằng tiếng Anh, trao đổi với chủ project bằng tiếng Việt. Không tự đoán ý đồ thiết kế: thiếu thông tin thì hỏi.

## Bối cảnh

Boss màn 2 "Rootwraith" (Unity 6000.5, URP 2D) đã chạy được. Hiện mọi hiệu ứng skill được dựng bằng code/shader (hình tròn, vệt sáng, particle) nên trông chung chung. Việc cần làm: **vẽ tay hiệu ứng theo từng frame** rồi **ghép vào thay cho hình tạm**, giữ lại particle/rung/ánh sáng làm lớp phụ.

- Boss: `Assets/Prefabs/Enemies/Man2/Rootwraith.prefab`, art `Assets/Art/Generated/Enemies/Rootwraith/HandDrawn_v2/` (vẽ tay, mịn, Bilinear).
- Code boss: `Assets/Scripts/Enemies/RootwraithController.cs` (tính sát thương, vị trí đòn).
- Code hiệu ứng: `Assets/Scripts/Effects/RootwraithFx.cs` (chỉ hiển thị). Đây là nơi ghép ảnh.
- Shader tạm: `Assets/Shaders/RootwraithEnergy.shader`.
- Thông số đòn (kích thước, thời gian): `Assets/Settings/Enemies/Rootwraith.asset`.
- Test: scene `Assets/Scenes/Test/Test_LongDat.unity`, đi tới cuối màn bên phải (đấu trường x 100–131, sàn y = 0).

## Phong cách

- Vẽ tay, nét mềm giống art boss hiện tại (không phải pixel art cứng). Viền rõ, mảng khối dễ đọc, kiểu Hollow Knight / Nine Sols. Không chép hiệu ứng có bản quyền.
- Nền PNG trong suốt thật (RGBA).
- Màu (nhóm "nguy hiểm" trong AGENTS.md): lõi kem `#FFE0C7`, đỏ cam `#EA6547`, đỏ tối `#8F393D`, khói đen tím `#0C0A14` / `#292734`. **Không** dùng xanh ngọc `#45DCC4` (màu của người chơi).
- Đấu trường lúc đánh boss có nền đỏ tối `#2A0E14`: hiệu ứng phải nổi rõ trên nền này (lõi sáng, viền tối).

## Danh sách hiệu ứng

Mật độ điểm ảnh: **PPU 80.38835** (giống boss và Player). 1 unit = 80 px. Kích thước dưới đây là khung mỗi frame.

| # | Hiệu ứng | Khung (px) | Frame | Lặp | Pivot | Kích thước trong game |
|---|---|---|---|---|---|---|
| 1 | Đạn (Orb) | 96×96 | 4–6 | có | giữa | đường kính hình ~1 unit (82 px). Bay ngang, có thể có đuôi lửa ngắn phía sau (vẽ hướng sang phải) |
| 2 | Đạn vỡ (OrbPop) | 128×128 | 5 | không | giữa | |
| 3 | Gai rễ trồi (Spike) | 96×192 | 6 (trồi → giữ → rút) | không | **giữa đáy** | rộng ~0,8 unit, cao ~2,2 unit. Gai rễ nhọn, thân tối, mép đỏ cam |
| 4 | Vệt cảnh báo dưới gai (SpikeWarn) | 128×32 | 3 | có | giữa đáy | rộng ~1,3 unit, nằm sát sàn, nhấp nháy đỏ |
| 5 | Khói dịch chuyển (Teleport) | 256×256 | 6–8 | không | giữa | ~3 unit, khói đen tím có tàn đỏ, bung ra rồi tan |
| 6 | Vệt lao (DiveTrail) | 384×128 | 4 | có | **giữa mép phải** | dài ~4,5 unit, đầu sáng bên phải, đuôi tan bên trái (code tự xoay theo hướng lao) |
| 7 | Va chạm sàn sau lao (DiveImpact) | 256×128 | 5–6 | không | giữa đáy | ~3 unit ngang, bụi + tia lửa bung hai bên |
| 8 | Sóng xung kích (Shockwave) | 256×256 | 5 | không | giữa | vòng tròn loang, có thể dùng chung cho chuyển giai đoạn và màn xuất hiện |
| 9 | Mắt lóe báo đòn (EyeFlare) | 64×64 | 3 | có | giữa | chớp sáng đỏ-kem, hình sao/tia nhọn |

Xuất từng PNG: `Assets/Art/Generated/Enemies/Rootwraith/Fx/Frames/<TenHieuUng>/<TenHieuUng>_00.png`, `_01.png`…
Import: Sprite (Single), PPU 80.38835, Bilinear, không mipmap, pivot như bảng.

Làm **1 hiệu ứng mẫu (Đạn) trước**, gửi ảnh preview cho chủ project duyệt rồi mới làm tiếp (quy trình trong AGENTS.md).

## Cách ghép vào game

Hiện `RootwraithFx` có 2 ô ảnh tĩnh `orbSprite`, `spikeSprite` (trống thì dùng hình tạm). Cần mở rộng cho ảnh có nhiều frame:

1. Thêm vào `RootwraithFx` các mảng `Sprite[]` cho từng hiệu ứng (vd `orbFrames`, `spikeFrames`, `smokeFrames`, `trailFrames`, `impactFrames`, `ringFrames`, `eyeFrames`, `warnFrames`, `popFrames`) và fps tương ứng. Mảng trống thì giữ cách vẽ hiện tại, để không hỏng khi thiếu ảnh.
2. Chỗ cần thay (đều trong `RootwraithFx.cs`, trừ khi ghi khác):
   - Đạn: `RootwraithController.SpawnOrb` / `TickOrbs` (đổi frame theo `o.age`). Đạn vỡ: `RootwraithFx.OrbPop`.
   - Gai + vệt cảnh báo: `RootwraithController.SpawnSpike` / `TickSpikes`. Lưu ý code đang co giãn gai theo trục Y (`spikeHeight * h`) để làm hiệu ứng trồi; với ảnh có frame trồi/rút thì nên giữ scale cố định và chọn frame theo thời gian thay vì co giãn. Hitbox gai rộng `spikeWidth * 0.7`, cao `spikeHeight`: hình vẽ phải khớp hitbox.
   - Khói dịch chuyển: `RootwraithFx.Smoke`.
   - Vệt lao: `RootwraithFx.Trail`.
   - Va chạm sàn: `RootwraithFx.Impact`.
   - Sóng xung kích: `RootwraithFx.Ring` (đang co giãn một sprite theo thời gian).
   - Mắt lóe: `RootwraithFx.EyeFlare`.
3. Dùng pool (tạo sẵn vài SpriteRenderer rồi tái sử dụng), không Instantiate/Destroy mỗi lần. Đặt các object hiệu ứng ngoài boss (boss lật `localScale.x` và bị tắt khi chết), như `RootwraithFx_World` hiện có.
4. Ảnh có quầng sáng có thể dùng material cộng sáng có sẵn `Fx/M_RootwraithAdditive.mat` (shader `LuHanh/SpriteAdditive`); khói tối dùng `Sprite-Unlit-Default`. Sorting: layer Default, order quanh 20 (boss là 10).
5. Giữ nguyên particle, rung màn hình, khựng hình hiện có, chỉ thay phần hình chính.
6. **Không đổi** logic sát thương, thời gian báo đòn, thông số trong `Rootwraith.asset` nếu chủ project không yêu cầu.

## Kiểm tra trước khi báo xong

- Unity biên dịch không lỗi (Console sạch).
- Mở `Test_LongDat`, Play, vào đấu trường, xem đủ các đòn và màn xuất hiện. Chụp ảnh Game View gửi chủ project; không tuyên bố đã kiểm tra nếu chưa thực sự Play.
- Không lưu scene có thay đổi chưa lưu của chủ project.
- Cập nhật `TUTORIAL.md`: thêm dòng nhật ký với số tiếp theo (hiện mới nhất là 230).
- Xóa file tạm / ảnh nháp không dùng.
- Hướng dẫn chủ project commit theo từng phần nhỏ (máy dùng Windows PowerShell 5.1: nối lệnh bằng `;`, không dùng `&&`).
