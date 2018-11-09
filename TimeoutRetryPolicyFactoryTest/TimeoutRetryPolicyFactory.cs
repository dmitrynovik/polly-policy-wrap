using System;
using System.ComponentModel;
using System.Diagnostics;
using Polly;
using Polly.Timeout;

namespace TimeoutRetryPolicyFactoryTest
{
    public interface IPolicyFactory
    {
        Policy Create();
    }

    public interface IPolicyLogger
    {
        void LogInfo(string id, TimeSpan duration, string message = null);
        void LogError(string id, TimeSpan duration, string error, Exception ex = null);
    }

    [Serializable]
    public class TimeoutRetryPolicySettings
    {
        [Description("The timeout per communications attempt")]
        [DefaultValue("00:00:05")]
        public virtual string PerRetryTimeout { get; set; }

        [Description("The amount of time to wait before retrying a failed communications attempt")]
        [DefaultValue("00:00:02")]
        public virtual string RetryWaitTime { get; set; }

        [Description("The total timeout for all communications attempts")]
        [DefaultValue("00:00:10")]
        public virtual string TotalTimeout { get; set; }

        [Description("The maximum amount of communications attempt")]
        [DefaultValue(5)]
        public virtual int? RetryCount { get; set; }
    }

    public class TimeoutRetryPolicyFactory : IPolicyFactory
    {
        //private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public int RetryCount { get; }

        public TimeSpan TotalTimeout { get; }

        public TimeSpan RetryWaitTime { get; }

        public TimeSpan PerRetryTimeout { get; }

        public TimeoutRetryPolicyFactory(TimeoutRetryPolicySettings policySettings)
        {
            RetryCount = policySettings.RetryCount ?? 5;

            PerRetryTimeout = TimeSpan.TryParse(policySettings.PerRetryTimeout, out var perRetryTimeout)
                ? perRetryTimeout
                : TimeSpan.FromSeconds(5);

            RetryWaitTime = TimeSpan.TryParse(policySettings.RetryWaitTime, out var retryWaitTime)
                ? retryWaitTime
                : TimeSpan.FromSeconds(2);

            TotalTimeout = TimeSpan.TryParse(policySettings.TotalTimeout, out var totalTimeout)
                ? totalTimeout
                : TimeSpan.FromSeconds(10);
        }

        public TimeoutRetryPolicyFactory(TimeSpan? perRetryTimeout = null, int retryCount = 5, TimeSpan? retryWaitTime = null, TimeSpan? totalTimeout = null)
        {
            RetryCount = retryCount;
            PerRetryTimeout = perRetryTimeout ?? TimeSpan.FromSeconds(5);
            RetryWaitTime = retryWaitTime ?? TimeSpan.FromSeconds(2);
            TotalTimeout = totalTimeout ?? TimeSpan.FromSeconds(10);
        }

        public Policy Create()
        {
            var perRetryTimeoutPolicy = Policy
                .TimeoutAsync(PerRetryTimeout, TimeoutStrategy.Pessimistic, async (context, span, arg3) =>
                {
                    Trace.TraceError($"Timout {span} Sending Message");
                });

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(RetryCount, retryAttemp => RetryWaitTime, (exception, span, context) =>
                {
                    Trace.TraceError($"Error Sending Message, retry attempt: {span}, error: {exception}");
                });

            var totalTimeoutPolicy = Policy
                .TimeoutAsync(TotalTimeout, TimeoutStrategy.Pessimistic, async (context, span, arg3) =>
                {
                    Trace.TraceError($"Timout {span} Sending Message");
                });

            var policy = Policy.WrapAsync(totalTimeoutPolicy, retryPolicy, perRetryTimeoutPolicy);
            return policy;
        }
    }
}