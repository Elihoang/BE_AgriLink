# 🔥 Bộ Câu Hỏi Ôn Tập Phỏng Vấn Backend - Dự án AgriLink DH

Tài liệu này tổng hợp toàn bộ những câu hỏi cốt lõi về **Kiến trúc (Architecture)** và **Hiệu suất (Redis Caching)** dựa trên mã nguồn thực tế của dự án AgriLink_DH để bạn ôn tập trước buổi phỏng vấn.

---

## 🏗️ 1. Câu Hỏi Về Kiến Trúc Hệ Thống (Architecture)

### Q1. Kiến trúc hiện tại của dự án em là gì?
**Trả lời:** 
> "Dự án của em được thiết kế dựa trên tư tưởng của **Clean Architecture** (Kiến trúc Sạch). Em chia hệ thống thành 4 lớp (Layers) chính bao gồm: `Domain`, `Share`, `Core` (chứa các Services) và `Infrastructure` (chứa Repositories & DbContext), cùng với lớp ngoài cùng là `Api` (chứa Controller). 
> 
> Đặc điểm nổi bật nhất là em áp dụng triệt để nguyên lý **Dependency Inversion** (Đảo ngược phụ thuộc): Các `Services` ở tầng Core không bao giờ chọc trực tiếp vào Database thông qua `DbContext`, mà nó bắt buộc phải giao tiếp thông qua các **Interfaces** (như `IRepository`, `IUnitOfWork`) được định nghĩa ở tầng `Domain`. Mọi thao tác kết nối cơ sở dữ liệu vật lý đều do tầng `Infrastructure` gánh vác."

### Q2. Unit of Work và Repository Pattern dùng để làm gì?
**Trả lời:**
> "Em dùng **Repository Pattern** để tạo ra một lớp trừu tượng (`BaseRepository`) chuyên biệt hóa việc gọi các câu lệnh SQL/LINQ. Nhờ nó, Code ở `Services` của em rất sạch, dễ đọc và không hề dính dáng đến Entity Framework.
> 
> Còn **Unit Of Work (UoW)** em dùng để đảm bảo tính toàn vẹn dữ liệu (ACID). Nếu API của em cần Thêm 1 bài viết, sau đó Cập nhật lịch sử tác giả ở 2 Repository khác nhau, em dùng UoW gom tất cả lại thành 1 Transaction duy nhất (`SaveChangesAsync`). Lỗi ở bất cứ đâu, toàn bộ dữ liệu sẽ tự Rollback, không lo bị lưu nửa vời."

---

## ⚡ 2. Câu Hỏi Về Tối Ưu Hiệu Suất (Redis)

### Q3. Em dùng Redis trong dự án để làm gì? Giải thích mô hình Cache-Aside?
**Trả lời:**
> "Trong AgriLink, em dùng Redis đóng vai trò là một **Distributed Cache (Cache phân tán)** và **Quản lý Refresh Token**.
> 
> **Cache-Aside Pattern:** Khi có Request gọi lấy danh sách bài viết (Blog), Code C# của em sẽ chọc vào Redis trước theo cơ chế:
> 1. Kiểm tra RAM (Redis) xem có dữ liệu bài viết chưa. Nếu có (HIT), em trả về luôn cho Web/App (Tốc độ ~1-5ms).
> 2. Nếu chưa có (MISS), em mới chọc xuống PostgreSQL để Query, lấy dữ liệu xong em lưu ngược trở lại Redis, đặt cho nó cái thời gian sống (TTL - Time To Live) là khoảng 15 phút, rồi mới trả về cho User. Các lượt truy cập sau sẽ tự động HIT."

### Q4. Thế Redis của em là lưu thẳng vào RAM máy tính của em (In-Memory cục bộ) đúng không?
**Trả lời:**
> "Dạ **KHÔNG** ạ. Trong dự án này, em thuê và sử dụng một cụm **Managed Redis Server** trên môi trường Cloud của hãng **Aiven**. 
> - Đúng là Redis lưu dữ liệu bằng **In-Memory** (nhờ đó đạt max tốc độ), nhưng là nó lưu vào thanh RAM của cái Máy Chủ ảo trên nền tảng Cloud của Aiven. 
> - Ứng dụng ASP.NET API của em kết nối với cái máy chủ Aiven đó thông qua mạng. Làm như vậy để tương lai khi em Scale App thành 3-4 con Web Servers, các con Web Servers này vẫn dùng chung 1 chỗ chứa Cache, dữ liệu sẽ luôn đồng bộ thay vì mượn RAM cục bộ của từng máy."

