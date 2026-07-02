# ☕ CHIẾN LƯỢC CÀO DỮ LIỆU GIÁ NÔNG SẢN (WEB SCRAPING) - ĐÁP ÁN TRÌNH ĐỘ MID/SENIOR

Nếu ứng tuyển Fresher/Intern, người ta chỉ mong bạn biết xài thư viện lấy HTML. 
Nhưng nếu muốn **thể hiện trình độ Mid-Level**, bạn phải vạch ra chiến thuật phân tích hệ thống Đích (Target System) và tự động hóa (Background Job) trước khi nhắc đến code.

---

## 🛑 BÀI TOÁN SỐ 1: BẢN CHẤT CỦA VIỆC CÀO DATA? EM DÙNG CÁCH NÀO ĐỂ LẤY GIÁ CÀ PHÊ?

**Câu hỏi: "Trong dự án AgriLink, em cào (scrape) giá cà phê từ trang web của người khác như thế nào? Em xài thư viện gì và nguyên lý hoạt động của nó ra sao?"**

**Trả lời (Phô diễn tư duy phân tích của Mid-Dev):**
"Dạ thưa anh, khi nhận bài toán cào dữ liệu giá nông sản, em không bao giờ vội vàng code ngay công cụ Parser HTML cồng kềnh. Ở tầm nhìn Mid-level, em chia phương pháp tiếp cận mục tiêu thành 3 kỹ thuật xếp hạng từ Nhẹ nhất đến Chuyên sâu nhất tùy vào độ khó của trang Web Đích:

### Kỹ thuật 1 (Cực kỳ Tối ưu & Chuyên nghiệp): Reverse Engineering XHR (Bắt ngược API)
Trước khi cào, em mở trình duyệt dùng tool **F12 (DevTools) $\rightarrow$ tab Network**. Em tải lại trang để xem dữ liệu giá cà phê thực chất nó chui từ cái lỗ nào ra. 
* *Trường hợp lý tưởng:* Nếu trang web đích viết bằng React/Angular, 90% nó sẽ có một luồng gọi API ngầm (XHR) trả về 1 file JSON sạch sẽ ví dụ như `GET target.com/api/v1/coffee-prices`. 
* Khi phát hiện luồng này, em vứt bỏ hoàn toàn ý định cào HTML. Em dùng chính `HttpClient` của C# bắn trực tiếp vào cái link API ngầm đó, Fake lại cái Header (User-Agent, Referer) để đánh lừa máy chủ. Nhờ vậy em ăn trọn cục JSON nguyên chất, tốc độ parse nhanh hơn ánh sáng và không lo bị vỡ Layout!

### Kỹ thuật 2 (Cào Truyền Thống): Phân tách HTML DOM Parsing (HtmlAgilityPack)
Nếu trang Web đích là trang Web đồ cổ (dùng PHP thuần hoặc ASP.NET MVC cũ) nó nhồi luôn dữ liệu vào HTML (Server-side rendering), bắt buộc phải cào thô.
* Quá trình: Em dùng thư viện đỉnh cao của C# là **`HtmlAgilityPack`** (hoặc `AngleSharp`).
* Đầu tiên dùng `HttpClient` tải nguyên cục text HTML của trang web về. Em nhét nó vào cái Phễu `HtmlDocument`.
* Lúc này cái cây HTML biến thành 1 kho DOM. Em chỉ việc dùng kỹ thuật **XPath** để bóc tách tàn bạo: "Đi vào thẻ `<table>` có `ID="price"`, quẹo xuống `<tbody>`, lấy ra vòng `<tr>` số 2, tóm ra cái `<td>` chứa giá."

