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

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    PostLogoutRedirectUri = postLogoutRedirectUri,
                    RedirectUri = redirectUri,
                    Authority = aadInstance + tenant,
                    Notifications =
                        new OpenIdConnectAuthenticationNotifications
                        {
                            AuthorizationCodeReceived = OwinStartup.AuthorizationCodeRecieved,
                            AuthenticationFailed = OwinStartup.AuthenticationFailed,
                            RedirectToIdentityProvider = OwinStartup.RedirectToIdentityProvider,
                            SecurityTokenValidated = OwinStartup.SecurityTokenValidated,
                        },
                });
        }

        #region Notification hooks

        public static Task RedirectToIdentityProvider(RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            // If a challenge was issued by the SingleSignOut javascript
            if (notification.Request.Path.Value == "/Account/SessionChanged")
            {
                // Store an app-specific cookie so we can identify OIDC messages that occurred
                // as a result of the SingleSignOut javascript.
                notification.Response.Cookies.Append("SingleSignOut" + clientId, notification.ProtocolMessage.State);

                notification.ProtocolMessage.Prompt = "none";
                string redirectUrl = notification.ProtocolMessage.BuildRedirectUrl();
                notification.Response.Redirect("/Account/SessionChanged?" + notification.ProtocolMessage.BuildRedirectUrl());
                notification.HandleResponse();
            }

            return Task.FromResult<object>(null);
        }
        
        // We need to update these values each time we receive a new token, so the SingleSignOut
        // javascript has access to the correct values.
        public static Task SecurityTokenValidated(SecurityTokenValidatedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            CheckSessionIFrame = notification.AuthenticationTicket.Properties.Dictionary[Microsoft.IdentityModel.Protocols.OpenIdConnectSessionProperties.CheckSessionIFrame];
            SessionState = notification.AuthenticationTicket.Properties.Dictionary[Microsoft.IdentityModel.Protocols.OpenIdConnectSessionProperties.SessionState];
            return Task.FromResult<object>(null);
        }

        // If the javascript issues an OIDC authorize request, and it fails (meaning the user needs to login)
        // this notification will be triggered with the error message 'login_required'
        public static Task AuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            // If the failed authentication was a result of a request by the SingleSignOut javascript
            if (notification.Request.Cookies["SingleSignOut" + clientId] != null 
                && notification.Request.Cookies["SingleSignOut" + clientId].Contains(notification.ProtocolMessage.State) 
                && notification.Exception.Message == "login_required")
            {
                // Clear the SingleSignOut cookie, and clear the OIDC session state so 
                //that we don't see any further "Session Changed" messages from the iframe.
                notification.Response.Cookies.Append("SingleSignOut" + clientId, "");
                SessionState = "";
                notification.Response.Redirect("Account/SingleSignOut");
                notification.HandleResponse();
            }

            return Task.FromResult<object>(null);
        }
        
        // If the javascript issues an OIDC authorize reuest, and it succeeds, the user is already logged
        // into AAD.  Since the AAD session cookie has changed, we need to check if the same use is still
        // logged in.
        public static Task AuthorizationCodeRecieved(AuthorizationCodeReceivedNotification notification)
        {   
            // If the successful authorize request was issued by the SingleSignOut javascript
            if (notification.AuthenticationTicket.Properties.RedirectUri.Contains("SessionChanged")) 
            {
                // Clear the SingleSignOutCookie
                notification.Response.Cookies.Append("SingleSignOut" + clientId, "");

                Claim existingUserObjectId = notification.OwinContext.Authentication.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");
                Claim incomingUserObjectId = notification.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");

                if (existingUserObjectId.Value != null && incomingUserObjectId != null)
                {   
                    // If a different user is logged into AAD
                    if(existingUserObjectId.Value != incomingUserObjectId.Value)
                    {
                        // No need to clear the session state here. It has already been
                        // updated with the new user's session state in SecurityTokenValidated.
                        notification.Response.Redirect("Account/SingleSignOut");
                        notification.HandleResponse();            
                    }
                    // If the same user is logged into AAD
                    else if (existingUserObjectId.Value == incomingUserObjectId.Value)
                    {
                        // No need to clear the session state, SecurityTokenValidated will do so.
                        // Simply redirect the iframe to a page other than SingleSignOut to reset
                        // the timer in the javascript.
                        notification.Response.Redirect("/");
                        notification.HandleResponse();
                    }
                }
            }
            
            return Task.FromResult<object>(null);
        }
        #endregion
    }
}