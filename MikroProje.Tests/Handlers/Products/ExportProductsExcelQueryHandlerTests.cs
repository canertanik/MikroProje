using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.EntityFrameworkCore;
using MikroProje.Application.Common.Excel;
using MikroProje.Application.Features.Products.Queries.ExportProductsExcel;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Products;

public class ExportProductsExcelQueryHandlerTests : TestBase
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<IExcelExportService> _excelServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly ExportProductsExcelQueryHandler _handler;

    public ExportProductsExcelQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _excelServiceMock = new Mock<IExcelExportService>();
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.Setup(c => c["ExcelExport:MaxRowCount"]).Returns("20000");

        _handler = new ExportProductsExcelQueryHandler(_dbContextMock.Object, _excelServiceMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUnderLimit()
    {
        var products = new List<Product> 
        { 
            new Product { Id = 1, Code = "P1", Name = "Test Product", IsDeleted = false }
        };
        _dbContextMock.Setup(c => c.Products).ReturnsDbSet(products);

        var excelResult = new ExcelExportResult { Content = new byte[] { 1 }, FileName = "Urunler.xlsx", ContentType = "test" };
        
        _excelServiceMock.Setup(e => e.ExportAsync(
            It.IsAny<IReadOnlyCollection<MikroProje.Application.Features.Products.DTOs.ProductExportDto>>(), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>())).ReturnsAsync(excelResult);

        var query = new ExportProductsExcelQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.FileName.Should().Be("Urunler.xlsx");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenOverLimit()
    {
        _configurationMock.Setup(c => c["ExcelExport:MaxRowCount"]).Returns("0"); 

        var products = new List<Product> 
        { 
            new Product { Id = 1, Code = "P1", Name = "Test Product", IsDeleted = false }
        };
        _dbContextMock.Setup(c => c.Products).ReturnsDbSet(products);

        var query = new ExportProductsExcelQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        
        _excelServiceMock.Verify(e => e.ExportAsync(It.IsAny<IReadOnlyCollection<MikroProje.Application.Features.Products.DTOs.ProductExportDto>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

