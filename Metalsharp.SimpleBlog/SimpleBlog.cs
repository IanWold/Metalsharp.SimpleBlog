namespace Metalsharp.SimpleBlog;

public class SimpleBlog(SimpleBlogOptions? options = null) : IMetalsharpPlugin
{
	readonly SimpleBlogOptions _options = options ?? new();

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