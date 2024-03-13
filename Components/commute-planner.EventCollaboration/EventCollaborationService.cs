using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace commute_planner.EventCollaboration;

public class EventCollaborationService : IHostedService
{
  private const string QueueName = "queue";
  private const string ExchangeName = "exchange";
  private const string RoutingKey = "routingKey";
  
  private readonly CancellationTokenSource _cts;
  private readonly IConnection _messaging;
  private readonly IModel _channel;
  private readonly ILogger<EventCollaborationService> _log;
  private readonly string _consumerTag;

  public EventCollaborationService(
    IConnection messaging,
    ILogger<EventCollaborationService> log)
  {
    _cts = new();
    _messaging = messaging;
    _channel = messaging.CreateModel();
    _log = log;
    
    _channel.QueueDeclare(QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null!);
    _channel.QueueBind(QueueName, ExchangeName, RoutingKey, arguments: null!);

    _consumerTag = _channel.BasicConsume(QueueName, autoAck: true,
      consumerTag: "", noLocal: false, exclusive: false, arguments: null!,
      consumer: null!);
  }
  public async Task StartAsync(CancellationToken token = default)
  {
    var cts =
      CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, token);
    
    await Task.CompletedTask;
  }

  public async Task StopAsync(CancellationToken token = default)
  {
    var cts =
      CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, token);
    
    _channel.BasicCancel(_consumerTag);

    await Task.CompletedTask;
  }
}