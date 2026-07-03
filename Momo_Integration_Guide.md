# 💸 Kiến Trúc Tích Hợp Thanh Toán MoMo (Momo Payment Gateway)

Tài liệu này trình bày chi tiết luồng nghiệp vụ và cách hệ thống kỹ thuật phân tách giữa "Giao diện (FE)" và "Cơ sở dữ liệu (BE)" để đảm bảo quy trình thanh toán cực kỳ bảo mật (Webhook IPN).

---

## 1. QUY TRÌNH NGHIỆP VỤ (BUSINESS FLOW)
Dự án ứng dụng MoMo vào tính năng **Thanh toán Lương (Salary Payment)**:
1. Admin ấn Thanh Toán Lương $\rightarrow$ Hệ thống tính ra "Số tiền thực lĩnh".
2. Hệ thống đẩy Admin sang màn hình App MoMo / Web MoMo. Admin tiến hành quẹt mã QR và trừ tiền.
3. Admin nhận thông báo Thành công trên màn hình, bị đá về lại trang hệ thống Lương.
4. Hệ thống ngầm cập nhật Database: Chuyển Lương thành `Success` và tự động tìm các khoản Nợ tạm ứng của nhân viên đó để gạch bỏ (`IsDeducted = true`).

---

## 2. CHUỖI THỜI GIAN (TIMELINE FLOW) - TỪ LÚC TẠO REQUEST ĐẾN LÚC KẾT THÚC
Để dễ hình dung tổng thể, đây là trục thời gian theo thứ tự logic của một đơn thanh toán Lương:
1. **[Ngưởi dùng]** Click nút "Thanh TOÁN MoMo" trên màn hình.
2. **[FE]** Gọi API sang **[BE]** (Backend) gửi yêu cầu: Khởi tạo thanh toán Lương cho ông Nhân công A.
3. **[BE] Ghi trước vào Database (Khởi tạo Pending):** BE tính toán số tiền, sau đó lập tức tạo một bản ghi `SalaryPayment` lưu thẳng xuống Database mang trạng thái **`Pending`** (hoặc `Processing`). Khúc này sinh ra một cái mã ID duy nhất (Ví dụ: `OrderId = DonLuong_100`). Bước này CỰC KỲ QUAN TRỌNG làm gốc để đối chiếu gạch nợ sau này.
4. **[BE] Gọi sang MoMo:** BE lấy cái `OrderId` vừa lưu kia, cấu trúc chung với số tiền thành chuỗi JSON và kẹp thêm Chữ ký số (HMAC-SHA256). Kế đó, BE gọi HTTP POST bay thẳng sang **[Máy chủ MoMo]** xin cấp quyền thu tiền ứng với `OrderId` này.
5. **[Máy chủ MoMo]** Đồng ý, sinh ra một cái Link điều hướng (`payUrl`). **[BE]** lấy cái Link đó ném ngược trở lại ra thẻ Network cho **[FE]**.
6. **[FE]** Lấy được Link, dùng javascript ép trình duyệt của khách hàng nảy trang (Redirect) sang hẳn sân vận động đỏ chót của MoMo.
7. **[Người dùng]** Bật quét QR và móc hầu bao trả tiền trên trang MoMo thành công.
8. Đúng tại khoảnh khắc Tiền vừa trừ này, luồng đi bị **CHIA THÀNH 2 NGÃ RẼ XẢY RA CÙNG MỘT LÚC** (Luồng hiển thị múa may và Luồng Dữ liệu pháp lý):
   - **Ngã 1 (Báo cáo Frontend):** MoMo đá (Redirect) trình duyệt khách hàng đáp cánh về nhà Website của mình và tự ghi kết quả lên thanh Địa Chỉ. FE mổ xẻ thanh địa chỉ URL ra -> Thấy số dư nên hiện hình Mặt Cười/Màu Xanh lá cây!
   - **Ngã 2 (Báo cáo Backend):** Không liên quan gì tới trình duyệt của khách. MoMo tự dùng cáp mạng riêng nối vào tận giường của Server Backend (gọi là Webhook IPN). Backend tiến hành phân tích chữ ký số bảo mật, duyệt hồ sơ rồi móc SQL thay đổi Database Lưu trữ vĩnh viễn. Đơn hàng kết thúc!

---

