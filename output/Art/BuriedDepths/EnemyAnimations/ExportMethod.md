# Phương pháp xuất đã duyệt

Chủ project duyệt dùng PowerShell tách theo alpha thực tế, giữ pixel thân/chi, loại quái cạnh bên và đệm nền trong suốt.

`ExportAlpha.ps1` chạy bằng Windows PowerShell để dùng System.Drawing. Không phải script gameplay Unity.

1. Đọc RGBA thật, tìm các vùng liên thông alpha >=128.
2. Chọn 12 vùng thân lớn, kiểm tra mỗi hàng đúng 4 thân; sắp theo hàng rồi tọa độ ngang.
3. Gán mảnh rời gần thân theo khoảng cách; giữ fringe alpha gốc trong khoảng 3 pixel quanh vùng chính, loại alpha nền xa.
4. Tách theo vùng đo được, không dùng ô đều. Mỗi quái có một kích thước canvas chung; thêm 8 pixel đệm mỗi phía, giữ pixel không scale/rotate/mirror.
5. Xuất 12 frame/trạng thái, SHA256 và vùng cắt vào FrameMeasurements.csv. AuditFrames.ps1 đọc mọi pixel để đếm frame rỗng/mép alpha >=128.

Sheet gốc không bị xóa. Hai sheet cần vẽ lại để tách toàn bộ hình là EarthGolem Attack và Beetle Idle, đặt ở AttackPreview/*MarginPreview.png và dùng làm Source trong project. Prompt sinh các bộ gốc tại GenerationPrompts.md; xem RepairNotes.md cho các lượt sửa.

Không tự chọn PPU/pivot hoặc tỉ lệ world size. Canvas chung chỉ giữ bố trí frame; hình AI giữa các sheet vẫn có thể khác tỷ lệ thân/chi. Cần duyệt kích thước và đối chiếu khi tạo clip. Hash chỉ xác nhận dữ liệu khác, không chứng minh chuyển động mượt. Không tuyên bố đã Play test hoặc xem Game View.
