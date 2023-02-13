using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ToDoListClient.Models;
using ToDoListClient.Utils;

namespace ToDoListClient.Services
{
    public static class TodoListServiceExtensions
    {
        public static void AddTodoListService(this IServiceCollection services)
        {
            // https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            services.AddHttpClient<IToDoListService, ToDoListService>();
        }
    }
    public class ToDoListService : IToDoListService
    {
        private readonly HttpClient _httpClient;
        private readonly string _TodoListServiceScope = string.Empty;
        private readonly string _TodoListBaseAddress = string.Empty;
        private readonly string _RedirectUri = string.Empty;
        private readonly string _ApiRedirectUri = string.Empty;
        private readonly ITokenAcquisition _tokenAcquisition;

        public ToDoListService(ITokenAcquisition tokenAcquisition, HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _tokenAcquisition = tokenAcquisition;
            _TodoListServiceScope = configuration["TodoList:TodoListServiceScope"];
            _TodoListBaseAddress = configuration["TodoList:TodoListBaseAddress"];
            _RedirectUri = configuration["RedirectUri"];
            _ApiRedirectUri = configuration["TodoList:AdminConsentRedirectApi"];

            if (!string.IsNullOrEmpty(_TodoListBaseAddress))
            {
                if (!_TodoListBaseAddress.EndsWith("/"))
                {
                    _TodoListBaseAddress = _TodoListBaseAddress+"/";
                }
            }
        }
        public async Task<ToDoItem> AddAsync(ToDoItem todo)
        {
            await PrepareAuthenticatedClient();

            var jsonRequest = JsonConvert.SerializeObject(todo);
            var jsoncontent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await this._httpClient.PostAsync($"{ _TodoListBaseAddress}api/todolist", jsoncontent);

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

            var response = await _httpClient.DeleteAsync($"{ _TodoListBaseAddress}api/todolist/{id}");

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
            var response = await _httpClient.PatchAsync($"{ _TodoListBaseAddress}api/todolist/{todo.Id}", jsoncontent);

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
            var response = await _httpClient.GetAsync($"{ _TodoListBaseAddress}api/todolist");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                IEnumerable<ToDoItem> todolist = JsonConvert.DeserializeObject<IEnumerable<ToDoItem>>(content);

                return todolist;
            }

            throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
        }
        public async Task<IEnumerable<string>> GetAllGraphUsersAsync()
        {
            await PrepareAuthenticatedClient();
            var response = await _httpClient.GetAsync($"{ _TodoListBaseAddress}api/todolist/getallgraphusers");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                IEnumerable<string> Users = JsonConvert.DeserializeObject<IEnumerable<string>>(content);
                return Users;
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                 HandleChallengeFromWebApi(response);
            }

            throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
        }

        public async Task<ToDoItem> GetAsync(int id)
        {
            await PrepareAuthenticatedClient();
            var response = await _httpClient.GetAsync($"{ _TodoListBaseAddress}api/todolist/{id}");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                ToDoItem todo = JsonConvert.DeserializeObject<ToDoItem>(content);

                return todo;
            }

            throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
        }

        private async Task PrepareAuthenticatedClient()
        {
            var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { _TodoListServiceScope });
            Debug.WriteLine($"access token-{accessToken}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// If signed-in user does not have consent for a permission on the Web API, for instance "user.read.all" in this sample, 
        /// then Web API will throw MsalUiRequiredException. The response contains the details about consent Uri and proposed action. 
        /// </summary>
        /// <param name="response"></param>
        /// <exception cref="WebApiMsalUiRequiredException"></exception>
        private void HandleChallengeFromWebApi(HttpResponseMessage response)
        {
            //proposedAction="consent"
            List<string> result = new List<string>();
            AuthenticationHeaderValue bearer = response.Headers.WwwAuthenticate.First(v => v.Scheme == "Bearer");
            IEnumerable<string> parameters = bearer.Parameter.Split(',').Select(v => v.Trim()).ToList();
            string proposedAction = GetParameter(parameters, "proposedAction");

            if (proposedAction == "consent")
            {
                string consentUri = GetParameter(parameters, "consentUri");

                var uri = new Uri(consentUri);

                //Set values of query string parameters
                var queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
                queryString.Set("redirect_uri", _ApiRedirectUri);
                queryString.Add("prompt", "consent");
                queryString.Add("state", _RedirectUri);
                //Update values in consent Uri
                var uriBuilder = new UriBuilder(uri);
                uriBuilder.Query = queryString.ToString();
                var updateConsentUri = uriBuilder.Uri.ToString();
                result.Add("consentUri");
                result.Add(updateConsentUri);

                //throw custom exception
                throw new WebApiMsalUiRequiredException(updateConsentUri);
            }
        }

        private static string GetParameter(IEnumerable<string> parameters, string parameterName)
        {
            int offset = parameterName.Length + 1;
            return parameters.FirstOrDefault(p => p.StartsWith($"{parameterName}="))?.Substring(offset)?.Trim('"');
        }
    }
}