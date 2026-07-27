using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.EntityFrameworkCore;
using MikroProje.Application.Common.Excel;
using MikroProje.Application.Features.Sales.Queries.ExportSalesExcel;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Sales;

public class ExportSalesExcelQueryHandlerTests : TestBase
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<IExcelExportService> _excelServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly ExportSalesExcelQueryHandler _handler;

    public ExportSalesExcelQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _excelServiceMock = new Mock<IExcelExportService>();
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.Setup(c => c["ExcelExport:MaxRowCount"]).Returns("20000");

        _handler = new ExportSalesExcelQueryHandler(_dbContextMock.Object, _excelServiceMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUnderLimit()
    {
        var sales = new List<Sale> 
        { 
            new Sale { Id = 1, CurrentAccount = new CurrentAccount { Code = "C1", Name = "Customer" }, IsDeleted = false }
        };
        _dbContextMock.Setup(c => c.Sales).ReturnsDbSet(sales);

        var excelResult = new ExcelExportResult { Content = new byte[] { 1 }, FileName = "Satislar.xlsx", ContentType = "test" };
        
        _excelServiceMock.Setup(e => e.ExportAsync(
            It.IsAny<IReadOnlyCollection<MikroProje.Application.Features.Sales.DTOs.SaleExportDto>>(), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>())).ReturnsAsync(excelResult);

        var query = new ExportSalesExcelQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.FileName.Should().Be("Satislar.xlsx");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenOverLimit()
    {
        _configurationMock.Setup(c => c["ExcelExport:MaxRowCount"]).Returns("0"); 

        var sales = new List<Sale> 
        { 
            new Sale { Id = 1, CurrentAccount = new CurrentAccount { Code = "C1", Name = "Customer" }, IsDeleted = false }
        };
        _dbContextMock.Setup(c => c.Sales).ReturnsDbSet(sales);

        var query = new ExportSalesExcelQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        
        _excelServiceMock.Verify(e => e.ExportAsync(It.IsAny<IReadOnlyCollection<MikroProje.Application.Features.Sales.DTOs.SaleExportDto>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

