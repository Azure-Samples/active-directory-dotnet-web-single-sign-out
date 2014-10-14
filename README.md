WebApp-DistributedSignOut-DotNet
================================

This sample shows how to build an MVC web application that uses Azure AD for sign-in using the OpenID Connect protocol and provides Single Sign Out across web apps.

For more information about how the OpenID Connect protocol works in this scenario, see the [OpenID Connect Session Management Specfication](http://openid.net/specs/openid-connect-session-1_0.html).

##About The Sample
If you would like to get started immediately, skip this section and jump to *How To Run The Sample*. 

This MVC 5 web application allows the user to sign in to the application with an AAD account.  Once signed in, if the user signs out of another application that has been authenticated using the same AAD tenant, this application will automatically sign the user out and display a message notifying the user.  Similarly, when the user signs out of this application, they will be signed out of any other applications that use the same AAD tenant and that have implemented Single Sign Out.

This sample will demonstrate the Single Sign Out capability by using the [Azure Management Portal](https://manage.windowsazure.com) as a second application that uses AAD for authentication.

## How To Run The Sample

To run this sample you will need:
- Visual Studio 2013
- An Internet connection
- An Azure subscription (a free trial is sufficient)

Every Azure subscription has an associated Azure Active Directory tenant.  If you don't already have an Azure subscription, you can get a free subscription by signing up at [http://wwww.windowsazure.com](http://www.windowsazure.com).  All of the Azure AD features used by this sample are available free of charge.

### Step 1:  Clone or download this repository

From your shell or command line:

`git clone https://github.com/AzureADSamples/WebApp-GroupClaims-DotNet.git`

### Step 2:  Create a user account in your Azure Active Directory tenant

If you already have a user account with Global Administrator rights in your Azure Active Directory tenant, you can skip to the next step.  This sample will not work with a Microsoft account, so if you signed in to the Azure portal with a Microsoft account and have never created a user account in your directory before, you need to do so now, and ensure it has the Global Administrator Directory Role.  If you create an account and want to use it to sign-in to the Azure portal, don't forget to add the user account as a co-administrator of your Azure subscription.

### Step 3:  Register the sample with your Azure Active Directory tenant

1. Sign in to the [Azure management portal](https://manage.windowsazure.com).
2. Click on Active Directory in the left hand nav.
3. Click the default directory tenant in which you created a Global Administrator user.  You will need to register your application in this tenant so that you can log into both the application and the portal with your Global Administrator user.
4. Click the Applications tab.
5. In the drawer, click Add.
6. Click "Add an application my organization is developing".
7. Enter a friendly name for the application, for example "SingleSignOutWebApp", select "Web Application and/or Web API", and click next.
8. For the sign-on URL, enter the base URL for the sample, which is by default `https://localhost:44308/`.  NOTE:  It is important, due to the way Azure AD matches URLs, to ensure there is a trailing slash on the end of this URL.  If you don't include the trailing slash, you will receive an error when the application attempts to redeem an authorization code.
9. For the App ID URI, enter `https://<your_tenant_name>/<your_application_name>`, replacing `<your_tenant_name>` with the name of your Azure AD tenant and `<your_application_name>` with the name you chose above.  Click OK to complete the registration.
10. While still in the Azure portal, click the Configure tab of your application.
11. Find the Client ID value and copy it aside, you will need this later when configuring your application.
12. Create a new key for the application.  Save the configuration so you can view the key value.  Save this aside for when you configure the project in Visual Studio.
13. In the Permissions to Other Applications configuration section, ensure that "Enable sign-on and read user's profiles" is selected under "Delegated permissions."  Save the configuration.

### Step 4:  Configure the sample to use your Azure AD tenant

1. Open the solution in Visual Studio 2013.
2. Open the `web.config` file.
3. Find the app key `ida:Tenant` and replace the value with your AAD tenant name, i.e. "defaulttenant.onmicrosoft.com".
4. Find the app key `ida:ClientId` and replace the value with the Client ID for the application from the Azure portal.
5. Find the app key `ida:AppKey` and replace the value with the key for the application from the Azure portal.
6. If you changed the base URL of the TodoListWebApp sample, find the app key `ida:PostLogoutRedirectUri` and replace the value with the new base URL of the sample.

### Step 5:  Run the sample

Clean the solution, rebuild the solution, and run it.  NOTE: Be sure not to run the sample in Internet Explorer, or you will get unexpected behavior.  Sign into the application by clicking one of the tabs, such as "About."  Be sure to sign in with a user that can also sign in to the Azure Management Portal.  Once signed in, sign in to the [Azure Management Portal](https://manage.windowsazure.com) as well.  Try signing out of either application; you will be signed out of the other in a matter of seconds.
