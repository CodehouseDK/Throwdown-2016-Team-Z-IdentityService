using System;
using System.Net.WebSockets;
using IdentityAndEmotionService.Services;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace IdentityAndEmotionService
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton<IFaceApi, FaceApi>();
            services.AddSingleton<IConfigurationRoot>(_ => Configuration);
            services.AddSingleton<IFaceIdentityRepository, FaceIdentityRepository>();
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseWebSockets();
            app.Use(async (http, next) =>
            {
                if (http.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await http.WebSockets.AcceptWebSocketAsync();
                    if (webSocket != null && webSocket.State == WebSocketState.Open)
                    {
                        WebsocketConnections.AddSocket(webSocket);
                    }
                }
                else
                {
                    await next();
                }
            });

          

            app.UseIISPlatformHandler();

            app.UseDeveloperExceptionPage();

            app.UseMvcWithDefaultRoute();

            app.UseStaticFiles();
        }
    }

    public class FaceIdentity
    {
        public string UserName { get; set; }
        public string Id { get; set; }
        public Uri Url { get; set; }
        public string PersonId { get; set; }
    }
}