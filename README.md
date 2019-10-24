# RESTful API with authentication using Web API and JWT

[![Build status](https://dev.azure.com/TheNikhilK/TheNikhilK/_apis/build/status/JWT%20WebAPI%20Template)](https://dev.azure.com/TheNikhilK/TheNikhilK/_build/latest?definitionId=1) [![Release status](https://vsrm.dev.azure.com/TheNikhilK/_apis/public/Release/badge/bb0a4e04-afca-4551-8acd-9a8a4ee21621/1/1)](https://vsrm.dev.azure.com/TheNikhilK/_apis/public/Release/badge/bb0a4e04-afca-4551-8acd-9a8a4ee21621/1/1)

## Introduction

We will be creating a RESTful (REST like) HTTP service using Web API feature of the ASP.NET framework.

The purpose of this code is to develop the Restaurent API, using Microsoft Web API with (C#),which authenticates and authorizes some requests, exposes OAuth2 endpoints, and returns data about meals and reviews for consumption by the caller. The caller in this case will be Postman, a useful utility for querying API’s.

**Note:** Anytime you are stuck you can directly download the code from [here](https://github.com/thenikhilk/jwt-auth-webapi "GitHub repo link") on GitHub or ask in the comments below.

## Pre-requisite

1. Basic knowledge [Web API with C#](https://www.asp.net/web-ap "Web API with C#")
2. [Visual Studio](https://visualstudio.microsoft.com/downloads/ "Visual Studio")
3. [Postman](https://www.getpostman.com/ "Postman")

## Project Template

* Open Visual Studio. I'm using VS 2017 Community Edition, you can use any version you have
* Create a new Project with the following configuration

![Create New Project](/assets/images/new-project.jpg "Create New Project")

![Project Template](/assets/images/project-template.jpg "Project Template")

![Create New Project](/assets/images/project-config.jpg "Create New Project")

## Install Packages

Run the collowing commands in the package manager console

![Open NuGet Package Manager](/assets/images/open-nuget-package-manager.jpg "Open NuGet Package Manager")

```powershell
install-package EntityFramework
install-package Microsoft.AspNet.Cors
install-package Microsoft.AspNet.Identity.Core
install-package Microsoft.AspNet.Identity.EntityFramework
install-package Microsoft.AspNet.Identity.Owin
install-package Microsoft.AspNet.WebApi.Cors
install-package Microsoft.AspNet.WebApi.Owin
install-package Microsoft.Owin.Cors
install-package Microsoft.Owin.Security.Jwt
install-package Microsoft.Owin.Host.SystemWeb
install-package System.IdentityModel.Tokens.Jwt
install-package Thinktecture.IdentityModel.Core
```

These are the minimum number of packages required to provide data persistence, enable CORS (Cross-Origin Resource Sharing), and enable generating and autAhenticating/authorizing with JWT.

## Entity Framework Setup

We will be using Entity Framework for data persistence. Entity Framework will take care of generating a database, adding tables, stored procedures and so on. As an added benefit, Entity Framework will also upgrade the schema automatically as we make changes.

Create a new **IdentityDbContext** called **MealsContext**, which will give us Users, Roles and Claims in our database. Add this under a folder called Core, for organization. We will add our entities to this later.

```csharp
namespace Meals.Service.Core
{
    using Microsoft.AspNet.Identity.EntityFramework;

    public class MealsContext : IdentityDbContext
    {
    }
}
```

Claims are used to describe useful information that the user has associated with them. We will use claims to tell the client which roles the user has. The benefit of roles is that we can prevent access to certain methods/controllers to a specific group of users, and permit access to others.

Add a **DbMigrationsConfiguration** class and allow automatic migrations, but prevent automatic data loss

```csharp
namespace Meals.Service.Core
{
    using System.Data.Entity.Migrations;

    public class Configuration : DbMigrationsConfiguration<MealsContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;
        }
    }
}
```

Now tell Entity Framework how to update the database schema using an initializer, as follows;

```csharp
namespace Meals.Service.Core
{
    using System.Data.Entity;

    public class Initializer : MigrateDatabaseToLatestVersion<MealsContext, Configuration>
    {
    }
}
```

This tells Entity Framework to go ahead and upgrade the database to the latest version automatically for us.

Finally, tell your application about the initializer by updating the Global.asax.cs file as follows;

Also we will configure our application to return camel-case JSON (thisIsCamelCase), instead of the default pascal-case (ThisIsPascalCase).

```csharp
namespace Meals.Service
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using System.Data.Entity;
    using System.Web.Http;

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            Database.SetInitializer(new Initializer());
            var formatters = GlobalConfiguration.Configuration.Formatters;
            var jsonFormatter = formatters.JsonFormatter;
            var settings = jsonFormatter.SerializerSettings;
            settings.Formatting = Formatting.Indented;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}
```

## Data Provider

By default, Entity Framework will configure itself to use LocalDB. If this is not desirable, say you want to use SQL Express instead, you need to make the following adjustments;

Open the **Web.config** file and delete the following code

```xml
<entityFramework>
    <defaultConnectionFactory type="System.Data.Entity.Infrastructure.LocalDbConnectionFactory, EntityFramework">
        <parameters>
            <parameter value="mssqllocaldb" />
        </parameters>
    </defaultConnectionFactory>
    <providers>
        <provider invariantName="System.Data.SqlClient" type="System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer" />
    </providers>
</entityFramework>
```

And add the connection string

```xml
<connectionStrings>
    <add name="BooksContext" providerName="System.Data.SqlClient" connectionString="Server=.;Database=Books;Trusted_Connection=True;" />
</connectionStrings>
```

Now we’re using SQL Server directly rather than LocalDB.

## CORS (Cross-Origin Resource Sharing)

**This step is completely optional.** We are adding in CORS support here because when we come to write our client app we will likely use a separate HTTP server (for testing and debugging purposes). When released to production, these two apps would use the same host (Internet Information Services (IIS)).

To enable CORS, open **WebApiConfig.cs** and add the following code to the beginning of the **Register** method

```csharp
var cors = new EnableCorsAttribute("*", "*", "*");
config.EnableCors(cors);
config.MessageHandlers.Add(new PreflightRequestsHandler());
```

Now create a new class in App_Start folder

```csharp
namespace Meals.Service
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class PreflightRequestsHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Contains("Origin") && request.Method.Method == "OPTIONS")
            {
                var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Headers", "Origin, Content-Type, Accept, Authorization");
                response.Headers.Add("Access-Control-Allow-Methods", "*");
                var tsc = new TaskCompletionSource<HttpResponseMessage>();
                tsc.SetResult(response);
                return tsc.Task;
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}
```

In the CORS workflow, before sending a DELETE, PUT or POST request, the client sends an OPTIONS request to check that the domain from which the request originates is the same as the server. If the request domain and server domain are not the same, then the server must include various access headers that describe which domains have access. To enable access to all domains, we just respond with an origin header (Access-Control-Allow-Origin) with an asterisk to enable access for all.

The **Access-Control-Allow-Headers** header describes which headers the API can accept/is expecting to receive. The **Access-Control-Allow-Methods** header describes which HTTP verbs are supported/permitted.

## Data Model

The API will expose meals, and meals will have reviews.

Under the Models folder add a new class called **Meal**. Add the following code

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Meals.Service.Models
{
    using System.Collections.Generic;

    public class Meal
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public virtual List<Review> Reviews { get; set; }
    }
}
```

And add **Review**

```csharp
namespace Meals.Service.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
        public int MealId { get; set; }
    }
}
```

Add these entities to the **IdentityDbContext** in MealsContext.cs

```csharp
namespace Meals.Service.Core
{
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System.Data.Entity;

    public class MealsContext : IdentityDbContext
    {
        public DbSet<Meal> Meals { get; set; }
        public DbSet<Review> Reviews { get; set; }
    }
}
```

## Abstractions

We need to abstract a couple of classes that we need to make use of, in order to keep our code clean and ensure that it works correctly.

Under the **Core** folder, add the following classes

```csharp
namespace Meals.Service.Core
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    public class MealUserManager : UserManager<IdentityUser>
    {
        public MealUserManager() : base(new MealUserStore())
        {
        }
    }
}
```

We will make heavy use of the **UserManager&lt;T&gt;** in our project, and we don’t want to have to initialise it with a **UserStore&lt;T&gt;** every time we want to make use of it. Whilst adding this is not strictly necessary, it does go a long way to helping keep the code clean.

Now add another class for the UserStore

```csharp
namespace Meals.Service.Core
{
    using Microsoft.AspNet.Identity.EntityFramework;

    public class MealUserStore : UserStore<IdentityUser>
    {
        public MealUserStore() : base(new MealsContext())
        {
        }
    }
}
```

This code is really important. If we fail to tell the UserStore which DbContext to use, it falls back to some default value.

## API Controller

We need to expose some data to our client (when we write it). let’s take advantage of Entity Frameworks **Seed** method. The **Seed** method will pre-populate some books and reviews automatically for us.

Kindly refer to [Configuration.cs](https://github.com/thenikhilk/jwt-auth-webapi/blob/master/Meals/Meals.Service/Core/Configuration.cs "Configuration.cs") for the code.

### Meals Endpoint

Create a new controller called Meals with the following code

```csharp
namespace Meals.Service.Controllers
{
    using Core;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using System.Web.Http;

    public class MealsController : ApiController
    {
        [HttpGet]
        public async Task<IHttpActionResult> Get()
        {
            using (var context = new MealsContext())
            {
                return Ok(await context.Meals.Include(meal => meal.Reviews).ToListAsync());
            }
        }
    }
}
```

### Reviews Endpoint

We’re also going to enable authorized users to post reviews and delete reviews. For this we will need a **ReviewsController** with the relevant Post and Delete methods. Add the following code;

Create a new Web API controller called **ReviewsController** and add the following code

```csharp
namespace Meals.Service.Controllers
{
    using Core;
    using Models;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using System.Web.Http;
    using ViewModels;

    public class ReviewsController : ApiController
    {
        [HttpPost]
        public async Task<IHttpActionResult> Post([FromBody] ReviewViewModel review)
        {
            using (var context = new MealsContext())
            {
                var meal = await context.Meals.FirstOrDefaultAsync(b => b.Id == review.MealId);
                if (meal == null)
                {
                    return NotFound();
                }

                var newReview = context.Reviews.Add(new Review
                {
                    MealId = meal.Id,
                    Description = review.Description,
                    Rating = review.Rating
                });

                await context.SaveChangesAsync();
                return Ok(new ReviewViewModel(newReview));
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Administrator")]
        public async Task<IHttpActionResult> Delete(int id)
        {
            using (var context = new MealsContext())
            {
                var review = await context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
                if (review == null)
                {
                    return NotFound();
                }

                context.Reviews.Remove(review);
                await context.SaveChangesAsync();
            }
            return Ok();
        }
    }
}
```

The [FromBody] attribute tells Web API to look for the data for the method argument in the body of the HTTP message that we received from the client, and not in the URL. The second parameter is a view model that wraps around the **Review** entity itself. Add a new folder to your project called **ViewModels**, add a new class called **ReviewViewModel** and add the following code

```csharp
namespace Meals.Service.ViewModels
{
    using Models;

    public class ReviewViewModel
    {
        public ReviewViewModel()
        {
        }

        public ReviewViewModel(Review review)
        {
            if (review == null)
            {
                return;
            }

            MealId = review.MealId;
            Rating = review.Rating;
            Description = review.Description;
        }

        public int MealId { get; set; }
        public int Rating { get; set; }
        public string Description { get; set; }

        public Review ToReview()
        {
            return new Review
            {
                MealId = MealId,
                Description = Description,
                Rating = Rating
            };
        }
    }
}
```

**Note**: In order to keep our API RESTful, we return the newly created entity (or its view model representation) back to the client for consumption, removing the need to re-fetch the entire data set.

## Authentication and Authorization Using OAuth and JSON Web Tokens (JWT)

 We will open up an OAuth endpoint to client credentials and return a token which describes the users claims. For each of the users roles we will add a claim (which will be used to control which views the user has access to on the client-side).
 We use OWIN to add our OAuth configuration into the pipeline. Add a new class to the project called **Startup.cs** and add the following code

```csharp
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Meals.Service.Startup))]

namespace Meals.Service
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureOAuth(app);
        }
    }
}
```

Notice that Startup is a partial class. I've done that because I want to keep this class as simple as possible, because as the application becomes more complicated and we add more and more middle-ware, this class will grow exponentially. You could use a static helper class here, but the preferred method from the MSDN documentation seems to be leaning towards using partial classes specifically.

Under the **App_Start** folder add a new class called **Startup.OAuth.cs** and add the following code

```csharp
using Meals.Service.Core;
using Meals.Service.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Configuration;

