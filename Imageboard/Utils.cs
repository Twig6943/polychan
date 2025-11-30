using System.Diagnostics;

namespace Imageboard;

public static class Utils
{
    public static DateTime UnixToDateTime(long unixTime)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTime);
        DateTime dateTime = dateTimeOffset.UtcDateTime;

        return dateTime;
    }

    public static AttachmentType GetAttachmentTypeFromExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return AttachmentType.Null;
        }
        
        Debug.Assert(extension.StartsWith('.'));
        
        switch (extension)
        {
            case ".png":
            case ".jpg":
            case ".jpeg":
            case ".bmp":
            case ".tiff":
            case ".webp":
                return AttachmentType.Image;
            case ".gif":
                return AttachmentType.ImageAnimated;
            case ".webm":
            case ".mp4":
                return AttachmentType.Video;
            case ".mp3":
            case ".ogg":
            case ".wav":
            case ".flac":
                return AttachmentType.Audio;
            default:
                return AttachmentType.File;
        }
    }
}