### Kỹ thuật 3 (Khủng nhất): Cào Xuyên Anti-Bot vả Render bằng JS (PuppeteerSharp)
Trường hợp quá xui, trang web gắn Cloudflare chống cào, hoặc Data bị mã hóa Javascript (load lên rồi mới chờ tự giải mã). Thì `HtmlAgilityPack` chết nghẽn.
* Lúc này em tung vũ khí hạng nặng: **`PuppeteerSharp`** (hoặc `Playwright`). Thư viện này giật dây một cái Trình Duyệt Google Chrome Ẩn (Headless Browser) chạy ngầm dưới nền máy chủ Backend. Nó bấm nút, lướt web y chang 1 người thật, vượt qua cái khiên Cloudflare. Đợi 5 giây cho dữ liệu load ra thẻ `<div>` rồi em móc cái Text đó về. Do nuôi tiến trình Chrome khá tốn RAM máy chủ, nên em chỉ xài khi 2 cách trên phá sản.

---

## 🛑 BÀI TOÁN SỐ 2: TỰ ĐỘNG HÓA VÀ CỨU CÁNH CHỐNG CHẶN BĂNG THÔNG

**Câu hỏi sấy: Vậy em lên web người ta cào lúc nào? Có phải mỗi lần người dùng bấm F5 vô xem giá là em lại mò sang trang kia cào không? Như thế trang kia sập vì ddos và app em thì rùa bò à?**

**Trả lời (Thiết lập Background Service & Caching):**
"Trời, dạ tuyệt đối không thưa anh! Thiết kế kiểu đó là sập toàn tập cả 2 hệ thống. Trong AgriLink, tiến trình cào dữ liệu chạy độc lập và câm lặng hoàn toàn ở Hậu đài nhờ mô hình **Background Job / Hosted Service**.

1. **Hẹn giờ Tự động bằng Hangfire (hoặc Quartz / IHostedService):** Cứ đúng 6h00 Sáng và 12h00 Trưa mỗi ngày (Lúc chốt phiên giá), máy chủ C# của em tự động đánh thức 1 cái Thread chạy ngầm (Worker). Thằng trinh sát này âm thầm đột nhập vào trang Web đích cào gói giá mới nhất về.
2. **Gắn chặt vào Redis Cache:** Lấy Data về xong, em tống ngay dải Số Liệu đó xuống **Redis Cache** trên máy chủ của em, cất vào một cái Key: `Daily_CoffeePrice_V1`.
3. **Frontend ăn sẵn:** Xuyên suốt một ngày dài, 10.000 ông nông dân vào mở App AgriLink coi giá, Backend của em lập tức tóm cổ tụi nó chẹt xuống móc con số từ Redis ra chưng ngay tắp lự. Chấp cả thế giới F12 xem giá thì em cũng chẳng sợ DDOS trang Web nguồn, vì Data là Data trong bộ nhớ đệm (RAM) Redis của em rồi!
4. **Tối thượng - Rotate Proxy (Quay Đầu IP):** Để không bị admin trang đích chặn dải IP mạng, em bọc HttpClient qua 1 dàn Proxy luân phiên đổi IP liên tục, bảo đảm chiến dịch đi cào data giấu mặt như Ninja thưa anh."

---

## 🛑 BÀI TOÁN SỐ 3: BÓC TRẦN SỰ THẬT TẦNG LÕI ĐANG CHẠY TRONG ĐỒ ÁN AGRILINK

**Câu hỏi: "Nãy giờ em kể 3 cách, vậy túm lại trong cái file Source Code của AgriLink em rốt cuộc dùng công cụ Cào (Scraper) nào để rọc cái Bảng giá từ chocaphe.vn?"**