### Q5. Vậy API của em kết nối với máy chủ Redis trên Aiven qua HTTP à?
**Trả lời:**
> "Dạ **KHÔNG**. HTTP là giao thức cồng kềnh dành cho Web. Để kết nối với Redis Server, em dùng thư viện `StackExchange.Redis` hoạt động dựa trên phương thức mạng **TCP/TLS** (bảo mật). Nó trao đổi Data bằng các gói tin thô qua một giao thức siêu nhẹ của riêng Redis gọi là **RESP (Redis Serialization Protocol)**. Chính vì không phải 'tải' đống JSON/Headers nặng nề của HTTP nên Redis mới có thể đọc/ghi hàng triệu Request mỗi giây."

### Q6. Khi ta Update một dữ liệu (ví dụ: sửa tiêu đề bài viết), làm sao Redis biết cái danh sách (list) cũ đã bị lỗi thời để tự nạp lại dữ liệu mới từ DB?
**Trả lời:**
> "Dạ, trong dự án AgriLink, em sử dụng cơ chế **Chủ động xóa Cache (Explicit Invalidation)** kết hợp với mô hình **Cache-Aside**.
> 
> Thay vì đợi Redis tự nhận biết dữ liệu cũ, ngay khi Backend C# cập nhật Database thành công thông qua `SaveChangesAsync()`, em sẽ gọi lệnh xóa hoàn toàn cái Key chứa dữ liệu đó (hoặc xóa cả danh sách liên quan) khỏi Redis.
> 
> Ví dụ trong `ArticleService.cs`, khi em Update một bài viết, em gọi hàm xóa Cache chi tiết và Cache danh sách. Khi đó, ở lần truy cập tiếp theo của người dùng, hệ thống kiểm tra Redis sẽ gặp tình trạng **Cache Miss** (không thấy dữ liệu). Lúc này, code mới 'buộc lòng' phải xuống DB lấy bản mới nhất vừa update, rồi nạp ngược lại vào Redis. Như vậy, dữ liệu trên Redis luôn được làm mới ngay lập tức sau khi DB thay đổi."

---

## 🤖 3. Tính Năng Thông Minh (AI Voice & Text-To-Speech)

### Q7. Tính năng "Đọc bài viết thành tiếng (Text-to-Speech)" của em cài đặt thế nào? Có tốn tiền gọi API ngoài không?
**Trả lời:**
> "Tính năng này em thiết kế hoàn toàn **Bảo mật và Miễn phí**. 
> Ở góc độ hệ thống, Backend ASP.NET của em thiết kế bảng `Articles` có thêm một cột lưu trữ `AudioUrl` (phòng hờ trường hợp Admin muốn upload 1 file mp3 giọng đọc xịn tải lên Cloudinary).
> 
> Tuy nhiên, công nghệ lõi để đọc văn bản chủ động (Agri AI Voice) lại nằm ở tầng Frontend (React). Thay vì em phải gọi một API bên thứ 3 (như Google TTS tốn phí), em tận dụng sức mạnh của trình duyệt web thông qua công nghệ **Web Speech API (`window.speechSynthesis`)**. 
> - Khi user bấm nút 'Nghe bài viết', trình duyệt của họ sẽ tự động phân tích và dùng chính cỗ máy AI (của hệ điều hành Windows/Android/iOS trên thiết bị của user) để tổng hợp giọng nói.
> - **Lợi ích:** Trải nghiệm tức thời không có độ trễ mạng (Zero-latency), hoàn toàn miễn phí, hệ thống Backend của em cũng không bị ăn một tí tải nào cho việc xử lý Audio."

---

## ⚖️ 4. Tích Hợp Cân Bluetooth (IoT & Real-time)

### Q8. Tính năng Cân Bluetooth của dự án hoạt động ra sao? App giả lập chạy mất bao nhiêu thời gian?
**Trả lời:**
> "Tính năng này em thiết kế theo kiến trúc **Real-time Streaming (Luồng dữ liệu thời gian thực)**. 
> Trong môi trường code giả lập hiện tại để trình diễn (demo), luồng hoạt động mất đúng **45 giây cho 5 bao (Mỗi bao ~9 giây)**. Trong 9 giây này:
> 1. Mất 2.5s để App giao động số liệu (mô phỏng sự rung lắc khi công nhân quăng đồ lên cân).
> 2. Mất 5.0s tiếp theo để số cứng ở 50.0kg (thời điểm cân đã nằm im trên bàn cân).
> 3. Mất 1.5s cuối để số nhảy về 0.0 (bao tải được nhấc xuống mui xe, sẵn sàng vòng tiếp theo). 
> Ngay khi đủ chu kỳ 5 bao, ứng dụng tự động ngắt kết nối và chốt quy trình."