## 3. KHỞI TẠO ĐƠN: SỰ TỒN TẠI CỦA "2 CON ĐƯỜNG URL" CHẠY SONG SONG
Điểm mấu chốt gây lú trong tích hợp thanh toán là mọi người thường gộp chung trả kết quả làm 1 nhánh (Ngã 1 và 2 nói trên). Thực tế, ở ngay cái Bước số 3 (Lúc gọi xin MoMo), BE đã phải cấu hình và nhét vào họng MoMo **2 đường link (URL) hoàn toàn khác nhau** để MoMo dội bom theo 2 Ngã:

1. **`redirectUrl` (Là Ngã 1 - Đường trả khách về Frontend):** Link này để dẫn khách hàng từ trang MoMo quay về lại Website của mình để xem giao diện thành công.
   *(Ví dụ: `http://localhost:5173/ket-qua-luong`)*
2. **`ipnUrl` (Là Ngã 2 - Đường gọi thầm Webhook về Backend):** Link API phía Server này dành riêng cho Tổng đài MoMo gọi ngầm đến để báo cáo Database. Khách hàng không bao giờ thấy Link này.
   *(Ví dụ: `https://api.agrilink.com/api/momo/callback`)*

---

## 4. "BỀ NỔI": FRONTEND NHẬN THÔNG TIN TỪ `RedirectUrl` NHƯ THẾ NÀO? (MINH HỌA)

Đây là quá trình giúp hiển thị giao diện báo hiệu cho khách xem (Ngã 1), **tuyệt đối không dùng để lưu Database**.

1. **Cú đá trang (Redirect):** Sau khi khách quẹt thẻ QR trên màn hình MoMo xong, MoMo xử lý và ép trình duyệt Web của khách hàng nhảy về lại cái nhà FE (`redirectUrl`).
2. **Cách MoMo báo thông tin cho FE:** 
    Khi ép khách nhảy về nhà, **Server MoMo tự động viết thêm tham số kết quả vào đuôi URL**.
    > **Ví dụ URL trả về ngự trên màn hình:**
    >
    > `http://localhost:5173/ket-qua-luong?orderId=Don_123&amount=5000` **`&resultCode=0`** `&message=Thanh_Cong...`
3. **Frontend đoạt tham số hiển thị UI:** 
    Khi trang FE tải lên, Code Javascript không cần gọi Backend, nó chỉ việc dùng hàm cắt chuỗi trên thanh địa chỉ kia ra: Lấy chữ `resultCode`, thấy bằng Số `0` thì FE hiểu là thành công, liền phun pháo hoa và bật giao diện Màu Xanh.  
    *(Lưu ý: FE làm bước này chỉ để vẽ màn hình cho đẹp. FE KHÔNG HỀ cập nhật Database, vì Hacker có thể lấy chuột sửa dãy URL kia thành `resultCode=0` để lừa FE báo xanh).*

---

## 5. "TẢNG BĂNG CHÌM": BACKEND XÁC THỰC BẢO MẬT GHI DATABASE QUA `IpnUrl` (CALLBACK) NHƯ THẾ NÀO?

Vì Frontend không đáng tin cậy do bị phơi bày trên trình duyệt, việc ghi nhận Status Lương (Success/Failed) và gạch nợ Tạm ứng bắt buộc tuân theo (Ngã 2) do **Backend cầm trịch 100%**.

**Bước 1: MoMo gọi ngầm cho BE**
Ngay khoảnh khắc quẹt QR thành công, Song song với việc đá trình duyệt khách về FE, Tổng đài máy chủ MoMo dùng mạng riêng gửi một Request `POST` cực mạnh bằng Data JSON ngầm đâm thẳng vào cái cổng lưng API `ipnUrl` (`/api/momo/callback`) của BE. 

**Bước 2: Backend Xác thực tính Chính Chủ (Verify Signature)**
Hacker có thể tự Code 1 con tool nhỏ giả vờ mạo danh Server MoMo gửi Request bắn vào cổng API `ipnUrl` của mình báo gạch nợ để lừa bịp BE! **Làm sao BE biết đúng là máy chủ MoMo gọi đến?**

