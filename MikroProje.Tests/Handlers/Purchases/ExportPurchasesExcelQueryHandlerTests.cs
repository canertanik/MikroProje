using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.EntityFrameworkCore;
using MikroProje.Application.Common.Excel;
using MikroProje.Application.Features.Purchases.Queries.ExportPurchasesExcel;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Purchases;

public class ExportPurchasesExcelQueryHandlerTests : TestBase
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<IExcelExportService> _excelServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly ExportPurchasesExcelQueryHandler _handler;

    public ExportPurchasesExcelQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _excelServiceMock = new Mock<IExcelExportService>();
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.Setup(c => c["ExcelExport:MaxRowCount"]).Returns("20000");

        _handler = new ExportPurchasesExcelQueryHandler(_dbContextMock.Object, _excelServiceMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUnderLimit()
    {
        var purchases = new List<Purchase> 
        { 
            new Purchase { Id = 1, CurrentAccount = new CurrentAccount { Code = "S1", Name = "Supplier" }, IsDeleted = false }
        };
        _dbContextMock.Setup(c => c.Purchases).ReturnsDbSet(purchases);

        var excelResult = new ExcelExportResult { Content = new byte[] { 1 }, FileName = "Alislar.xlsx", ContentType = "test" };
        
        _excelServiceMock.Setup(e => e.ExportAsync(
            It.IsAny<IReadOnlyCollection<MikroProje.Application.Features.Purchases.DTOs.PurchaseExportDto>>(), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>())).ReturnsAsync(excelResult);

        var query = new ExportPurchasesExcelQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.FileName.Should().Be("Alislar.xlsx");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenOverLimit()
    {
        _configurationMock.Setup(c => c["ExcelExport:MaxRowCount"]).Returns("0"); 

        var purchases = new List<Purchase> 
        { 
            new Purchase { Id = 1, CurrentAccount = new CurrentAccount { Code = "S1", Name = "Supplier" }, IsDeleted = false }
        };
        _dbContextMock.Setup(c => c.Purchases).ReturnsDbSet(purchases);

        var query = new ExportPurchasesExcelQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        
        _excelServiceMock.Verify(e => e.ExportAsync(It.IsAny<IReadOnlyCollection<MikroProje.Application.Features.Purchases.DTOs.PurchaseExportDto>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

