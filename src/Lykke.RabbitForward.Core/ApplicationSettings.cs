using System;
using JetBrains.Annotations;

namespace Lykke.RabbitForward.Core
{
    [UsedImplicitly]
    public class ApplicationSettings
    {
        public RabbitForwardSettings RabbitForwardService { get; set; }

        public SlackNotificationsSettings SlackNotifications { get; set; }

        [UsedImplicitly]
        public class RabbitForwardSettings
        {
            public RabbitForwardListSettings RabbitForwardList { get; set; }
            public LogsSettings Logs { get; set; }
        }

        [UsedImplicitly]
        public class RabbitForwardListSettings
        {
            public RabbitSettings SourceRabbit { get; set; }
            public RabbitSettings DestinationRabbit { get; set; }
            public bool IsDurable { get; set; }
        }

        [UsedImplicitly]
        public class LogsSettings
        {
            public string DbConnectionString { get; set; }
        }

        [UsedImplicitly]
        public class SlackNotificationsSettings
        {
            public AzureQueueSettings AzureQueue { get; set; }

            public int ThrottlingLimitSeconds { get; set; }
        }

        [UsedImplicitly]
        public class AzureQueueSettings
        {
            public string ConnectionString { get; set; }

            public string QueueName { get; set; }
        }

        [UsedImplicitly]
        public class RabbitSettings
        {
            public string ConnectionString { get; set; }
            public string ExchangeName { get; set; }
            public string QueueName { get; set; }
        }
    }

    
}