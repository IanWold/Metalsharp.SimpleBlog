namespace Metalsharp.SimpleBlog;

public static class Extensions
{
	public static MetalsharpProject UseSimpleBlog(this MetalsharpProject project, SimpleBlogOptions? options = null) =>
		project.Use(new SimpleBlog(options));
}