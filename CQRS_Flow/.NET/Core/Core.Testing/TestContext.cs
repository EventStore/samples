using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Testing
{
    public class TestContext<TStartup>: TestContext
        where TStartup : class
    {
        public TestContext(
            Func<string, Dictionary<string, string>>? getConfiguration = null,
            Action<IServiceCollection>? setupServices = null,
            Func<IWebHostBuilder, IWebHostBuilder>? setupWebHostBuilder = null
        ): base(getConfiguration, setupServices, (webHostBuilder =>
        {
            SetupWebHostBuilder(webHostBuilder);
            setupWebHostBuilder?.Invoke(webHostBuilder);
            return webHostBuilder;
        }))
        {
        }

        private static IWebHostBuilder SetupWebHostBuilder(IWebHostBuilder webHostBuilder)
            => webHostBuilder.UseStartup<TStartup>();
    }

    public class TestContext: IDisposable
    {
        public HttpClient Client { get; }

        private readonly TestServer server;

        private readonly Func<string, Dictionary<string, string>> getConfiguration =
            _ => new Dictionary<string, string>();

        public TestContext(
            Func<string, Dictionary<string, string>>? getConfiguration = null,
            Action<IServiceCollection>? setupServices = null,
            Func<IWebHostBuilder, IWebHostBuilder>? setupWebHostBuilder = null
        )
        {
            if (getConfiguration != null)
            {
                this.getConfiguration = getConfiguration;
            }

            var fixtureName = new StackTrace().GetFrame(3)!.GetMethod()!.DeclaringType!.Name;

            var configuration = this.getConfiguration(fixtureName);

            setupWebHostBuilder ??= webHostBuilder => webHostBuilder;
            server = new TestServer(setupWebHostBuilder(TestWebHostBuilder.Create(configuration, services =>
            {
                setupServices?.Invoke(services);
            })));


            Client = server.CreateClient();
        }

        public void Dispose()
        {
            server.Dispose();
            Client.Dispose();
        }
    }
}