Đó là lúc phép thử Phân Giải Chữ Ký Số xuất hiện tại `MomoCallbackController.cs`:
1. Trong gói JSON gửi ngầm tới có một trường gọi là: `"signature": "kjasdn12093...1"` do MoMo sinh ra.
2. BE gom cục JSON gửi tới, băm trộn tất cả các thuộc tính (`amount, orderId...`) ghép nối theo vần Alphabet Bảng chữ cái.
3. BE lấy mảnh chìa khóa bí mật **`SecretKey`** (Chỉ duy nhất Lập trình viên BE và MoMo có) để khóa và băm tạo thành 1 mã `HMAC-SHA256`. 
4. **Phán Quyết:** BE đem chuỗi băm của mình so sánh với cái `"signature"` của gói thư gửi tới. Giống hệt 100% $\rightarrow$ Chắc chắn Data này đi ra từ tổng bộ MoMo (Hàng xịn). Lệch 1 kí tự $\rightarrow$ Hàng mạo danh, chối từ Request.

**Bước 3: Thao tác Database khép kín**
Qua cửa xác thực mạo danh, lúc này BE mới soi cái cờ `resultCode` trong JSON MoMo gửi:
- Nếu bằng `0`: Gọi Entity Framework (`_salaryPaymentRepository`) sang cờ `Success` và vòng lặp `Worker Advance` cập nhật `IsDeducted = True`.
- `_unitOfWork.SaveChangesAsync()` dội đinh xuống Database Postgres. Nghiệp vụ hoàn toàn kết thúc!.

---

## 🔥 PHẦN 6: BỘ CÂU HỎI PHỎNG VẤN TRỊ GIÁ ĐIỂM 10 

Đây là các câu hỏi cực sâu về ngách tích hợp cổng thanh toán (Payment Gateway):

**Q1: Cái cơ chế Callback (IPN) của MoMo, nhỡ Server FE hoặc khách hàng đang thanh toán mà sụp Wifi tắt tab trình duyệt ngang thì sao?**
> **Trả lời:**
> "Dạ chẳng sao cả! Vì luồng hiển thị FE và luồng xử lý BE cách ly nhau. Khách có thể tắt Tab trình duyệt không thèm quay về FE coi kết quả. Nhưng Máy móc của MoMo vẫn gọi API thầm (IPN) đến Server BE của em. Database vẫn được cập nhật thành công hoàn hảo."

**Q2: Nếu đang chạy IPN thì đứt mạng diện rộng, Backend nằm chết, MoMo gọi đến ngầm mà Backend không trả lời kịp thì đơn hàng đó treo vĩnh viễn à?**
> **Trả lời:**
> "MoMo có hệ thống Retry - Gọi lại tự động. Nếu IPN của em quá tải không phản hồi mã trạng thái 200 OK ngay, tụi MoMo sẽ tự dọng dội gọi Callback lại sau 5 phút. Ngoài ra hệ thống BE của em có gắn một hàm chủ động chủ động `QueryDisbursementAsync`. Nửa ngày trôi qua mà đơn vẫn ngâm, bên kế toán chỉ việc ấn nút, BE sẽ chủ động chọc API móc thông tin về bù đắp là xong."

**Q3: Kẻ xấu biết em đang dùng HMAC-SHA256. Lỡ tụi nó rình hoặc nội bộ xì cái `SecretKey` trên mạng. Tụi Hacker tự dùng Postman sinh Signature gọi Fake IPN báo thành công liên tục thì tính sao?**
> **Trả lời:**
> * Quả thực lộ `SecretKey` là tai họa. Để bảo vệ mã này, em lưu trữ kín trong file cấu hình máy chủ Appsettings / Environment biến Docker chứ tuyệt đối không Upload Github public. 
> * Lớp phòng thủ số 2 chặn đứng Hacker đó chính là Firewall / IP Whitelisting. Cổng API IPN của hệ thống em được set up trên tường lửa chặn mọi truy cập, chỉ cho phép Traffic đến từ những cái IP Server chính chủ của tập đoàn MoMo. Hacker ở nhà cắm mạng VNPT gọi lên là ăn lệnh chặn 403 Access Denied ngay từ ngoài cổng mạng chứ chưa kịp chạm vào cái Controller IPN của em đâu ạ!