namespace Meals.Service
{
    public partial class Startup
    {
        public void ConfigureOAuth(IAppBuilder app)
        {
            var issuer = ConfigurationManager.AppSettings["issuer"];
            var secret = TextEncodings.Base64Url.Decode(ConfigurationManager.AppSettings["secret"]);
            app.CreatePerOwinContext(() => new MealsContext());
            app.CreatePerOwinContext(() => new MealUserManager());
            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                AllowedAudiences = new[] { "Any" },
                IssuerSecurityKeyProviders = new IIssuerSecurityKeyProvider[] {
                    new SymmetricKeyIssuerSecurityKeyProvider(issuer, secret)
                }
            });
            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/oauth2/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30),
                Provider = new CustomOAuthProvider(),
                AccessTokenFormat = new CustomJwtFormat(issuer)
            });
        }
    }
}
```

### OAuth secrets

Notice the code in the above file

```csharp
var issuer = ConfigurationManager.AppSettings["issuer"];
var secret = TextEncodings.Base64Url.Decode(ConfigurationManager.AppSettings["secret"]);
```

* Issuer - a unique identifier for the entity that issued the token (not to be confused with Entity Framework's entities)
* Secret - a secret key used to secure the token and prevent tampering

Split these values out into their own configuration file called keys.config and add a reference to that file in the main Web.config. I do this so that I can exclude just the keys from source control by adding a line to my .gitignore file.

To do this, open Web.config and change the &lt;appSettings&gt; section as follows

```xml
<appSettings file="keys.config">
</appSettings>
```

Now add a new file to your project called keys.config and add the following code

```xml
<appSettings>
  <add key="issuer" value="http://localhost:56228/"/>
  <add key="secret" value="IxrAjDoa2FqElO7IhrSrUJELhUckePEPVpaePlS_Xaw"/>
