namespace TradingPlatform.BackgroundServices
{
    /// <summary>
    /// A hosted background service that periodically reconciles open orders and
    /// updates stale market-data state. It runs every minute in the background
    /// without blocking the request pipeline.
    /// </summary>
    public class MarketDataReconciliationService : BackgroundService
    {
        private static readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

        private readonly ILogger<MarketDataReconciliationService> _logger;

        public MarketDataReconciliationService(
            ILogger<MarketDataReconciliationService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "{Service} started. Reconciliation interval: {Interval}.",
                nameof(MarketDataReconciliationService),
                _interval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ReconcileAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error during market-data reconciliation.");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("{Service} stopped.", nameof(MarketDataReconciliationService));
        }

        /// <summary>
        /// Performs the reconciliation work for one cycle.
        /// Override or extend this method to add domain-specific logic such as
        /// re-running the matching engine on stale PARTIAL orders.
        /// </summary>
        private Task ReconcileAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Market-data reconciliation cycle started at {Time}.", DateTime.UtcNow);
            // TODO: query for PARTIAL orders older than a threshold and re-attempt matching.
            return Task.CompletedTask;
        }
    }
}
