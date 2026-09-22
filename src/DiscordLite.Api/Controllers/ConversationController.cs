using DiscordLite.Application.Conversations.GetConversations;
using DiscordLite.Application.Conversations.StartDirectConversation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiscordLite.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ConversationController(ISender sender) : ControllerBase
{
    [HttpPost("direct")]
    public async Task<ActionResult<StartDirectConversationResponse>> StartDirectConversation(
        [FromBody] StartDirectConversationCommand command,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<GetConversationsResponse>> GetConversations(CancellationToken ct)
    {
        return Ok(await sender.Send(new GetConversationsQuery(), ct));
    }
}
