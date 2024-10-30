using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserAPI.DataModels;

namespace UserAPI.Services
{
    public interface IRabbitMqService
    {
        Task PublishNotificationToQueue(RabbitMqData rabbitMqData);
        void StartConsuming(string userId, Action<RabbitMqData> onNotificationReceived);
    }
}