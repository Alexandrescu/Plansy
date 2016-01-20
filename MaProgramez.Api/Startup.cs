using MaProgramez.Api;
using MaProgramez.Api.Providers;
using MaProgramez.Repository.DbContexts;
using Microsoft.Owin;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Data.Entity;
using System.Web.Http;

[assembly: OwinStartup(typeof(Startup))]

namespace MaProgramez.Api
{
    public class Startup
    {
        public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }

        public static GoogleOAuth2AuthenticationOptions GoogleAuthOptions { get; private set; }

        public static FacebookAuthenticationOptions FacebookAuthOptions { get; private set; }

        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            ConfigureOAuth(app);

            WebApiConfig.Register(config);
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            app.UseWebApi(config);
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AppDbContext, Repository.Migrations.Configuration>());
        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            //use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ExternalCookie);
            OAuthBearerOptions = new OAuthBearerAuthenticationOptions();

            var oAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(60),
                Provider = new SimpleAuthorizationServerProvider(),
                RefreshTokenProvider = new SimpleRefreshTokenProvider()
            };

            // Token Generation
            app.UseOAuthAuthorizationServer(oAuthServerOptions);
            app.UseOAuthBearerAuthentication(OAuthBearerOptions);

            //Configure Google External Login
            GoogleAuthOptions = new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "502256246791-c68uakfblt5l27mbtaf5gp4a4mvmrlgl.apps.googleusercontent.com",
                ClientSecret = "QW4iXDxQwMyXaPba3-lwWyY9",
                Provider = new GoogleAuthProvider()
            };
            app.UseGoogleAuthentication(GoogleAuthOptions);

            //Configure Facebook External Login
            FacebookAuthOptions = new FacebookAuthenticationOptions()
            {
                AppId = "174264189592316",
                AppSecret = "8163d6b3fb6c9631597f3839cce85c66",
                Provider = new FacebookAuthProvider()
            };
            app.UseFacebookAuthentication(FacebookAuthOptions);
        }
    }
}