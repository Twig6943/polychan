using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;

namespace Polychan.App.Utils;

public static class HttpHelpers
{
    public static async Task LoadThumbnailsAsync<T>(IEnumerable<(T commentId, string? attachmentUrl)> imageIds, Action<T, SKImage?> onComplete) where T : Imageboard.IId
    {
        var tasks = imageIds.Select(async tim =>
        {
            await ChanApp.ImageboardClient.Throttler.WaitAsync();
            try
            {
                if (tim.attachmentUrl != null)
                {
                    var img = await DownloadAttachmentFromPostAsync(tim.attachmentUrl);

                    if (img != null)
                    {
                        onComplete.Invoke(tim.commentId, img);
                    }
                }
            }
            finally
            {
                ChanApp.ImageboardClient.Throttler.Release();
            }
        });

        await Task.WhenAll(tasks);
    }
    
    public static async Task<SKImage?> DownloadAttachmentFromPostAsync(string url)
    {
        try
        {
            byte[] imageBytes = await ChanApp.ImageboardClient.HttpClient.GetByteArrayAsync(url);
            using var ms = new MemoryStream(imageBytes);
            var ret = SKImage.FromEncodedData(ms); // Decode into SKBitmap
            return ret;
        }
        catch
        {
            return null; // Handle gracefully if image isn't available
        }
    }
    
    public class GifFrame
    {
        public required SKImage Image;
        public int Delay; // in milliseconds
    }
    
    public static async Task<List<GifFrame>> LoadGifFromUrlAsync(string url)
    {
        byte[] data = await ChanApp.ImageboardClient.HttpClient.GetByteArrayAsync(url);
        using var stream = new MemoryStream(data);

        var frames = new List<GifFrame>();

        using var image = Image.Load<Rgba32>(stream);

        foreach (var frame in image.Frames)
        {
            // Delay (10ms units)
            int delay = 100;
            if (frame.Metadata.TryGetGifMetadata(out var gifMeta))
            {
                delay = gifMeta.FrameDelay * 10; // 1 unit = 10 ms
            }

            using var skBitmap = new SKBitmap(frame.Width, frame.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            frame.CopyPixelDataTo(MemoryMarshal.Cast<byte, Rgba32>(skBitmap.GetPixelSpan()));

            frames.Add(new GifFrame
            {
                Image = SKImage.FromBitmap(skBitmap),
                Delay = delay
            });
        }

        return frames;
    }
}