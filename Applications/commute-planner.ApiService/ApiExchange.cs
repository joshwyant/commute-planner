using commute_planner.EventCollaboration;
using RabbitMQ.Client;

namespace commute_planner.ApiService;

public class ApiExchange : CommutePlannerExchange
{
  public ApiExchange(IConnection messaging, ILogger<CommutePlannerExchange> log)
    : base(messaging, log, ApiRoutingKey)
  {
  }

  protected override void OnMessage(string messageType, string routingKey, string message)
  {
    throw new NotImplementedException();
  }
}