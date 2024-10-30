using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UserAPI.DataModels;

namespace UserAPI.Services
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqService()
        {
            var factory = new ConnectionFactory
            { HostName = "localhost" ,
            UserName="guest",
            Password="guest"
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }
        public async Task PublishNotificationToQueue(RabbitMqData rabbitMqData)
        {
            try
            {
                var queueName = $"notificationQueue_{rabbitMqData.UserId}";

                _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(rabbitMqData));
                _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RabbitMQ'ya gönderim hatası: {ex.Message}");
            }


        }
        public void StartConsuming(string userId, Action<RabbitMqData> onNotificationReceived)
        {
            var queueName = $"notificationQueue_{userId}";
            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"Ham mesaj alındı: {message}");

                var notification = JsonConvert.DeserializeObject<RabbitMqData>(message);
                if (notification == null)
                {
                    Console.WriteLine("Mesajın serileştirilmesi başarısız oldu.");
                    return;
                }

                Console.WriteLine($"Bildirim alındı: {notification.Title} - {notification.Message}");
                onNotificationReceived(notification);
            };

            Console.WriteLine($"Kuyruk için tüketici başlatılıyor: {queueName}");
            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            Console.WriteLine("Tüketici başlatıldı, mesaj bekleniyor...");
        }
    }
}
