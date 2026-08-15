namespace Metalsharp.SimpleBlog;

/// <summary>
/// Options for configuring <see cref="SimpleBlog"/>.
/// </summary>
public class SimpleBlogOptions
{
	/// <summary>
	/// The virtual directory that posts live in. Only output files whose <see cref="MetalsharpFile.Directory"/> matches this value exactly are treated as posts.
	/// </summary>
	public string PostsDirectory { get; set; } = "";

	/// <summary>
	/// Whether <see cref="PostsOrderQuery"/> sorts posts newest/highest first. Has no effect if <see cref="PostsOrderQuery"/> is <see langword="null"/>.
	/// </summary>
	public bool PostsOrderedDescending { get; set; } = true;

	/// <summary>
	/// Selects the sort key for each post, typically a publish date. If <see langword="null"/>, posts are kept in whatever order they're found in.
	/// </summary>
	public Func<MetalsharpFile, DateTime>? PostsOrderQuery { get; set; }

	/// <summary>
	/// Runs against each post file; any metadata this returns is merged into that file's metadata before it's copied into the generated list of posts.
	/// </summary>
	public Func<MetalsharpFile, Dictionary<string, object>>? PostMetadata { get; set; }

	/// <summary>
	/// The virtual path of the output file <see cref="SimpleBlog"/> generates.
	/// </summary>
	public string BlogFilePath { get; set; } = "blog.html";

	/// <summary>
	/// Additional metadata to merge onto the generated blog output file, alongside the generated list of posts.
	/// </summary>
	public Dictionary<string, object>? BlogMetadata { get; set; }
}
