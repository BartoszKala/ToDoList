using System.Net.Http.Json;
using ToDoList.Domain;
using FluentAssertions;
using System.Net;

namespace ToDoList.Tests.Integration
{
    public class ToDoControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ToDoControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAll_ShouldReturnOkAndList()
        {
            var response = await _client.GetAsync("/api/todo");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<List<ToDoItem>>();
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateToDo_ShouldReturnOkAndCreateItem()
        {
            var newItem = GenerateNewItem();

            var response = await _client.PostAsJsonAsync("/api/todo", newItem);
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task GetSpecific_ShouldReturnCorrectItem()
        {
            var newItem = GenerateNewItem();
            await _client.PostAsJsonAsync("/api/todo", newItem);

            var response = await _client.GetAsync($"/api/todo/{newItem.Id}");
            response.EnsureSuccessStatusCode();

            var item = await response.Content.ReadFromJsonAsync<ToDoItem>();
            item.Should().NotBeNull();
            item!.Id.Should().Be(newItem.Id);
        }

        [Fact]
        public async Task UpdateToDo_ShouldModifyItem()
        {
            var newItem = GenerateNewItem();
            await _client.PostAsJsonAsync("/api/todo", newItem);

            newItem.Title = "Updated Title";
            newItem.PercentCompleted = 50;

            var updateResponse = await _client.PutAsJsonAsync($"/api/todo/{newItem.Id}", newItem);
            updateResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"/api/todo/{newItem.Id}");
            var updatedItem = await getResponse.Content.ReadFromJsonAsync<ToDoItem>();
            updatedItem!.Title.Should().Be("Updated Title");
            updatedItem.PercentCompleted.Should().Be(50);
        }

        [Fact]
        public async Task UpdatePercent_ShouldSetCorrectValue()
        {
            var newItem = GenerateNewItem();
            await _client.PostAsJsonAsync("/api/todo", newItem);

            var percent = 77;
            var response = await _client.PatchAsync($"/api/todo/{newItem.Id}/{percent}", null);
            response.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"/api/todo/{newItem.Id}");
            var updatedItem = await getResponse.Content.ReadFromJsonAsync<ToDoItem>();
            updatedItem!.PercentCompleted.Should().Be(percent);
        }

        [Fact]
        public async Task SetAsDone_ShouldSetTo100Percent()
        {
            var newItem = GenerateNewItem();
            await _client.PostAsJsonAsync("/api/todo", newItem);

            var response = await _client.PatchAsync($"/api/todo/{newItem.Id}/done", null);
            response.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"/api/todo/{newItem.Id}");
            var updatedItem = await getResponse.Content.ReadFromJsonAsync<ToDoItem>();
            updatedItem!.PercentCompleted.Should().Be(100);
        }

        [Fact]
        public async Task DeleteToDo_ShouldRemoveItem()
        {
            var newItem = GenerateNewItem();
            await _client.PostAsJsonAsync("/api/todo", newItem);

            var deleteResponse = await _client.DeleteAsync($"/api/todo/{newItem.Id}");
            deleteResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"/api/todo/{newItem.Id}");
            getResponse.StatusCode.Should().Match(status =>
            status == HttpStatusCode.NotFound || status == HttpStatusCode.BadRequest);

        }

        private ToDoItem GenerateNewItem() => new ToDoItem
        {
            Id = Guid.NewGuid(),
            Title = "Test Item",
            Description = "Generated in test",
            PercentCompleted = 0,
            TimeOfExpiry = DateTime.UtcNow.AddDays(1)
        };
    }
}
