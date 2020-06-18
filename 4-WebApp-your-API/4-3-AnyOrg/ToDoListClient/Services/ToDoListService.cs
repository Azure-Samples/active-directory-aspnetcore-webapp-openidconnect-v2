using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ToDoListClient.Models;

namespace ToDoListClient.Services
{
    public static class TodoListServiceExtensions
    {
        public static void AddTodoListService(this IServiceCollection services, IConfiguration configuration)
        {
            // https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            services.AddHttpClient<IToDoListService, ToDoListService>();
        }
    }
    public class ToDoListService : IToDoListService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly HttpClient _httpClient;
        private readonly string _TodoListScope = string.Empty;
        private readonly string _TodoListBaseAddress = string.Empty;
        private readonly string _ClientId = string.Empty;
        private readonly ITokenAcquisition _tokenAcquisition;
        //private readonly IConfiguration _configuration;

        public ToDoListService(ITokenAcquisition tokenAcquisition, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            _httpClient = httpClient;
            _tokenAcquisition = tokenAcquisition;
            _contextAccessor = contextAccessor;
            _TodoListScope = configuration["TodoList:TodoListScope"];
            _TodoListBaseAddress = configuration["TodoList:TodoListBaseAddress"];
            _ClientId = configuration["AzureAd:ClientId"];
        }
        public async Task<ToDoItem> AddAsync(ToDoItem todo)
        {
            await PrepareAuthenticatedClient();

            var jsonRequest = JsonConvert.SerializeObject(todo);
            var jsoncontent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await this._httpClient.PostAsync($"{ _TodoListBaseAddress}/api/todolist", jsoncontent);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                todo = JsonConvert.DeserializeObject<ToDoItem>(content);

                return todo;
            }

            throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
        }

        public async Task DeleteAsync(int id)
        {
            await PrepareAuthenticatedClient();

            var response = await _httpClient.DeleteAsync($"{ _TodoListBaseAddress}/api/todolist/{id}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return;
            }

            throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
        }

        public async Task<ToDoItem> EditAsync(ToDoItem todo)
        {
            await PrepareAuthenticatedClient();

            var jsonRequest = JsonConvert.SerializeObject(todo);
            var jsoncontent = new StringContent(jsonRequest, Encoding.UTF8, "application/json-patch+json");
            var response = await _httpClient.PutAsync($"{ _TodoListBaseAddress}/api/todolist/{todo.Id}", jsoncontent);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                todo = JsonConvert.DeserializeObject<ToDoItem>(content);

                return todo;
            }

            throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
        }

        public async Task<IEnumerable<ToDoItem>> GetAsync()
        {
            await PrepareAuthenticatedClient();
            var response = await _httpClient.GetAsync($"{ _TodoListBaseAddress}/api/todolist");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                IEnumerable<ToDoItem> todolist = JsonConvert.DeserializeObject<IEnumerable<ToDoItem>>(content);

                return todolist;
            }

            throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
        }
        public async Task<IEnumerable<string>> GetAllUsersAsync()
        {
            await PrepareAuthenticatedClient();
            var response = await _httpClient.GetAsync($"{ _TodoListBaseAddress}/api/todolist/getallusers");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                IEnumerable<string> Users = JsonConvert.DeserializeObject<IEnumerable<string>>(content);

                return Users;
            }
            throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
        }
        private async Task PrepareAuthenticatedClient()
        {
            var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { _TodoListScope });
            Debug.WriteLine($"access token-{accessToken}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<ToDoItem> GetAsync(int id)
        {
            await PrepareAuthenticatedClient();
            var response = await _httpClient.GetAsync($"{ _TodoListBaseAddress}/api/todolist/{id}");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                ToDoItem todo = JsonConvert.DeserializeObject<ToDoItem>(content);

                return todo;
            }

            throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
        }
        public string GetAdminConsentEndpoint(string tenantId, string redirectUrl)
        {
            string adminConsent = "https://login.microsoftonline.com/" +
                       tenantId + "/v2.0/adminconsent?client_id="+ _ClientId
                       + "&redirect_uri="+ redirectUrl + "&scope="+_TodoListScope;
            return adminConsent;
        }
    }
}