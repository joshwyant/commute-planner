using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace commute_planner.EventCollaboration;

public class EventCollaborationService(
  ICommutePlannerExchange exchange,
  ILogger<EventCollaborationService> log)
  : IHostedService
{
  private readonly ICommutePlannerExchange _exchange = exchange;
  private readonly ILogger<EventCollaborationService> _log = log;

  public Task StartAsync(CancellationToken token = default)
   => _exchange.OpenAsync(token);

  public Task StopAsync(CancellationToken token = default)
    => _exchange.CloseAsync(token);
}