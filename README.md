<div align="center">

# Metalsharp.SimpleBlog

[![NuGet](https://img.shields.io/nuget/v/Metalsharp.SimpleBlog.svg?logo=nuget&logoColor=white&style=for-the-badge)](https://www.nuget.org/packages/Metalsharp.SimpleBlog/)

A barebones blog plugin for [Metalsharp](https://github.com/IanWold/Metalsharp)

</div>

---

`Metalsharp.SimpleBlog` doesn't render anything itself. It walks the posts you've already generated, collects their metadata into an ordered list, and attaches that list to a new output file for a templating plugin (or your own code) to turn into an actual blog index page:

```c#
new MetalsharpProject()
    .AddInput("Posts")
    .UseFrontmatter()
    .UseMarkdown()
    .UseSimpleBlog(new SimpleBlogOptions
    {
        PostsDirectory = "Posts",
        PostsOrderQuery = file => DateTime.Parse((string)file.Metadata["date"])
    })
    .Build();
```

## What SimpleBlog Does

1. **Gathers posts** — filters `project.OutputFiles` down to whatever sits in `SimpleBlogOptions.PostsDirectory`, optionally sorted by a key you supply (e.g. publish date).
2. **Annotates each post** — optionally runs your `PostMetadata` function against each file to inject extra fields, adds a `fileName` entry, then snapshots that file's metadata into a `posts` list.
3. **Emits an index file** — adds a new output file (`blog.html` by default, empty content) whose metadata is your `BlogMetadata` merged with that `posts` list.

Nothing gets rendered to HTML for you — the plugin's whole job is handing a `posts` collection to whatever runs *after* it in the pipeline.

## Tutorial

This walks through building a minimal blog with a listing page. It assumes you're already comfortable with the basics of [Metalsharp](https://github.com/IanWold/Metalsharp) itself — if not, its [quickstart](https://github.com/IanWold/Metalsharp/blob/master/Metalsharp.Documentation/quickstart.md) is a good five-minute primer.

### Lay out the project

```text
MyBlog
├── Posts
│   ├── hello-world.md
│   └── a-second-post.md
└── Program.cs
```

Each post is a Markdown file with frontmatter:

```markdown
---
title: Hello, World!
date: 2026-01-15
---

This is my first post.
```

```markdown
---
title: A Second Post
date: 2026-02-01
---

And here's another one.
```

### Build the pipeline

```c#
using Metalsharp;
using Metalsharp.SimpleBlog;

new MetalsharpProject()
    .AddInput("Posts")
    .UseFrontmatter()
    .UseMarkdown()
    .UseSimpleBlog(new SimpleBlogOptions
    {
        PostsDirectory = "Posts",
        PostsOrderQuery = file => DateTime.Parse((string)file.Metadata["date"])
    })
    .Build();
```

`UseFrontmatter` reads the `title`/`date` block into each file's `Metadata`, and `UseMarkdown` turns the Markdown body into HTML — so by the time `UseSimpleBlog` runs, `project.OutputFiles` already contains `Posts/hello-world.html` and `Posts/a-second-post.html`, each carrying its own frontmatter.

SimpleBlog picks those two files out (their virtual `Directory` matches `PostsDirectory`), sorts them newest-first by the parsed `date` (`PostsOrderedDescending` defaults to `true`), and adds a third output file, `blog.html`, whose metadata looks like:

```text
Metadata["posts"] = [
    { title = "A Second Post", date = "2026-02-01", fileName = "a-second-post" },
    { title = "Hello, World!", date = "2026-01-15", fileName = "hello-world" }
]
```

Note that frontmatter values arrive as plain `string`s — that's why `PostsOrderQuery` parses `date` with `DateTime.Parse` rather than casting it directly.

### Render the listing page

`blog.html` is added with empty content — SimpleBlog only prepares the metadata. Render it with whatever templating plugin you're already using, or do it by hand with a plugin of your own. `MetalsharpFile.Text` is read-only, so write to `Contents` instead:

```c#
using System.Text;

new MetalsharpProject()
    .AddInput("Posts")
    .UseFrontmatter()
    .UseMarkdown()
    .UseSimpleBlog(new SimpleBlogOptions
    {
        PostsDirectory = "Posts",
        PostsOrderQuery = file => DateTime.Parse((string)file.Metadata["date"])
    })
    .Use(project =>
    {
        var blog = project.OutputFiles.First(f => f.FilePath == "blog.html");
        var posts = (List<Dictionary<string, object>>)blog.Metadata["posts"];

        var html = string.Join('\n', posts.Select(post =>
            $"<article><h2>{post["title"]}</h2><p>{post["date"]}</p></article>"
        ));

        blog.Contents = Encoding.UTF8.GetBytes($"<main>{html}</main>");
    })
    .Build();
```

Running this produces `blog.html` listing both posts, newest first, alongside the two rendered post pages.

## Configuring `SimpleBlogOptions`

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `PostsDirectory` | `string` | `""` | The virtual directory (as set by `AddInput`/`AddOutput`) that posts live in. Only output files whose `Directory` matches exactly are treated as posts. |
| `PostsOrderedDescending` | `bool` | `true` | Whether `PostsOrderQuery` sorts newest/highest first. Ignored if `PostsOrderQuery` isn't set. |
| `PostsOrderQuery` | `Func<MetalsharpFile, DateTime>?` | `null` | Selects the sort key for each post (typically a publish date parsed from metadata). If `null`, posts keep whatever order they're found in. |
| `PostMetadata` | `Func<MetalsharpFile, Dictionary<string, object>>?` | `null` | Runs against each post file; anything it returns is merged into that file's metadata before it's copied into the `posts` list. |
| `BlogFilePath` | `string` | `"blog.html"` | The virtual path of the output file `SimpleBlog` creates. |
| `BlogMetadata` | `Dictionary<string, object>?` | `null` | Extra metadata to merge onto the blog output file, alongside the generated `posts` list. |

## Usage

```c#
project.UseSimpleBlog(options);
```

is shorthand for:

```c#
project.Use(new SimpleBlog(options));
```

`options` is optional in both forms — omit it to use every default in the table above.
