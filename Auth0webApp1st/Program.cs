using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie()
    .AddOpenIdConnect("Auth0", options =>
   {
        // Set The Authority to auth0 domain
        options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}";

        // Configure Auth0 creds

        options.ClientId = builder.Configuration["Auth0:ClientId"];
       options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];

        // Set response type to code
        options.ResponseType = OpenIdConnectResponseType.Code;

       // set scope 
       options.Scope.Clear();
       options.Scope.Add("openid");

       //call back path 

       options.CallbackPath = new PathString("/callback");

       //options.CallbackPath = new PathString("/");

       // set Issuer

       options.ClaimsIssuer = "Auth0";

       options.Events = new OpenIdConnectEvents
       {
           //handle the logout redirection

           OnRedirectToIdentityProviderForSignOut = (context) =>
           {
               var logouturi = $"https://{builder.Configuration["Auth0:Domain"]}/v2/logout?client_id={builder.Configuration["Auth0:ClientId"]}";
               var postLogoutUri = context.Properties.RedirectUri;
               if (!string.IsNullOrEmpty(postLogoutUri))
               {
                   if (postLogoutUri.StartsWith("/"))
                   {
                       var request = context.Request;
                       postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                   }
                   logouturi += $"&returnTo={Uri.EscapeDataString(postLogoutUri)}";
               }
               context.Response.Redirect(logouturi);

               context.HandleResponse();
               return Task.CompletedTask;
           }
       };
   });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseAuthentication();


app.MapControllerRoute(
    name: "default",
    //pattern: "{controller=Home}/{action=Index}/{id?}");
    pattern: "{controller=Account}/{action=Login}");

app.Run();
