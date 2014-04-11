using TodoListClient.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Windows.UI.Popups;
using System.Net.Http;
using System.Net.Http.Headers;
using Windows.Data.Json;


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace TodoListClient
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class TodoListPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public TodoListPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

       
        // Connect to the web API as the current user to retrieve a list of todo items 
        private async void GetTodoList()
        {
            // get a token for the API. Note that this will often come from the cache
            AuthenticationResult result = await App.AuthenticationContext.AcquireTokenAsync(App.ResourceID, App.ClientID);

            if (result.Status != AuthenticationStatus.Succeeded)
            {
                if (result.Error == "authentication_canceled")
                {
                    // The user cancelled the sign-in
                }
                else
                {
                    MessageDialog dialog = new MessageDialog(string.Format("If the error continues, please contact your administrator.\n\nError: {0}\n\nError Description:\n\n{1}", result.Error, result.ErrorDescription), "Sorry, an error occurred while signing you in.");
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
                txtFirstName.Text = result.UserInfo.UserId;
                txtLastName.Text = "(Temporary identifier)";
            }
            // Call the web API presenting the access token
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await httpClient.GetAsync(App.APIHostname+App.APITodoListPath);

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
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // If the To Do list service returns access denied, clear the token cache and have the user sign-in again.
                    MessageDialog dialog = new MessageDialog("Sorry, you don't have access to the To Do Service.  You might need to sign up.");
                    await dialog.ShowAsync();
                    App.AuthenticationContext.TokenCacheStore.Clear();
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
            // Get a token for the API. Note that this will almost always come from the cache
            // given that a POST will always follow a get of all the todos, whihc will have already acquired the token if necessary
            AuthenticationResult result = await App.AuthenticationContext.AcquireTokenAsync(App.ResourceID, App.ClientID);

            if (result.Status != AuthenticationStatus.Succeeded)
            {
                if (result.Error == "authentication_canceled")
                {
                    // The user cancelled the sign-in
                }
                else
                {
                    MessageDialog dialog = new MessageDialog(string.Format("If the error continues, please contact your administrator.\n\nError: {0}\n\nError Description:\n\n{1}", result.Error, result.ErrorDescription), "Sorry, an error occurred while signing you in.");
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
                txtFirstName.Text = result.UserInfo.UserId;
                txtLastName.Text = "(Temporary identifier)";
            }
            // Call the web API presenting the access token
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpContent content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("Description", TodoText.Text) });
            
            var response = await httpClient.PostAsync(App.APIHostname+App.APITodoListPath, content);

            if (response.IsSuccessStatusCode)
            {
                TodoText.Text = "";
                GetTodoList();
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // If the To Do list service returns access denied, clear the token cache and have the user sign-in again.
                    MessageDialog dialog = new MessageDialog("Sorry, you don't have access to the To Do Service. You might need to sign up.");
                    await dialog.ShowAsync();
                    App.AuthenticationContext.TokenCacheStore.Clear();
                    this.Frame.Navigate(typeof(MainPage));
                }
                else
                {
                    MessageDialog dialog = new MessageDialog("Sorry, an error occurred accessing your To Do list.  Please try again.");
                    await dialog.ShowAsync();                    
                }
            }
        }
        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // as soon as the use reaches the page, populate the UI by calling the web API
            GetTodoList();
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        // Clean up the current user session
        private async void btnAccount_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog dialog = new MessageDialog("Do you want to clear the current user session?","Sign Out");
            dialog.Commands.Add(new UICommand(("Yes"), (command) => 
            {
                // delete the current token
                App.AuthenticationContext.TokenCacheStore.Clear();
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

        private async void btnAddTodo_Click(object sender, RoutedEventArgs e)
        {
            // post a new Todo 
            PostNewTodo();
            
            

        }
    }
}
