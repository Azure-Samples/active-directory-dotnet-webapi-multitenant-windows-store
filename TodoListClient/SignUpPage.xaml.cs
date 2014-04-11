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
using Windows.Security.Authentication.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using Windows.UI.Popups;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace TodoListClient
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class SignUpPage : Page
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


        public SignUpPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
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

        // This handler simulates the sign up process of the multitenant solution.
        // First, the app requests a token for the target web API using the common endpoint. This will allow the user to choose any tenant he/she likes, and will trigger the consent process.
        // Second, the app uses that token to secure a call to the sign up API. 
        // In a real app, such call would contain some data (like payment receipts) that help the web API to assess whether the caller should be signed up.        
        private async void btnSignUp_Click(object sender, RoutedEventArgs e)
        {
                   
            // get a token for the web API and in so doing present the user with the consent experience
            AuthenticationResult ar = await App.AuthenticationContext.AcquireTokenAsync(App.ResourceID,App.ClientID,App.ReturnUri);
            
            // call the onboarding API with the new token
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ar.AccessToken);           
            // test data. Ina  real app, the data would be something that proves that the user has gone throuhg whatever onboarding steps are required
            HttpContent content = new StringContent((@"{ ""name"": """ + txtOrgName.Text + @"""}"));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await httpClient.PostAsync(App.APIHostname+App.APIOnboardPath, content);

            if (response.IsSuccessStatusCode)
            {   // successfully onboarded.
                MessageDialog dialog = new MessageDialog(string.Format("Congratulations, you successfully signed up as {0}.\n\n If you want to sign in as a different user, simply click on the user control on the top right corner of the Todo screen", ar.UserInfo.UserId));
                await dialog.ShowAsync();
                string cachedAuthority = App.AuthenticationContext.TokenCacheStore.First().Key.Authority;
                App.AuthenticationContext = new AuthenticationContext(cachedAuthority);
                // redirect the user to the actual app page
                this.Frame.Navigate(typeof(TodoListPage));
            }
            else 
            {
                // failure. Notify the user, clean up the token cache and remain on this page
                MessageDialog dialog = new MessageDialog(string.Format("Something went wrong. Apparently the user {0} didn't work out. Try again", ar.UserInfo.UserId));
                await dialog.ShowAsync();
                App.AuthenticationContext.TokenCacheStore.Clear();
            }
        }
    }
}
