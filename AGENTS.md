# Quy chuẩn màu sắc dành cho AI — Lữ Hành Bí Ẩn

Áp dụng cho toàn bộ project Unity 6000.5, URP 2D. AI phải đọc tài liệu này trước khi tạo hoặc sửa asset, vật liệu, ánh sáng, hiệu ứng, giao diện và thiết kế màn.

## Định hướng hình ảnh

Game dùng pixel art, không khí tím đêm, kiến trúc công nghiệp và máy móc cũ. Tím đêm là màu nền; xanh ngọc là điểm nhận diện của người chơi và những thứ có lợi. Tham chiếu không khí, phòng và camera từ Hollow Knight; hiệu ứng từ Nine Sols; di chuyển từ Celeste; chuyển màn từ Soul Knight. Tạo hình phù hợp thế giới riêng của game.

## Bảng màu chuẩn

Tỷ lệ là mục tiêu diện tích màu nhìn thấy trong một khung hình gameplay điển hình, tổng bằng 100%. Đây không phải số đo hiện trạng, tỷ lệ số lượng asset hay thông số intensity/alpha của đèn. Không ép từng sprite phải chứa đủ các nhóm màu.

| Nhóm | Tỷ lệ mục tiêu | Màu chuẩn (sRGB HEX) | Vai trò |
|---|---:|---|---|
| Nền xa | 40% | Tím đêm `#171329`, tím khói `#302345` | Bầu trời, bóng công trình, cảnh xa |
| Địa hình và kiến trúc | 30% | Xám tím `#454052`, thép tối `#292734` | Sàn, tường, platform, máy móc; mép đường đi sáng hơn nền |
| Tiền cảnh và vùng tối sâu | 15% | Đen tím `#0C0A14` | Cột, xích, mép hang, bóng khuất; ưu tiên quanh mép màn |
| Người chơi và thứ có lợi | 7% | Xanh ngọc `#45DCC4`, sáng nhạt `#B8FFF0` | Điểm nhấn nhân vật, kỹ năng, lõi năng lượng, vật phẩm, checkpoint |
| Ánh sáng an toàn | 5% | Vàng cam `#E7AA59`, lõi kem `#FFE0A3` | Đèn đường, chỗ nghỉ, điểm thu hút chú ý an toàn |
| Nguy hiểm | 3% | Đỏ cam `#EA6547`, đỏ tối `#8F393D` | Dấu báo đòn, đạn địch, bộ phận bẫy đang hoạt động |

Mã màu là màu tham chiếu khi thiết kế; ánh sáng URP, vật liệu và hậu kỳ có thể làm màu hiển thị khác. Đánh giá kết quả bằng ảnh trong Game View, không chỉ bằng màu sprite gốc.

### Tông màu theo từng màn

"Màn" là chủ đề (không phải tên scene LevelX). Mỗi màn có tông nền và ánh sáng riêng; các nhóm Người chơi, Ánh sáng an toàn, Nguy hiểm giữ nguyên ở mọi màn. Không dùng ánh sáng màu cam.

| Màn | Scene | Nền xa / ánh sáng chung | Ghi chú |
|---|---|---|---|
| 1 – Nhà máy | Level1, Level2 | Tím đêm theo bảng trên | Mặc định của prefab |
| 2 – Vùng đất (Rêu đá ẩm) | `Test_LongDat` (đang dựng) | Nền `#121A14` / `#22302A`, ánh sáng `#A9C2A8` | Override trong scene, không sửa prefab dùng chung; thông số đầy đủ ở TUTORIAL.md mục 9d |

Quái phải tách khỏi nền ở mọi màn: đèn riêng trung tính và viền đỏ tối `#8F393D` (TUTORIAL.md mục 9d).

## Quy tắc sử dụng

