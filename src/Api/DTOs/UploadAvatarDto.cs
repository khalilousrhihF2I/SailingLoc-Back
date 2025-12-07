using Microsoft.AspNetCore.Http;

namespace Api.DTOs;

public class UploadAvatarDto
{
    // IMPORTANT: nommez la propriété comme le champ du formulaire (ex: "file")
    public IFormFile? File { get; set; }
}
