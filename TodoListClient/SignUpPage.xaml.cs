using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TodoListClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SignUpPage : Page
    {
        public SignUpPage()
        {
            this.InitializeComponent();
        }

        // This handler simulates the sign up process of the multitenant solution.
        // First, the app requests a token for the target web API using the common endpoint. This will allow the user to choose any tenant he/she likes, and will trigger the consent process.
        // Second, the app uses that token to secure a call to the sign up API. 
        // In a real app, such call would contain some data (like payment receipts) that help the web API to assess whether the caller should be signed up.        
        private async void btnSignUp_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                // get a token for the web API and in so doing present the user with the consent experience
                AuthenticationResult ar = await App.AuthenticationContext.AcquireTokenAsync(App.ResourceID, App.ClientID, App.ReturnUri, new PlatformParameters(PromptBehavior.Always, false));

                // call the onboarding API with the new token
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ar.AccessToken);
                HttpContent content = new StringContent((@"{ ""name"": """ + txtOrgName.Text + @"""}"));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await httpClient.PostAsync(App.APIHostname + App.APIOnboardPath, content);

                if (response.IsSuccessStatusCode)
                {   // successfully onboarded.
                    MessageDialog dialog = new MessageDialog(string.Format("Congratulations, you successfully signed up as {0}.\n\n If you want to sign in as a different user, simply click on the user control on the top right corner of the Todo screen", ar.UserInfo.DisplayableId));
                    await dialog.ShowAsync();
                    string cachedAuthority = App.AuthenticationContext.TokenCache.ReadItems().First().Authority;
                    App.AuthenticationContext = new AuthenticationContext(cachedAuthority);
                    // redirect the user to the actual app page
                    this.Frame.Navigate(typeof(TodoListPage));
                }
                else
                {
                    throw new HttpRequestException(string.Format("Something went wrong. Apparently the user {0} didn't work out. Try again", ar.UserInfo.DisplayableId));
                }
            }
            catch (Exception ex)
            {
                // failure. Notify the user, clean up the token cache and remain on this page
                MessageDialog dialog = new MessageDialog(ex.Message);
                await dialog.ShowAsync();

                if (ex is AdalException)
                {
                    App.AuthenticationContext.TokenCache.Clear();
                }
            }


        }
    }
}
