using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PatientApp.Api.Controllers;
using PatientApp.Application.DTOs;
using PatientApp.Application.Interfaces;

namespace PatientApp.Api.Tests;

public class HealthLinksControllerTests
{
    private readonly IHealthLinkService _healthLinkService;
    private readonly HealthLinksController _sut;

    public HealthLinksControllerTests()
    {
        _healthLinkService = Substitute.For<IHealthLinkService>();
        _sut = new HealthLinksController(_healthLinkService);
    }

    private static SmartHealthLinkDto CreateTestShlDto() => new()
    {
        Url = "https://example.com/api/v1/healthlinks/test-id",
        Flag = "U",
        Key = "rxTgYlOaKJPFtcEd0qcceN8wEU4p94SqAwIWQe6uX7Q",
        Exp = 1706745600,
        Label = "Jessica Argonaut's health summary"
    };

    private void SetupRequestBody(string body)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(body));
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("example.com");
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    private void SetupEmptyRequestBody()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Body = new MemoryStream(Array.Empty<byte>());
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("example.com");
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    // --- ProcessBundle ---

    [Fact]
    public async Task Given_ValidBundle_When_ProcessBundle_Then_Returns200OkWithShl()
    {
        // Arrange
        var bundleJson = """{"resourceType": "Bundle", "type": "collection"}""";
        SetupRequestBody(bundleJson);

        var shlDto = CreateTestShlDto();
        _healthLinkService.ProcessBundleAsync(bundleJson, "https://example.com").Returns(shlDto);

        // Act
        var result = await _sut.ProcessBundle();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var returnedShl = okResult.Value.Should().BeOfType<SmartHealthLinkDto>().Subject;
        returnedShl.Flag.Should().Be("U");
        returnedShl.Key.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Given_EmptyBody_When_ProcessBundle_Then_Returns400BadRequest()
    {
        // Arrange
        SetupEmptyRequestBody();

        // Act
        var result = await _sut.ProcessBundle();

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // --- Retrieve ---

    [Fact]
    public async Task Given_ValidId_When_Retrieve_Then_Returns200WithJoseContentType()
    {
        // Arrange
        var jweString = "eyJhbGciOiJkaXIiLCJlbmMiOiJBMjU2R0NNIn0..iv.ciphertext.tag";
        _healthLinkService.RetrieveAsync("test-id", "Test Hospital")
            .Returns(new HealthLinkRetrievalResult
            {
                Found = true,
                Expired = false,
                JweCompactSerialization = jweString
            });

        // Act
        var result = await _sut.Retrieve("test-id", "Test Hospital");

        // Assert
        var contentResult = result.Should().BeOfType<ContentResult>().Subject;
        contentResult.ContentType.Should().Be("application/jose");
        contentResult.Content.Should().Be(jweString);
    }

    [Fact]
    public async Task Given_NonexistentId_When_Retrieve_Then_Returns404NotFound()
    {
        // Arrange
        _healthLinkService.RetrieveAsync("nonexistent", "Test Hospital")
            .Returns(new HealthLinkRetrievalResult { Found = false });

        // Act
        var result = await _sut.Retrieve("nonexistent", "Test Hospital");

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Given_ExpiredSubmission_When_Retrieve_Then_Returns410Gone()
    {
        // Arrange
        _healthLinkService.RetrieveAsync("expired-id", "Test Hospital")
            .Returns(new HealthLinkRetrievalResult { Found = true, Expired = true });

        // Act
        var result = await _sut.Retrieve("expired-id", "Test Hospital");

        // Assert
        var statusResult = result.Should().BeOfType<StatusCodeResult>().Subject;
        statusResult.StatusCode.Should().Be(410);
    }

    [Fact]
    public async Task Given_MissingRecipient_When_Retrieve_Then_Returns400BadRequest()
    {
        // Act
        var result = await _sut.Retrieve("test-id", null);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
