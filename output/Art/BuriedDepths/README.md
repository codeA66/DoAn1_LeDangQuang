# Bộ art vùng đất — bản duyệt

Chủ project duyệt concept vùng đất, đất/rễ nâu cam và đá tối; tím dành cho nền xa/bóng sâu. Đã chốt tile 32×32 pixel, PPU 32. Bộ quái chỉ là tạo hình, animation/kích thước/gameplay chốt sau.

## Ảnh đã vẽ

- BG_Far.png: nền hang xa, ảnh kín.
- BG_Mid.png: lớp vách đất, rễ và di tích công nghiệp; có alpha.
- BG_Near.png: lớp rễ/ống và đèn gần hơn; có alpha.
- FG_RootsStone.png: sáu mẫu trang trí rễ, cột đá, ụ đất và chống hầm trên một sheet; giữ bản sáng nguồn.
- FG_RootsStone_Dark.png: bản foreground hạ sáng cuối, giữ texture đất/rễ và viền nâu nhẹ; ưu tiên bản này cho tiền cảnh.
- Gallery.md: xem cả bộ ảnh.
- Tileset_Earth_Source.png: sheet vật liệu/cảnh vật chi tiết đầu tiên, không coi mọi ô là tile nối tự động.
- Terrain_16_Source.png: ảnh nguồn terrain riêng.
- Terrain_16_32px.png: atlas 128×128, 4×4 ô.
- Tiles32/Earth_r0_c0.png ... Earth_r3_c3.png: 16 PNG kích thước thật 32×32, xuất bằng nearest-neighbor từ ảnh nguồn; CropMeasurements.txt ghi vùng cắt thực tế.
- Platforms_Stone.png: ba platform đá/rễ, đá/ống và đá/chống hầm trên một sheet.
- Spike_Ground.png: cụm gai hướng lên.
- Spike_Ceiling.png: một gai đá hướng xuống, vẽ riêng để sau này dùng làm bẫy rung/rơi.
- Enemy_Worm.png, Enemy_Beetle.png, Enemy_RootCrawler.png, Enemy_EarthGolem.png, Enemy_Scorpion.png: năm tạo hình quái.
- Preview_5_Enemies.png: bố trí năm ảnh để duyệt, thứ tự trên: giun/bọ/rễ, dưới: golem/bọ cạp. Tỉ lệ ảnh trong bảng không phải kích thước quái trong game.
- GenerationPrompts.md: prompt và công cụ.

## Trạng thái kỹ thuật

Ảnh được lưu ngoài Assets để duyệt, chưa tham chiếu bởi scene/prefab. PPU 32 là thông số đã được duyệt cho tile; chưa có TextureImporter Unity áp dụng trong thư mục output. Không thay đổi code, prefab, scene, camera, quái hiện có.

Tile 32×32 đã xuất và kiểm tra kích thước; đã loại khoảng trống phân ô khi cắt các mặt nối từ bounding box alpha thực tế. Ảnh nguồn có nhiều chi tiết hơn bản thu nhỏ. Bộ này chưa có RuleTile asset. Chưa kiểm tra các cách ghép góc trong Unity hoặc kết quả URP/Game View; không tuyên bố đã đạt seamless cho mọi tổ hợp. Bốn mẫu góc lõm ở cột cuối cần đối chiếu hướng khi thiết lập RuleTile.

Background chưa được chứng minh lặp ngang seamless, chưa thiết lập parallax. Các sheet foreground/platform chưa có collider/prefab; kích thước đặt trong màn cần chủ project chốt. Gai trần mới là art, chưa có code rung/rơi.

## Kiểm tra hình ảnh

Đã xem các ảnh nguồn tạo ra: bộ gần dùng nâu cam, đá tối, rễ và dấu tích công nghiệp; hang xa tím. Quái có năm dáng riêng (miệng vòng, giáp thấp, thân rễ dài, thân đứng tay lớn, đuôi bọ cạp). Đây là đánh giá hình ảnh của AI, chủ project là người duyệt chất lượng cuối.

Ảnh tách có alpha thật: kiểm tra mẫu pixel trong các PNG và góc ảnh; không suy ra độ trong suốt chỉ từ nền của cửa sổ preview. Chi tiết dày có thể cạnh tranh với nhân vật khi đặt cùng nhau; cần duyệt bố cục và độ tương phản trong Game View trước khi ghép thật.

## Nguồn về cách dựng cảnh Hollow Knight

Phỏng vấn trực tiếp Team Cherry mô tả dựng hình học bằng ô đen 64×64 rồi phủ cảnh vật; không coi 64×64 là kích thước chung cho art cuối:
https://www.pcgamer.com/how-to-design-a-great-metroidvania-map/

Ảnh trước/sau chính thức có minh họa việc thêm background/foreground:
https://www.teamcherry.com.au/blog/hollow-knight-then-and-now

Project hiện tại được đối chiếu bằng meta: Industry tile 32×32, PPU 32, platform Industry PPU 32. Bộ mới là tạo hình riêng, không sao chép sprite Hollow Knight.

