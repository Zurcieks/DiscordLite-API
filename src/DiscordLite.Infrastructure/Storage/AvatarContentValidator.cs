using DiscordLite.Application.Abstractions;
using DiscordLite.Application.Exceptions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace DiscordLite.Infrastructure.Storage;

public sealed class AvatarContentValidator : IAvatarContentValidator
{
    private const int MaxDimension = 2048;

    public async Task ValidateAsync(
        Stream content,
        string declaredContentType,
        CancellationToken cancellationToken)
    {
        //canSeek czy mozna przesuwac jego pozycje.
        // czytamy ten sam plik kilka razy
        // informacje o obrazie, piksele, po walidacji plik odczyta klient miniio
        if (!content.CanRead || !content.CanSeek)
        {
            throw new BadRequestException(
                "AVATAR_STREAM_INVALID",
                "Avatar stream must support reading and seeking.");
        }
        
        // zapamietuje miejsce od ktorego zaczynamy czytanie
        var initialPosition = content.Position;

        try
        {
            var decoderOptions = new DecoderOptions
            {
                SkipMetadata = true,
                MaxFrames = 2 // odrzucamy animowane 
            };

            var info = await Image.IdentifyAsync(
                decoderOptions,
                content,
                cancellationToken);

            var detectedType =
                info.Metadata.DecodedImageFormat?.DefaultMimeType;

            if (detectedType is not ("image/jpeg" or "image/png")
                || detectedType != declaredContentType)
            {
                throw new BadRequestException(
                    "AVATAR_FORMAT_INVALID",
                    "Avatar must be JPEG or PNG and match its content type.");
            }

            if (info.Width > MaxDimension || info.Height > MaxDimension)
            {
                throw new BadRequestException(
                    "AVATAR_DIMENSIONS_INVALID",
                    "Avatar dimensions cannot exceed 2048 x 2048 pixels.");
            }

            if (info.FrameMetadataCollection.Count > 1)
            {
                throw new BadRequestException(
                    "AVATAR_ANIMATION_NOT_SUPPORTED",
                    "Animated avatars are not supported.");
            }

            
            content.Position = initialPosition;
            
            // loadasync probuje zdekodowac obraz do pikseli w pamieci
            //using zwalnia zasoby przy wyjsciu z try.
            using var image = await Image.LoadAsync(
                decoderOptions,
                content,
                cancellationToken);
        }
        catch (UnknownImageFormatException)
        {
            throw new BadRequestException(
                "AVATAR_FORMAT_INVALID",
                "File format could not be recognized.");
        }
        catch (InvalidImageContentException)
        {
            throw new BadRequestException(
                "AVATAR_CONTENT_INVALID",
                "Image content is invalid.");
        }
        finally
        {
            // przywracamy pozycje, zeby po udanej walidacji handler mogl przekazac strumien dalej
            content.Position = initialPosition;
        }
    }
}