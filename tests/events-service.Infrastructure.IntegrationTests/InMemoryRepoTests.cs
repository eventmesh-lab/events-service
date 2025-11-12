using System;
using System.Threading.Tasks;
using Xunit;
using events-service.Infrastructure.Repositories;
using events-service.Domain.Entities;

namespace events-service.Infrastructure.IntegrationTests
{
    public class InMemoryRepoTests
    {
        [Fact]
        public async Task AddAndGet_ReturnsEntity()
        {
            var repo = new InMemoryExampleRepository();
            var agg = new ExampleAggregate("name");

            await repo.AddAsync(agg);
            var fetched = await repo.GetByIdAsync(agg.Id);

            Assert.NotNull(fetched);
            Assert.Equal(agg.Id, fetched!.Id);
        }
    }
}
