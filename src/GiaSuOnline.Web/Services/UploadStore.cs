namespace GiaSuOnline.Web.Services;
public class UploadStore(IWebHostEnvironment env)
{
    private string Root => Path.Combine(env.ContentRootPath, "App_Data", "uploads");
    public static async Task<string?> ValidateAsync(IFormFile file, bool photo)
    {
        if (file.Length is <= 0 or > 5 * 1024 * 1024) return "Mỗi tệp phải có dữ liệu và không vượt quá 5 MB.";
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        byte[] head = new byte[8];
        await using var stream = file.OpenReadStream();
        var count = await stream.ReadAsync(head);
        var valid = ext switch
        {
            ".jpg" or ".jpeg" => count >= 3 && head[0] == 255 && head[1] == 216 && head[2] == 255,
            ".png" => count >= 8 && head.SequenceEqual(new byte[] {137,80,78,71,13,10,26,10}),
            ".pdf" => !photo && count >= 5 && System.Text.Encoding.ASCII.GetString(head,0,5) == "%PDF-",
            _ => false
        };
        return valid ? null : photo ? "Ảnh phải là JPG/PNG hợp lệ." : "Chứng chỉ phải là JPG/PNG/PDF hợp lệ.";
    }
    public async Task<(string Key, string ContentType)> SaveAsync(IFormFile file)
    {
        Directory.CreateDirectory(Root);
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var key = Guid.NewGuid().ToString("N") + extension;
        await using var output = new FileStream(Path.Combine(Root, key), FileMode.CreateNew);
        await file.CopyToAsync(output);
        return (key, extension == ".pdf" ? "application/pdf" : extension == ".png" ? "image/png" : "image/jpeg");
    }
    public string PathFor(string key)
    {
        if (key != Path.GetFileName(key)) throw new ArgumentException("Invalid file key");
        return Path.Combine(Root, key);
    }
    public void Delete(string? key) { if (key != null && File.Exists(PathFor(key))) File.Delete(PathFor(key)); }
}
