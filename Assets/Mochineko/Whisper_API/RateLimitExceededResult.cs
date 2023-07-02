#nullable enable
using Mochineko.Relent.UncertainResult;

namespace Mochineko.Whisper_API
{
    /// <summary>
    /// A result that indicates that the rate limit of API has been exceeded.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class RateLimitExceededResult<T>
        : IUncertainFailureResult<T>
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