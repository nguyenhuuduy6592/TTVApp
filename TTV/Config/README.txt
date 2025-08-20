Hướng dẫn sử dụng TTV App với Gemini AI

1. Cấu hình ứng dụng
   - Tạo file config.json trong thư mục chạy ứng dụng
   - Điền thông tin cần thiết theo mẫu bên dưới
   - Nếu muốn sử dụng Gemini AI, thêm API key vào phần cấu hình

Mẫu config.json:
{
    "userId": "your-user-id",
    "token": "your-token",
    "storyId": "story-id",
    "outputFileName": "output.html",
    "startChapter": 1,
    "endChapter": 10,
    "gemini": {
        "apiKey": "your-api-key",
        "modelId": "gemini-2.0-flash"
    }
}

2. Chạy ứng dụng
   TTV.exe config.json

3. Xử lý lỗi và hướng dẫn khắc phục

3.1. Lỗi API Key
    - Thông báo: "API Key không hợp lệ"
    - Nguyên nhân:
      + API key không chính xác hoặc hết hạn
      + API key bị vô hiệu hóa
    - Cách khắc phục:
      + Kiểm tra API key trong file config.json
      + Tạo API key mới từ Google AI Studio (https://makersuite.google.com/app/apikey)
      + Đảm bảo API key được sao chép đầy đủ và chính xác

3.2. Lỗi hạn mức sử dụng
    - Thông báo: "Đã vượt quá giới hạn sử dụng API"
    - Nguyên nhân:
      + Đã sử dụng hết hạn mức miễn phí
      + Đạt giới hạn tốc độ gọi API
    - Cách khắc phục:
      + Chờ đến khi hạn mức được làm mới (thường là 24h)
      + Nâng cấp lên gói trả phí
      + Chuyển sang mô hình tiết kiệm hơn (gemini-2.0-flash-lite)

3.3. Lỗi kết nối mạng
    - Thông báo: "Lỗi kết nối mạng"
    - Nguyên nhân:
      + Mất kết nối internet
      + Tường lửa chặn kết nối
      + DNS không phân giải được
    - Cách khắc phục:
      + Kiểm tra kết nối internet
      + Thử kết nối qua VPN
      + Kiểm tra cấu hình tường lửa
      + Thử đổi DNS (ví dụ: 8.8.8.8 hoặc 1.1.1.1)

3.4. Thử lại chương bị lỗi
    - Ứng dụng tự động thử lại 3 lần với thời gian chờ tăng dần
    - Nếu vẫn thất bại:
      + Chạy lại ứng dụng với cùng file cấu hình
      + Ứng dụng sẽ tự động phát hiện và xử lý các chương chưa được làm mượt

4. Hướng dẫn chọn mô hình AI

4.1. So sánh các mô hình

gemini-2.0-flash-lite:
    - Ưu điểm:
      + Tốc độ xử lý nhanh nhất
      + Chi phí thấp nhất ($0.075/1M token input, $0.30/1M token output)
      + Phù hợp cho dự án nhỏ hoặc thử nghiệm
    - Nhược điểm:
      + Chất lượng văn bản ở mức cơ bản

gemini-2.0-flash:
    - Ưu điểm:
      + Cân bằng giữa tốc độ và chất lượng
      + Chi phí hợp lý ($0.15/1M token input, $0.45/1M token output)
      + Phù hợp cho hầu hết trường hợp
    - Nhược điểm:
      + Đôi khi cần thử lại với đoạn văn phức tạp

gemini-2.0-pro:
    - Ưu điểm:
      + Chất lượng văn bản cao nhất
      + Xử lý tốt các đoạn văn phức tạp
      + Giữ được phong cách và cảm xúc của truyện
    - Nhược điểm:
      + Chi phí cao nhất ($0.25/1M token input, $0.75/1M token output)
      + Tốc độ xử lý chậm hơn

4.2. Gợi ý lựa chọn
    - Dự án nhỏ/thử nghiệm: gemini-2.0-flash-lite
    - Sử dụng thường xuyên: gemini-2.0-flash
    - Yêu cầu chất lượng cao: gemini-2.0-pro

Lưu ý: 
- Nếu không cung cấp API key, ứng dụng vẫn hoạt động bình thường nhưng không làm mượt nội dung
- Đảm bảo có đủ hạn mức API trước khi sử dụng tính năng làm mượt nội dung
- Có thể thay đổi mô hình bất kỳ lúc nào bằng cách cập nhật file config.json
