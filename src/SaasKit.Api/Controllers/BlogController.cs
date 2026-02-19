using Content.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasKit.Api.Filters;
using SaasKit.Infrastructure.Auth;
using SaasKit.SharedKernel.Api;
using SaasKit.SharedKernel.Interfaces;

namespace SaasKit.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/blog")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class BlogController : ControllerBase
{
    private readonly IBlogService _blogService;
    private readonly ICurrentUser _currentUser;

    public BlogController(IBlogService blogService, ICurrentUser currentUser)
    {
        _blogService = blogService;
        _currentUser = currentUser;
    }

    [HttpGet("posts")]
    [HasPermission("content.view")]
    public async Task<IActionResult> GetPosts(Guid tenantId, [FromQuery] QueryParameters qp, CancellationToken ct)
        => Ok(await _blogService.GetPostsAsync(tenantId, qp, ct));

    [HttpGet("posts/{postId:guid}")]
    [HasPermission("content.view")]
    public async Task<IActionResult> GetPost(Guid tenantId, Guid postId, CancellationToken ct)
    {
        var result = await _blogService.GetPostByIdAsync(tenantId, postId, ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPost("posts")]
    [HasPermission("content.create")]
    public async Task<IActionResult> CreatePost(Guid tenantId, [FromBody] CreateBlogPostRequest request, CancellationToken ct)
    {
        var result = await _blogService.CreatePostAsync(tenantId, _currentUser.UserId, request, ct);
        if (!result.IsSuccess) return result.ErrorCode == "CONFLICT" ? Conflict(new { error = result.Error }) : BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetPost), new { tenantId, postId = result.Value!.Id }, result.Value);
    }

    [HttpPatch("posts/{postId:guid}")]
    [HasPermission("content.edit")]
    public async Task<IActionResult> UpdatePost(Guid tenantId, Guid postId, [FromBody] UpdateBlogPostRequest request, CancellationToken ct)
    {
        var result = await _blogService.UpdatePostAsync(tenantId, postId, request, ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPut("posts/{postId:guid}/publish")]
    [HasPermission("content.publish")]
    public async Task<IActionResult> PublishPost(Guid tenantId, Guid postId, CancellationToken ct)
    {
        var result = await _blogService.PublishPostAsync(tenantId, postId, ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpDelete("posts/{postId:guid}")]
    [HasPermission("content.delete")]
    public async Task<IActionResult> DeletePost(Guid tenantId, Guid postId, CancellationToken ct)
    {
        var result = await _blogService.DeletePostAsync(tenantId, postId, ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return NoContent();
    }

    [HttpGet("categories")]
    [HasPermission("content.view")]
    public async Task<IActionResult> GetCategories(Guid tenantId, [FromQuery] QueryParameters qp, CancellationToken ct)
        => Ok(await _blogService.GetCategoriesAsync(tenantId, qp, ct));
}
