namespace Metalsharp.SimpleBlog;

/// <summary>
/// A Metalsharp plugin which collects the files in a directory of posts into an ordered list and attaches that list, along with any additional metadata, to a new output file for a templating plugin to render.
/// </summary>
/// <param name="options">
/// The options configuring how posts are found, ordered, and annotated, and how the generated blog file is created. If <see langword="null"/>, the defaults on <see cref="SimpleBlogOptions"/> are used.
/// </param>
public class SimpleBlog(SimpleBlogOptions? options = null) : IMetalsharpPlugin
{
	readonly SimpleBlogOptions _options = options ?? new();

	/// <summary>
	/// Collects the files in <see cref="SimpleBlogOptions.PostsDirectory"/>, orders and annotates them, and adds a new output file whose metadata contains the resulting list of posts.
	/// </summary>
	/// <param name="project">
	/// The <see cref="MetalsharpProject"/> being built.
	/// </param>
	public void Execute(MetalsharpProject project)
	{
		var posts = new List<Dictionary<string, object>>();

		var postFiles = project.OutputFiles.Where(f => f.Directory == _options.PostsDirectory);

		if (_options.PostsOrderQuery != null)
		{
			postFiles = _options.PostsOrderedDescending
				? postFiles.OrderByDescending(_options.PostsOrderQuery)
				: postFiles.OrderBy(_options.PostsOrderQuery);
		}

		foreach (var file in postFiles)
		{
			if (_options.PostMetadata != null)
			{
				var postMetadata = _options.PostMetadata(file);

				foreach (var (key, value) in postMetadata)
				{
					file.Metadata.Add(key, value);
				}
			}

			file.Metadata.Add("fileName", file.Name);

			posts.Add(file.Metadata.ToDictionary());
		}

		project.AddOutput(new MetalsharpFile("", _options.BlogFilePath)
		{
			Metadata = new Dictionary<string, object>(_options.BlogMetadata ?? [])
			{
				["posts"] = posts
			}
		});
	}
}