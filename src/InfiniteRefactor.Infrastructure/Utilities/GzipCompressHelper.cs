using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace InfiniteRefactor.Infrastructure.Utilities;

public static class GzipCompressHelper
{
    public static string GZipCompress(string text, Encoding encoding = null)
    {
        var bytes = (encoding ?? Encoding.UTF8).GetBytes(text);
        if (bytes == null || bytes.Length <= 0) return null;

        using (var compressedStream = new MemoryStream())
        {
            using (var compressionStream =
                   new GZipStream(compressedStream,
                       CompressionMode.Compress))
            {
                compressionStream.Write(bytes, 0, bytes.Length);
            }

            return Convert.ToBase64String(compressedStream.ToArray());
        }
    }

    public static string GZipDecompress(string base64text, Encoding encoding = null)
    {
        var bytes = Convert.FromBase64String(base64text);
        if (bytes == null || bytes.Length <= 0) return null;

        using (var originalStream = new MemoryStream(bytes))
        {
            using (var decompressedStream = new MemoryStream())
            {
                using (var decompressionStream = new GZipStream(originalStream,
                           CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(decompressedStream);
                }

                return (encoding ?? Encoding.UTF8).GetString(decompressedStream.ToArray());
            }
        }
    }
}