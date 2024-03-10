using System.Net;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

namespace commute_planner.TransitApi;

/// <summary>
/// Supports HttpClient for the Transit API by applying the given API key, as
/// well as requesting the JSON data format.
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
    query["format"] = "json";
    builder.Query = query.ToString();

    request.RequestUri = builder.Uri;

    return await base.SendAsync(request, cancellationToken);
  }
}
