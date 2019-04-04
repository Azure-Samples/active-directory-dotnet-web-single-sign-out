---
services: active-directory
platforms: dotnet
author: jmprieur
level: 400
client: .NET 4.5 Web App (MVC)
service: ASP.NET Web API
endpoint: AAD V1
---

# Performing single sign out of all web apps using Azure AD

> You might also be interested in this sample: https://github.com/azure-samples/ms-identity-aspnetcore-webapp-tutorial/tree/master/1-WebApp-OIDC/1-6-SignOut
>
> This newer sample takes advantage of the Microsoft identity platform (formerly Azure AD v2.0).
>
> While still in public preview, every component is supported in production environments.

This sample shows how to build an MVC web application that uses Azure AD for sign-in using the OpenID Connect protocol and provides Single Sign Out across web apps.

- An Azure AD Office Hours session covered Single sign-out for applications registred with azure AD. Watch the video [Single sign-on best practices for Azure Active Directory and Microsoft Accounts](https://www.youtube.com/watch?v=9q9N3iUWwGk)

For more information about how the OpenID Connect protocol works in this scenario, see the [OpenID Connect Session Management Specfication](http://openid.net/specs/openid-connect-session-1_0.html).

## About The Sample
If you would like to get started immediately, skip this section and jump to *How To Run The Sample*. 

This MVC 5 web application allows the user to sign in to the application with an AAD account.  Once signed in, if the user signs out of another application that has been authenticated using the same AAD tenant, this application will automatically sign the user out and display a message notifying the user.  Similarly, when the user signs out of this application, they will be signed out of any other applications that use the same AAD tenant and that have implemented Single Sign Out.

This sample will demonstrate the Single Sign Out capability by using the [Azure Management Portal](https://manage.windowsazure.com) as a second application that uses AAD for authentication.

## How To Run The Sample

- [Visual Studio 2017](https://aka.ms/vsdownload)
- An Internet connection
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/)
- A user account in your Azure AD tenant. This sample will not work with a Microsoft account (formerly Windows Live account). Therefore, if you signed in to the [Azure portal](https://portal.azure.com) with a Microsoft account and have never created a user account in your directory before, you need to do that now.

### Step 1:  Clone or download this repository

From your shell or command line:

`git clone https://github.com/Azure-Samples/active-directory-dotnet-web-single-sign-out.git`

> Given that the name of the sample is pretty long, and so are the name of the referenced NuGet pacakges, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

There are one projects in this sample. Each needs to be separately registered in your Azure AD tenant. To register these projects, you can:

- either follow the steps in the paragraphs below ([Step 2](#step-2--register-the-sample-with-your-azure-active-directory-tenant) and [Step 3](#step-3--configure-the-sample-to-use-your-azure-ad-tenant))
- or use PowerShell scripts that:
  - **automatically** create for you the Azure AD applications and related objects (passwords, permissions, dependencies)
  - modify the Visual Studio projects' configuration files.

If you want to use this automation, read the instructions in [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md)

#### First step: choose the Azure AD tenant where you want to create your applications

As a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com).
1. On the top bar, click on your account, and then on **Switch Directory**.
1. Once the *Directory + subscription* pane opens, choose the Active Directory tenant where you wish to register your application, from the *Favorites* or *All Directories* list.
1. Click on **All services** in the left-hand nav, and choose **Azure Active Directory**.

> In the next steps, you might need the tenant name (or directory name) or the tenant ID (or directory ID). These are presented in the **Properties**
of the Azure Active Directory window respectively as *Name* and *Directory ID*

#### Register the service app (WebApp-DistributedSignOut-DotNet)

1. In the  **Azure Active Directory** pane, click on **App registrations** and choose **New application registration**.
1. Enter a friendly name for the application, for example 'WebApp-DistributedSignOut-DotNet' and select 'Web app / API' as the *Application Type*.
1. For the *Sign-on URL*, enter the base URL for the sample. By default, this sample uses `https://localhost:44308/`.
1. Click **Create** to create the application.
1. In the succeeding page, Find the *Application ID* value and record it for later. You'll need it to configure the Visual Studio configuration file for this project.
1. Then click on **Settings**, and choose **Properties**.
1. For the App ID URI, replace the guid in the generated URI 'https://\<your_tenant_name\>/\<guid\>', with the name of your service, for example, 'https://\<your_tenant_name\>/WebApp-DistributedSignOut-DotNet' (replacing `<your_tenant_name>` with the name of your Azure AD tenant)
1. From the **Settings** | **Reply URLs** page for your application, update the Reply URL for the application to be `https://localhost:44308/`
1. For **Logout URL**, provide the value `https://localhost:44308/Account/EndSession`
1. From the Settings menu, choose **Keys** and add a new entry in the Password section:

   - Type a key description (of instance `app secret`),
   - Select a key duration of either **In 1 year**, **In 2 years**, or **Never Expires**.
   - When you save this page, the key value will be displayed, copy, and save the value in a safe location.
   - You'll need this key later to configure the project in Visual Studio. This key value will not be displayed again, nor retrievable by any other means,
     so record it as soon as it is visible from the Azure portal.
1. Configure Permissions for your application. To that extent, in the Settings menu, choose the 'Required permissions' section and then,
   click on **Add**, then **Select an API**, and type `Microsoft Graph` in the textbox. Then, click on  **Select Permissions** and select **User.Read**.

### Step 3:  Configure the sample to use your Azure AD tenant

In the steps below, "ClientID" is the same as "Application ID" or "AppId".

Open the solution in Visual Studio to configure the projects

#### Configure the service project

1. Open the `WebApp-DistributedSignOut-DotNet\Web.Config` file
1. Find the app key `ida:Tenant` and replace the existing value with your Azure AD tenant name.
1. Find the app key `ida:ClientId` and replace the existing value with the application ID (clientId) of the `WebApp-DistributedSignOut-DotNet` application copied from the Azure portal.
1. Find the app key `ida:AppKey` and replace the existing value with the key you saved during the creation of the `WebApp-DistributedSignOut-DotNet` app, in the Azure portal.
1. Find the app key `ida:PostLogoutRedirectUri` and replace the existing value with the base address of the WebApp-DistributedSignOut-DotNet project (by default `https://localhost:44308/`).

### Step 5:  Run the sample

Clean the solution, rebuild the solution, and run it.  NOTE: Be sure not to run the sample in Internet Explorer, or you will get unexpected behavior.  Sign into the application by clicking one of the tabs, such as "About."  Be sure to sign in with a user that can also sign in to the Azure Management Portal.  Once signed in, sign in to the [Azure Management Portal](https://manage.windowsazure.com) as well.  Try signing out of either application; you will be signed out of the other in a matter of seconds.

## Code Walk-Through

For the most part, you can simply cut and paste the code from this sample into your OWIN application in order to provide Single Sign Out functionality.  But for a deeper understanding of the code and the OpenID Connect Session Managment protocol, take a look at the following five files:

#### _Layout.cshtml

In `_Layout.cshtml`, you simply need to render the _SingleSignOut.cshtml partial view, so that the Single Sign Out related javascript is loaded into every page in the application.

#### _SingleSignOut.cshtml

This partial view is where the majority of the action takes place.  In order to know when to perform Single Sign Out, the application needs a way to check the status of the user's session with Azure AD.  This could be achieved by polling AAD periodically, but would incur more network cost than is necessary.  Instead, the application will periodically check the value of a cookie that is set by AAD on login, as directed by the OpenID Connect Session Management specfication.  Only if the value of the cookie has changed will the application then submit a request to AAD to check the status of the user's session with AAD.  AAD provides a "CheckSessionIframe" to peform this check for you that is used in this sample.

When a page in the application loads, the javascript loads the CheckSessionIframe in a hidden iFrame.  On a periodic basis, it triggers the CheckSessionIFrame to check the AAD cookie and notify the application of any changes.  If a change in the AAD session has been detected, the iFrame is pointed to AAD's authorize endpoint, submitting an authorization request to AAD without requring user interaction (since the iFrame is hidden, of course). The result of this authorization request will be processed by the OWIN OpenIDConnect Middleware, described below in `Startup.Auth.cs`.

#### AccountController.cs

In `AccountController.cs`, there are two actions to note.  The `SessionChanged` action is a shortcut that is used to construct the authorization request that is submitted to AAD.  The javascript submits an ajax request to this action, which subsequently issues an OpenID Connect challenge.  This challenge triggers OWIN to construct an authorization request and submit it to AAD.  But instead of allowing OWIN to submit the authorization request, the request is intercepted in `Startup.Auth.cs`'s `RedirectToIdentityProvider` notification and is returned to the originating javascript via `SessionChanged` as the result of the ajax request.  In this way, the javascript does not have to construct an authorization request on its own.

The other action to note is `SingleSignOut`, which actually signs the user out of the application and displays a message telling the user that a Single Sign Out has occurred.

#### SingleSignOut.cshtml

Presents the Single Sign Out occurred message to the user.

#### Startup.Auth.cs

In `Startup.Auth.cs`, two `OpenIDConnectAuthenticationNotifications` callbacks are used to process the authorization request result from AAD.  If the request fails, it can be interpreted as the user needing to reauthenticate with AAD (and that the user should be signed out of the application).  OWIN triggers the `AuthenticationFailed` callback, which signs the user out using the `SingleSignOut` action.

If the authorization request succeeds, there are two possibilities.  First, that the user is still authenticated with AAD and no Single Sign Out is necessary.  Second, that the user is authenticated with AAD but as a different user than before.  In this case, a Single Sign Out is necessary.  In either case, OWIN triggers the `AuthorizationCodeRecieved` callback, which handles each case individually.

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/adal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`adal` `dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## More information

For more information, check out the following links

- [Send a sign-out request](https://docs.microsoft.com/en-us/azure/active-directory/develop/v1-protocols-openid-connect-code#send-a-sign-out-request)
- [Single Sign-Out SAML Protocol](https://docs.microsoft.com/en-us/azure/active-directory/develop/single-sign-out-saml-protocol)
