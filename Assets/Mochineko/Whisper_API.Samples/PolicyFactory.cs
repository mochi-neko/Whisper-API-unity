#nullable enable
using System;
using Mochineko.Relent.Resilience;
using Mochineko.Relent.Resilience.Bulkhead;
using Mochineko.Relent.Resilience.Retry;
using Mochineko.Relent.Resilience.Timeout;
using Mochineko.Relent.Resilience.Wrap;

namespace Mochineko.Whisper_API.Samples
{
    internal static class PolicyFactory
    {
        private const float TotalTimeoutSeconds = 60f;
        private const int MaxRetryCount = 10;
        private const double ExponentialBackoffFactor = 0.1d;
        private const double ExponentialBackoffBaseNumber = 2d;
        private const int MaxParallelization = 1;

        public static IPolicy<string> Build()
        {
            var totalTimeout = TimeoutFactory.Timeout<string>(
                timeout: TimeSpan.FromSeconds(TotalTimeoutSeconds));

            var retry = RetryFactory.RetryWithExponentialBackoff<string>(
                MaxRetryCount,
                factor: ExponentialBackoffFactor,
                baseNumber: ExponentialBackoffBaseNumber);

            var bulkheadPolicy = BulkheadFactory.Bulkhead<string>(
                MaxParallelization);

            return totalTimeout
                .Wrap(retry)
                .Wrap(bulkheadPolicy);
        }
    }
}