namespace Metalsharp.SimpleBlog.Tests;

public class SimpleBlogTests
{
	static MetalsharpFile GetBlogFile(MetalsharpProject project) =>
		project.OutputFiles.Single(f => f.FilePath == "blog.html");

	static List<Dictionary<string, object>> GetPosts(MetalsharpProject project) =>
		(List<Dictionary<string, object>>)GetBlogFile(project).Metadata["posts"];

	[Fact]
	public void Execute_OnlyIncludesFilesInPostsDirectory()
	{
		var project = new MetalsharpProject();
		project.AddOutput(new MetalsharpFile("<p>A</p>", "Posts/a.html", new Dictionary<string, object> { ["title"] = "A" }));
		project.AddOutput(new MetalsharpFile("<p>B</p>", "Other/b.html", new Dictionary<string, object> { ["title"] = "B" }));

		new SimpleBlog(new SimpleBlogOptions { PostsDirectory = "Posts" }).Execute(project);

		var posts = GetPosts(project);
		var post = Assert.Single(posts);
		Assert.Equal("A", post["title"]);
	}

	[Fact]
	public void Execute_DefaultPostsDirectory_MatchesRootLevelFiles()
	{
		var project = new MetalsharpProject();
		project.AddOutput(new MetalsharpFile("<p>A</p>", "a.html", new Dictionary<string, object> { ["title"] = "A" }));
		project.AddOutput(new MetalsharpFile("<p>B</p>", "Posts/b.html", new Dictionary<string, object> { ["title"] = "B" }));

		new SimpleBlog().Execute(project);

		var posts = GetPosts(project);
		var post = Assert.Single(posts);
		Assert.Equal("A", post["title"]);
	}

	[Fact]
	public void Execute_WithNoMatchingPosts_CreatesBlogFileWithEmptyPostsList()
	{
		var project = new MetalsharpProject();
		project.AddOutput(new MetalsharpFile("<p>A</p>", "Other/a.html", new Dictionary<string, object>()));

		new SimpleBlog(new SimpleBlogOptions { PostsDirectory = "Posts" }).Execute(project);

		Assert.Empty(GetPosts(project));
	}

	[Fact]
	public void Execute_WithoutPostsOrderQuery_KeepsOriginalOrder()
	{
		var project = new MetalsharpProject();
		project.AddOutput(new MetalsharpFile("", "Posts/second.html", new Dictionary<string, object> { ["title"] = "Second" }));
		project.AddOutput(new MetalsharpFile("", "Posts/first.html", new Dictionary<string, object> { ["title"] = "First" }));

		new SimpleBlog(new SimpleBlogOptions { PostsDirectory = "Posts" }).Execute(project);

		Assert.Equal(["Second", "First"], GetPosts(project).Select(p => p["title"]));
	}

	[Fact]
	public void Execute_WithPostsOrderQuery_DefaultsToDescendingOrder()
	{
		var project = new MetalsharpProject();
		project.AddOutput(new MetalsharpFile("", "Posts/older.html", new Dictionary<string, object> { ["title"] = "Older", ["date"] = "2026-01-01" }));
		project.AddOutput(new MetalsharpFile("", "Posts/newer.html", new Dictionary<string, object> { ["title"] = "Newer", ["date"] = "2026-02-01" }));

		new SimpleBlog(new SimpleBlogOptions
		{
			PostsDirectory = "Posts",
			PostsOrderQuery = f => DateTime.Parse((string)f.Metadata["date"])
		}).Execute(project);

		Assert.Equal(["Newer", "Older"], GetPosts(project).Select(p => p["title"]));
	}

	[Fact]
	public void Execute_WithPostsOrderedDescendingFalse_SortsAscending()
	{
		var project = new MetalsharpProject();
		project.AddOutput(new MetalsharpFile("", "Posts/newer.html", new Dictionary<string, object> { ["title"] = "Newer", ["date"] = "2026-02-01" }));
		project.AddOutput(new MetalsharpFile("", "Posts/older.html", new Dictionary<string, object> { ["title"] = "Older", ["date"] = "2026-01-01" }));

		new SimpleBlog(new SimpleBlogOptions
		{
			PostsDirectory = "Posts",
			PostsOrderedDescending = false,
			PostsOrderQuery = f => DateTime.Parse((string)f.Metadata["date"])
		}).Execute(project);

		Assert.Equal(["Older", "Newer"], GetPosts(project).Select(p => p["title"]));
	}

	[Fact]
	public void Execute_WithoutPostMetadata_OnlyAddsFileNameToPost()
	{
		var project = new MetalsharpProject();
		project.AddOutput(new MetalsharpFile("", "Posts/hello.html", new Dictionary<string, object> { ["title"] = "Hello" }));

		new SimpleBlog(new SimpleBlogOptions { PostsDirectory = "Posts" }).Execute(project);

		var post = Assert.Single(GetPosts(project));
		Assert.Equal(2, post.Count);
		Assert.Equal("Hello", post["title"]);
		Assert.Equal("hello", post["fileName"]);
	}

