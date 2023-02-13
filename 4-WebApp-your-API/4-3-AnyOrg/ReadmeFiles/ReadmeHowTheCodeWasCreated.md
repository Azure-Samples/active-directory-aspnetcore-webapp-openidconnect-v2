
## How the code was created

<details>
 <summary>Expand the section</summary>

 The sample is based on [ASP.NET CORE API template](https://learn.microsoft.com/aspnet/core/tutorials/first-web-api?view=aspnetcore-7.0&tabs=visual-studio)

 Because there are two parts - Client and Service, you will have to create 2 separate projects under same solution.

 During the project configuration, specify `Microsoft Identity Platform` inside `Authentication Type` dropdown box. As IDE installs the solution, it might require to install an additional components.

 After the initial project was created, we have to continue with further configuration and tweaking. The most of configuration changes are inside Setup.cs files, so please follow with [Client Setup.cs](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/master/4-WebApp-your-API/4-3-AnyOrg/ToDoListClient/Startup.cs) and [Service Setup.cs](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/master/4-WebApp-your-API/4-3-AnyOrg/TodoListService/Startup.cs) for further details.

 You will have to delete the default controllers and all relevant data from the projects and create Home and TodoList controller for bot Client and Service projects. Refer to the controller sections accordingly.
 </details>