</appSettings>
```

We made use of OWIN to manage instances of objects for us, on a per request basis. The pattern is comparable to IoC, in that you tell the "container" how to create an instance of a specific type of object, then request the instance using a Get&lt;T&gt; method.

### OWIN context

The first time we request an instance of BooksContext for example, the lambda expression will execute and a new BooksContext will be created and returned to us. Subsequent requests will return the same instance.

**Note:** The life-cycle of object instance is per-request. As soon as the request is complete, the instance is cleaned up.

### Bearer Authentication/Authorization

We used the following code to enable bearer authentication

```csharp
app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                AllowedAudiences = new[] { "Any" },
                IssuerSecurityKeyProviders = new IIssuerSecurityKeyProvider[] {
                    new SymmetricKeyIssuerSecurityKeyProvider(issuer, secret)
                }
            });
```

The key takeaway of this code;

State who is the audience (we're specifying "Any" for the audience, as this is a required field but we're not fully implementing it).
State who is responsible for generating the tokens. Here we're using SymmetricKeyIssuerSecurityTokenProvider and passing it our secret key to prevent tampering. We could use the X509CertificateSecurityTokenProvider, which uses a X509 certificate to secure the token.
This code adds JWT bearer authentication to the OWIN pipeline.

### Enabling OAuth

We need to expose an OAuth endpoint so that the client can request a token (by passing a user name and password).

```csharp
app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/oauth2/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30),
                Provider = new CustomOAuthProvider(),
                AccessTokenFormat = new CustomJwtFormat(issuer)
            });
