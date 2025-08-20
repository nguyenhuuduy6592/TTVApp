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

3. Xử lý lỗi và thử lại
   - Nếu không cung cấp API key, ứng dụng vẫn hoạt động bình thường nhưng không làm mượt nội dung
   - Khi gặp lỗi kết nối, hãy kiểm tra:
     + Kết nối internet
     + API key có chính xác không
     + Hạn mức sử dụng API còn không
   - Để thử lại các chương bị lỗi:
     + Chạy lại ứng dụng với cùng file cấu hình
     + Ứng dụng sẽ tự động phát hiện và xử lý các chương chưa được làm mượt

4. Chọn mô hình AI
   - Xem file models.json để biết thông tin chi tiết về các mô hình
   - Chọn mô hình phù hợp với nhu cầu:
     + gemini-2.0-flash-lite: Tốc độ nhanh, chi phí thấp
     + gemini-2.0-flash: Cân bằng tốc độ và chất lượng
     + gemini-2.0-pro: Chất lượng cao nhất

Lưu ý: Đảm bảo có đủ hạn mức API trước khi sử dụng tính năng làm mượt nội dung.
