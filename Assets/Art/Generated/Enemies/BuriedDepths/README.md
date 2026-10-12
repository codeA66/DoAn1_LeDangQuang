# Bộ quái vùng đất — BuriedDepths

## Phạm vi và trạng thái
Bộ art của năm quái cùng concept đất/rễ nâu cam, giáp đá tối và dấu nguy hiểm đỏ. Được tạo bằng built-in imagegen theo tạo hình đã duyệt. Đây là **ảnh hoạt ảnh**, chưa phải enemy có cơ chế hoạt động. Không lấy thông số chiến đấu từ Hollow Knight.

Mỗi bộ có 12 frame, thứ tự 00–11. Mỗi quái có Idle, Move, Attack, Hurt, Die; giun có thêm Emerge và Burrow. Tổng thiết kế: 27 bộ / 324 frame.

## Cấu trúc
- Worm/: giun đá.
- Beetle/: bọ giáp đá.
- RootCrawler/: sinh vật rễ.
- EarthGolem/: golem đất.
- Scorpion/: bọ cạp đất.
- Mỗi thư mục có Source/ chứa sheet nguồn và Frames/<State>/ chứa PNG tách theo thứ tự.
- Bộ quái tách khỏi Assets/Art/Environment/BuriedDepths (art dựng cảnh).
- Prompt, số liệu cắt, hash và bản xem nằm tại output/Art/BuriedDepths/EnemyAnimations/.

## Cách tấn công đã duyệt

| Quái | Đòn đánh | Dấu hiệu chuẩn bị | Ra đòn và hồi lại |
|---|---|---|---|
| Giun đá | Ngoạm từ trên xuống | Ngửa đầu cao, uốn cổ ra sau, há hàm | Cổ quật xuống phía trước, hàm khép sát đất; rút đầu rồi dựng lại |
| Bọ giáp đá | Húc bằng đầu | Thu chân trước, hạ đầu, co chân sau lấy đà | Duỗi chân sau, lao thân và đầu về trước; chống chân phanh rồi đứng lại |
| Sinh vật rễ | Rễ xuyên đất | Ghim chân sau, nâng phần thân trước, giơ nhánh rễ | Đâm rễ xuống đất, truyền xung dưới đất đến vị trí Player đã chốt, rễ trồi lên; rút rễ và hạ thân |
| Golem đất | Đập hai tay | Chùng gối, nâng hai nắm đấm qua đầu | Vung cả hai tay xuống chạm đất, cúi thân theo lực; nâng tay rồi trở lại tư thế chờ |
| Bọ cạp đất | Chọc đuôi | Chống càng, cuộn đuôi ra sau rồi nâng cao | Duỗi đuôi chọc mạnh về trước; rút ngòi rồi cuộn về tư thế thủ |

Attack là chuỗi **chuẩn bị → ra đòn → hồi**, không phải Idle có đổi độ sáng. Bộ Attack đã được vẽ lại sau phản hồi rằng bản đầu chưa đọc rõ đòn.

### Lưu ý riêng sinh vật rễ
Sheet Attack chỉ biểu diễn thân quái và rễ ngay cạnh nó. Sóng truyền dưới đất, rễ trồi ở vị trí Player và VFX đánh trúng là asset/cơ chế riêng cần thực hiện sau. Không dùng hình rễ trong sheet để tự suy ra hitbox.

## Thông số chưa chốt
Chưa có HP, sát thương, tầm phát hiện, tốc độ, thời gian báo đòn/hồi, hitbox, quy tắc ngắt đòn, kích thước quái so với Player, PPU/pivot cho quái hoặc FPS gameplay. **Phải hỏi chủ project trước khi code.** PPU 32 trước đó chỉ được chốt cho tileset, không tự áp dụng cho quái.

## Hướng dẫn sử dụng và giới hạn
1. Giữ Source nguyên bản để có thể cắt/chỉnh lại.
2. Chủ project đã duyệt tách theo alpha bằng PowerShell: đo 12 thân chính, sắp xếp trái sang phải/trên xuống dưới; giữ pixel thân/chi và fringe tối đa 3 pixel quanh vùng alpha >=128, tách hình cạnh bên. Các mảnh rời gần thân được gán theo khoảng cách. Không còn cắt ô đều 4×3. Sheet nguồn giữ nguyên để đối chiếu.
3. Kích thước ảnh thật, vùng cắt, vị trí đặt trên canvas và hash ghi trong FrameMeasurements.csv; mỗi quái dùng cùng một canvas cho tất cả trạng thái, đệm trong suốt tối thiểu 8 pixel, đặt giữa ngang và sát đáy phần đệm. Đây là bố trí ảnh xuất, chưa phải pivot/FPS/PPU gameplay.
4. Hash khác nhau chứng minh không có PNG trùng nguyên dữ liệu; không chứng minh độ mượt, sự nhất quán giải phẫu hoặc chỉ khác vị trí.
5. Bản cắt ô đều ban đầu có 294/324 frame chạm mép với alpha >=128. Đã thay bằng tách alpha theo duyệt; có 324 frame, 12 hash khác nhau mỗi bộ, không có thân chạm biên sheet nguồn sau khi sửa Beetle Idle và EarthGolem Attack. Số liệu cuối trong FrameMeasurements.csv và FrameAudit.csv. Canvas chung không tự giải quyết chênh tỉ lệ thân/chi giữa các sheet AI; cần đối chiếu khi dựng clip. Chưa kiểm tra trong Game View, chưa tạo AnimationClip/Animator, importer, prefab, collider hoặc script chiến đấu.
6. Preview FPS chỉ là tốc độ xem thử, không phải thông số đã duyệt cho game.

