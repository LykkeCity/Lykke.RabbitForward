using System;
using System.Threading;
using Autofac;
using Common.Log;
using Lykke.RabbitForward.Core;
using Lykke.RabbitForward.Core.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Lykke.RabbitForward.Services
{
    public class RabbitReader : IRabbitReader
        
    {
        private readonly ILog _log;
        private readonly ApplicationSettings.RabbitForwardSettings _settings;
        private readonly IRabbitTranslator _translator;
        private Thread _thread;
        private readonly int _reconnectTimeOut = 3000;

        public RabbitReader(ILog log, ApplicationSettings.RabbitForwardSettings settings, IRabbitTranslator translator)
        {
            _log = log;
            _settings = settings;
            _translator = translator;
        }

        void IStartable.Start()
        {
            Start();
        }

        public RabbitReader Start()
        {
           

            if (_thread != null) return this;

            _thread = new Thread(ReadThread);
            _thread.Start();
            return this;
        }

        private bool IsStopped()
        {
            return _thread == null;
        }

        private void ReadThread()
        {
            while (!IsStopped())
                try
                {
                    ConnectAndReadAsync();
                }
                catch (Exception ex)
                {
                    _log?.WriteFatalErrorAsync(Constants.ComponentName, "ReadThread", "", ex).Wait();
                }
                finally
                {
                    Thread.Sleep(_reconnectTimeOut);
                }
        }

        private void ConnectAndReadAsync()
        {
            var factory = new ConnectionFactory { Uri =  new Uri(_settings.RabbitForwardList.SourceRabbit.ConnectionString) };
            

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {

                var queueName = $"{_settings.RabbitForwardList.SourceRabbit.ExchangeName}.{_settings.RabbitForwardList.SourceRabbit.QueueName}";

                var consumer = new QueueingBasicConsumer(channel);
                string tag = channel.BasicConsume(queueName, true, consumer);

                //consumer.Received += MessageReceived;

                while (connection.IsOpen && !IsStopped())
                {
                    BasicDeliverEventArgs eventArgs;
                    var delivered = consumer.Queue.Dequeue(2000, out eventArgs);
                    if (delivered)
                        MessageReceived(eventArgs);
                }

                channel.BasicCancel(tag);
                connection.Close();

            }
        }


        private void MessageReceived(BasicDeliverEventArgs basicDeliverEventArgs)
        {
            try
            {
                var body = basicDeliverEventArgs.Body;
                _translator.Send(body);
            }
            catch (Exception ex)
            {
                _log?.WriteErrorAsync(Constants.ComponentName, "Message Recieveing", "", ex);
            }

        }

        public void Stop()
        {
            var thread = _thread;

            if (thread == null)
                return;

            _thread = null;
            thread.Join();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}