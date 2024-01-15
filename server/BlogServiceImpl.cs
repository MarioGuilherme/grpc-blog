using Blog;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Metadata;
using static Blog.BlogService;

namespace server;

public class BlogServiceImpl : BlogServiceBase {
    private readonly SqlConnection _sqlConnection = new("Server=(localdb)\\mssqllocaldb;Database=BlogGRPC;Trusted_Connection=True;MultipleActiveResultSets=true");
    
    public override async Task<CreateBlogResponse> CreateBlog(CreateBlogRequest request, ServerCallContext context) {
        Blog.Blog blog = request.Blog;
        await this._sqlConnection.OpenAsync();

        string sql = "INSERT INTO Blogs (AuthorId, Title, Content) VALUES (@authorId, @title, @content) SELECT SCOPE_IDENTITY()";
        using SqlCommand cmd = new(sql, this._sqlConnection);
        cmd.Parameters.Add("@authorId", SqlDbType.Int).Value = blog.AuthorId;
        cmd.Parameters.Add("@title", SqlDbType.VarChar, 100).Value = blog.Title;
        cmd.Parameters.Add("@content", SqlDbType.VarChar, 800).Value = blog.Content;
        cmd.CommandType = CommandType.Text;

        blog.BlogId = Convert.ToInt32(cmd.ExecuteScalar());
        await this._sqlConnection.CloseAsync();

        return new() { Blog = blog };
    }

    public override async Task<ReadBlogResponse> ReadBlog(ReadBlogRequest request, ServerCallContext context) {
        int blogId = request.BlogId;
        Blog.Blog? blog = null;
        await this._sqlConnection.OpenAsync();

        string sql = "SELECT * FROM Blogs WHERE BlogId = @blogId";
        using SqlCommand cmd = new(sql, this._sqlConnection);
        cmd.Parameters.Add("@blogId", SqlDbType.Int).Value = blogId;
        cmd.CommandType = CommandType.Text;

        using (SqlDataReader reader = await cmd.ExecuteReaderAsync()) {
            if (reader.Read())
                blog = new() {
                    BlogId = (int)reader["BlogId"],
                    AuthorId = (int)reader["AuthorId"],
                    Content = (string)reader["Content"],
                    Title = (string)reader["Title"]
                };
        }

        if (blog is null)
            throw new RpcException(new(StatusCode.NotFound, $"The blog id {blogId} wasn't find"));

        await this._sqlConnection.CloseAsync();
        return new() { Blog = blog };
    }

    public override async Task<UpdateBlogResponse> UpdateBlog(UpdateBlogRequest request, ServerCallContext context) {
        Blog.Blog blog = request.Blog;
        Blog.Blog updatedBlog = null!;
        await this._sqlConnection.OpenAsync();

        string sqlCheck = "SELECT * FROM Blogs WHERE BlogId = @blogId";
        using SqlCommand cmdCheck = new(sqlCheck, this._sqlConnection);
        cmdCheck.Parameters.Add("@blogId", SqlDbType.Int).Value = blog.BlogId;
        cmdCheck.CommandType = CommandType.Text;

        if (await cmdCheck.ExecuteScalarAsync() is null)
            throw new RpcException(new(StatusCode.NotFound, $"The blog id {blog.BlogId} wasn't find"));

        string sqlUpdate = "UPDATE Blogs SET AuthorId = @authorId, Title = @title, Content = @content WHERE BlogId = @blogId";
        using SqlCommand cmdUpdate = new(sqlUpdate, this._sqlConnection);
        cmdUpdate.Parameters.Add("@authorId", SqlDbType.Int).Value = blog.AuthorId;
        cmdUpdate.Parameters.Add("@title", SqlDbType.VarChar, 100).Value = blog.Title;
        cmdUpdate.Parameters.Add("@content", SqlDbType.VarChar, 800).Value = blog.Content;
        cmdUpdate.Parameters.Add("@blogId", SqlDbType.Int).Value = blog.BlogId;
        cmdUpdate.CommandType = CommandType.Text;

        await cmdUpdate.ExecuteNonQueryAsync();

        using (SqlDataReader reader = await cmdCheck.ExecuteReaderAsync()) {
            if (reader.Read())
                updatedBlog = new() {
                    BlogId = (int)reader["BlogId"],
                    AuthorId = (int)reader["AuthorId"],
                    Content = (string)reader["Content"],
                    Title = (string)reader["Title"]
                };
        }

        await this._sqlConnection.CloseAsync();
        return new() { Blog = updatedBlog };
    }

    public override async Task<DeleteBlogResponse> DeleteBlog(DeleteBlogRequest request, ServerCallContext context) {
        int blogId = request.BlogId;
        await this._sqlConnection.OpenAsync();

        string sql = "DELETE Blogs WHERE BlogId = @blogId";
        using SqlCommand cmd = new(sql, this._sqlConnection);
        cmd.Parameters.Add("@blogId", SqlDbType.Int).Value = blogId;
        cmd.CommandType = CommandType.Text;

        if (cmd.ExecuteNonQuery() != 1)
            throw new RpcException(new(StatusCode.NotFound, $"The blog id {blogId} wasn't find"));

        await this._sqlConnection.CloseAsync();
        return new() { BlogId = blogId };
    }

    public override async Task ListBlog(Empty request, IServerStreamWriter<ListBlogResponse> responseStream, ServerCallContext context) {
        await this._sqlConnection.OpenAsync();

        string sql = "SELECT * FROM Blogs";
        using SqlCommand cmd = new(sql, this._sqlConnection);
        cmd.CommandType = CommandType.Text;

        using (SqlDataReader reader = await cmd.ExecuteReaderAsync()) {
            while (reader.Read())
                await responseStream.WriteAsync(new() {
                    Blog = new() {
                        BlogId = (int)reader["BlogId"],
                        AuthorId = (int)reader["AuthorId"],
                        Content = (string)reader["Content"],
                        Title = (string)reader["Title"]
                    }
                });
        }

        await this._sqlConnection.CloseAsync();
    }
}