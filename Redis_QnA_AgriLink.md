# 🚀 Tài Liệu Toàn Tập Về Redis (Lý Thuyết & Phỏng Vấn Thực Chiến Giá Trị Cao)

Tài liệu này tổng hợp toàn bộ kiến thức lý thuyết nền tảng của Redis và cách thức vận hành thực tế thông qua các kịch bản sử dụng (Use Cases) trong dự án **AgriLink**, kèm theo bộ câu hỏi phỏng vấn chi tiết để bạn tự tin làm chủ kiến thức hệ thống.

---

## PHẦN 1: TỔNG QUAN LÝ THUYẾT REDIS

### 1. Redis Là Gì?
**Redis** (Remote Dictionary Server) là một hệ quản trị cơ sở dữ liệu phi quan hệ (NoSQL) mã nguồn mở. Đặc trưng lớn nhất của Redis là lưu trữ dữ liệu trực tiếp trên **RAM (In-Memory)** dưới dạng cặp giá trị `Key-Value` thay vì lưu trên ổ cứng (HDD/SSD) như các RDBMS truyền thống (PostgreSQL, SQL Server).

### 2. Ưu Điểm Cốt Lõi Vượt Trội
* **Tốc độ đọc/ghi ánh sáng (Microseconds):** Thay vì các thao tác I/O trên ổ cứng tốn kém thời gian, việc đọc/ghi trên bộ nhớ RAM giúp các phản hồi của Redis luôn tính bằng mili-giây hoặc micro-giây. Thời gian tra cứu bản ghi có độ phức tạp thuật toán là **O(1)**.
* **Cấu trúc dữ liệu phong phú:** Hỗ trợ String, List, Set, Hash, Sorted Set,...
* **Single-threaded (Đơn luồng):** Redis sử dụng kiến trúc Event Loop đơn luồng để xử lý các câu lệnh. Nhờ không mất thời gian context-switching (đổi luồng) hay locking, nó xử lý và đảm bảo được tính tuần tự cho hàng triệu request/giây mà không bị Race Condition.

---

## PHẦN 2: STRATEGY TRIỂN KHAI REDIS TRONG AGRILINK

Trong dự án AgriLink, Redis được triển khai với vai trò là một **Distributed Cache (Cache phân tán)** nằm ngoài Server API, cụ thể được host trên Cloud (Aiven) dùng thư viện kết nối **`StackExchange.Redis`**.
Hệ thống sử dụng Redis cho 2 mục đích sống còn:
1. **Triển khai Cache-Aside Pattern (Tối ưu truy xuất):** Tối ưu hóa Database (PostgreSQL) khỏi các câu query lặp lại nhiều lần.
2. **Quản lý Session Identity (Refresh Token):** Bổ khuyết lại sự thiếu sót (Stateless) của JWT trong quản lý trạng thái đăng nhập.

---

## PHẦN 3: BỘ CÂU HỎI PHỎNG Vấn THỰC CHIẾN TỪ DỰ ÁN

### 🎯 Nhóm 1: Cache Dữ Liệu Cơ Bản & Design Pattern

**Câu 1: Em hãy giải thích lý do vì sao dự án AgriLink lại dùng Redis Caching mà không lấy thẳng dữ liệu từ PostgreSQL luôn?**
> **Trả lời:**
> Dạ, mặc dù DB đã được Index nhưng nhiều bảng phải JOIN với nhau khá nặng nề (ví dụ như bài phân tích, thống kê bảng giá). Thay vì để mỗi request của người dùng lại query DB và tính toán lại từ đầu, em dùng Redis làm một lớp ở giữa để Cache. Tốc độ lấy dữ liệu từ RAM của Redis xuống API chỉ từ 1-5ms, giúp API response ngay lập tức, tiết kiệm tài nguyên Server và bảo vệ CSDL khỏi tình trạng Overload (cổ chai).

**Câu 2: Design Pattern em áp dụng cho quá trình Caching tên là gì? Em hãy cho biết quy trình luồng đi (flow) của nó.**
> **Trả lời:**
> Em sử dụng **Cache-Aside Pattern**. Trong dự án em viết chung một class `BaseCachedService` (chứa hàm `GetOrSetCacheAsync`). Flow có 3 bước thủ tục:
> 1. **Check Cache (Cache Hit):** API kiểm tra xem trong Redis đã có Key (ví dụ: `article:1`) hay chưa. Trả ngay lập tức nếu đang có.
> 2. **Fallback DB (Cache Miss):** Nếu chưa có, query gọi xuống DB thông qua Repository lấy bản ghi.
> 3. **Set Cache:** Chuyển List Object lấy dưới DB về định dạng JSON, lưu vào Redis với thời gian hết hạn (TTL), sau đó trả về cho client. 

