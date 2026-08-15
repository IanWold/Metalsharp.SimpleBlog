namespace Metalsharp.SimpleBlog;

/// <summary>
/// Extension methods for using <see cref="SimpleBlog"/> with a <see cref="MetalsharpProject"/>.
/// </summary>
public static class Extensions
{
	/// <summary>
	/// Invokes <see cref="SimpleBlog"/> as a plugin.
	/// </summary>
	/// <param name="project">
	/// The <see cref="MetalsharpProject"/> being built.
	/// </param>
	/// <param name="options">
	/// The options configuring how posts are found, ordered, and annotated, and how the generated blog file is created. If <see langword="null"/>, the defaults on <see cref="SimpleBlogOptions"/> are used.
	/// </param>
	/// <returns>
	/// The current <see cref="MetalsharpProject"/>, allowing it to be fluent.
	/// </returns>
	public static MetalsharpProject UseSimpleBlog(this MetalsharpProject project, SimpleBlogOptions? options = null) =>
		project.Use(new SimpleBlog(options));
}