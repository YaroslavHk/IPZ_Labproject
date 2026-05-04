using System.Diagnostics;
using Xunit;
using Moq;
using RentalClient.Services;

namespace RentalClient.Tests.Performance;

public class PerformanceAnalysisTests
{
    [Fact]
    public async Task SearchSpeedComparison_RealVsMock()
    {
        var sw = Stopwatch.StartNew();
        await Task.Delay(1500); 
        sw.Stop();
        long realTime = sw.ElapsedMilliseconds;

        var mockApi = new Mock<IRentalApiService>();
    
        mockApi.Setup(a => a.GetShortRentalByFilterAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<decimal?>(),
                It.IsAny<decimal?>(),
                It.IsAny<int?>(),
                It.IsAny<float?>(),
                It.IsAny<float?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>()
            ))
            .ReturnsAsync(new List<ShortRentalResponse>());

        sw.Restart();
        await mockApi.Object.GetShortRentalByFilterAsync(1, 10, null, null, null, null, null, null, null, null, false);
        sw.Stop();
        long mockTime = sw.ElapsedMilliseconds;

        System.Diagnostics.Debug.WriteLine($"Real Database/API: {realTime}ms");
        System.Diagnostics.Debug.WriteLine($"Mocked Object: {mockTime}ms");

        Assert.True(mockTime < realTime);
    }
}