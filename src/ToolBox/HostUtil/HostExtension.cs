public static class HostExtension
{
    /// <summary>
    /// use log4net.config in working diretory
    /// </summary>
    /// <param name="hostBuilder"></param>
    private static IHostBuilder ConfigureDefaultLogging(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureLogging((context, loggingbuilder) =>
        {
            loggingbuilder.AddFilter("System", LogLevel.Warning); //ingore system info log
                loggingbuilder.AddFilter("Microsoft", LogLevel.Warning);//ignore Microsoft info log
                loggingbuilder.AddLog4Net();
            loggingbuilder.AddConsole();
            //loggingbuilder.AddSimpleConsole();
        });
    }

    private static IHostBuilder CreateDefaultHostBuilder()
    {
        return Host.CreateDefaultBuilder().ConfigureDefaultLogging();
    }

    /// <summary>
    /// simple host will use log4net for logger, include file logger and console logger
    /// simple host will Host service implement HostedServiceBase
    /// simple host will use appsettings.json as configuration file
    /// simple host will use environment variables as well 
    /// </summary>
    /// <typeparam name="THostService"></typeparam>
    /// <param name="servicesInjection"></param>
    /// <returns></returns>
    public static IHost CreateSimpleHost<THostService>(Action<IServiceCollection> servicesInjection) where THostService : HostedServiceBase
    {
        return
            CreateDefaultHostBuilder()
            .ConfigureDefaultLogging()
            .ConfigureAppConfiguration
            (
                configurationBuilder =>
                {
                    configurationBuilder.AddJsonFile("appsettings.json", true)
                      .AddEnvironmentVariables();
                })
            .ConfigureServices(services =>
            {
                servicesInjection(services);
                services.AddHostedService<THostService>();
            })
            .Build();
    }
}