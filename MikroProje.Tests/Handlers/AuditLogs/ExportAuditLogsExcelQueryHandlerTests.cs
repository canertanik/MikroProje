using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.EntityFrameworkCore;
using MikroProje.Application.Common.Excel;
using MikroProje.Application.Features.AuditLogs.Queries.ExportAuditLogsExcel;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.AuditLogs;

public class ExportAuditLogsExcelQueryHandlerTests : TestBase
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<IExcelExportService> _excelServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly ExportAuditLogsExcelQueryHandler _handler;

    public ExportAuditLogsExcelQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _excelServiceMock = new Mock<IExcelExportService>();
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.Setup(c => c["ExcelExport:MaxRowCount"]).Returns("20000");

        _handler = new ExportAuditLogsExcelQueryHandler(_dbContextMock.Object, _excelServiceMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUnderLimit()
    {
        var logs = new List<AuditLog> 
        { 
            new AuditLog { Id = 1, EntityName = "User", Action = MikroProje.Domain.Enums.AuditAction.Create }
        };
        _dbContextMock.Setup(c => c.AuditLogs).ReturnsDbSet(logs);

        var excelResult = new ExcelExportResult { Content = new byte[] { 1, 2, 3 }, FileName = "Islem_Gecmisi.xlsx", ContentType = "test" };
        
        _excelServiceMock.Setup(e => e.ExportAsync(
            It.IsAny<IReadOnlyCollection<MikroProje.Application.Features.AuditLogs.DTOs.AuditLogExportDto>>(), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>())).ReturnsAsync(excelResult);

        var query = new ExportAuditLogsExcelQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.FileName.Should().Be("Islem_Gecmisi.xlsx");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenOverLimit()
    {
        _configurationMock.Setup(c => c["ExcelExport:MaxRowCount"]).Returns("0"); // Force fail

        var logs = new List<AuditLog> 
        { 
            new AuditLog { Id = 1, EntityName = "User", Action = MikroProje.Domain.Enums.AuditAction.Create }
        };
        _dbContextMock.Setup(c => c.AuditLogs).ReturnsDbSet(logs);

        var query = new ExportAuditLogsExcelQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("en fazla");
        
        _excelServiceMock.Verify(e => e.ExportAsync(It.IsAny<IReadOnlyCollection<MikroProje.Application.Features.AuditLogs.DTOs.AuditLogExportDto>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

