using System;
using System.Linq;
using System.Text;

namespace EstiloMB.Core
{
    public enum ImageFormat
    {
        BMP,
        JPEG,
        GIF,
        TIFF,
        PNG,
        Unknown
    }

    public class ImageHelper
    {
        public static ImageFormat GetFormat(byte[] bytes)
        {
            // see http://www.mikekunz.com/image_file_header.html  
            var bmp = Encoding.ASCII.GetBytes("BM");     // BMP
            var gif = Encoding.ASCII.GetBytes("GIF");    // GIF
            var png = new byte[] { 137, 80, 78, 71 };    // PNG
            var tiff = new byte[] { 73, 73, 42 };         // TIFF
            var tiff2 = new byte[] { 77, 77, 42 };         // TIFF
            var jpeg = new byte[] { 255, 216, 255, 224 }; // jpeg
            var jpeg2 = new byte[] { 255, 216, 255, 225 }; // jpeg canon

            if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
            { return ImageFormat.BMP; }

            if (gif.SequenceEqual(bytes.Take(gif.Length)))
            { return ImageFormat.GIF; }

            if (png.SequenceEqual(bytes.Take(png.Length)))
            { return ImageFormat.PNG; }

            if (tiff.SequenceEqual(bytes.Take(tiff.Length)))
            { return ImageFormat.TIFF; }

            if (tiff2.SequenceEqual(bytes.Take(tiff2.Length)))
            { return ImageFormat.TIFF; }

            if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
            { return ImageFormat.JPEG; }

            if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
            { return ImageFormat.JPEG; }

            return ImageFormat.Unknown;
        }
    }
}