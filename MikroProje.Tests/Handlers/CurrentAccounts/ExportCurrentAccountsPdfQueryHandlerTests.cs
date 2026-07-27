using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.EntityFrameworkCore;
using MikroProje.Application.Common.Pdf;
using MikroProje.Application.Features.CurrentAccounts.Queries.ExportCurrentAccountsPdf;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.CurrentAccounts;

public class ExportCurrentAccountsPdfQueryHandlerTests : TestBase
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<IPdfExportService> _pdfServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly ExportCurrentAccountsPdfQueryHandler _handler;

    public ExportCurrentAccountsPdfQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _pdfServiceMock = new Mock<IPdfExportService>();
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.Setup(c => c["PdfExport:MaxRowCount"]).Returns("5000");

        _handler = new ExportCurrentAccountsPdfQueryHandler(_dbContextMock.Object, _pdfServiceMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUnderLimit()
    {
        var accounts = new List<CurrentAccount> 
        { 
            new CurrentAccount { Id = 1, Name = "Test", Type = MikroProje.Domain.Enums.CurrentAccountType.Customer, IsDeleted = false }
        };
        _dbContextMock.Setup(c => c.CurrentAccounts).ReturnsDbSet(accounts);

        var pdfResult = new PdfExportResult { Content = new byte[] { 1 }, FileName = "Cariler.pdf", ContentType = "application/pdf" };
        
        _pdfServiceMock.Setup(e => e.ExportAsync(
            It.IsAny<IReadOnlyCollection<MikroProje.Application.Features.CurrentAccounts.DTOs.CurrentAccountPdfExportDto>>(), 
            It.IsAny<string>(), 
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(pdfResult);

        var query = new ExportCurrentAccountsPdfQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.FileName.Should().Be("Cariler.pdf");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenOverLimit()
    {
        _configurationMock.Setup(c => c["PdfExport:MaxRowCount"]).Returns("0"); 

        var accounts = new List<CurrentAccount> 
        { 
            new CurrentAccount { Id = 1, Name = "Test", IsDeleted = false }
        };
        _dbContextMock.Setup(c => c.CurrentAccounts).ReturnsDbSet(accounts);

        var query = new ExportCurrentAccountsPdfQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        
        _pdfServiceMock.Verify(e => e.ExportAsync(It.IsAny<IReadOnlyCollection<MikroProje.Application.Features.CurrentAccounts.DTOs.CurrentAccountPdfExportDto>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

