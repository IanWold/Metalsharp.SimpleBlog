namespace Metalsharp.SimpleBlog;

public class SimpleBlogOptions
{
	public string PostsDirectory { get; set; } = "";

	public bool PostsOrderedDescending { get; set; } = true;

	public Func<MetalsharpFile, DateTime>? PostsOrderQuery { get; set; }

	public Func<MetalsharpFile, Dictionary<string, object>>? PostMetadata { get; set; }

	public string BlogFilePath { get; set; } = "blog.html";

	public Dictionary<string, object>? BlogMetadata { get; set; }
}
