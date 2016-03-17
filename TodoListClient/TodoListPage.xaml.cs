using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TodoListClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TodoListPage : Page
    {
        public TodoListPage()
        {
            this.InitializeComponent();
        }


        // Connect to the web API as the current user to retrieve a list of todo items 
        private async void GetTodoList()
        {
            AuthenticationResult result = null;
            try
            {
                // get a token for the API. Note that this will often come from the cache
                result = await App.AuthenticationContext.AcquireTokenAsync(App.ResourceID, App.ClientID, App.ReturnUri, new PlatformParameters(PromptBehavior.Auto, false));
            }
            catch (AdalException ex)
            {
                if (ex.ErrorCode == "authentication_canceled")
                {
                    // The user cancelled the sign-in
                }
                else
                {
                    MessageDialog dialog = new MessageDialog(string.Format("If the error continues, please contact your administrator.\n\nError: {0}\n\nError Description:\n\n{1}", ex.ErrorCode, ex.Message), "Sorry, an error occurred while signing you in.");
                    await dialog.ShowAsync();
                }
                // we failed to obtain a token, hence we cannot render this page. We need to go back to main.
                this.Frame.Navigate(typeof(MainPage));
                return;
            }
                
            // Populate the current user controol on the top right corner.
            // The code below works around a bug for whihc in some cases the service fails to return user information
            if (result.UserInfo.GivenName != null)
            {
                txtFirstName.Text = result.UserInfo.GivenName;
                txtLastName.Text = result.UserInfo.FamilyName;
            }
            else
            {
                txtFirstName.Text = result.UserInfo.UniqueId;
                txtLastName.Text = "(Temporary identifier)";
            }

            btnAccount.Visibility = Windows.UI.Xaml.Visibility.Visible;
            // Call the web API presenting the access token
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(App.APIHostname + App.APITodoListPath));

            if (response.IsSuccessStatusCode)
            {

                // Read the response as a Json Array and databind to the GridView to display todo items
                string rezstring = await response.Content.ReadAsStringAsync();
                var todoArray = JsonArray.Parse(rezstring);

                TodoList.ItemsSource = from todo in todoArray
                                       select new
                                       {
                                           Description = todo.GetObject()["Description"].GetString()
                                       };
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // If the To Do list service returns access denied, clear the token cache and have the user sign-in again.
                    MessageDialog dialog = new MessageDialog("Sorry, you don't have access to the To Do Service.  You might need to sign up.");
                    await dialog.ShowAsync();
                    App.AuthenticationContext.TokenCache.Clear();
                    this.Frame.Navigate(typeof(MainPage));
                }
                else
                {
                    MessageDialog dialog = new MessageDialog("Sorry, an error occurred accessing your To Do list.  Please try again.");
                    await dialog.ShowAsync();
                    this.Frame.Navigate(typeof(MainPage));
                }
            }
        }
        // Connect to the web API as the current user to post a new todo item
        private async void PostNewTodo()
        {
            AuthenticationResult result = null;
            try
            {
                // get a token for the API. Note that this will often come from the cache
                result = await App.AuthenticationContext.AcquireTokenAsync(App.ResourceID, App.ClientID, App.ReturnUri, new PlatformParameters(PromptBehavior.Auto, false));
            }
            catch (AdalException ex)
            {
                if (ex.ErrorCode == "authentication_canceled")
                {
                    // The user cancelled the sign-in
                }
                else
                {
                    MessageDialog dialog = new MessageDialog(string.Format("If the error continues, please contact your administrator.\n\nError: {0}\n\nError Description:\n\n{1}", ex.ErrorCode, ex.Message), "Sorry, an error occurred while signing you in.");
                    await dialog.ShowAsync();
                }
                // we failed to obtain a token, hence we cannot render this page. We need to go back to main.
                this.Frame.Navigate(typeof(MainPage));
                return;
            }

            // Populate the current user controol on the top right corner.
            // The code below works around a bug for whihc in some cases the service fails to return user information
            if (result.UserInfo.GivenName != null)
            {
                txtFirstName.Text = result.UserInfo.GivenName;
                txtLastName.Text = result.UserInfo.FamilyName;
            }
            else
            {
                txtFirstName.Text = result.UserInfo.UniqueId;
                txtLastName.Text = "(Temporary identifier)";
            }
            btnAccount.Visibility = Windows.UI.Xaml.Visibility.Visible;
            // Call the web API presenting the access token
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", result.AccessToken);
            HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(new[] { new KeyValuePair<string, string>("Description", TodoText.Text) });

            var response = await httpClient.PostAsync(new Uri(App.APIHostname + App.APITodoListPath), content);

            if (response.IsSuccessStatusCode)
            {
                TodoText.Text = "";
                GetTodoList();
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // If the To Do list service returns access denied, clear the token cache and have the user sign-in again.
                    MessageDialog dialog = new MessageDialog("Sorry, you don't have access to the To Do Service. You might need to sign up.");
                    await dialog.ShowAsync();
                    App.AuthenticationContext.TokenCache.Clear();
                    this.Frame.Navigate(typeof(MainPage));
                }
                else
                {
                    MessageDialog dialog = new MessageDialog("Sorry, an error occurred accessing your To Do list.  Please try again.");
                    await dialog.ShowAsync();
                }
            }
        }
        
        // Clean up the current user session
        private async void btnAccount_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog dialog = new MessageDialog("Do you want to clear the current user session?", "Sign Out");
            dialog.Commands.Add(new UICommand(("Yes"), (command) =>
            {
                // delete the current token
                App.AuthenticationContext.TokenCache.Clear();
                // hide the account icon
                btnAccount.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                // re-bind AuthenticationContext to the common, tenant-less endpoint
                App.AuthenticationContext = new AuthenticationContext(App.CommonAuthority);
                // navigate back to the welcome page
                this.Frame.Navigate(typeof(MainPage));
            }));
            dialog.Commands.Add(new UICommand(("No"), (command) =>
            {
                return;
            }));
            await dialog.ShowAsync();
        }

        private void btnAddTodo_Click(object sender, RoutedEventArgs e)
        {
            // post a new Todo 
            PostNewTodo();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            GetTodoList();
            base.OnNavigatedTo(e);
        }
    }
}
