using FluentValidation;

namespace DiscordLite.Application.Users.UploadAvatar;

public sealed class UploadAvatarCommandValidator
    : AbstractValidator<UploadAvatarCommand>
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    public UploadAvatarCommandValidator()
    {
        RuleFor(x => x.Content)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Avatar content is required.")
            .Must(stream => stream.CanRead)
            .WithMessage("Avatar stream must be readable.");

        RuleFor(x => x.ContentLength)
            .GreaterThan(0)
            .WithMessage("Avatar cannot be empty.")
            .LessThanOrEqualTo(MaxFileSize)
            .WithMessage("Avatar cannot exceed 5 MB.");

        RuleFor(x => x.ContentType)
            .Must(type => type is "image/jpeg" or "image/png")
            .WithMessage("Only JPEG and PNG avatars are supported.");
    }
}