### Q9. Công nghệ tích hợp Cân Bluetooth là gì? Làm sao để chống việc nhân viên gian lận số liệu cân ngay tại App điện thoại?
**Trả lời:**
> "Để tích hợp, em kết hợp **SignalR (WebSockets)** cho luồng mạng thời gian thực và **Web Bluetooth API** (để App trực tiếp đọc sóng của thiết bị Cân Bluetooth).
> 
> **Kiến trúc chống gian lận cực kỳ chặt chẽ (Anti-cheat Architecture):**
> Em quy định App Mobile/Web (Điện thoại) chỉ đóng vai trò là một cái ăng-ten (Dumb Client). Nó **không được quyền** tự chốt sổ. Nhiệm vụ duy nhất của nó là đọc sóng Bluetooth liên tục và gửi dòng raw-data đó lên Server Backend qua mạng 4G/Wifi (với tần suất 2 lần/giây).
>
> Tại **Backend C# (Bộ não thật sự)**, em xây dựng 1 thuật toán 'Khóa Khối Lượng' (Auto-Lock) cache trên bộ nhớ: nó sẽ gom 5 mẫu dữ liệu mới gửi lên nhất (Tương đương 2.5s thực tế), nếu sự chênh lệch (Max - Min) của 5 mẫu liên tiếp đó `<= 0.05kg` -> Có nghĩa là trọng lượng thực đã thật sự ổn định. 
> Lúc này, chính Backend sẽ tự động tính toán trừ bì (Deduction) và **lưu vào Database PostgreSQL** với cờ đánh dấu `IsAutoWeighed = True` kèm theo địa chỉ MAC của cái cân. Sau đó Backend mới Ping một tín hiệu `OnBagLocked` ngược về App để máy phát ra tiếng 'TÍT' báo chốt bao thành công.
>
> Nhờ đó, dù cho ai cố tình Hack cái App React để gửi số khống đi nữa thì cũng vô trúng tường mây lửa, bởi vì mọi quyết định chốt sổ đều dựa trên thuật toán kiểm tra tính bất biến nằm 100% trên con C# Server Backend ạ."

---

## 🌿 5. Tính Năng AI Nhận Diện Mầm Bệnh (Computer Vision)

### Q10. Quá trình "Train" (huấn luyện) AI mầm bệnh diễn ra như thế nào? Hệ thống C# giao tiếp với AI ra sao?
**Trả lời:**
> "Việc nhận diện bệnh trên lá cây nông sản (cà phê, tiêu...) được thiết kế dưới dạng một **Microservice độc lập** chuyên về AI. Quá trình làm ra nó trải qua 3 bước cốt lõi:
> 
> **1. Huấn luyện (Training):**
> - Em thu thập dữ liệu (Dataset) từ Kaggle (như PlantVillage) chứa hàng ngàn tấm ảnh lá bệnh (Rỉ sắt, nấm hồng...) và lá khỏe. Tất cả ảnh đều được dán nhãn (Labeling).
> - Em dùng ngôn ngữ **Python** và thư viện **TensorFlow/PyTorch** để build mô hình. Thuật toán lõi em chọn là **Mạng nơ-ron tích chập (CNN - Convolutional Neural Networks)** (Ví dụ: kiến trúc MobileNet hoặc YOLO). Vì CNN cắt lớp bức ảnh ra thành ma trận điểm ảnh để học các đường nét, đốm bệnh cực kỳ chính xác và dung lượng lại rất nhẹ, phù hợp cho Mobile.
> 
> **2. Đóng gói AI (Deployment):**
> - Sau khi Train xong ra một file Model (hàng ngàn ma trận trọng số), em không nhét nó vào C# vì C# không tối ưu chạy AI. Em bọc nó lại bằng một Framework của Python là **FastAPI** (hoặc Flask) để biến con AI thành một API Server nhỏ gọn.
> 
> **3. Luồng tích hợp (Integration Flow) với C# Backend:**
> - Khi nông dân cầm app React chụp ảnh lá cây, frontend sẽ gửi file ảnh đó (dạng `IFormFile`) lên **Backend C#** của em.
> - C# của em đóng vai trò là một cái Cổng (Proxy/API Gateway). Nó sẽ forward (gửi tiếp) tấm ảnh đó sang con **Python AI API**.
> - Server Python phân tích cái ảnh trong chớp mắt (Inference), trả ngược về cho C# một cục JSON kiểu: `{"disease": "Coffee Leaf Rust", "confidence": 0.95}`.
> - Cuối cùng, Backend C# sẽ đem thông tin phát hiện bệnh này lưu lịch sử vào Database PostgreSQL (để sau này thống kê vùng dịch vụ), rồi trả ngay tên mầm bệnh đó cộng lịch xịt thuốc ra giao diện màn hình cho nông dân!"