1. Giữ phần lớn khung cảnh tối và trầm; dùng màu sáng để hướng mắt tới thông tin gameplay.
2. Xanh ngọc dành cho người chơi và thứ có lợi. Tránh dùng cùng điểm nhấn xanh ngọc cho đạn địch hoặc vật thể gây hại. Màu này không yêu cầu tô toàn bộ nhân vật thành xanh.
3. Đèn an toàn thiên vàng; nguy hiểm thiên đỏ. Báo nguy hiểm thêm bằng hình dạng, hoạt ảnh hoặc chuyển động; không chỉ dựa vào màu.
4. Mặt đất, mép platform, gai và chỗ tiếp đất phải dễ phân biệt với nền. Không để tiền cảnh che thông tin cần cho nhảy, dash hoặc né đòn.
5. Có thể biến thiên sắc độ để tạo khối và chất liệu, nhưng phải giữ vai trò của từng nhóm. Đề xuất và xin duyệt trước khi bổ sung màu nhấn chủ đạo mới.
6. Tỷ lệ được phép thay đổi theo tình huống: nơi nghỉ tăng ánh vàng; lúc báo đòn tăng đỏ. Không cố giữ tỷ lệ tuyệt đối trong mọi frame hoặc hiệu ứng chớp sáng.
7. Khi tạo asset, xác định trước lớp hiển thị và vai trò: nền xa, kiến trúc, tiền cảnh, có lợi, an toàn hay nguy hiểm. Đối chiếu kích thước pixel, PPU, pivot, độ trong suốt và sorting với asset thật trong project; không tự đoán thông số.
8. Tên màn và chữ trong game bằng tiếng Anh. Font dùng Beholden Book trong `Assets/Art/Fonts`, không dùng font mặc định. Trao đổi và tên hiển thị cho chủ project bằng tiếng Việt.

## Quy trình bắt buộc trước khi thay đổi

- Đề xuất cụ thể làm gì, file nào, thông số nào và chờ chủ project đồng ý trước khi sửa. Việc có bảng màu này không tự cấp phép đổi màu hàng loạt asset hiện có.
- Gửi ảnh/preview để duyệt trước khi áp dụng thật vào scene hoặc prefab.
- Sửa thành phần dùng chung ở prefab. Tilemap, Room, quái và đèn riêng từng màn có thể sửa trong scene đã được duyệt. Thông số camera chỉ nằm ở `Assets/Prefabs/CameraRig.prefab`; không tạo override trong LevelEssentials.
- Không lưu scene có thay đổi chưa lưu của chủ project; không đóng hoặc xóa scene khi chưa hỏi.
- Sau mỗi thay đổi, cập nhật mục liên quan trong `TUTORIAL.md` và thêm dòng nhật ký với số tiếp theo thực tế. Cập nhật `HUONG_DAN_BUILD_LEVEL.md` khi cách dựng màn thay đổi.

## Tài liệu liên quan

- [TUTORIAL.md](TUTORIAL.md): thiết lập kỹ thuật và nhật ký thay đổi.
- [HUONG_DAN_BUILD_LEVEL.md](HUONG_DAN_BUILD_LEVEL.md): quy trình dựng màn.

Khi mô tả kết quả, phân biệt rõ màu mục tiêu, cấu hình đã áp dụng và kết quả thực sự quan sát được. Không tuyên bố đã đo tỷ lệ hoặc đã kiểm tra trong Unity nếu chưa thực hiện.

## Quy tắc dựng màn (bắt buộc)

1. **Tỷ lệ phòng khóa:** `số phòng khóa / tổng số phòng × 100 ≥ 30%`. Phòng khóa = phòng có `RoomEncounter` (cửa sập, đánh theo đợt) hoặc phòng boss. Ví dụ 5 phòng → tối thiểu 2 phòng khóa; 7 phòng → tối thiểu 3. Kiểm tra lại tỷ lệ mỗi khi thêm/bớt phòng và ghi rõ con số khi báo cáo.
2. **Platform ≠ tileset:** bệ/kệ lơ lửng đặt bằng prefab trong `Assets/Prefabs/Platform/` (khối đặc) hoặc `Assets/Prefabs/Platform/OneWayPlatform` (một chiều). Tilemap chỉ dùng cho nền, tường, trần.
3. **NGHIÊM CẤM TỰ ĐOÁN — bằng mọi giá.** Khi thiếu thông tin về ý đồ thiết kế (chủ đề màn, tên màn, số phòng, cơ chế, vị trí/loại quái, độ khó, thông số), AI **phải dừng lại và hỏi** chủ project, không được tự chọn rồi làm. Không được coi im lặng, câu trả lời mơ hồ hay một quyết định ở màn khác là sự đồng ý cho màn này. Chỉ được làm theo những gì chủ project đã nói rõ; mọi đề xuất phải được trình bày dưới dạng câu hỏi/phương án và chờ duyệt.
