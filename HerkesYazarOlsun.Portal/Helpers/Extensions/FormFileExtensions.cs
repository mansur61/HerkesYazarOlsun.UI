namespace HerkesYazarOlsun.Portal.Helpers.Extensions
{
    public static class FormFileExtensions
    {
        public static IFormFile? ToImageFormFile(
            this byte[] bytes,
            string fileName,
            string? contentType = null,
            string formFieldName = "PageFotoDosyalar",
            bool throwIfNotImage = false)
        {
            if (bytes == null || bytes.Length < 4)
                return HandleFail("Byte array boş veya geçersiz.", throwIfNotImage);

            // Image mi?
            contentType ??= DetectImageContentType(bytes);

            if (contentType == null)
                return HandleFail("Byte array bir resim değil.", throwIfNotImage);

            var stream = new MemoryStream(bytes);

            return new FormFile(
                stream,
                0,
                bytes.Length,
                formFieldName,
                fileName
            )
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        private static IFormFile? HandleFail(string message, bool throwEx)
        {
            if (throwEx)
                throw new InvalidDataException(message);

            return null;
        }

        private static string? DetectImageContentType(byte[] bytes)
        {
            return bytes[0] switch
            {
                0xFF when bytes[1] == 0xD8 => "image/jpeg",
                0x89 when bytes[1] == 0x50 => "image/png",
                0x47 when bytes[1] == 0x49 => "image/gif",
                0x42 when bytes[1] == 0x4D => "image/bmp",
                _ => null
            };
        }
    }


}
