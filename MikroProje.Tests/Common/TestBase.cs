using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace MikroProje.Tests.Common;

public abstract class TestBase
{
    protected readonly IMapper Mapper;

    protected TestBase()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(_ => { }, typeof(MikroProje.Application.DependencyInjection).Assembly);
        var provider = services.BuildServiceProvider();
        Mapper = provider.GetRequiredService<IMapper>();
    }
}


