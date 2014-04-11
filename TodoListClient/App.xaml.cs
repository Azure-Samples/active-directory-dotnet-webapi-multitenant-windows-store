using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;



namespace TodoListClient
{

    sealed partial class App : Application
    {
       
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        // The following Azure AD properties are used by multiple pages in the app
        
        // Properties of the web API invoked
        public const string ResourceID = "https://developertenant.onmicrosoft.com/TodoListServiceMT";        
        public const string APIHostname = "https://localhost:44301";
        public const string APIOnboardPath = "/API/SignUp/Onboard";
        public const string APITodoListPath = "/API/todolist";

        // Properties of the native client app
        public const string ClientID = "94da0930-763f-45c7-8d26-04d5938baab2";
        public static Uri ReturnUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri();

        // Properties used for communicating with the Windows Azure AD tenant of choice
        public const string CommonAuthority = "https://login.windows.net/common";
        public static AuthenticationContext AuthenticationContext { get; set; }

          
        // At start time we verify whether we already have a cached token (hence we can already use the app)
        // or if we need to drive the user through the sign up/sign in experience
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                // Set the default language
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // Initialize the AuthenticationContext with the common (tenantless) endpoint
                App.AuthenticationContext = new AuthenticationContext(App.CommonAuthority);
                // if we aready have tokens in the cache
                if (App.AuthenticationContext.TokenCacheStore.Count > 0)
                {
                    // re-bind the AuthenticationContext to the authority that sourced the token in the cache
                    // this is needed for the cache to work when asking a token from that authority
                    // (the common endpoint never triggers cache hits)
                    string cachedAuthority = App.AuthenticationContext.TokenCacheStore.First().Key.Authority;
                    App.AuthenticationContext = new AuthenticationContext(cachedAuthority);
                    // navigate directly to the main app page
                    rootFrame.Navigate(typeof(TodoListPage), e.Arguments);
                }
                else 
                {
                    // no previous tokens. Navigate to the welcome page
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }                
            }

            

            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
