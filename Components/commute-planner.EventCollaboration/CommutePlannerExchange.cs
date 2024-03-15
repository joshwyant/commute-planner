using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace commute_planner.EventCollaboration;

public abstract class CommutePlannerExchange : ICommutePlannerExchange
{
  private const string ExchangeName = "CommutePlannerExchange";
  private const string QueueName = "queue";
  public const string ApiRoutingKey = "apiRoutingKey";
  public const string DataCollectionRoutingKey = "dataCollectionRoutingKey";
  public const string DataProcessingRoutingKey = "dataProcessingRoutingKey";
  
  private readonly IConnection _messagingConnection;
  private readonly IModel _channel;
  private readonly ILogger<CommutePlannerExchange> _log;
  private readonly string _consumerTag;
  private readonly string _consumerRoutingKey;

  public CommutePlannerExchange(
    IConnection messaging,
    ILogger<CommutePlannerExchange> log,
    string consumerRoutingKey)
  {
    _messagingConnection = messaging;
    _consumerRoutingKey = consumerRoutingKey;
    _log = log;
    
    _channel = Model = messaging.CreateModel();
    _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct);
    _channel.QueueDeclare(QueueName, durable: false, exclusive: false,
      autoDelete: false, arguments: null!);
    _channel.QueueBind(QueueName, ExchangeName, consumerRoutingKey,
      arguments: null!);

    _consumerTag = _channel.BasicConsume(QueueName, autoAck: true, this);
  }

  public void Open(string routingKey)
  {
    
  }

  public void Close()
  {
    _channel.BasicCancel(_consumerTag);
    
    _channel.Close();
    _messagingConnection.Close();
  }

  public void SignUp()
  {
    
  }

  public void Publish<T>(string routingKey, T message)
  {
    var messageBodyBytes =
      Encoding.UTF8.GetBytes(JsonSerializer.Serialize<T>(message));
    var props = _channel.CreateBasicProperties();
    props.Headers["MessageType"] = nameof(T);
    _channel.BasicPublish(ExchangeName, routingKey, null, messageBodyBytes);
  }

  protected abstract void OnMessage(string messageType, string routingKey,
    string message);

  public void HandleBasicDeliver(string consumerTag, ulong deliveryTag,
    bool redelivered,
    string exchange, string routingKey, IBasicProperties properties,
    ReadOnlyMemory<byte> body)
  {
    var bytes = body.ToArray();
    using var ms = new MemoryStream(bytes);
    using var reader = new StreamReader(ms);
    var messageType = (string)properties.Headers["MessageType"];
    var message = reader.ReadToEnd();

    // Subclass handles the message
    OnMessage(routingKey, messageType, message);
  }

  #region Unneeded IBasicConsumer members
  public void HandleBasicCancel(string consumerTag) {}
  public void HandleBasicCancelOk(string consumerTag) {}
  public void HandleBasicConsumeOk(string consumerTag) {}
  public void HandleModelShutdown(object model, ShutdownEventArgs reason) {}
  public IModel Model { get; }
  public event EventHandler<ConsumerEventArgs>? ConsumerCancelled;
  #endregion
}
