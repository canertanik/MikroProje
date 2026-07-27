using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.EntityFrameworkCore;
using MikroProje.Application.Common.Pdf;
using MikroProje.Application.Features.Purchases.Queries.ExportPurchasesPdf;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Purchases;

public class ExportPurchasesPdfQueryHandlerTests : TestBase
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<IPdfExportService> _pdfServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly ExportPurchasesPdfQueryHandler _handler;

    public ExportPurchasesPdfQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _pdfServiceMock = new Mock<IPdfExportService>();
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.Setup(c => c["PdfExport:MaxRowCount"]).Returns("5000");

        _handler = new ExportPurchasesPdfQueryHandler(_dbContextMock.Object, _pdfServiceMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUnderLimit()
    {
        var purchases = new List<Purchase> 
        { 
            new Purchase { Id = 1, CurrentAccount = new CurrentAccount { Code = "S1", Name = "Supplier" }, IsDeleted = false }
        };
        _dbContextMock.Setup(c => c.Purchases).ReturnsDbSet(purchases);

        var pdfResult = new PdfExportResult { Content = new byte[] { 1 }, FileName = "Alislar.pdf", ContentType = "application/pdf" };
        
        _pdfServiceMock.Setup(e => e.ExportAsync(
            It.IsAny<IReadOnlyCollection<MikroProje.Application.Features.Purchases.DTOs.PurchasePdfExportDto>>(), 
            It.IsAny<string>(), 
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(pdfResult);

        var query = new ExportPurchasesPdfQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.FileName.Should().Be("Alislar.pdf");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenOverLimit()
    {
        _configurationMock.Setup(c => c["PdfExport:MaxRowCount"]).Returns("0"); 

        var purchases = new List<Purchase> 
        { 
            new Purchase { Id = 1, CurrentAccount = new CurrentAccount { Code = "S1", Name = "Supplier" }, IsDeleted = false }
        };
        _dbContextMock.Setup(c => c.Purchases).ReturnsDbSet(purchases);

        var query = new ExportPurchasesPdfQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        
        _pdfServiceMock.Verify(e => e.ExportAsync(It.IsAny<IReadOnlyCollection<MikroProje.Application.Features.Purchases.DTOs.PurchasePdfExportDto>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

