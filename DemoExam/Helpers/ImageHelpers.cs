using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace DemoExam.Helpers
{
    public static class ImageHelpers
    {
        private static readonly string _imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");

        public static string SaveResizedImage(string sourcePath, string targetFolder)
        {
            if(!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);
            string fileName = Guid.NewGuid().ToString() + ".jpg";
            string savePath = Path.Combine(targetFolder, fileName);
            using (var img = Image.FromFile(sourcePath))
            {
                var thumbnail = img.GetThumbnailImage(300, 200, null, IntPtr.Zero);
                thumbnail.Save(savePath, ImageFormat.Jpeg);
            }
            return savePath;
        }

        public static string? GetFullPath(string? imageFileName)
        {
            if (string.IsNullOrEmpty(imageFileName))
                return null;

            string fullPath = Path.Combine(_imagesFolder, imageFileName);
            bool exists = File.Exists(fullPath);
            if (!exists)
                System.Diagnostics.Debug.WriteLine($"Файл не найден: {fullPath}");
            return exists ? fullPath : null;
        }

        public static string GetFallbackPath()
        {
            string fallback = Path.Combine(_imagesFolder, "picture.png");
            return File.Exists(fallback) ? fallback : null;
        }

        public static string GetImagesFolder() => _imagesFolder;
    }
}
