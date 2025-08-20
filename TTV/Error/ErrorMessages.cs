using System;

namespace TTV.Error
{
    public class EnhancementException : Exception
    {
        public ErrorType Type { get; }
        public string Resolution { get; }

        public EnhancementException(ErrorType type, string message, string resolution) 
            : base(message)
        {
            Type = type;
            Resolution = resolution;
        }
    }

    public enum ErrorType
    {
        ApiKeyInvalid,
        QuotaExceeded,
        NetworkError,
        EnhancementFailed,
        ConfigurationError
    }

    public static class ErrorMessages
    {
        public static class Api
        {
            public static readonly (string Message, string Resolution) InvalidApiKey = (
                "API Key không hợp lệ.",
                "Vui lòng kiểm tra lại API key trong file cấu hình config.json. " +
                "API key có thể lấy từ Google AI Studio (https://makersuite.google.com/app/apikey)."
            );

            public static readonly (string Message, string Resolution) QuotaExceeded = (
                "Đã vượt quá giới hạn sử dụng API.",
                "Vui lòng thử lại sau hoặc nâng cấp gói dịch vụ của bạn tại Google AI Studio. " +
                "Bạn cũng có thể tạm thời sử dụng mô hình gemini-2.0-flash-lite để tiết kiệm hạn mức."
            );

            public static readonly (string Message, string Resolution) NetworkError = (
                "Lỗi kết nối mạng.",
                "1. Kiểm tra kết nối internet\n" +
                "2. Thử kết nối qua VPN nếu có\n" +
                "3. Kiểm tra tường lửa\n" +
                "4. Đợi vài phút và thử lại"
            );

            public static EnhancementException CreateInvalidApiKey() =>
                new EnhancementException(ErrorType.ApiKeyInvalid, InvalidApiKey.Message, InvalidApiKey.Resolution);

            public static EnhancementException CreateQuotaExceeded() =>
                new EnhancementException(ErrorType.QuotaExceeded, QuotaExceeded.Message, QuotaExceeded.Resolution);

            public static EnhancementException CreateNetworkError() =>
                new EnhancementException(ErrorType.NetworkError, NetworkError.Message, NetworkError.Resolution);
        }

        public static class Enhancement
        {
            public static readonly (string Message, string Resolution) EnhancementFailed = (
                "Không thể làm mượt nội dung chương.",
                "1. Kiểm tra kết nối mạng\n" +
                "2. Đảm bảo API key còn hạn mức sử dụng\n" +
                "3. Thử lại sau vài phút\n" +
                "4. Nếu vẫn lỗi, có thể chọn mô hình khác trong file config.json"
            );

            public static EnhancementException CreateEnhancementFailed() =>
                new EnhancementException(ErrorType.EnhancementFailed, EnhancementFailed.Message, EnhancementFailed.Resolution);
        }

        public static class Config
        {
            public static readonly (string Message, string Resolution) ConfigurationError = (
                "Lỗi cấu hình ứng dụng.",
                "1. Kiểm tra định dạng file config.json\n" +
                "2. Đảm bảo các trường bắt buộc đã được điền đầy đủ\n" +
                "3. Tham khảo file README.txt để biết cách cấu hình đúng"
            );

            public static EnhancementException CreateConfigurationError() =>
                new EnhancementException(ErrorType.ConfigurationError, ConfigurationError.Message, ConfigurationError.Resolution);
        }
    }
}