	[Fact]
	public void Execute_WithPostMetadata_MergesReturnedMetadataIntoPost()
	{
		var project = new MetalsharpProject();
		project.AddOutput(new MetalsharpFile("", "Posts/hello.html", new Dictionary<string, object> { ["title"] = "Hello" }));

		new SimpleBlog(new SimpleBlogOptions
		{
			PostsDirectory = "Posts",
			PostMetadata = f => new Dictionary<string, object> { ["url"] = $"/blog/{f.Name}" }
		}).Execute(project);

		var post = Assert.Single(GetPosts(project));
		Assert.Equal("Hello", post["title"]);
		Assert.Equal("/blog/hello", post["url"]);
		Assert.Equal("hello", post["fileName"]);
	}

	[Fact]
	public void Execute_WhenPostMetadataReturnsExistingKey_Throws()
	{
		var project = new MetalsharpProject();
		project.AddOutput(new MetalsharpFile("", "Posts/hello.html", new Dictionary<string, object> { ["title"] = "Hello" }));

		var plugin = new SimpleBlog(new SimpleBlogOptions
		{
			PostsDirectory = "Posts",
			PostMetadata = _ => new Dictionary<string, object> { ["title"] = "Overwritten" }
		});

		Assert.Throws<ArgumentException>(() => plugin.Execute(project));
	}

	[Fact]
	public void Execute_AddsFileNameMetadataFromFileName()
	{
		var project = new MetalsharpProject();
		project.AddOutput(new MetalsharpFile("", "Posts/hello-world.html", new Dictionary<string, object>()));

		new SimpleBlog(new SimpleBlogOptions { PostsDirectory = "Posts" }).Execute(project);

		Assert.Equal("hello-world", Assert.Single(GetPosts(project))["fileName"]);
	}

	[Fact]
	public void Execute_DefaultBlogFilePath_IsBlogHtml()
	{
		var project = new MetalsharpProject();

		new SimpleBlog().Execute(project);

		Assert.Contains(project.OutputFiles, f => f.FilePath == "blog.html");
	}

	[Fact]
	public void Execute_WithCustomBlogFilePath_UsesThatPath()
	{
		var project = new MetalsharpProject();

		new SimpleBlog(new SimpleBlogOptions { BlogFilePath = "Blog/index.html" }).Execute(project);

		Assert.Contains(project.OutputFiles, f => f.FilePath == "Blog/index.html");
		Assert.DoesNotContain(project.OutputFiles, f => f.FilePath == "blog.html");
	}

	[Fact]
	public void Execute_WithoutBlogMetadata_OnlyAddsPostsKey()
	{
		var project = new MetalsharpProject();

		new SimpleBlog().Execute(project);

		var key = Assert.Single(GetBlogFile(project).Metadata.Keys);
		Assert.Equal("posts", key);
	}

	[Fact]
	public void Execute_WithBlogMetadata_MergesIntoBlogFileMetadata()
	{
		var project = new MetalsharpProject();

		new SimpleBlog(new SimpleBlogOptions
		{
			BlogMetadata = new Dictionary<string, object> { ["title"] = "My Blog" }
		}).Execute(project);

		var blog = GetBlogFile(project);
		Assert.Equal("My Blog", blog.Metadata["title"]);
		Assert.True(blog.Metadata.ContainsKey("posts"));
	}

	[Fact]
	public void Execute_DoesNotMutateCallersBlogMetadataDictionary()
	{
		var project = new MetalsharpProject();
		var blogMetadata = new Dictionary<string, object> { ["title"] = "My Blog" };

		new SimpleBlog(new SimpleBlogOptions { BlogMetadata = blogMetadata }).Execute(project);

		Assert.Single(blogMetadata);
		Assert.False(blogMetadata.ContainsKey("posts"));
	}

	[Fact]
	public void Execute_WhenBlogMetadataAlreadyHasPostsKey_OverwritesIt()
	{
		var project = new MetalsharpProject();
		project.AddOutput(new MetalsharpFile("", "Posts/hello.html", new Dictionary<string, object> { ["title"] = "Hello" }));

		new SimpleBlog(new SimpleBlogOptions
		{
			PostsDirectory = "Posts",
			BlogMetadata = new Dictionary<string, object> { ["posts"] = "placeholder" }
		}).Execute(project);

		var posts = Assert.IsType<List<Dictionary<string, object>>>(GetBlogFile(project).Metadata["posts"]);
		Assert.Single(posts);
	}

	[Fact]
	public void Execute_AddsExactlyOneNewOutputFile()
	{
		var project = new MetalsharpProject();
		project.AddOutput(new MetalsharpFile("", "Posts/a.html", new Dictionary<string, object>()));
		project.AddOutput(new MetalsharpFile("", "Other/b.html", new Dictionary<string, object>()));
		var countBefore = project.OutputFiles.Count();

		new SimpleBlog(new SimpleBlogOptions { PostsDirectory = "Posts" }).Execute(project);

		Assert.Equal(countBefore + 1, project.OutputFiles.Count());
	}

	[Fact]
	public void Execute_GeneratedBlogFile_HasEmptyContent()
	{
		var project = new MetalsharpProject();

		new SimpleBlog().Execute(project);

		Assert.Empty(GetBlogFile(project).Text);
	}
}
