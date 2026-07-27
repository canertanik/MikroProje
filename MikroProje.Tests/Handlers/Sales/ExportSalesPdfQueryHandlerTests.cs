using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.EntityFrameworkCore;
using MikroProje.Application.Common.Pdf;
using MikroProje.Application.Features.Sales.Queries.ExportSalesPdf;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Sales;

public class ExportSalesPdfQueryHandlerTests : TestBase
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<IPdfExportService> _pdfServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly ExportSalesPdfQueryHandler _handler;

    public ExportSalesPdfQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _pdfServiceMock = new Mock<IPdfExportService>();
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.Setup(c => c["PdfExport:MaxRowCount"]).Returns("5000");

        _handler = new ExportSalesPdfQueryHandler(_dbContextMock.Object, _pdfServiceMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUnderLimit()
    {
        var sales = new List<Sale> 
        { 
            new Sale { Id = 1, CurrentAccount = new CurrentAccount { Code = "C1", Name = "Customer" }, IsDeleted = false }
        };
        _dbContextMock.Setup(c => c.Sales).ReturnsDbSet(sales);

        var pdfResult = new PdfExportResult { Content = new byte[] { 1 }, FileName = "Satislar.pdf", ContentType = "application/pdf" };
        
        _pdfServiceMock.Setup(e => e.ExportAsync(
            It.IsAny<IReadOnlyCollection<MikroProje.Application.Features.Sales.DTOs.SalePdfExportDto>>(), 
            It.IsAny<string>(), 
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(pdfResult);

        var query = new ExportSalesPdfQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.FileName.Should().Be("Satislar.pdf");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenOverLimit()
    {
        _configurationMock.Setup(c => c["PdfExport:MaxRowCount"]).Returns("0"); 

        var sales = new List<Sale> 
        { 
            new Sale { Id = 1, CurrentAccount = new CurrentAccount { Code = "C1", Name = "Customer" }, IsDeleted = false }
        };
        _dbContextMock.Setup(c => c.Sales).ReturnsDbSet(sales);

        var query = new ExportSalesPdfQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        
        _pdfServiceMock.Verify(e => e.ExportAsync(It.IsAny<IReadOnlyCollection<MikroProje.Application.Features.Sales.DTOs.SalePdfExportDto>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

