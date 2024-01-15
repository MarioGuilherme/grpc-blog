using Blog;
using Grpc.Core;
using Grpc.Reflection;
using Grpc.Reflection.V1Alpha;
using server;

const int PORT = 50552;

Server? server = null;

try {
    ReflectionServiceImpl reflectionServiceImpl = new(BlogService.Descriptor, ServerReflection.Descriptor);

    server = new() {
        Services = {
            BlogService.BindService(new BlogServiceImpl()),
            ServerReflection.BindService(reflectionServiceImpl)
        },
        Ports = {
            new("localhost", PORT, ServerCredentials.Insecure)
		}
    };
    server.Start();
    Console.WriteLine($"The server is listening on the port: {PORT}");
    Console.ReadKey();
} catch (IOException e) {
    Console.WriteLine($"The server failed to start: {e.Message}");
    throw;
} finally {
    if (server is not null)
        await server.ShutdownAsync();
}