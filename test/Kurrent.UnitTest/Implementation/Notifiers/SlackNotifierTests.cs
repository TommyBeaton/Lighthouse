using System.Net;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Execution;
using Kurrent.Implementation.Notifications.Notifiers;
using Kurrent.Models.Data;
using Kurrent.Models.Data.Notifiers;
using Kurrent.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;

namespace Kurrent.UnitTest.Implementation.Notifiers;

public class SlackNotifierTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<SlackNotifier>> _loggerMock;
    private readonly HttpClient httpClient;
    private readonly MockHttpMessageHandler mockHttp;
    private readonly SlackNotifier _sut;

    public SlackNotifierTests()
    {
        _loggerMock = new Mock<ILogger<SlackNotifier>>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        mockHttp = new MockHttpMessageHandler();
        httpClient = mockHttp.ToHttpClient();
        _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
        
        _sut = new SlackNotifier(_httpClientFactoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task NotifyAsync_Successful_Response_Returns_Valid_NotifierResult()
    {
        // Arrange
        var container = new Image("test-host", "test-repo", "test-tag");
        var repositoryConfig = new RepositoryConfig { Name = "test-repo", Url = "https://github.com/test-repo" };
        var notifierConfig = new NotifierConfig { Name = "slack", Token = "test-token", Channel = "test-channel" };

        var slackResponse = new SlackMessageResponse { Ok = true };
        var slackResponseString = JsonSerializer.Serialize(slackResponse);

        mockHttp.When("https://slack.com/api/chat.postMessage")
            .Respond("application/json", slackResponseString); // Respond with JSON

        // Act
        var result = await _sut.NotifyAsync(container, repositoryConfig, notifierConfig, "test-commitSha");

        // Assert
        using var _ = new AssertionScope();
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task NotifyAsync_Error_Response_Returns_Valid_NotifierResult()
    {
        // Arrange
        var container = new Image("test-host", "test-repo", "test-tag");
        var repositoryConfig = new RepositoryConfig { Name = "test-repo", Url = "https://github.com/test-repo" };
        var notifierConfig = new NotifierConfig { Name = "slack", Token = "test-token", Channel = "test-channel" };

        var slackResponse = new SlackMessageResponse { Ok = false, Error = "slack error" };
        var slackResponseString = JsonSerializer.Serialize(slackResponse);

        mockHttp.When("https://slack.com/api/chat.postMessage")
            .Respond("application/json", slackResponseString); // Respond with JSON

        // Act
        var result = await _sut.NotifyAsync(container, repositoryConfig, notifierConfig, "test-commitSha");

        // Assert
        using var _ = new AssertionScope();
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Error.Should().Be("slack error");
    }

    [Fact]
    public async Task NotifyAsync_Throws_Exception_Returns_Valid_NotifierResult()
    {
        // Arrange
        var container = new Image("test-host", "test-repo", "test-tag");
        var repositoryConfig = new RepositoryConfig { Name = "test-repo", Url = "https://github.com/test-repo" };
        var notifierConfig = new NotifierConfig { Name = "slack", Token = "test-token", Channel = "test-channel" };

        mockHttp.When("https://slack.com/api/chat.postMessage")
            .Throw(new HttpRequestException("mocked exception")); // Throws exception when called

        // Act
        var result = await _sut.NotifyAsync(container, repositoryConfig, notifierConfig, "test-commitSha");

        // Assert
        using var _ = new AssertionScope();
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Error.Should().Be("mocked exception");
    }
}