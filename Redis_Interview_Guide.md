# 🚀 Hướng Dẫn Ôn Tập Phỏng Vấn: Kiến trúc Redis trong AgriLink_DH

Tài liệu này tổng hợp chi tiết cách Redis được triển khai và sử dụng trong dự án AgriLink_DH dựa trên source code thực tế (`RedisService.cs`, `BaseCachedService.cs`), giúp bạn tự tin trả lời phỏng vấn.

---

## 1. Tổng quan về Redis trong dự án
**Q: Trong dự án AgriLink, em sử dụng Redis làm gì?**
**Trả lời:**
Em sử dụng Redis với 2 mục đích chính:
1. **Distributed Cache (Cache phân tán):** Lưu trữ tạm thời các dữ liệu thường xuyên được truy vấn nhưng ít thay đổi (ví dụ: danh sách bài viết/Blog, cấu hình, v.v.) để giảm tải cho Database (PostgreSQL) và tăng tốc độ response của API (chỉ mất ~1-5ms).
2. **Quản lý Session/Refresh Token:** Lưu trữ Refresh Token của người dùng, cho phép tốc độ tra cứu O(1) cực nhanh xác thực token hợp lệ, cũng như có thể chủ động chặn, thu hồi quyền truy cập (Revoke token) của một User ngay lập tức.

---

## 2. Mô hình kết nối hạ tầng
**Q: Redis của em chạy ở đâu? Nó giao tiếp với API như thế nào?**
**Trả lời:**
*   **Hạ tầng:** Redis không được cài cục bộ trên server API mà em sử dụng dịch vụ **Managed Redis Server** trên môi trường Cloud của **Aiven**. Điều này giúp ứng dụng có thể scale ra nhiều server (Web nodes) mà vẫn dùng chung một cụm Cache duy nhất, bảo đảm tính nhất quán (Consistency).
*   **Giao thức kết nối:** Ứng dụng .NET kết nối đến Aiven Redis bằng thư viện `StackExchange.Redis` thông qua socket mạng **TCP/TLS** thay vì HTTP. Dữ liệu trao đổi qua **RESP (Redis Serialization Protocol)** siêu nhẹ, giúp đạt được hàng triệu request/giây mà không bị overhead bởi cấu trúc JSON hay headers như HTTP.
*   **Định dạng dữ liệu:** Trước khi lưu object (như danh sách bài viết) vào Redis, em serialize nó thành JSON string bằng `System.Text.Json` và khi gọi ra thì deserialize ngược lại.

---

## 3. Design Pattern: Cache-Aside và BaseCachedService
**Q: Quy trình lấy dữ liệu từ Cache trong mã nguồn xử lý như thế nào?**
**Trả lời:**
Em áp dụng **Cache-Aside Pattern**, code được em tổng hợp thành một abstract class tên là `BaseCachedService` để các Service khác dùng chung. Quy trình có 3 bước (như hàm `GetOrSetCacheAsync`):
1. **Check Cache:** Khi có request, em lấy key đi kiểm tra xem trong Redis có tồn tại dữ liệu hay chưa (`RedisService.GetAsync<T>`). Nếu có *(Cache Hit)*, trả về ngay lập tức.
2. **Query DB:** Nếu dữ liệu không tồn tại *(Cache Miss)*, hệ thống tiếp tục gọi xuống Database (PostgreSQL) thông qua Repository để lấy bản ghi gốc.
3. **Set Cache & Return:** Cập nhật lại dữ liệu vừa lấy lên Redis (`RedisService.SetAsync`), gán TTL (Time To Live - thời gian sống) rồi trả về cho client. Các request sau đó lúc này sẽ tự động *"Hit"* cache.

---

## 4. Quản lý đồng bộ (Cache Invalidation)
**Q: Khi dữ liệu bị thay đổi ở DB, làm sao để dữ liệu trên Redis không bị cũ (Stale / Out-of-sync)?**
**Trả lời:**
Em áp dụng chiến thuật **Explicit Invalidation** (Chủ động xóa ngay khi thấy cập nhật).
*   Ví dụ khi một bài viết bị thay đổi tiêu đề: Sau khi quá trình `_unitOfWork.SaveChangesAsync()` cập nhật DB thành công, em sẽ lập tức gọi hàm xóa key cache `InvalidateCacheAsync(key)` hoặc xóa hàng loạt bằng `InvalidateCacheByPatternAsync(pattern)`.
*   Nhờ đó, user khác khi request thông tin của bài viết này sẽ gặp *Cache Miss*. Code bắt buộc phải xuống DB lấy dữ liệu cập nhật mới nhất, qua đó thiết lập lại cache một cách chính xác.