```

Some important notes with this code;

* We're going to allow insecure HTTP requests whilst we are in development mode. You might want to disable this using a *#IF Debug* directive so that you don't allow insecure connections in production.
* Open an endpoint under /oauth2/token that accepts post requests.
* When generating a token, make it expire after 30 minutes (1800 seconds).
* We will use our own provider, CustomOAuthProvider, and formatter, CustomJwtFormat, to take care of authentication and building the actual token itself.
* We need to write the provider and formatter next.

## Formatting the JWT

Create a new class under the **Identity** folder called **CustomJwtFormat.cs**.

```csharp
namespace Meals.Service.Identity
{
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.DataHandler.Encoder;
    using System;
    using System.Configuration;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using Thinktecture.IdentityModel.Tokens;

    public class CustomJwtFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private static readonly byte[] _secret = TextEncodings.Base64Url.Decode(ConfigurationManager.AppSettings["secret"]);
        private readonly string _issuer;

        public CustomJwtFormat(string issuer)
        {
            _issuer = issuer;
        }

        public string Protect(AuthenticationTicket data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var issued = data.Properties.IssuedUtc;
            var expires = data.Properties.ExpiresUtc;
            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(_secret);
            var signingCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(_issuer, "Any", data.Identity.Claims, issued.Value.UtcDateTime, expires.Value.UtcDateTime, signingCredentials));
        }

        public AuthenticationTicket Unprotect(string protectedText)
        {
            throw new NotImplementedException();
        }
    }
}
```

## Custom OAuth Provider

Now we want to authenticate the user, create CustomOAuthProvider in Identity folder

```csharp
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Meals.Service.Core;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

