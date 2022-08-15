namespace Metalsharp.SimpleBlog;

public class SimpleBlog : IMetalsharpPlugin
{
	readonly BlogOptions _options;

	public SimpleBlog(BlogOptions? options = null) =>
		_options = options ?? new();

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

		foreach (MetalsharpFile file in postFiles)
		{
			if (_options.PostMetadata != null)
			{
				foreach (var (key, value) in _options.PostMetadata)
				{
					file.Metadata.Add(key, value);
				}
			}

			file.Metadata.Add("fileName", file.Name);

			posts.Add(file.Metadata.ToDictionary(p => p.Key, p => p.Value));
		}

		project.AddOutput(new MetalsharpFile("", _options.BlogFilePath)
		{
			Metadata =
				(_options.BlogMetadata ?? new())
				.Concat(new[]
				{
					new KeyValuePair<string, object>("posts", posts)
				})
				.ToDictionary(p => p.Key, p => p.Value)
		});
	}
}