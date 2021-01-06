using Google.Protobuf.Collections;
using Greet;
using Grpc.Core;
using Grpc.Net.Client;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace grpcclient
{
    class Program
    {
        static  async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions { HttpHandler = GetHttpHandler() });
            var client = new HelloService.HelloServiceClient(channel);
            ////
            var hellorequest=new HelloRequest { Greeting = "Hello Server 1 !!"};
            hellorequest.Infos.Add("hello", "world");
  
            Console.WriteLine($"SayHello Resp1: {GetHelloResponsString( client.SayHello(hellorequest))}");
            Console.WriteLine($"SayHello Resp2:{GetHelloResponsString(client.SayHello(new HelloRequest { Greeting= "Hello Server 2 !!" }))}");
           ///
            var list = client.ListHello(new HelloRequest { Greeting = "Hello Server List Hello" });
            while (await list.ResponseStream.MoveNext()) {
                Console.WriteLine("ListHello Server Resp:" + list.ResponseStream.Current.Reply);
            }
            ///
            using (var clientcall = client.SayMoreHello()) {
                for (int i = 0; i < 3; i++)
                {
                   await clientcall.RequestStream.WriteAsync(new HelloRequest { Greeting = $"sayMoreHello Hello Server {i + 1}" });
                }
                await clientcall.RequestStream.CompleteAsync();
                var response = await clientcall.ResponseAsync;
                Console.WriteLine($"SayMoreHello Server Resp {response.Reply}");
            }

            /////
            ///
            using (var clientcall2 = client.SayHelloChat()) {
                var response2 = Task.Run(async () =>
                {
                    while (await clientcall2.ResponseStream.MoveNext())
                    {
                        Console.WriteLine($"SayHelloChat Server Say {clientcall2.ResponseStream.Current.Greeting}");
                    }
                });
                for (int i = 0; i < 3; i++)
                {
                  await  clientcall2.RequestStream.WriteAsync(new HelloRequest { Greeting = $"SayHelloChat Hello Server {i + 1}" });
                    await Task.Delay(1000);
                }
              
                await clientcall2.RequestStream.CompleteAsync();
            }

            Console.ReadKey();
           
        }
        static HttpClientHandler GetHttpHandler()
        {
            var handler = new HttpClientHandler()
            {
                SslProtocols = SslProtocols.Tls12,
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (message, cer, chain, errors) =>
                {
                    return chain.Build(cer);
                }
            };
            var path = AppDomain.CurrentDomain.BaseDirectory + "cert\\client.pfx";
            var crt = new X509Certificate2(path, "123456789");
            handler.ClientCertificates.Add(crt);
            return handler;
        }
        static string GetHelloResponsString(HelloResponse response) {
            string msg = "Reply:" + response.Reply + "Details:" + response.Details[0].Value.ToString(System.Text.Encoding.UTF8);
            return msg;
        }
    }
}
