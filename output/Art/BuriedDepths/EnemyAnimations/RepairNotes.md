# Ghi chú sửa xuất frame

Audit bản cắt ô đều: 324 frame không rỗng, 294 frame có alpha >=128 chạm mép. Phân tích connected components trên sheet nguồn cho thấy phần lớn có 12 thân riêng, không nằm đúng ô lưới. RootCrawler/Attack có 13 thành phần >1000 pixel; 12 thành phần thân lớn và 1 phần rễ rời. EarthGolem/Attack có các tư thế chạm nhau: không thể tách chỉ bằng connected component.

Đã dùng built-in imagegen thử sửa margin Worm/Idle và Worm/Move, lưu RepairTrial; thử nghiệm vẫn có 21/24 frame chạm mép nên không áp dụng. Sau đó thử prompt sparse thumbnail atlas; không áp dụng vào project.

Đã tạo EarthGolem_Attack_MarginPreview.png để tách các tư thế golem đang chạm nhau. Bản này giữ chuỗi 12 động tác nhưng kích thước hình trong sheet khác bản đầu; cần đối chiếu kích thước khi ghép clip, không tự coi là thông số kích thước gameplay.

Chủ project đã duyệt PowerShell tách theo alpha thực tế. Đã áp dụng vào project và xuất lại báo cáo chính; RepairTrial không được dùng. Hai bản mới áp dụng là EarthGolem_Attack_MarginPreview.png và Beetle_Idle_MarginPreview.png. Kết quả xuất 324 frame / 27 bộ, 12 hash riêng mỗi bộ, 0 thân chạm biên nguồn. Xem ExportMethod.md cho thuật toán và giới hạn. Bản nguồn trước sửa vẫn được tham chiếu trong ManifestFinal.json; ExportAlpha.ps1 ghi rõ hai override nguồn cho lần xuất cuối.
