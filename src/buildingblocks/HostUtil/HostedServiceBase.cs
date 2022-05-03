public abstract class HostedServiceBase : IHostedService
{
    protected virtual ILogger Logger { get; set; }
    protected virtual IConfiguration Configuration { get; set; }
    public HostedServiceBase(IConfiguration configuration,ILoggerFactory loggerFactory)
    {
        Configuration = configuration;
        Logger=loggerFactory.CreateLogger<HostedServiceBase>();
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await StartInternalAsync(cancellationToken);
    }

    protected abstract Task StartInternalAsync(CancellationToken cancellationToken);
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await StopInternalAsync(cancellationToken);
    }

    protected abstract Task StopInternalAsync(CancellationToken cancellationToken);
}