using Google.Protobuf.WellKnownTypes;
using Greet;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace grpcserver
{
    public class GreeterService : HelloService.HelloServiceBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override  Task<HelloResponse> SayHello(HelloRequest request, ServerCallContext context)
        {
            var response = new HelloResponse() {  Reply= "Hello World!!"};
            Any any = new Any();
            string val;
            if (request.Infos.TryGetValue("hello",out val) && val==  "world")
            {
                any = Any.Pack(new Hello { Msg = "Good Request" });
            }
            else {
                any = Any.Pack(new Hello { Msg = "Bad Request, Wrong Info Msg" });
            }
            response.Details.Add(any);
            return  Task.FromResult(response);
        }
        public override async Task ListHello(HelloRequest request, IServerStreamWriter<HelloResponse> response, ServerCallContext context)
        {
           await response.WriteAsync(new HelloResponse {  Reply=$"ListHello Reply {request.Greeting} 1" });
            Thread.Sleep(1000);
            await response.WriteAsync(new HelloResponse { Reply= $"ListHello Reply {request.Greeting} 2" });
            Thread.Sleep(1000);
            await response.WriteAsync(new HelloResponse { Reply= $"ListHello Reply {request.Greeting} 3" });
        }
        public override async Task<HelloResponse> SayMoreHello(IAsyncStreamReader<HelloRequest> request, ServerCallContext context)
        {
            while (await request.MoveNext()) {
                Console.WriteLine($"SayMoreHello Client Say: {request.Current.Greeting}");
            }
            return new HelloResponse { Reply="SayMoreHello Recv Muti Greeting" };
        }
       
        public override async Task SayHelloChat(IAsyncStreamReader<HelloRequest> request, IServerStreamWriter<HelloRequest>response, ServerCallContext context)
        {
            var i = 1;
            while (await request.MoveNext()) {
              await  response.WriteAsync(new HelloRequest { Greeting=$"SayHelloChat Server Say Hello {i}" });
                Console.WriteLine($"SayHelloChat Client Say:"+request.Current.Greeting);
            }
        }
    }
}
