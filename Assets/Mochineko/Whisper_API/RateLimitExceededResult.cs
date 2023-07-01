#nullable enable
using Mochineko.Relent.UncertainResult;

namespace Mochineko.Whisper_API
{
    public sealed class RateLimitExceededResult<T>
        : IUncertainRetryableResult<T>
    {
        public bool Success => false;
        public bool Retryable => false;
        public bool Failure => true;
        public string Message { get; }
        
        public RateLimitExceededResult(string message)
        {
            Message = message;
        }
    }
}