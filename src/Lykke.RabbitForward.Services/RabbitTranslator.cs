using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common.Log;
using Lykke.RabbitForward.Core;
using Lykke.RabbitForward.Core.Services;
using RabbitMQ.Client;

namespace Lykke.RabbitForward.Services
{
    public class RabbitTranslator : IRabbitTranslator
    {

        private readonly ILog _log;
        private readonly ApplicationSettings.RabbitForwardSettings _settings;
        private Thread _thread;
        private readonly Queue<byte[]> _items = new Queue<byte[]>();



        public RabbitTranslator(ApplicationSettings.RabbitForwardSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;

        }

        void IStartable.Start()
        {
            Start();
        }

        public void Dispose()
        {
            Stop();
        }


        public RabbitTranslator Start()
        {
            if (_thread == null)
            {
                _thread = new Thread(ConnectionThread);
                _thread.Start();
            }

            return this;
        }

        private bool IsStopped()
        {
            return _thread == null;
        }

        private void ConnectionThread()
        {
            while (!IsStopped())
            {
                try
                {
                    ConnectAndRead();
                }
                catch (Exception e)
                {
                    _log?.WriteErrorAsync(Constants.ComponentName, "ConnectionThread", "", e).Wait();
                }
            }
        }



        private void ConnectAndRead()
        {
            var factory = new ConnectionFactory { Uri = new Uri(_settings.RabbitForwardList.DestinationRabbit.ConnectionString) };

            

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {

                while (true)
                {
                    if (!connection.IsOpen)
                        throw new Exception($"{Constants.ComponentName}: connection to {_settings.RabbitForwardList.DestinationRabbit.ConnectionString} is closed");

                    channel.ExchangeDeclare(exchange: _settings.RabbitForwardList.DestinationRabbit.ExchangeName , type: "fanout", durable: _settings.RabbitForwardList.IsDurable);
                    var message = EnqueueMessage();

                    if (message == null)
                    {
                        if (IsStopped())
                        {
                            return;
                        }

                        Thread.Sleep(300);
                        continue;
                    }


                    channel.BasicPublish(exchange: _settings.RabbitForwardList.DestinationRabbit.ExchangeName,
                        routingKey: string.Empty,
                        basicProperties: null,
                        body: message);
                }
            }
        }

        private byte[] EnqueueMessage()
        {
            lock (_items)
            {
                if (_items.Count > 0)
                    return _items.Dequeue();
            }

            return default(byte[]);
        }

        public void Send(byte[] obj)
        {
            lock (_items)
                _items.Enqueue(obj);
        }

        public void Stop()
        {
            var thread = _thread;

            if (thread == null)
                return;

            _thread = null;
            thread.Join();
        }
    }
}
