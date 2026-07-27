using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.EntityFrameworkCore;
using MikroProje.Application.Common.Pdf;
using MikroProje.Application.Features.Products.Queries.ExportProductsPdf;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Products;

public class ExportProductsPdfQueryHandlerTests : TestBase
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<IPdfExportService> _pdfServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly ExportProductsPdfQueryHandler _handler;

    public ExportProductsPdfQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _pdfServiceMock = new Mock<IPdfExportService>();
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.Setup(c => c["PdfExport:MaxRowCount"]).Returns("5000");

        _handler = new ExportProductsPdfQueryHandler(_dbContextMock.Object, _pdfServiceMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUnderLimit()
    {
        var products = new List<Product> 
        { 
            new Product { Id = 1, Code = "P1", Name = "Test Product", IsDeleted = false }
        };
        _dbContextMock.Setup(c => c.Products).ReturnsDbSet(products);

        var pdfResult = new PdfExportResult { Content = new byte[] { 1 }, FileName = "Urunler.pdf", ContentType = "application/pdf" };
        
        _pdfServiceMock.Setup(e => e.ExportAsync(
            It.IsAny<IReadOnlyCollection<MikroProje.Application.Features.Products.DTOs.ProductPdfExportDto>>(), 
            It.IsAny<string>(), 
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(pdfResult);

        var query = new ExportProductsPdfQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.FileName.Should().Be("Urunler.pdf");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenOverLimit()
    {
        _configurationMock.Setup(c => c["PdfExport:MaxRowCount"]).Returns("0"); 

        var products = new List<Product> 
        { 
            new Product { Id = 1, Code = "P1", Name = "Test Product", IsDeleted = false }
        };
        _dbContextMock.Setup(c => c.Products).ReturnsDbSet(products);

        var query = new ExportProductsPdfQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        
        _pdfServiceMock.Verify(e => e.ExportAsync(It.IsAny<IReadOnlyCollection<MikroProje.Application.Features.Products.DTOs.ProductPdfExportDto>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

