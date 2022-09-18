using KarlCube;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.UsingAzureServiceBus((context,cfg) =>
            {
                cfg.Host("");
            });
        });
        services.AddHostedService<GameHostedService>();
        services.AddSingleton<ScreenSaver>();
    })
    .Build();

host.Run();
