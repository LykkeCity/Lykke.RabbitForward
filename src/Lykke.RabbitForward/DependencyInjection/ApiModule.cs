using System;
using Autofac;
using Common.Log;
using Lykke.RabbitForward.Core;
using Lykke.RabbitForward.Core.Services;
using Lykke.RabbitForward.Services;

namespace Lykke.RabbitForward.DependencyInjection
{
    public class ApiModule : Module
    {
        private readonly ApplicationSettings _settings;
        private readonly ILog _log;

        public ApiModule(ApplicationSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log).SingleInstance();

            builder.RegisterInstance(_settings).SingleInstance();
            builder.RegisterInstance(_settings.RabbitForwardService).SingleInstance();



            RegisterWallet(builder);

        }

        private void RegisterWallet(ContainerBuilder builder)
        {

            builder.RegisterType <RabbitTranslator>()
                .As<IStartable>()
                .As<IRabbitTranslator>()
                .SingleInstance();

            builder.RegisterType<RabbitReader>()
                .As<IStartable>()
                .As<IRabbitReader>()
                .SingleInstance();

           
        }

       
    }
}