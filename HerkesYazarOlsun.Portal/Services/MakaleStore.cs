using System.Text.Json;

namespace HerkesYazarOlsun.Portal.Services;

public sealed class Makale
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public long YazarId { get; set; }
    public string Yazar { get; set; } = "";
    public string Baslik { get; set; } = "";
    public string Metin { get; set; } = "";
    public string Uzanti { get; set; } = "";
    public DateTimeOffset? YayinTarihi { get; set; }
}

// Keep this directory on a persistent volume when deploying the portal.
public sealed class MakaleStore
{
    private readonly string root;
    private readonly object gate = new();
    public MakaleStore(IWebHostEnvironment env, IConfiguration configuration)
    {
        root = configuration["Articles:StoragePath"] ?? Path.Combine(env.ContentRootPath, "App_Data", "Makaleler");
        Directory.CreateDirectory(root);
    }
    public Makale? Get(Guid id)
    {
        lock (gate)
        {
            var path = Path.Combine(root, id + ".json");
            return File.Exists(path) ? JsonSerializer.Deserialize<Makale>(File.ReadAllText(path)) : null;
        }
    }
    public List<Makale> List(long? author = null, bool drafts = false)
    {
        lock (gate)
            return Directory.EnumerateFiles(root, "*.json")
                .Select(path => JsonSerializer.Deserialize<Makale>(File.ReadAllText(path))!)
                .Where(a => (!author.HasValue || a.YazarId == author) && (drafts || a.YayinTarihi.HasValue))
                .OrderByDescending(a => a.YayinTarihi).ToList();
    }
    public void Save(Makale article)
    {
        lock (gate)
        {
            var path = Path.Combine(root, article.Id + ".json");
            var temp = path + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(article));
            File.Move(temp, path, true);
        }
    }
    public string DocumentPath(Makale article) => Path.Combine(root, article.Id + article.Uzanti);
}
