using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace commute_planner.EventCollaboration;

public class EventCollaborationService : IHostedService
{
  private readonly ICommutePlannerExchange _exchange;
  private readonly ILogger<EventCollaborationService> _log;

  public EventCollaborationService(
    ICommutePlannerExchange exchange,
    ILogger<EventCollaborationService> log
    )
  {
    _exchange = exchange;
    _log = log;
  }
  public Task StartAsync(CancellationToken token = default)
   => Task.CompletedTask;

  public Task StopAsync(CancellationToken token = default)
  {
    _exchange.Close();
    return Task.CompletedTask;
  }
}