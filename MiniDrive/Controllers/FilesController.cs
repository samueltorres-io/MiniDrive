using Microsoft.AspNetCore.Mvc;
using MiniDrive.Services;

namespace MiniDrive.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(
        [FromQuery] int userId,
        [FromQuery] int? folderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var files = await _fileService.ListAsync(
                new ListFilesRequest(userId, folderId),
                cancellationToken);

            return Ok(files);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(FileResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [RequestSizeLimit(536_870_912)] /* 512 MB */
    public async Task<IActionResult> Upload(
        [FromForm] int userId,
        [FromForm] int? folderId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fileService.UploadAsync(
                new UploadFileRequest(userId, folderId, file),
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(
        [FromRoute] int id,
        [FromQuery] int userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _fileService.DownloadAsync(
                new DownloadFileRequest(userId, id),
                cancellationToken);

            return File(result.Content, result.ContentType, result.FileName);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        [FromQuery] int userId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _fileService.DeleteAsync(
                new DeleteFileRequest(userId, id),
                cancellationToken);

            return NoContent();
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}