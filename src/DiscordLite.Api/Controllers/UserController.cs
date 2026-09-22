using DiscordLite.Application.Exceptions;
using DiscordLite.Application.Users.GetMyProfile;
using DiscordLite.Application.Users.UploadAvatar;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiscordLite.Api.Controllers;

[ApiController]
[Route("api/user")]
public sealed class UserController(ISender sender) : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileResponse>> GetMyProfile(CancellationToken ct)
    {
        var result = await sender.Send(new GetMyProfileQuery(), ct);
        return Ok(result);  
    }

    [Authorize]
    [HttpPost("me/avatar")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 5 * 1024 * 1024)]
    public async Task<ActionResult<UploadAvatarResponse>> UploadAvatar(
        IFormFile? file,
        CancellationToken ct)
    {
        if (file is null)
        {
            throw new BadRequestException(
                "AVATAR_FILE_REQUIRED",
                "Avatar file is required.");
        }

        await using var content = file.OpenReadStream();

        var command = new UploadAvatarCommand(
            content,
            file.Length,
            file.ContentType);

        var result = await sender.Send(command, ct);

        return Ok(result);
    }



}