namespace Meals.Service.Identity
{
    public class CustomOAuthProvider : OAuthAuthorizationServerProvider
    {
        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            var user = context.OwinContext.Get<MealsContext>().Users.FirstOrDefault(u => u.UserName == context.UserName);
            if (!context.OwinContext.Get<MealUserManager>().CheckPassword(user, context.Password))
            {
                context.SetError("invalid_grant", "The user name or password is incorrect");
                context.Rejected();
                return Task.FromResult<object>(null);
            }

            var ticket = new AuthenticationTicket(SetClaimsIdentity(context, user), new AuthenticationProperties());
            context.Validated(ticket);

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            return Task.FromResult<object>(null);
        }

        private static ClaimsIdentity SetClaimsIdentity(OAuthGrantResourceOwnerCredentialsContext context, IdentityUser user)
        {
            var identity = new ClaimsIdentity("JWT");
            identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
            identity.AddClaim(new Claim("sub", context.UserName));

            var userRoles = context.OwinContext.Get<MealUserManager>().GetRoles(user.Id);
            foreach (var role in userRoles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            return identity;
        }
    }
}
```

As we're not checking the audience, when ValidateClientAuthentication is called we can just validate the request. When the request has a grant_type of password, which all our requests to the OAuth endpoint will have, the above GrantResourceOwnerCredentials method is executed. This method authenticates the user and creates the claims to be added to the JWT.

## Testing

Now it's time to build the code and test it. If your code doesn't build, please check with the GitHub version [here](https://github.com/thenikhilk/jwt-auth-webapi "GitHub repo link")

Open Postman and hit your Meals endpoint

You should be able to get data of meals with it's reviews this means are service is up and running.

![Get Data](/assets/images/get-data.jpg "Get Data")

## Authenticating Endpoints

Add a new file to the App_Start folder, called FilterConfig.cs and add the following code

```csharp
namespace Meals.Service
{
    using System.Web.Http;

    public class FilterConfig
    {
        public static void Configure(HttpConfiguration config)
        {
            config.Filters.Add(new AuthorizeAttribute());
        }
    }
}
```

To restrict access to all endpoints (except the OAuth endpoint) to requests that have been authenticated add this code from Global.asax.cs

```csharp
GlobalConfiguration.Configure(FilterConfig.Configure);
```

But if you wish to restrict access to selected endpoints methods then you can add the following code before each method

```csharp
[Authorize(Roles = "Administrator")]
```

Multiple roles can be added seperated by a comma (',').

And to restrict for all roles, just add

```csharp
[Authorize]
```

### Generating Token

Make a POST request to the OAuth endpoint, and include the following;

#### Headers

* **Accept** application/json
* **Accept-Language** en-us
* **Audience** Any

#### Body

* **username** administrator
* **password** administrator123
* **grant_type** password

![Token](/assets/images/token.jpg "Token")

Make sure you set the message type as **x-www-form-urlencoded**

Now in our code we have restriced the delete review method in **ReviewsController.cs** for users with **Administrator** role.

To test this, generate the token as mentioned above and pass it in the **Authorization** header as a **Bearer** oken while hitting the endpoint [http://localhost:62996/api/reviews/2](http://localhost:62996/api/reviews/2) in a **DELETE** method

e.g.

```text
Authorization Bearer eyJ0eXAiOiJ...RWZQ
```
