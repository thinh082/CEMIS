using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CEMIS.Common;

public static class SignatureImageLoader
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly TimeSpan DownloadTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Tải và xác thực an toàn ảnh chữ ký tay (chống SSRF, chống Path Traversal, kiểm tra magic bytes).
    /// </summary>
    public static async Task<byte[]> LoadAndValidateSignatureImageAsync(string pathOrUrl, string? contentRootPath = null)
    {
        if (string.IsNullOrWhiteSpace(pathOrUrl))
            throw new ArgumentException("Đường dẫn ảnh chữ ký không được để trống.", nameof(pathOrUrl));

        pathOrUrl = pathOrUrl.Trim();
        byte[] imageBytes;

        if (pathOrUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
            pathOrUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            imageBytes = await DownloadRemoteImageSecurelyAsync(pathOrUrl);
        }
        else
        {
            imageBytes = await LoadLocalImageSecurelyAsync(pathOrUrl, contentRootPath);
        }

        ValidateImageMagicBytes(imageBytes);
        return imageBytes;
    }

    private static async Task<byte[]> LoadLocalImageSecurelyAsync(string localPath, string? contentRootPath)
    {
        var basePath = !string.IsNullOrEmpty(contentRootPath) 
            ? contentRootPath 
            : Directory.GetCurrentDirectory();

        // Chuẩn hóa đường dẫn bắt đầu bằng / hoặc \
        var cleanPath = localPath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(basePath, cleanPath));
        var baseFullPath = Path.GetFullPath(basePath);

        // Chống Path Traversal: đường dẫn phải nằm trong basePath
        if (!fullPath.StartsWith(baseFullPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Đường dẫn tệp cục bộ không hợp lệ (vi phạm giới hạn thư mục ứng dụng).");
        }

        if (!File.Exists(fullPath))
        {
            // Thử kiểm tra thư mục uploads hoặc wwwroot nếu chưa tìm thấy
            var uploadsPath = Path.GetFullPath(Path.Combine(basePath, "uploads", cleanPath));
            var wwwrootPath = Path.GetFullPath(Path.Combine(basePath, "wwwroot", cleanPath));

            if (File.Exists(uploadsPath))
                fullPath = uploadsPath;
            else if (File.Exists(wwwrootPath))
                fullPath = wwwrootPath;
            else
                throw new FileNotFoundException($"Không tìm thấy tệp ảnh chữ ký trên máy chủ: {cleanPath}");
        }

        var fileInfo = new FileInfo(fullPath);
        if (fileInfo.Length > MaxFileSizeBytes)
            throw new InvalidOperationException($"Dung lượng ảnh vượt quá giới hạn cho phép (tối đa {MaxFileSizeBytes / 1024 / 1024}MB).");

        return await File.ReadAllBytesAsync(fullPath);
    }

    private static async Task<byte[]> DownloadRemoteImageSecurelyAsync(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            throw new InvalidOperationException("URL ảnh chữ ký không hợp lệ.");

        // Chỉ cho phép scheme HTTPS (hoặc HTTP nếu dev)
        if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
            throw new InvalidOperationException("Chỉ chấp nhận giao thức HTTPS an toàn cho đường dẫn ảnh chữ ký.");

        // Chống SSRF: Phân giải DNS và kiểm tra dải IP
        await ValidateIpAddressNotInternalAsync(uri.DnsSafeHost);

        using var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = false, // Chặn redirect để tránh bypass kiểm tra IP
            ConnectTimeout = DownloadTimeout
        };

        using var httpClient = new HttpClient(handler)
        {
            Timeout = DownloadTimeout
        };

        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CEMIS-Signature-Verifier/1.0");

        using var cts = new CancellationTokenSource(DownloadTimeout);
        using var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cts.Token);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Không thể tải ảnh chữ ký từ máy chủ từ xa (Mã lỗi HTTP: {(int)response.StatusCode}).");

        var contentLength = response.Content.Headers.ContentLength;
        if (contentLength.HasValue && contentLength.Value > MaxFileSizeBytes)
            throw new InvalidOperationException($"Dung lượng ảnh tải về vượt quá giới hạn tối đa ({MaxFileSizeBytes / 1024 / 1024}MB).");

        await using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
        using var ms = new MemoryStream();
        var buffer = new byte[8192];
        long totalBytesRead = 0;
        int bytesRead;

        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cts.Token)) > 0)
        {
            totalBytesRead += bytesRead;
            if (totalBytesRead > MaxFileSizeBytes)
                throw new InvalidOperationException($"Dung lượng ảnh tải về vượt quá giới hạn tối đa ({MaxFileSizeBytes / 1024 / 1024}MB).");

            ms.Write(buffer, 0, bytesRead);
        }

        return ms.ToArray();
    }

    private static async Task ValidateIpAddressNotInternalAsync(string host)
    {
        // Chặn localhost / hostname nội bộ
        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Không được phép truy cập địa chỉ localhost nội bộ.");
        }

        IPAddress[] ips;
        try
        {
            ips = await Dns.GetHostAddressesAsync(host);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Không thể phân giải tên miền ảnh chữ ký: {ex.Message}");
        }

        if (ips == null || ips.Length == 0)
            throw new InvalidOperationException("Không thể xác định địa chỉ IP của máy chủ ảnh chữ ký.");

        foreach (var ip in ips)
        {
            if (IPAddress.IsLoopback(ip) || IsPrivateOrLinkLocal(ip))
            {
                throw new InvalidOperationException("Địa chỉ IP của máy chủ ảnh nằm trong dải mạng nội bộ hoặc riêng tư không được phép (SSRF Protection).");
            }
        }
    }

    private static bool IsPrivateOrLinkLocal(IPAddress ip)
    {
        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            return ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || ip.IsIPv6Multicast;
        }

        var bytes = ip.GetAddressBytes();
        // 10.0.0.0 - 10.255.255.255
        if (bytes[0] == 10) return true;
        // 172.16.0.0 - 172.31.255.255
        if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;
        // 192.168.0.0 - 192.168.255.255
        if (bytes[0] == 192 && bytes[1] == 168) return true;
        // 169.254.0.0 - 169.254.255.255 (Link local / AWS metadata)
        if (bytes[0] == 169 && bytes[1] == 254) return true;
        // 0.0.0.0/8
        if (bytes[0] == 0) return true;

        return false;
    }

    private static void ValidateImageMagicBytes(byte[] bytes)
    {
        if (bytes == null || bytes.Length < 8)
            throw new InvalidOperationException("Dữ liệu tệp ảnh chữ ký quá ngắn hoặc không hợp lệ.");

        // PNG Magic bytes: 89 50 4E 47 0D 0A 1A 0A
        bool isPng = bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
                     bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A;

        // JPEG Magic bytes: FF D8 FF
        bool isJpeg = bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;

        // WebP Magic bytes: 52 49 46 46 (RIFF) .... 57 45 42 50 (WEBP)
        bool isWebp = bytes.Length >= 12 &&
                      bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
                      bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50;

        if (!isPng && !isJpeg && !isWebp)
        {
            throw new InvalidOperationException("Tệp ảnh không hợp lệ (chỉ hỗ trợ định dạng ảnh PNG, JPG hoặc WebP).");
        }
    }
}
