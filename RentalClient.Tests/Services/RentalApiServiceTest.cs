using System.Net;
using Xunit;
using Moq;
using Moq.Protected; 
using RentalClient.Services;


namespace RentalClient.Tests.Services;

public class RentalApiServiceTest
{
    [Fact]
    public async Task UploadListingPhotosAsync_ServerReturns500_ReturnsFalse()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Server crashed")
            });

        var httpClient = new HttpClient(handlerMock.Object) 
        { 
            BaseAddress = new Uri("http://test.com") 
        };
        
        var mockUserSession = new Mock<IUserSession>();
        
        var apiService = new RentalApiService(httpClient, mockUserSession.Object);
        var fakePaths = new[] { "fake_image.jpg" };

        bool result = await apiService.UploadListingPhotosAsync(Guid.NewGuid(), fakePaths);

        Assert.False(result); 
    }
}