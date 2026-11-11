using Xunit;
using Moq;
using EncurtadorURLs.Services;
using EncurtadorURLs.Data;
using EncurtadorURLs.Models;

namespace EncurtadorURLs.Tests.Services;

/// <summary>
/// Testes unitários para UrlsService.CreateShortUrlAsync.
/// FASE RED do TDD: estes testes foram escritos ANTES da implementação existir.
/// Todos devem FALHAR neste momento — isso é o esperado e desejado.
/// </summary>
public class UrlsServiceTests
{
    private readonly Mock<IUrlContext> _mockContext;
    private readonly UrlsService _service;

    public UrlsServiceTests()
    {
        _mockContext = new Mock<IUrlContext>();
        _service = new UrlsService(_mockContext.Object);
    }

    [Fact]
    public async Task CreateShortUrlAsync_ComUrlValida_DeveRetornarUrlItemComShortCode()
    {
        // Arrange
        var urlOriginal = "https://www.google.com";
        _mockContext.Setup(c => c.ShortCodeExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var resultado = await _service.CreateShortUrlAsync(urlOriginal);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(urlOriginal, resultado.OriginalUrl);
        Assert.False(string.IsNullOrEmpty(resultado.ShortCode));
        _mockContext.Verify(c => c.AddAsync(It.IsAny<UrlItem>()), Times.Once);
    }

    [Fact]
    public async Task CreateShortUrlAsync_ComUrlVazia_DeveLancarArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateShortUrlAsync(string.Empty)
        );
    }

    [Fact]
    public async Task CreateShortUrlAsync_ComEsquemaInvalido_DeveLancarArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateShortUrlAsync("ftp://arquivo-invalido.com")
        );
    }

    [Fact]
    public async Task CreateShortUrlAsync_ComUrlNula_DeveLancarArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateShortUrlAsync(null)
        );
    }

    [Fact]
    public async Task CreateShortUrlAsync_ComUrlLonga_DeveLancarArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateShortUrlAsync("https://example.com/" + new string('a', 2100))
        );
    }

    [Fact]
    public async Task GetUrlByIdAsync_ComGuidVazio_DeveLancarArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetUrlByIdAsync(Guid.Empty)
        );
    }

    [Fact]
    public async Task GetUrlByIdAsync_ComGuidInexistente_DeveRetornarNulo()
    {
        // Arrange
        var guidInexistente = Guid.NewGuid();
        _mockContext.Setup(c => c.GetByIdAsync(guidInexistente))
            .ReturnsAsync((UrlItem)null);

        //Act
        var resultado = await _service.GetUrlByIdAsync(guidInexistente);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task GetUrlByIdAsync_ComGuidExistente_DeveRetornarUrlItem()
    {
        // Arrange
        var guidExistente = Guid.NewGuid();
        var urlItemEsperado = new UrlItem { 
            Id = guidExistente,
            OriginalUrl = "https://example.com",
            ShortCode = "abc123"
        };
        _mockContext.Setup(c => c.GetByIdAsync(guidExistente))
            .ReturnsAsync(urlItemEsperado);

        // Act
        var resultado = await _service.GetUrlByIdAsync(guidExistente);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(urlItemEsperado.Id, resultado!.Id);
        Assert.Equal(urlItemEsperado.OriginalUrl, resultado!.OriginalUrl);
        Assert.Equal(urlItemEsperado.ShortCode, resultado!.ShortCode);
    }
}