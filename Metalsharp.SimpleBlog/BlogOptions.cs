namespace Metalsharp.SimpleBlog;

public class BlogOptions
{
	public string PostsDirectory { get; set; } = "";

	public bool PostsOrderedDescending { get; set; } = true;

	public Func<MetalsharpFile, DateTime>? PostsOrderQuery { get; set; }

	public Dictionary<string, object>? PostMetadata { get; set; }

	public string BlogFilePath { get; set; } = "blog.html";

	public Dictionary<string, object>? BlogMetadata { get; set; }
}