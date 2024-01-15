using Blog;
using Grpc.Core;

Channel channel = new("localhost", 50552, ChannelCredentials.Insecure);

await channel.ConnectAsync();

BlogService.BlogServiceClient client = new(channel);
//CreateBlogResponse response = await client.CreateBlogAsync(new() {
//    Blog = new() {
//        AuthorId = 1,
//        Title = "New Blog",
//        Content = "Hellow World, this is a new blog"
//    }
//});
//Console.WriteLine($"The blog {response.Blog.BlogId} was created!");

try {
    Blog.Blog blog = new() {
        AuthorId = 1,
        Title = "New Blog",
        Content = "Hello World, this is a new blog"
    };

    // CREATE
    //CreateBlogResponse response = await client.CreateBlogAsync(new() { Blog = blog });
    //Console.WriteLine($"The blog {response.Blog.BlogId} was created !");

    // READ
    //ReadBlogResponse response = await client.ReadBlogAsync(new() { BlogId = blog.BlogId });
    //Console.WriteLine(response.Blog);

    // UPDATE
    //blog.AuthorId = 32;
    //blog.Title = "New Blog [UPDATED]";
    //blog.Content = "Hello World, this is a new blog [UPDATED]";
    //UpdateBlogResponse response = await client.UpdateBlogAsync(new() { Blog = blog });

    // DELETE
    //DeleteBlogResponse response = await client.DeleteBlogAsync(new() { BlogId = blog.BlogId });
    //Console.WriteLine($"The blog with id {response.BlogId} was deleted");

    // LIST BLOGS
    AsyncServerStreamingCall<ListBlogResponse> response = client.ListBlog(new());
    while (await response.ResponseStream.MoveNext())
        Console.WriteLine(response.ResponseStream.Current.Blog);
} catch (RpcException e) {
    Console.WriteLine(e.Status.Detail);
}

await channel.ShutdownAsync();
Console.ReadKey();