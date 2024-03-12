using System.Net;
using commute_planner.MapsApi;
using Moq;
using Moq.Protected;

namespace commute_planner.MapsApiClient.Tests;

public class MapsApiClientTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task TestComputeRoutesResult()
    {
        // Arrange
        var json =
            await File.ReadAllTextAsync("Resources/computeRoutesResponse.json");
        // Set up a mock HTTP handler.
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(msg =>
                    msg.Method == HttpMethod.Post &&
                    msg.RequestUri != null &&
                    msg.RequestUri.AbsolutePath
                        == "/directions/v2:computeRoutes"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });
        var httpClient = new HttpClient(handler.Object);
        httpClient.BaseAddress = new Uri("https://example.maps.api.com/");
        var client = new MapsApi.MapsApiClient(httpClient);

        // Act
        var response = await client.ComputeRoutes(new ComputeRoutesRequest(
            new Waypoint() { Address = "From" },
            new Waypoint() { Address = "To" }));

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response?.Routes, Is.Not.Null);
        Assert.That(response?.Routes, Has.Length.Not.Zero);
        Assert.That(response?.Routes?[0].Duration, Is.Not.Null);
        Assert.That(response?.Routes?[0].Duration, Does.Match("\\d+s"));
        Assert.That(response?.Routes?[0].DistanceMeters, Is.Not.Zero);
    }
}