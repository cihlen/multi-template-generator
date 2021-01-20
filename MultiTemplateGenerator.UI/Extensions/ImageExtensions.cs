using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace MultiTemplateGenerator.UI
{
    public static class ImageExtensions
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        public static BitmapSource ToImageSource(this string filePath)
        {
            var bitmap = Bitmap.FromFile(filePath) as Bitmap;
            IntPtr hBitmap = bitmap.GetHbitmap();

            var wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new Win32Exception();
            }

            return wpfBitmap;

            //return Imaging.CreateBitmapSourceFromHIcon(
            //    icon.Handle,
            //    Int32Rect.Empty,
            //    BitmapSizeOptions.FromEmptyOptions());
        }

        public static BitmapSource ToImageSource(this Icon icon)
        {
            return ToImageSource(icon.ToBitmap());
        }

        public static BitmapSource ToImageSource(this Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();

            var wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new Win32Exception();
            }

            return wpfBitmap;
        }

        public static BitmapSource ToBitmapSource(this string filePath)
        {
            return new BitmapImage(new Uri(filePath));
        }
    }
}
