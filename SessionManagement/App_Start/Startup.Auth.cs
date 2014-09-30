using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Net.Security;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SessionManagement.App_Start
{
    public partial class OwinStartup
    {
        private static string appKey = ConfigurationManager.AppSettings["ida:AppKey"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];
        private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        
        public static string PostLogoutRedirectUri
        {
            get { return postLogoutRedirectUri;  }
        }

        public static string AADInstance
        {
            get { return aadInstance; }
        }

        public static string ClientId
        {
            get { return clientId; }
        }

        public static string CheckSessionIFrame
        {
            get;
            set;
        }

        public static string RedirectUri
        {
            get { return redirectUri; }
        }

        public static string SessionState
        {
            get;
            set;
        }

        public static PropertiesDataFormat StateDataFormat
        {
            get;
            set;
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    MetadataAddress = aadInstance + tenant + "/.well-known/openid-configuration",
                    Notifications =
                        new OpenIdConnectAuthenticationNotifications
                        {
                            AuthorizationCodeReceived = OwinStartup.AuthorizationCodeRecieved,
                            AuthenticationFailed = OwinStartup.AuthenticationFailed,
                            RedirectToIdentityProvider = OwinStartup.RedirectToIdentityProvider,
                            SecurityTokenValidated = OwinStartup.SecurityTokenValidated,
                        },
                    PostLogoutRedirectUri = postLogoutRedirectUri,
                    RedirectUri = redirectUri,
                });

            var dataProtector = app.CreateDataProtector(typeof(OpenIdConnectAuthenticationMiddleware).FullName, OpenIdConnectAuthenticationDefaults.AuthenticationType, "v1");
            StateDataFormat = new PropertiesDataFormat(dataProtector);
        }

        #region Notification hooks
        public static Task AuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            var index = notification.ProtocolMessage.State.IndexOf("=");
            string cleaned = notification.ProtocolMessage.State.Substring(index + 1);
            var unescaped = Uri.UnescapeDataString(cleaned);
            var authProps = StateDataFormat.Unprotect(unescaped);

            // TODO: A better way to know if prompt=none fail came as a result of session checking.
            // If we received an OIDC message with a login_required error as a result of session change event, the user has logged out elsewhere.
            if (notification.Exception.Message == "login_required" && authProps.RedirectUri.Contains("SessionChanged"))
            {
                // Clear the OIDC session state so that we don't see any further "Session Changed" messages from the iframe.
                SessionState = "";
                notification.Response.Redirect("Account/DistSignOut");
                notification.HandleResponse();
            }

            return Task.FromResult<object>(null);
        }
        
        public static Task AuthorizationCodeRecieved(AuthorizationCodeReceivedNotification notification)
        {
            var index = notification.ProtocolMessage.State.IndexOf("=");
            string cleaned = notification.ProtocolMessage.State.Substring(index + 1);
            var unescaped = Uri.UnescapeDataString(cleaned);
            var authProps = StateDataFormat.Unprotect(unescaped);

            // TODO: a better way. See above.
            // If we came from a check session request.
            if (authProps.RedirectUri.Contains("SessionChanged")) 
            {
                // If we recieved an Authorization Code, and the accompanying id_token is not for the currently logged in user, a different user has been logged into the STS
                if (notification.OwinContext.Authentication.User.Identity.IsAuthenticated && notification.AuthenticationTicket.Identity.Name != notification.OwinContext.Authentication.User.Identity.Name)
                {
                    // No need to clear the session state here. It has already been updated with the new user's value.
                    notification.Response.Redirect("Account/DistSignOut");
                    notification.HandleResponse();            
                }
                else if (notification.OwinContext.Authentication.User.Identity.IsAuthenticated && notification.AuthenticationTicket.Identity.Name == notification.OwinContext.Authentication.User.Identity.Name)
                {
                    notification.Response.Redirect("/");
                    notification.HandleResponse();
                }
            
            }
            
            return Task.FromResult<object>(null);
        }

        public static Task RedirectToIdentityProvider(RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            if (notification.Request.Path.Value == "/Account/SessionChanged")
            {
                // If challenge resulted from the session changed event, add a prompt=none parameter.
                notification.ProtocolMessage.Prompt = "none";
                //notification.ProtocolMessage.State += "DistSignOut";
                string redirectUrl = notification.ProtocolMessage.BuildRedirectUrl();
                notification.Response.Redirect("/Account/SessionChangedCallback?" + notification.ProtocolMessage.BuildRedirectUrl());
                notification.HandleResponse();
            }

            return Task.FromResult<object>(null);
        }

        public static Task SecurityTokenValidated(SecurityTokenValidatedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            CheckSessionIFrame = notification.AuthenticationTicket.Properties.Dictionary[Microsoft.IdentityModel.Protocols.OpenIdConnectSessionProperties.CheckSessionIFrame];
            SessionState = notification.AuthenticationTicket.Properties.Dictionary[Microsoft.IdentityModel.Protocols.OpenIdConnectSessionProperties.SessionState];
            return Task.FromResult<object>(null);
        }
        #endregion
    }
}