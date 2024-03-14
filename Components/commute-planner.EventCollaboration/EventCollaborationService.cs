using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace commute_planner.EventCollaboration;

public class EventCollaborationService : IHostedService
{
  private readonly CancellationTokenSource _cts;
  private readonly ICommutePlannerExchange _exchange;
  private readonly ILogger<EventCollaborationService> _log;

  public EventCollaborationService(
    ICommutePlannerExchange exchange,
    ILogger<EventCollaborationService> log
    )
  {
    _cts = new();
    _exchange = exchange;
    _log = log;
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

    _exchange.Close();

    await Task.CompletedTask;
  }
}