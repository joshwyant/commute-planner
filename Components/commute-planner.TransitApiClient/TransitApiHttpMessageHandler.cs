using System.Web;

namespace commute_planner.TransitApi;

/// <summary>
/// Supports HttpClient for the Transit API by applying the given API key
/// </summary>
/// <param name="apiKey">The Transit API key.</param>
public class TransitApiHttpMessageHandler(string apiKey)
  : DelegatingHandler
{
  private string ApiKey { get; } = apiKey;

  protected override async Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request, CancellationToken cancellationToken)
  {
    var builder = new UriBuilder(request.RequestUri!);

    // Modify query string
    var query = HttpUtility.ParseQueryString(builder.Query);
    query["api_key"] = ApiKey;
    builder.Query = query.ToString();

    request.RequestUri = builder.Uri;

    return await base.SendAsync(request, cancellationToken);
  }
}