**Q: Khi dùng hàm xoá nhiều Key bằng Pattern ("prefix:*"), em sử dụng lệnh nào của Redis để tối ưu?**
**Trả lời:**
Trong function `DeleteByPatternAsync` của `RedisService`, thay vì lạm dụng lệnh `KEYS *` (sẽ khóa toàn bộ Node Redis, gây nghẽn cổ chai ảnh hưởng trầm trọng resource), thư viện `StackExchange.Redis` bản chất đã bọc một lệnh an toàn hơn là lệnh **`SCAN`**. Lệnh `SCAN` sẽ lướt qua một số lượng nhỏ key mỗi vòng (như con trỏ cursor), giúp tiến trình xóa pattern diễn ra an toàn không làm block các request khác của hệ thống.

---

## 5. Từ khoá (Keywords) để chốt và tạo ấn tượng với nhà phỏng vấn:
Nên chèn các thuật ngữ này khi giao tiếp:
*   *"Em sử dụng StackExchange.Redis vì nó dùng Multiplexer, có khả năng maintain 1 kết nối TCP persistent và multiplex các request qua một socket duy nhất, rất tối ưu cho .NET."*
*   *"Em thiết kế BaseCachedService bằng Abstract class truyền delegate func vào tham số để code ở các Service con rất clean và đáp ứng nguyên lý DRY (Don't Repeat Yourself)."*
*   *" Về bảo mật token, Refresh Token trên Redis cung cấp tính năng O(1) tra cứu và cho phép em implement tính năng 'Force Logout' đối tượng một cách thời gian thực mà JSON Web Token (vốn là stateless) không tự làm được."*

---

## 6. Các câu hỏi mở rộng (Trọng tâm) 

**Q: Vì Redis đặt trên máy chủ Aiven (Cloud), vậy toàn bộ tốc độ Cache của ứng dụng sẽ phụ thuộc vào mạng (Network dependency) đúng không? Nhỡ đứt mạng thì sao?**
**Trả lời:**
*   *"Dạ đúng, vì cấu trúc phân tán (Distributed Cache) nên nó phụ thuộc vào tốc độ mạng (Network latency) giữa server API và server Redis. Tuy nhiên, thời gian chênh lệch là **cực kỳ nhỏ** (chỉ chừng vài mili-giây) so với lợi ích mà nó mang lại. "*
*   *"Nếu nhỡ có sự cố đứt kết nối mạng hoàn toàn tới Redis, dự án hiện tại của em dùng pattern Cache-Aside, khi `StackExchange.Redis` văng Exception không bắt được kết nối, em có thể bọc block `try-catch` để hệ thống tự động Fallback (lùi về) bước truy vấn thẳng xuống PostgreSQL. Nhờ đó ứng dụng **không bị chết hoàn toàn (Fault Tolerance)**, mà chỉ tạm thời chậm đi do mất Cache (gọi là degraded performance)."*

**Q: Redis lưu dữ liệu theo định dạng "Key-Value", vậy trên máy chủ Cloud (Aiven) nó có lưu giống như khi em cài localhost ở máy tính không?**
**Trả lời:**
*   *"Dạ hoàn toàn giống nhau ạ. Bản chất Redis dù cài ở máy cá nhân (Local) hay máy chủ Cloud (Aiven, AWS, v.v) thì đều là **cùng một bộ core phần mềm Redis**.*
*   *"Dữ liệu luôn được lưu vào thanh RAM của máy đó dươí dạng một cuốn từ điển siêu tốc: `Key: Value`. Trái với mô hình bảng (Table) chia cột dọc ngang như cơ sở dữ liệu quan hệ (PostgreSQL/SQL Server), Redis tìm kiếm dữ liệu theo cơ chế Hash Table dựa vào cái `Key` này, nên tốc độ đọc/ghi của nó luôn luôn là O(1) cực kỳ nhanh, bất kể lượng dữ liệu trên máy chủ là 1 nghìn hay 1 triệu bản ghi."*
