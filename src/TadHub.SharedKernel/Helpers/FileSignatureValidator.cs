namespace TadHub.SharedKernel.Helpers;

/// <summary>
/// Validates file contents against known magic byte signatures to prevent
/// malicious file uploads with spoofed Content-Type headers.
/// </summary>
public static class FileSignatureValidator
{
    private static readonly Dictionary<string, byte[][]> FileSignatures = new()
    {
        ["image/jpeg"] = [new byte[] { 0xFF, 0xD8, 0xFF }],
        ["image/png"] = [new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }],
        ["image/webp"] = [new byte[] { 0x52, 0x49, 0x46, 0x46 }], // "RIFF" prefix; "WEBP" at offset 8 checked separately
        ["application/pdf"] = [new byte[] { 0x25, 0x50, 0x44, 0x46 }], // "%PDF"
        ["video/mp4"] = [
            new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 }, // ftyp at offset 4
            new byte[] { 0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70 },
            new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70 },
        ],
        ["video/webm"] = [new byte[] { 0x1A, 0x45, 0xDF, 0xA3 }], // EBML header (Matroska/WebM)
    };

    private static readonly byte[] WebpMarker = "WEBP"u8.ToArray();

    /// <summary>
    /// Validates that the file stream content matches one of the allowed MIME types
    /// by checking magic byte signatures. Resets the stream position after reading.
    /// </summary>
    /// <param name="stream">The file stream to validate. Must be seekable.</param>
    /// <param name="allowedTypes">Array of allowed MIME types (e.g., "image/jpeg", "image/png").</param>
    /// <returns>True if the file signature matches at least one of the allowed types.</returns>
    public static bool IsValidFileSignature(Stream stream, string[] allowedTypes)
    {
        if (!stream.CanSeek || !stream.CanRead)
            return false;

        var originalPosition = stream.Position;
        try
        {
            stream.Position = 0;

            // Read enough bytes for the longest signature check (12 bytes for WebP: RIFF + 4 size bytes + WEBP)
            var headerBytes = new byte[12];
            var bytesRead = stream.Read(headerBytes, 0, headerBytes.Length);

            if (bytesRead == 0)
                return false;

            foreach (var allowedType in allowedTypes)
            {
                var lowerType = allowedType.ToLowerInvariant();

                if (!FileSignatures.TryGetValue(lowerType, out var signatures))
                    continue;

                foreach (var signature in signatures)
                {
                    if (bytesRead < signature.Length)
                        continue;

                    if (MatchesSignature(headerBytes, signature))
                    {
                        // WebP needs additional check: bytes 8-11 must be "WEBP"
                        if (lowerType == "image/webp")
                        {
                            if (bytesRead >= 12 && MatchesSignature(headerBytes.AsSpan(8), WebpMarker))
                                return true;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            // Special handling for MP4: the "ftyp" marker can appear at various offsets
            if (allowedTypes.Any(t => t.Equals("video/mp4", StringComparison.OrdinalIgnoreCase)))
            {
                // Check if "ftyp" appears at offset 4 regardless of the box size prefix
                if (bytesRead >= 8)
                {
                    var ftypBytes = new byte[] { 0x66, 0x74, 0x79, 0x70 }; // "ftyp"
                    if (MatchesSignature(headerBytes.AsSpan(4), ftypBytes))
                        return true;
                }
            }

            return false;
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    private static bool MatchesSignature(ReadOnlySpan<byte> data, ReadOnlySpan<byte> signature)
    {
        if (data.Length < signature.Length)
            return false;

        return data[..signature.Length].SequenceEqual(signature);
    }
}