**Câu 3: Dữ liệu (Objects / Danh sách lớp C#) được lưu vào Redis bằng hình thái (format) nào?**
> **Trả lời:**
> Redis là kiểu dữ liệu Key-Value. Do vậy, phần Value không thể ném thẳng nguyên một Object C# của bộ nhớ xuống được, mà em tiến hành serialization (chuyển đổi) các class/list C# về chuỗi **JSON** (thông qua `System.Text.Json`). Khi có dữ liệu gọi từ Redis lên, file `RedisService.cs` sẽ lo nhiệm vụ deserialize từ JSON trả ngược về Generic T cho hệ thống xử lý.

---

### 🎯 Nhóm 2: Bảo Mật Session & Refresh Token (Deep Dive)

**Câu 4: Em dùng Redis lưu Refresh Token, tại sao lại làm vậy mà không lưu ở DB (PostgreSQL) hay để JWT tự xoay sở?**
> **Trả lời:**
> * **Vì JWT là Stateless (Không trạng thái):** Bản thân thằng Access Token (JWT) đã được cấp thì **không thể thu hồi (Revoke)** cho đến khi hết hạn. Lỡ hệ thống phát hiện Hacker lấy cắp token, vô phương ngăn chặn nó. Do đó, cần cơ chế dùng Refresh Token kết hợp.
> * **Lưu DB vs. Lưu Redis:** 
>   * Lưu DB bắt ta phải viết Code (hoặc Job) dọn rác thủ công thường kì khi Token hết hạn để tránh sình DB ra. 
>   * Khi dùng Redis, nó có sẵn tính năng hỗ trợ vòng đời **TTL (Time-To-Live)**. Nếu em Set Token hết hạn 7 ngày, đúng 7 ngày trôi qua Redis tự động vứt Key đó khỏi bộ RAM luôn. Rất nhàn nhã và tối ưu!
>   * Ngoài ra, khi đến hạn gọi làm mới Token diễn ra rất thường xuyên. Dựng Redis (với tốc độ O(1)) là nơi kiểm tra Session rất phù hợp mà không cần tạo Transaction hay Locking rườm rà dưới DB gốc.

**Câu 5: Vậy Redis dùng cách nào (Cơ chế nào) để cấp lại token trên dự án? (Trình bày Flow)**
> **Trả lời:**
> Quy trình Refresh Token của API (`AuthService.cs`) chạy qua các bước này ạ:
> 1. **Tiếp nhận:** Client Gửi cái Access Token (dù đã hết hạn) và cái chuỗi Refresh Token lên `/refresh-token` API.
> 2. **Nhận dạng:** API tiến hành nhặt `UserId` nằm chìm trong Access token cũ đó ra.
> 3. **Validation ở Redis:** API lấy `UserId` vừa có làm Keyword tra xem trong Redis có tồn tại Key `refresh_token:{UserId}` không.
>    * Nếu Redis bảo "Không", hoặc Value tại đó sai lệch -> **Từ chối (Bắt User login lại)**.
>    * Nếu khớp 100% -> Chuyển bước 4.
> 4. **Tái cấp thẻ:** Chỉ cần tạo duy nhất 1 Access Token (thẻ mới) và trả ngược cho Client. Không cần phải sinh lại một chuỗi Refresh nữa. Đảm bảo UI ít giật lắc đổi session nhất.

---

### 🎯 Nhóm 3: Đồng Bộ, Kháng Lỗi, Xử Lý Sự Cố (Critical Thinking)

**Câu 6: Quá trình làm mới dữ liệu (Cache Invalidation) xảy ra như thế nào khi em Update hoặc Delete bản ghi DB?**
> **Trả lời:**
> Đây là một bài toán khó (Stale Data), nên em áp dụng chiến thuật **Explicit Invalidation** chủ động.
> Tại các service, sau khi lệnh Update/Delete thực hiện vào DB thành công (`SaveChangesAsync`), lập tức em gọi hàm `InvalidateCacheAsync(key)`.
> Các lượt request tiếp theo từ người sử dụng sẽ bị "Cache Miss". Luồng chạy tự Fallback xuống DB nhặt dữ liệu đã được Update và điền một luồng mơi tinh lên Redis.

**Câu 7: Lỡ hệ thống đang chạy thì Server Redis bị sập (Chết). Làm sao để thu hồi được Key khi bị phát hiện hack đang diễn ra? Hoặc lỡ chết thế mất dữ liệu Refresh Session của muôn vàn Client thì khôi phục kiểu gì?**
> **Trả lời (Đập tan sự do dự của nhà phỏng vấn):**
> * **Để Khôi phục dữ liệu Token:** Redis là Cache Ram nhưng nhà mạng như Aiven luôn bật tính năng lưu dự phòng xuống Ổ cứng (bằng **RDB file snapshot** hoặc **AOF (Append Only File)**). Khi Redis bị Reboot do sự cố, ngay lúc khởi động, nó tự động tải ngược lịch sử đó lên RAM. Do đó Token các User không bị xoá sạch, họ không bị Logout đồng loạt một cách lãng nhách 100%. 
>
> * **Nếu Redis đang SẬP mà muốn THU HỒI QUYỀN (Force Logout hacker):** Trong `AuthService.RefreshTokenAsync()` của em, trước khi tái cấp Session cuối cùng em luôn chèn 1 câu lệnh Check DB:
>   `var user = await _userRepository.GetByIdAsync(UserId);`
>   `if (user == null || !user.IsActive) return false;`
>   Vậy trong lúc mâm Redis đang sập không thể tương tác vô hiệu hoá Key được, thì Admin chỉ việc vào trang Admin chọc DataBase set trạng thái cái Acc-Cày-Thuê đó về `IsActive = False` hoặc Ban Acc. Nhờ đó, bất kể nó giữ cái thẻ làm mới như thế nào đi nữa, vòng kiềng check ở Database đã từ chối thẳng tay. Hệ thống vẫn bảo mật bất bại!.

**Câu 8: Nhỡ ngắt kết nối Cloud, Redis chết hẳn thì Hệ thống (API) lấy dữ liệu có bị liệt theo không?**
> **Trả lời:**
> Chắc chắn là KHÔNG. Trong **Cache-Aside Pattern**, nếu API văng lỗi mạng khi gọi Redis, em bọc nó trong `try-catch` (Graceful Fallback) để code làm lơ tiếp tục truy xuất vào PostgreSQL. Nên hệ thống vẫn **(Fault-tolerant - Kháng Lỗi)**, khách hàng vẫn xem được bài viết dù chỉ bị chậm hơn một tẹo chứ không phải màn hình Error 500 đen thùi!
