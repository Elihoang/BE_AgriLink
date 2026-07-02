# 🎧 TÍNH NĂNG CHUYỂN ĐỔI BÀI BÁO THÀNH GIỌNG NÓI (NATIVE FRONTEND TTS) 

Tính năng "Đọc Bài Viết cho Nông Dân" trong đồ án AgriLink là một "Bẫy Chuyên Môn" cực hay để rẽ hướng buổi phỏng vấn sang bài toán **Tối ưu chi phí Khởi nghiệp (Zero-Cost MVP Architecture)**. 

Thay vì vung tiền thuê máy chủ AI đắt đỏ, hệ thống AgriLink sử dụng chiến thuật "Mượn Lực" vô cùng thông minh: Ép chính điện thoại của Nông dân tự đọc tiếng Việt!

Dưới đây là kịch bản mang đi phỏng vấn để giám khảo phải "Wow" về tư duy Kiến Trúc Tối Ưu Tiền Bạc của bạn.

---

## 🛑 BÀI TOÁN SỐ 1: BẢN CHẤT VẬN HÀNH CỦA TÍNH NĂNG ĐỌC BÁO

**Câu hỏi: "Tính năng chuyển Text thành Audio trong AgriLink em tích hợp API của Viettel AI / FPT hay xử lý ở Backend ra sao?"**

**Trả lời (Phủ định sự cồng kềnh, Khẳng định cơ chế 0 Đồng - Zero Cost):**
"Dạ nếu là một hệ thống Doanh nghiệp lớn có doanh thu, em sẽ đẩy phần xử lý xuống Backend bằng API Viettel/AWS. Nhưng với AgriLink đang là bản chạy thử nghiệm (MVP), tiêu chí số 1 của em là **Chi Phí Bằng 0 (Zero-cost)**.

Nên em dứt khoát không setup tính năng Đọc (TTS) này dưới Backend. Backend C# của em nhẹ như lông hồng, chỉ trả nguyên cái Chuỗi Văn Bản (Text) về cho Frontend. 
Ở mặt trận Frontend (ReactJS), em khai thác trực tiếp **Web Speech API (`window.speechSynthesis`)** - vốn dĩ được gắn sẵn dưới đáy Hệ điều hành của khách.
Nghĩa là: Khi ông Nông Dân bấm nút Play, React của em sẽ đánh thức Mạng lưới AI có sẵn trong máy ổng (Ví dụ: Siri của iOS hoặc Google Voice của Android) để tự phát âm đọc bài báo. Em lấy mỡ khách hàng rán khách hàng, Server AgriLink không tốn 1 xu tiền phí AI nào thưa anh!"

---

## 🛑 BÀI TOÁN SỐ 2: KHẮC PHỤC GIỚI HẠN DUNG LƯỢNG ĐỌC (CHUNKING)

**Câu hỏi vặn (Sát thủ): "Đồng ý là em dùng `window.speechSynthesis`. Nhưng cái hàm này của trình duyệt nó bị lỗi chí mạng là: Trót nhét 1 bài báo dài quá 500 chữ vào, nó đang đọc giữa chừng bị Tắt Tiếng (Ngắt họng) hoặc báo lỗi. Dân Dev Frontend nào cũng bị dính, em fix chỗ đó kiểu gì?"**

**Trả lời (Phô diễn Code thuật toán Chia Nhỏ - Chunking trong file AudioPlayer.jsx):**
"Dạ đúng anh! Trình duyệt nó rất hay bị đứt bộ nhớ đệm nếu mình nhét 1 file Text rác quá dài vào lưỡi của nó. 
Nên trong file `AudioPlayer.jsx` của UI AgriLink, em KHÔNG BAO GIỜ nhét nguyên cả bài báo vào lệnh `speechSynthesis.speak()`. Mà em thiết kế **Thuật toán Băm Nhỏ Câu (Chunking)**:

1. **Rọc văn bản:** Em dùng Regex `content.match(/[^.!?\n]+[.!?\n]*/g)` để chặt bài báo ra thành một Mảng (Array), đứt ranh giới tại các dấu chấm `.` hoặc dấu chấm hỏi `?`. 
2. **Đọc Nối Tiếp (Queue):** Em thả cái mảng đó vào vòng lặp (Đầu đĩa). Đọc hết câu số 1, hàm Event Trigger `utterThis.onend = () => { speakNextChunk(); }` của em mới chớp lấy thời cơ đút câu số 2 vào mồm Trình duyệt để đọc tiếp.
Nhờ thuật toán bón từng muỗng này, Nông dân có thể nghe bài báo dài 10 vạn chữ máy cũng không bao giờ bị đứt bộ đệm hay treo Ram!"

---

## 🛑 BÀI TOÁN SỐ 3: BẪY GIỌNG "TIẾNG ANH" TRÊN MÁY TÍNH

**Câu hỏi: "Ủa em gọi chung chung hàm `speechSynthesis`, rủi cái điện thoại ổng đang cài Tiếng Anh, nó lấy giọng ông Tây bưng chữ Tiếng Việt lên đọc thành tiếng ngoài hành tinh thì sao?"**

**Trả lời (Logic Bắt Giọng Tiếng Việt Thông Minh):**
"Dạ chỗ này em chặn ngay từ trong trứng nước rồi anh! Nhét text tiếng Việt mà để ông Tây đọc là lỗi UX rất ngớ ngẩn. 
Trong code của em, trước khi ra lệnh `Speak()`, em gọi hàm `window.speechSynthesis.getVoices()` quét toàn bộ kho giọng nói trong con máy điện thoại đó. 
Em dùng hàm Find lùng sục: Giọng nào có mã `vi-VN` hoặc có chữ `vietnamese` / `tiếng việt` thì em mới **Ép cái loa (utterThis.voice)** phải ngậm cái giọng đó lên.
Nếu quét xong Không Thấy Giọng Tiếng Việt nào trên điện thoại đó (Máy hệ điều hành xách tay Mỹ lạc hậu), Hệ thống sẽ bật cảnh báo màu cam (Mantine Notification): *'Thiết bị trinh duyệt của bạn chưa cài gói giọng Tiếng Việt, hãy bật trong cài đặt...'* chứ dứt khoát không ép Tây đọc tiếng Việt anh ạ!"

---

Góc Tư Vấn: *(Khúc này anh mang đi Phỏng vấn, đảm bảo anh phô diễn được tư duy của một Người Làm Sản Phẩm Hiện Đại. Biết tùy biến linh hoạt (Frontend xử lý) để bảo vệ túi tiền của Backend trong giai đoạn Khởi nghiệp. Ông Giám khảo nào nghe kịch bản này cũng phải gật gù khen anh rất thực tế và lọc lõi).*
