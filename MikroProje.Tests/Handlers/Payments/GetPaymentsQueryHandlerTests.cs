using FluentAssertions;
using Moq;
using MikroProje.Application.Features.Payments.Queries.GetPayments;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Payments;

public class GetPaymentsQueryHandlerTests : TestBase
{
    private readonly Mock<IPaymentRepository> _repositoryMock;
    private readonly GetPaymentsQueryHandler _handler;

    public GetPaymentsQueryHandlerTests()
    {
        _repositoryMock = new Mock<IPaymentRepository>();
        _handler = new GetPaymentsQueryHandler(_repositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithPagedData()
    {
        var payments = new List<Payment>
        {
            new Payment { Id = 1, Amount = 100 },
            new Payment { Id = 2, Amount = 200 }
        };
        var totalCount = 2;

        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int?>(), 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((payments, totalCount));

        var query = new GetPaymentsQuery { PageNumber = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
    }
}
