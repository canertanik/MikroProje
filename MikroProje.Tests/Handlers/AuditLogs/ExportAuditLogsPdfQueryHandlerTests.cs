using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.EntityFrameworkCore;
using MikroProje.Application.Common.Pdf;
using MikroProje.Application.Features.AuditLogs.Queries.ExportAuditLogsPdf;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.AuditLogs;

public class ExportAuditLogsPdfQueryHandlerTests : TestBase
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<IPdfExportService> _pdfServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly ExportAuditLogsPdfQueryHandler _handler;

    public ExportAuditLogsPdfQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _pdfServiceMock = new Mock<IPdfExportService>();
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.Setup(c => c["PdfExport:MaxRowCount"]).Returns("5000");

        _handler = new ExportAuditLogsPdfQueryHandler(_dbContextMock.Object, _pdfServiceMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUnderLimit()
    {
        var logs = new List<AuditLog> 
        { 
            new AuditLog { Id = 1, EntityName = "User", Action = MikroProje.Domain.Enums.AuditAction.Create }
        };
        _dbContextMock.Setup(c => c.AuditLogs).ReturnsDbSet(logs);

        var pdfResult = new PdfExportResult { Content = new byte[] { 1, 2, 3 }, FileName = "Islem_Gecmisi.pdf", ContentType = "application/pdf" };
        
        _pdfServiceMock.Setup(e => e.ExportAsync(
            It.IsAny<IReadOnlyCollection<MikroProje.Application.Features.AuditLogs.DTOs.AuditLogPdfExportDto>>(), 
            It.IsAny<string>(), 
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(pdfResult);

        var query = new ExportAuditLogsPdfQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.FileName.Should().Be("Islem_Gecmisi.pdf");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenOverLimit()
    {
        _configurationMock.Setup(c => c["PdfExport:MaxRowCount"]).Returns("0"); // Force fail

        var logs = new List<AuditLog> 
        { 
            new AuditLog { Id = 1, EntityName = "User", Action = MikroProje.Domain.Enums.AuditAction.Create }
        };
        _dbContextMock.Setup(c => c.AuditLogs).ReturnsDbSet(logs);

        var query = new ExportAuditLogsPdfQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("en fazla");
        
        _pdfServiceMock.Verify(e => e.ExportAsync(It.IsAny<IReadOnlyCollection<MikroProje.Application.Features.AuditLogs.DTOs.AuditLogPdfExportDto>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

