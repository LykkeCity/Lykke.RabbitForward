using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace Lykke.RabbitForward.Core.Services
{
    public interface IRabbitTranslator : IStartable, IDisposable
    {
        void Send(byte[] obj);
    }
}