**Q4: Quá trình Payment IPN báo "Success", trong hàm BE em đem đi xóa nợ Tạm Ứng `WorkerAdvances` của nhân viên đó thàn `IsDeducted = Thực`. Lỡ lúc đang lặp xóa nợ, cúp điện cái Rụp. BE văng Exception thì Tình trạng dữ liệu sẽ rối rắm ra sao?**
> **Trả lời:**
> * Dạ vấn đề này đã bị em chặn đứng bằng **Unit of Work (Transaction Pattern)**. 
> Toàn bộ quá trình: *(1) Update Trạng Thái Status Lương + (2) Vòng Lặp thay đổi `IsDeducted`* được em gom vào và thao tác trên bộ nhớ trung gian của Entity Framework. 
> * Mọi thứ chỉ được đóng đinh xuống PostgreSQL nếu cái dòng dưới cùng `await _unitOfWork.SaveChangesAsync();` kích hoạt. Một khi Code giật Exception giữa màn kịch lặp, mọi thứ **Rollback** quay về nguyên trạng cũ tinh khôi. Từ đó Database luôn bất biến và Không bao giờ có hiện tượng Dữ Liệu nửa mùa (Lương Success nhưng chưa gạch nợ). Rất an toàn!

---

## 🎯 PHẦN 7: ÁNH XẠ KIẾN TRÚC CODE (CLASS & METHOD CỤ THỂ TRONG DỰ ÁN)

Để chứng minh cho nhà phỏng vấn thấy mình là người tự tay Code và hiểu lõi của AgriLink Project, hãy chỉ điểm đích danh các File và Hàm chịu trách nhiệm cho các luồng trên:

**1. Khởi tạo Pending & Lưu Database (Nguồn gốc `OrderId`):**
> * File: `AgriLink_DH.Core/Services/SalaryPaymentService.cs`
> * Hàm: `ExecutePaymentAsync(ExecutePaymentRequestDto request)`
> * Nhiệm vụ: Ghi bản ghi Lương (`SalaryPayment`) mang trạng thái `Status = SalaryPaymentStatus.Pending`. Tự sinh `MomoOrderId` duy nhất (Ví dụ: `SALARY_2026...`).

**2. Khởi tạo Signature & Gọi API `/v2/gateway/api/create` của MoMo:**
> * File: `AgriLink_DH.Core/Services/RealMomoService.cs`
> * Hàm: `SendDisbursementAsync(...)`
> * Nhiệm vụ: Lấy `MomoOrderId` từ Bước 1, nối và xào nấu các Key theo bảng chữ cái. Băm với `HMAC-SHA256` cùng `SecretKey` (Lấy từ `appsettings.json` thông qua `IOptions<MomoSettings>`). Sau khi gọi HttpClient sinh ra được `payUrl` đưa về cho FrontEnd vút đi.

**3. Đón lõng IPN & Xác thực Cờ "Thành Công":**
> * File: `AgriLink_DH.Api/Controllers/MomoCallbackController.cs`
> * Hàm: `HandleIpn([FromBody] JsonElement body)`
> * Nhiệm vụ: Đoạt lấy cục JSON thầm lặng từ MoMo gửi về. Viết hàm cục bộ `Verify(rawSig, signature)` để băm lại HMAC-SHA256. Lệch là `BadRequest` ngay lập tức.

**4. Dội Database & Gạch Nợ (Unit Of Work):**
> * File: `AgriLink_DH.Api/Controllers/MomoCallbackController.cs`
> * Hàm: `UpdatePaymentStatusAsync` (Gắn bên trong `HandleIpn`)
> * Nhiệm vụ: Sử dụng Idempotency check để chặn cập nhật đúp 2 lần `(if payment.Status != Pending return)`. Cập nhật `SalaryPayment` sang `Success`. Lôi CSDL bảng `WorkerAdvance` ra chốt gạch nợ `IsDeducted = True`. Quét toàn khối thay đổi vào `_unitOfWork.SaveChangesAsync()`.

**5. Giao Diện Frontend bóc tách kết quả từ URL:**
> * File: `ui-agrilink/src/pages/SalaryPayments/PaymentResultPage.jsx`
> * Hook: `useSearchParams()` (Của thư viện `react-router-dom`)
> * Nhiệm vụ: Sau khi MoMo kích hoạt Redirect đá URL về trang này, Component khởi chạy và lập tức dùng hàm `searchParams.get('resultCode')`. Nếu kết quả là `0`, màn hình Render Component `<IconCheck>` màu xanh lá bảo chứng thành công. Đồng thời có gọi ngầm về `/momo/update-status` (Áp dụng cho môi trường Dev Localhost).
