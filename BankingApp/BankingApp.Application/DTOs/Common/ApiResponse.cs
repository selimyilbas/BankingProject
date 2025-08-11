namespace BankingApp.Application.DTOs.Common
{
    /// <summary>
    /// API ve uygulama katmanında standart yanıt sarmalayıcısı.
    /// </summary>
    public class ApiResponse<T>
    {
        /// <summary>
        /// İşlem başarı durumu.
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// Kullanıcı dostu mesaj.
        /// </summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// Yanıt verisi.
        /// </summary>
        public T? Data { get; set; }
        /// <summary>
        /// Hata detayları.
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// Başarılı yanıt oluşturur.
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// Hatalı yanıt oluşturur.
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
