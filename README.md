# Mix Heart

[![CodeFactor](https://www.codefactor.io/repository/github/mixcore/mixcore-heart/badge)](https://www.codefactor.io/repository/github/mixcore/mixcore-heart)
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FMix IO%2FMix IO-Heart.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2FMix IO%2FMix IO-Heart?ref=badge_shield)
[![Build Status](https://travis-ci.org/Mix IO/Mix IO-Heart.svg?branch=master)](https://travis-ci.org/Mix IO/Mix IO-Heart)

## License
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FMix IO%2FMix IO-Heart.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2FMix IO%2FMix IO-Heart?ref=badge_large)

## Reference
https://github.com/Mix IO/Mix IO-Heart-Sample

## Sample Code
*Create Models*

```c#
using System;

namespace SimpleBlog.Data.Blog
{
    public class Post
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string SeoName { get; set; }
        public string Excerpt { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public DateTime CreatedDateUTC { get; set; }
    }
}

public class BlogContext : DbContext
{
    public DbSet<Post> Post { get; set; }
    public BlogContext()
    { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=blogging.db");
            //optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=demo-heart.db;Trusted_Connection=True;MultipleActiveResultSets=true");

        }
    }
}

```

*Using Heart*

### Create ViewModel Class
```c#
namespace SimpleBlog.ViewModels
{
    // Create ViewModel using Heart 
    public class PostViewModel: ViewModelBase<BlogContext, Post, PostViewModel>    
    {
        //Declare properties that this viewmodel need         
        public string Id { get; set; }
        [Required(ErrorMessage = "Title is required")]        
        public string Title { get; set; }        
        public DateTime CreatedDateUTC { get; set; }
        
        //Declare properties need for view or convert from model to view        
        public DateTime CreatedDateLocal { get { return CreatedDateUTC.ToLocalTime(); } }        
        
        public PostViewModel()
        {
        }

        public PostViewModel(Post model, BlogContext _context = null, IDbContextTransaction _transaction = null) : base(model, _context, _transaction)
        {
        }
    }
```

## Using
*Save*
```c#
var saveResult = await post.SaveModelAsync();
```
*Get Single*
```c#
var getPosts = await PostViewModel.Repository.GetSingleModelAsync(p=>p.Id==1);
return View(getPosts.Data);
```

*Get All*
```c#
var getPosts = await PostViewModel.Repository.GetModelListAsync();
return View(getPosts.Data);
```

*Get All with predicate*
```c#
var getPosts = await PostViewModel.Repository.GetModelListByAsync(p=>p.Title.Contains("some text"));
return View(getPosts.Data);
```

*Get Paging*
```c#
var getPosts = await PostViewModel.Repository.GetModelListAsync("CreatedDate", OrderByDirection.Descending, pageSize, pageIndex);
return View(getPosts.Data);
```
*Get Paging with predicate*
```c#
var getPosts = await PostViewModel.Repository.GetModelListByAsync(p=>p.Title.Contains("some text"), "CreatedDate", OrderByDirection.Descending, pageSize, pageIndex);
return View(getPosts.Data);
```
