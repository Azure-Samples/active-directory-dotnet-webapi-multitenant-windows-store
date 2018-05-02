---
services: active-directory
platforms: dotnet
author: jmprieur
---

# Building a multi-tenant web API secured by Azure AD

This sample demonstrates a Windows Store application calling a multi-tenant web API that is secured using Azure AD.  The Windows Store application uses the Active Directory Authentication Library (ADAL) to obtain a JWT access token through the OAuth 2.0 protocol.  The access token is sent to the web API to authenticate the user.
The web API project demonstrates how to structure your services for being accessed by users coming from multiple Azure AD tenants.
The Windows Store application shows how to handle in-up sign up for a new service and sign in from any Azure tenant.     

For more information about how the protocols work in this scenario and other scenarios, see [Authentication Scenarios for Azure AD](https://azure.microsoft.com/documentation/articles/active-directory-authentication-scenarios/).

> Looking for previous versions of this code sample? Check out the tags on the [releases](https://github.com/Azure-Samples/active-directory-dotnet-webapi-multitenant-windows-store/releases) GitHub page.

## How To Run This Sample

To run this sample you will need:
- Visual Studio 2017
- Windows 10
- An Internet connection
- A Microsoft account
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, please see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/) 
- A user account in your Azure AD tenant. This sample will not work with a Microsoft account, so if you signed in to the Azure portal with a Microsoft account and have never created a user account in your directory before, you need to do that now.

### Step 1:  Clone or download this repository

From your shell or command line:

`git clone https://github.com/Azure-Samples/active-directory-dotnet-webapi-multitenant-windows-store.git`

### Step 2:  Register the sample with your Azure Active Directory tenant

There are two projects in this sample.  Each needs to be separately registered in your Azure AD tenant.

#### Register the TodoListServiceMT web API

1. Sign in to the [Azure portal](https://portal.azure.com).
2. On the top bar, click on your account and under the **Directory** list, choose the Active Directory tenant where you wish to register your application.
3. Click on **All Services** in the left hand nav, and choose **Azure Active Directory**.
4. Click on **App registrations** and choose **New application registration**.
5. Enter a friendly name for the application, for example 'TodoListServiceMT' and select 'Web Application and/or Web API' as the Application Type. For the sign-on URL, enter the base URL for the sample, which is by default `https://localhost:44301`. Click on **Create** to create the application.
6. While still in the Azure portal, choose your application, click on **Settings** and choose **Properties**.
7. Find the Application ID value and copy it to the clipboard.
8. Find "multitenanted" switch and flip it to yes. Hit the Save button from the command bar.
9. For the App ID URI, enter https://\<your_tenant_name\>/TodoListServiceMT, replacing \<your_tenant_name\> with the name of your Azure AD tenant.

#### Find the TodoListClient app's redirect URI

Before you can register the TodoListClient application in the Azure portal, you need to find out the application's redirect URI.  Windows 8 provides each application with a unique URI and ensures that messages sent to that URI are only sent to that application.  To determine the redirect URI for your project:

1. Open the solution in Visual Studio 2013.
2. In the TodoListClient project, open the `App.xaml.cs` file.
3. Find this line of code and set a breakpoint on it.

```C#
public static Uri ReturnUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri();
```

4. Right-click on the TodoListClient project and Debug --> Start New Instance.
5. When the breakpoint is hit, use the debugger to determine the value of redirectURI, and copy it aside for the next step.
6. Stop debugging, and clear the breakpoint.

The redirectURI value will look something like this:

```
ms-app://s-1-15-2-2123189467-1366327299-2057240504-936110431-2588729968-1454536261-950042884/
```

#### Register the TodoListClient app

1. Sign in to the [Azure portal](https://portal.azure.com).
2. On the top bar, click on your account and under the **Directory** list, choose the Active Directory tenant where you wish to register your application.
2. Click on **More Services** in the left hand nav, and choose **Azure Active Directory**.
3. Click on **App registrations** and choose **New application registration**.
4. Enter a friendly name for the application, for example 'TodoListClient-WindowsStore' and select 'Native' as the Application Type. Enter the Redirect URI value that you obtained during the previous step. Click on **Create** to create the application.
5. While still in the Azure portal, choose your application, click on **Settings** and choose **Properties**.
6. Find the Application ID value and copy it to the clipboard.
7. Configure Permissions for your application - in the Settings menu, choose the 'Required permissions' section, click on **Add**, then **Select an API**, and type 'TodoListServiceMT' in the textbox. Then, click on  **Select Permissions** and select 'Access TodoListServiceMT'.

### Step 3: Add the client application to the known clients list of the API 

For the client application to be able to call the web API from a tenant other than the one where you developed the app, you need to explicitly bind the client app registration in Azure AD with the registration for the web API. You can do so by adding the "Client ID" of the client app, to the manifest of the web API. Here's how:

1. Return to the TodoListServiceMT application page in the Azure portal.
2. From the application page, click **Manifest** to open the inline manifest editor.
3. In the manifest, locate the `knownClientApplications` array property, and add the Client ID you saved in task 10 of step 3 ("Register the TodoListClient app") as an element. Your code should look like the following after you're done:
    `"knownClientApplications": ["94da0930-763f-45c7-8d26-04d5938baab2"]`
4. Click **Save** to save the TodoListServiceMT manifest.

### Step 4:  Configure the sample to use your Azure AD tenant

#### Configure the TodoListServiceMT project

1. Open the solution in Visual Studio 2015.
2. Open the `web.config` file.
3. Find the app key `ida:Tenant` and replace the value with your AAD tenant name.
4. Find the app key `ida:Audience` and replace the value with the App ID URI you registered earlier, for example `https://<your_tenant_name>/TodoListServiceMT`.


#### Configure the TodoListClient project

1. Open `App.xaml.cs'.
3. Find the declaration of `clientId` and replace the value with the Client ID from the Azure portal.
4. Find the declaration of `ResourceID` and `APIHostname` and ensure their values are set properly for your TodoListService project.


### Step 5:  [optional] Create an Azure Active Directory test tenant

This sample shows how to take advantage of the consent model in Azure AD to make a web API available to native clients ran by any user from any organization with a tenant in Azure AD. To see that part of the sample in action, you need to have access to user accounts from a tenant that is different from the one you used for developing the application. The simplest way of doing that is to create a new directory tenant in your Azure subscription (just navigate to the main Active Directory page in the portal and click Add) and add test users. This step is optional as you can also run the sample with accounts from the same directory, but if you do you will not see the consent prompts as the app is already approved. 

### Step 6:  Run the sample

Clean the solution, rebuild the solution, and run it.  You might want to go into the solution properties and set both projects as startup projects, with the service project starting first.

The sample application implements two tasks: signing up as a new user and signing in to access the Todo service. To be able to sing in, you must fisrt sign up with the account you want to use. 

#### Sign up

In the native app UI, choose Sign Up. In the sign up screen, enter some random text in the organization name and hit the Sign Up button. You will be prompted to enter your credentials.


- If you enter the credentials of a user from a tenant that is different from the one in which you configured the web API and the native app, you will be presentd with a consent dialog. Click OK.
- If you enter the credentials of a user from the same tenant, you will not see a consent dialog.

Upon successful authentication, you will be presented with a message that confirms that the sign up task took place. After having dismissed the message, you will be redirected to the Todo management screen. Create some todo items, then stop the debugging session.
 
#### Sign in

Launch again a debugging session for the solution. You will see that you are transported directly to the Todo management screen: that is because ADAL cached the token obtained in the former step. Click on the top-right account icon to clear the cache and the session.
You will be transported back to the welcome page. This time choose Sign In, then enter the same credentials you used for the sign up: you will get back to the Todo management screen. 
