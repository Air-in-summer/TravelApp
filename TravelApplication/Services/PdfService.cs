using Microsoft.Maui.Controls.PlatformConfiguration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TravelApplication.Models;
using Colors = QuestPDF.Helpers.Colors;
#if ANDROID
using Android.Content;
#endif
namespace TravelApplication.Services
{
    public class PdfService
    {
        private readonly HttpClient httpClient;
        public PdfService(HttpClient _httpClient)
        {
            this.httpClient = _httpClient;
        }


        public async Task<string> CreatePdfAsync(int tripId)
        {
            var response = await httpClient.GetAsync($"api/pdf/{tripId}");

            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            var bytes = await response.Content.ReadAsByteArrayAsync();

            // Lưu file PDF vào máy
            string filePath = await SavePdfToDownloads(bytes, $"trip_{tripId}.pdf");

            return filePath;
        }

        public async Task<string> SavePdfToDownloads(byte[] bytes, string filename)
        {
            #if ANDROID
                
                var context = Android.App.Application.Context;
                var contentResolver = context.ContentResolver;

                ContentValues values = new ContentValues();
                values.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, filename);
                values.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "application/pdf");
                values.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, Android.OS.Environment.DirectoryDownloads);

                var uri = contentResolver.Insert(
                    Android.Provider.MediaStore.Downloads.ExternalContentUri, 
                    values
                );

                using (var output = contentResolver.OpenOutputStream(uri))
                {
                    await output.WriteAsync(bytes);
                }

                return uri.ToString();    // người dùng có thể mở từ File Manager
            #else
                throw new PlatformNotSupportedException();
            #endif
        }
    }    
}