**Trả lời (Thực chiến Code C# bằng DOM Parser vững chắc):**
"Dạ báo cáo anh, trong AgriLink để đối phó với trang `chocaphe.vn`, một trang web Render truyền thống, em sử dụng bộ công cụ tiêu chuẩn **`HtmlAgilityPack`** để bóc tách cấu trúc HTML an toàn. Dùng DOM Tree để có sự vững chãi và tránh dễ vỡ code như Regex!

**Luồng rọc HTML vững chãi của em diễn ra như sau:**
* **Bước 1:** Dùng `HttpClient` tải thô toàn bộ mã HTML trang web về thành 1 chuỗi Chuỗi String siêu to.
* **Bước 2 (Dựng Cây DOM):** Bơm đoạn HTML vừa tải đó vào đối tượng `HtmlDocument`. Trình phân tích này sẽ đọc và dựng lên một Cấu trúc Cây Node y như Trình duyệt web tự làm.
* **Bước 3 (Thái Hàng & Cột bằng XPath):** Khi cấu trúc đã nằm gọn, em dùng kỹ thuật XPath: `.SelectNodes("//table//tr")` đi thẳng vào lõi Bảng. Vòng lặp bỏ qua dòng Header đầu tiên, cắm thẳng nhánh chĩa `td` nhặt chính xác Tên Tỉnh ở Cột 1 và Giá Cà phê ở Cột 2. Phương pháp này cực kỳ an toàn, bất chấp họ chèn khoảng trắng hay xuống dòng vì HAP hiểu kiến trúc XML thay vì so sánh Chuỗi!"

**Câu hỏi chốt (Bẫy hầm Redis): "Như em nói lúc nãy, 1 vạn ông Nông Dân vô xem giá thì lấy từ Redis ra. Ủa vậy nếu Redis đang rỗng, cái ông đầu tiên bấm xem thì lấy mẹ gì mà xem? Phải lôi từ Database SQL lên à?"**

**Trả lời (Khóa chặt kịch bản Data Caching):**
"Dạ anh đang vặn em luồng Khởi tạo Caching! Kịch bản em cấu hình là như vầy:
* Cái giá Cà phê này bản chất 1 ngày nó chỉ chốt giá 1 lần (vào Tầm trước 9h sáng).
* Nên em thiết lập **Luồng đằng sau (Background Worker)**: Cứ đúng 9h00 sáng máy chủ em tự nhổ neo đi cào Web đích. Cào xong, nó lấy Mảng Giá Đè vào cặp Key-Value trên RAM thần tốc của **Redis** (Với chìa khóa: `market_prices`), và lưu cả 1 gốc vào SQL Database để vẽ Lịch sử biểu đồ.
* **Suy ra:** Khi ông Nông Dân Số 1 mở App lúc 9h01'. Tủ lạnh Redis ĐÃ CÓ DATA SẴN từ luồng Ngầm chạy hồi 9h00 rồi. Ổng và 9.999 ông Nông Dân theo sau cứ đâm đầu vào Redis lấy cái ịch ra xem trong 0.001 giây (Không rớt xuống Database SQL 1 lần nào nữa).

---

## 🛑 BÀI TOÁN SỐ 4: VẾ ĐỈNH CAO CỦA CRAWLER - PHÒNG THỦ VỚI GIAO DIỆN "HAY ĐỔI"

**Câu hỏi lật ngược (Sát thủ): "Anh thấy em dùng thẻ XPath tĩnh `//table//tr//td` rủi ro quá! Lỡ ngày mai trang web đó nó thiết kế lại giao diện rũ bỏ luôn thẻ `<Table>` thành kiểu lưới Grid `<div><div class='cell'>` thì kiến trúc XPath của em nổ (Null) hết chứ lấy đâu mà cắt?"**

**Trả lời (Phô diễn Hệ thống Fallback (Cứu Hộ) Đa Tầng Của Cấp Bậc Senior):**
"Dạ anh hỏi cực kỳ chí mạng! Quả thật Web bên thứ 3 thay đổi cấu trúc UI thì cái Cây HAP DOM của em sẽ nổ banh xác. Nên trong lõi service của em KHÔNG để hệ thống sụp đổ, mà em thiết lập luồng Cứu Hộ Đa Tầng:

**1. Kích hoạt Lốp Dự Vòng: Trình Duyệt Nội Mạng (Puppeteer Fallback):** 
Ngay khi XPath báo hụt (Rows Null hoặc Count = 0), code của em lập tức quẹo sang Hàm Cấp Cứu: `FetchFromChoCaPheWithPuppeteerAsync()`. 
Nó tự động dựng 1 Trình duyệt Google Chrome ngầm trên máy chủ, mượn cỗ máy V8 Engine của Chrome load luôn Javascript của web người ta, đọc luôn kết quả cuối rồi móc ra giá! Nhờ vậy Web người ta có vẽ ra chiêu trò Javascript mã khóa chống cào, V8 Engine của e vẫn nhai gọn gàng!

**2. Gọi Hệ thống Còi hú & Dữ liệu quá khứ:**
Trường hợp cả Chrome ngầm cũng bó tay báo lỗi 500 do mạng rớt. Khối `Catch` của em sẽ kích hoạt. 
* Lập tức nhả lại: **"Giá của ngày hôm qua"** lấy từ Redis. Chưng lên màn hình kèm chú thích Nhỏ: *"Dữ liệu chưa được chốt phiên mới"*. 
* Ngầm bắn tín hiệu Telegram cảnh báo báo Dev vào cập nhật lại Node XPath. Trải nghiệm màn hình người Nông Dân tuyệt đối không bị Xé Rách hay văng Exception Đỏ Lòm thưa anh!"
---

## 🛑 BÀI TOÁN SỐ 5: LUỒNG GIAO TIẾP GIỮA FE VÀ BE SAU KHI CÀO XONG KHỚP NHAU NHƯ THẾ NÀO?

**Câu hỏi: "Cào cất vô Redis xong rồi. Vậy lúc FE (Ông nông dân) lướt App, nó làm sao biết ông đó ở Tỉnh nào để FE gọi lên BE lấy đúng giá Tỉnh đó? Và cục Response BE trả về mang hình hài cấu trúc ra sao?"**

**Trả lời (Phân tích Data Format và Luồng trích xuất của FE):**
"Dạ cái quy trình lấy giá cào nó đi theo cấu trúc Danh Sách Hàng Loạt rất gọn gàng. Em quy hoạch như sau:

**1. Giai đoạn BE Lặp và Cất (Sau màn Cào sáng 9h):**
Cái hàm Crawler của em nó không chỉ cào đúng 1 tỉnh, mà chạy vòng lặp `for` cào toàn bộ cái Bảng Giá gồm tất cả các tỉnh thành (Đắk Lắk, Lâm Đồng, Gia Lai...).
Hệ thống BE của em nhồi tất cả chúng nó thành 1 mảng (Array) JSON nguyên khối. Em bưng toàn bộ cái Cục Mảng Dữ Liệu đó tống thẳng vào File Redis bằng 1 cái thẻ Key: `"Daily_CoffeePrice_V1"`.

**2. Giai đoạn Tương tác lúc người dùng xài App:**
* **Frontend:** Trên giao diện Nông Dân nó có cái ComboBox (Hộp thả lùi) chọn Tỉnh Thành (Mặc định nó dùng API định vị lấy Tỉnh của ông đó). 
* **Gọi API:** Frontend chỉ gửi đi đúng 1 câu cộc lốc: `GET /api/v1/prices`. FE hoàn toàn KHÔNG CẦN CHUYỀN tham số `?province=DakLak` lên BE. 
* **Backend:** Lấy Request, chọc tay vào Redis vớt cục JSON bự chà bá rớt xuống. BE trả nguyên cái Mảng đó về cho FE cực nhanh:
  ```json
  {
     "success": true,
     "data": [
         { "province": "Đắk Lắk", "price": 120500 },
         { "province": "Lâm Đồng", "price": 119500 },
         { "province": "Gia Lai", "price": 120000 }
     ]
  }
  ```

**3. Khúc chốt Hạ tại FE:**
Thằng ReactJS (App) nhận nguyên cái Mảng này. Lúc này ông Nông dân chọn tỉnh "Lâm Đồng", code Frontend chỉ việc lấy mảng đó dùng lệnh `find()` hoặc Filter tìm chữ "Lâm Đồng" và chưng số `119.500` lên màn hình!

**Tại sao BE không tự Lọc giùm FE?**
Nếu anh vặn em là: *Sao bắt FE tự lọc? Khó quá vậy?*
Thì em đáp luôn: *"Dạ cái danh sách này có tầm chục tỉnh (Dung lượng file JSON chỉ có hơn 1 KB xíu). Bắt Backend lọc từng Tỉnh trả về là quá tốn Tài Nguyên Tính Toán Server vô ích! Thà bung mảng 1 KB xuống RAM thiết bị Nông Dân, để Frontend tự Filter thì Server BE nhẹ gánh hàng ngàn lần thưa anh!"*. 
Thiết kế này là chuẩn mực của Trải Nghiệm Offline-First (Lấy 1 lần, lọc được ngàn lần) ạ."

---
"Dạ, dữ liệu đi từ HttpClient về thực chất chỉ là một cục String (chuỗi văn bản phẳng). Thay vì chém chuỗi thô bằng Regex cực kỳ dễ vỡ nếu web nguồn chèn thêm khoảng trắng, em dùng HtmlAgilityPack đóng vai trò làm Trình Phân Tích (Parser). Khi em feed (bơm) cục String đó vào LoadHtml(), thư viện sẽ tự động phân tách và quy đổi chuỗi phẳng thành cấu trúc cây DOM Tree đa chiều trong bộ nhớ RAM. Lúc này, em thay vì đi lùng sục từng ký tự, em chỉ cần chĩa lệnh XPath đi dọc theo các nhánh cây từ table rẽ chĩa xuống tr và td y như cách Trình duyệt web thao tác với Javascript. Cách giải quyết vấn đề bằng 'mô hình dữ liệu có cấu trúc Node' này giúp server scraping hiếm khi bị nổ code hay NullReference!"
## 🛑 BÀI TOÁN SỐ 6 (PHỤ LỤC): GIẢI CHI TIẾT BẢN CHẤT VẬT LÝ CỦA "HTTPCLIENT" VÀ "DỰNG CÂY DOM"

**Câu hỏi sâu (Deep Dive): "Em giải thích cụm từ: Dùng HttpClient lấy thô HTML, sau đó Bơm vào HtmlDocument để Dựng Cây DOM y như trình duyệt web nghĩa là sao?"**

Để đi phỏng vấn tự tin và lột tả được cái "chất" cốt lõi của lập trình viên, bạn cần hiểu bản chất vật lý của những công cụ này:

### 1. `HttpClient` là gì?
Tưởng tượng `HttpClient` giống như **một trình duyệt web (Google Chrome) bị mù**. 
* Khi gõ url lên Chrome, Chrome làm 2 việc: (1) Gọi điện hỏi xin máy chủ trả về file HTML. (2) Vẽ (Render) cái file đó thành màu sắc và khối hình lên màn hình.
* **`HttpClient` trong C#** chỉ làm được đúng **việc số 1**. Nó là cỗ máy đi "gọi điện thoại" (gửi HTTP Request) đến mạng, bê nguyên một cục Text (văn bản) HTML thô kệch đi về. Nó vô hồn và không biết giao diện giao diện là gì. Kết quả của `HttpClient` trả về chỉ là một Chuỗi (String) chữ siêu dài chứa toàn mã ngoặc nhọn `<đầu>, <đuôi>`.

### 2. "Bơm" là sao? Và "Dựng Cây Node" (DOM) là làm cái gì?
Chữ **"Bơm"** ở đây là hành động **Truyền dữ liệu** qua hàm `htmlDoc.LoadHtml(chuỗi_HTML)`. 

* Nếu để nguyên Chuỗi văn bản thô (String) dài ngoằng kia mà đi dò tìm dữ liệu thì rất khổ (ví dụ dùng Regex chém chuỗi rất giòn và dể gãy vỡ).
* Điểm đặc biệt của mã HTML là nó có tính **Kế thừa và Bao bọc**. Ví dụ: Ngôi nhà `<body>` chứa cái Bàn `<table>`, cái Bàn chứa Ngăn kéo `<tr>`, Ngăn kéo chứa Tiền `<td>`.

Khi ta "Bơm" chuỗi Text thô đó vào bụng của `HtmlAgilityPack`, Trình phân tích cú pháp (Parser) của nó sẽ làm việc:
1. Đọc từng ký tự từ đầu đến cuối. Cứ thấy dấu `<` mở ra, nó hiểu đó là một **Cành cây (Node)**.
2. Nó từ từ gom các cành cây lại, tạo ra một sơ đồ Phả hệ đa chiều trong bộ nhớ RAM máy tính. Sơ đồ này gọi là **DOM Tree** (Document Object Model). 

**Kết quả nhận được:**
Toàn bộ chuỗi văn bản phẳng hỗn độn biến thành một "Cái Cây Gia Phả" cực kỳ quy củ:
`Cụ Tổ (Document)` $\rightarrow$ `Ông Nội (html)` $\rightarrow$ `Cha (body)` $\rightarrow$ `Con (table)` $\rightarrow$ `Cháu (tr)` $\rightarrow$ `Chắt (td)`.

### 3. Tại sao nói nó "y như Trình duyệt web tự làm"?
Khi Chrome tải HTML về, nhiệm vụ TỐI MẬT đầu tiên của bộ não Chrome (V8 Engine/Blink) trước khi cho phép quét sơn màu lên màn hình chính là chạy bộ **Parser để dựng ra cây DOM** y hệt như trên. `HtmlAgilityPack` chính là một phần mềm **bắt chước hoàn hảo** lại thuật toán giải phẫu đó của Trình duyệt nhưng chạy ngầm tĩnh lặng trong Server C#.

### 💡 BÍ KÍP ĐỠ ĐÒN TRONG PHỎNG VẤN (TỎA SÁNG)
> *"Dạ, dữ liệu đi từ HttpClient về thực chất chỉ là một cục String (chuỗi phẳng). Thay vì chép chuỗi thô bằng Regex rất giòn, dễ gãy, em dùng `HtmlAgilityPack` đóng vai trò làm Trình Phân Tích (Parser). Khi em feed cục String đó vào hàm `LoadHtml()`, lệnh này sẽ bẻ dẹt chuỗi phẳng và quy đổi nó thành cấu trúc Cây DOM Tree đa cấp đan lưới trong bộ nhớ RAM. Nhờ vậy, em không cần phải lùng sục Text chữ nữa, mà em dùng kỹ thuật 'XPath' đi dọc theo các nhánh cây từ `table` rẽ chĩa xuống `tr` và `td`, điều mà các Engine Trình duyệt web sử dụng mỗi ngày. Kiến trúc này giúp luồng scraping vững như bàn thạch và dẹp bỏ sạch lỗi gãy mã do rác giao diện trộn vào ạ!"*
Dòng số 1: (HttpClient "hỏi xin" HTML)

csharp
var response = await httpClient.GetStringAsync(url);
Ở dòng này, HttpClient đóng vai trò là một cái máy khoan. Nó phi thẳng tới URL chocaphe.vn, gõ cửa máy chủ bên kia, và tải toàn bộ mã HTML thô về. Cục mã HTML lúc này đang nằm gọn trong biến response (chỉ là một chuỗi văn bản String).

Dòng số 2: (Bơm vào HtmlDocument để dựng cây)

csharp
var htmlDoc = new HtmlDocument();
htmlDoc.LoadHtml(response);
Ở dòng này, bạn khởi tạo cái Đáy (Vỏ bọc) là HtmlDocument. Sau đó, lệnh LoadHtml(response) chính là hành động "bơm/nhồi" toàn bộ cái chuỗi thô vừa tải về đó vào trong. Lập tức, bộ não Parser của HtmlDocument sẽ chạy từ trên xuống dưới, bẻ các thẻ <> để dựng lên một cái Cây DOM (tức là cấu trúc đa chiều) trong RAM y hệt Trình duyệt!

Bạn hiểu rất đúng mạch chạy rồi đó. Khi đi phỏng vấn, giải thích bằng ngôn ngữ mô tả quy trình (hỏi xin -> tải text thô -> bơm vào doc -> dựng cây Node) như thế này sẽ khiến các sếp ngồi dưới rất sướng tai vì thấy được bạn hiểu rất sâu vòng đời của dữ liệu!