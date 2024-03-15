using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace commute_planner.EventCollaboration;

public abstract class CommutePlannerExchange : ICommutePlannerExchange
{
  private const string ExchangeName = "CommutePlannerExchange";
  private const string QueueName = "queue";
  public const string ApiRoutingKey = "apiRoutingKey";
  public const string DataCollectionRoutingKey = "dataCollectionRoutingKey";
  public const string DataProcessingRoutingKey = "dataProcessingRoutingKey";
  
  private readonly IConnectionFactory _connectionFactory;
  private IConnection? _connection;
  private IChannel _channel;
  private readonly ILogger<CommutePlannerExchange> _log;
  private string _consumerTag;
  private readonly string _consumerRoutingKey;
  private CancellationTokenSource _cts;

  public CommutePlannerExchange(
    IConnectionFactory factory,
    ILogger<CommutePlannerExchange> log,
    string consumerRoutingKey)
  {
    _connectionFactory = factory;
    _consumerRoutingKey = consumerRoutingKey;
    _log = log;
    _cts = new CancellationTokenSource();
  }

  public async Task OpenAsync(CancellationToken token = default)
  {
    var linkedCts =
      CancellationTokenSource.CreateLinkedTokenSource(token, _cts.Token);

    await Task.Run(async () =>
    {
      // Try to connect
      using (var connectionTimeoutCts = new CancellationTokenSource(300000)) // 5 minutes
      using (var linkedTimeoutCts =
             CancellationTokenSource.CreateLinkedTokenSource(linkedCts.Token,
               connectionTimeoutCts.Token))
      {
        var sw = new Stopwatch();
        sw.Start();
        var backoff = 250;
        do
        {
          try
          {
            _connection =
              await _connectionFactory.CreateConnectionAsync(linkedCts.Token);
          }
          catch (BrokerUnreachableException e)
          {
            _log.LogInformation(
              $"RabbitMQ broker was unreachable. Sleeping for {backoff}ms");
            await Task.Delay(backoff, linkedTimeoutCts.Token);
            backoff = backoff * 3 / 2; // 1.5x
          }
        } while (!linkedTimeoutCts.IsCancellationRequested &&
                 _connection is null);

        if (_connection is null)
        {
          throw new TimeoutException(
            "Could not connect to the RabbitMQ broker.");
        }

        sw.Stop();
        _log.LogInformation(
          $"Connected to the RabbitMQ broker in {sw.ElapsedMilliseconds}ms");
      }

      _channel = Channel = await _connection.CreateChannelAsync();
      await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct);
      await _channel.QueueDeclareAsync(
        QueueName,
        durable: false,
        exclusive: false,
        autoDelete: false,
        arguments: null!);
      await _channel.QueueBindAsync(
        QueueName,
        ExchangeName,
        _consumerRoutingKey,
        arguments: null!);
      _consumerTag =
        await _channel.BasicConsumeAsync(QueueName, autoAck: true, this);
    }, linkedCts.Token);
  }

  public async Task CloseAsync(CancellationToken token = default)
  {
    await Task.Run(async () =>
    {
      await _channel.BasicCancelAsync(_consumerTag);

      await _channel.CloseAsync();
      await _connection.CloseAsync();
    }, token); // original token may already be canceled.
  }

  public async Task PublishAsync<T>(string routingKey, T message, CancellationToken token = default)
  {
    var messageBodyBytes =
      Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
    Dictionary<string, object?> headers = new() { ["MessageType"] = nameof(T) };
    var props = new BasicProperties { Headers = headers };
    await _channel.BasicPublishAsync(ExchangeName, routingKey, props,
      messageBodyBytes);
  }

  protected abstract Task OnMessageAsync(string messageType, string routingKey,
    string message, CancellationToken token);

  public async Task HandleBasicDeliverAsync(string consumerTag,
    ulong deliveryTag, bool redelivered, string exchange, string routingKey,
    ReadOnlyBasicProperties properties, ReadOnlyMemory<byte> body)
  {
    var bytes = body.ToArray();
    using var ms = new MemoryStream(bytes);
    using var reader = new StreamReader(ms);
    var messageType = (string)properties.Headers["MessageType"];
    var message = reader.ReadToEnd();

    // Subclass handles the message
    await OnMessageAsync(routingKey, messageType, message, _cts.Token);
  }

  #region Unused IBasicConsumer members
  public void HandleBasicCancel(string consumerTag) {}
  public void HandleBasicCancelOk(string consumerTag) {}
  public void HandleBasicConsumeOk(string consumerTag) {}
  public void HandleChannelShutdown(object channel, ShutdownEventArgs reason) {}
  public IChannel Channel { get; private set; }
  public event EventHandler<ConsumerEventArgs>? ConsumerCancelled;
  #endregion
}
