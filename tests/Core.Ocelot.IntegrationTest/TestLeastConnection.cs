using Core.Ocelot.IntegrationTest.Setup;
using Newtonsoft.Json;
using System;
using Xunit;

namespace Core.Ocelot.IntegrationTest
{
    public class TestLeastConnection : TestFixture
    {
        [Fact]
        public async void Test1()
        {
            // Arrange

            // Act
            var response = await MainApiClient.GetAsync($@"api/Values");


            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            // var result = JsonConvert.DeserializeObject<string>(responseString);


        }
    }
}
