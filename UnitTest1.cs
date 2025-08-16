using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoiler.Models;
using System.Net;
using System.Text.Json;

namespace StorySpoiler
{
    [TestFixture]
    public class StorySpoilerTests
     {
        private RestClient client;
        private static string? createdStoryId;
        private const string baseURL = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("gabbypetrova", "gaby123");
            var options = new RestClientOptions(baseURL)
            {
                Authenticator = new JwtAuthenticator(token)
            };
            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseURL);
            var request = new RestRequest("/api/Story/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString();
        }

        [Test, Order(1)]
        public void CreateStory_WithRequiredFields()
        {
            var storyRequest = new StoryDTO
            {
                Title = "Test Story Spoiler",
                Description = "This is a test story spoiler description.",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(storyRequest);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var createdResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(createdResponse, Is.Not.Null);

            Assert.That(createdResponse!.Msg, Is.EqualTo("Successfully created!"));

            var jsonElement = JsonSerializer.Deserialize<JsonElement>(response.Content);
            createdStoryId = jsonElement.GetProperty("storyId").GetString();
    

        }

        [Test, Order(2)]
        public void EditStoryTitle_ShouldReturnOk()
        {
            var editRequest = new StoryDTO
            {
                Title = "Edited Story",
                Description = "This is an updated test story description.",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit", Method.Put);
            request.AddQueryParameter("storyId", createdStoryId);
            request.AddJsonBody(editRequest);
            var response = client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(editResponse!.Msg, Is.EqualTo("Successfully edited"));
        }

        [Test, Order(3)]
        public void GetAllStorySpoilers_ShouldReturnList()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var spoilers = JsonSerializer.Deserialize<List<object>>(response.Content);
            Assert.That(spoilers, Is.Not.Null.And.Not.Empty);
        }

        [Test, Order(4)]
        public void DeleteStorySpoiler_ShouldReturnOk()
        {
            var request = new RestRequest("/api/Story/Delete/{storyId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateStorySpoiler_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var spoiler = new
            {
                Title = "",
                Description = "",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(spoiler);

            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditNonExistingStorySpoiler_ShouldReturnNotFound()
        {
            string nonExistingStoryId = "2323";
            var editRequest = new StoryDTO
            {
                Title = "Edited Non-Existing story",
                Description = "This is an updated test story description for a non-existing spoiler.",
                Url = ""
            };
            var request = new RestRequest($"/api/Story/Edit", Method.Put);
            request.AddQueryParameter("ideaId", nonExistingStoryId);
            request.AddJsonBody(editRequest);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No spoilers..."));
        }

        [Test, Order(7)]
        public void DeleteNonExistingStorySpoiler_ShouldReturnNotFound()
        {
            string nonExistingStoryId = "2323";
            var request = new RestRequest($"/api/Story/Delete", Method.Delete);
            request.AddQueryParameter("ideaId", nonExistingStoryId);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));
        }

         [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}
