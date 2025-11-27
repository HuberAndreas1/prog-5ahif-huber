using System.Net;
using System.Net.Http.Json;

namespace WebApiTests;

public class TimeTrackingIntegrationTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture> {
    [Fact]
    public async Task GetEmployees_ReturnsOk() {

        var response = await fixture.HttpClient.GetAsync("/employees");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTimeEntry_WithNonExistentProjectId_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            Description = "Work description",
            EmployeeId = 1,
            ProjectId = 999999 // Non-existent project
        };

        // Act
        var response = await fixture.HttpClient.PutAsJsonAsync("/timeentries/1", updateDto);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected BadRequest or NotFound, got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task DeleteTimeEntry_WithNonExistentId_ReturnsNotFound() {
        var response = await fixture.HttpClient.DeleteAsync("/timeentries/9999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}