﻿using System;
using System.Threading.Tasks;
using Android.Content;
using MobileCenterDemoApp.Droid.Dependencies;
using MobileCenterDemoApp.Interfaces;
using MobileCenterDemoApp.Models;
using MobileCenterDemoApp.Services;
using Xamarin.Auth;
using Xamarin.Forms;

[assembly: Dependency(typeof(TwitterLoginAndroid))]
namespace MobileCenterDemoApp.Droid.Dependencies
{
    public class TwitterLoginAndroid : ITwitter
    {

        private readonly OAuth1Authenticator _oAuth1;
        private readonly Intent _authUi;
        private bool _isComplite;

        public TwitterLoginAndroid()
        {
            _oAuth1 = Helpers.SocialNetworServices.TwitterAuth;
            _authUi = _oAuth1.GetUI(MainActivity.Activity);
            
        }
        
        public async Task<SocialAccount> Login()
        {
            MainActivity.Activity.StartActivity(_authUi);
            SocialAccount account = null;
            _oAuth1.Completed += async (sender, args) =>
            {
                if (!args.IsAuthenticated)
                {
                    _isComplite = true;
                    return;
                }

                account = new SocialAccount
                {
                    UserId = args.Account.Properties["user_id"],
                    UserName = args.Account.Properties["screen_name"]
                };

                var request = new OAuth1Request("GET",
                    new Uri("https://api.twitter.com/1.1/account/verify_credentials.json"),
                    null, args.Account);
                var response = await request.GetResponseAsync();
                
                var jo = Newtonsoft.Json.Linq.JObject.Parse(response.GetResponseText());
                var uri = (string) jo["profile_image_url"];
                account.ImageSource = ImageSource.FromUri(new Uri(uri));

                _isComplite = true;
                DataStore.OAuth1 = _oAuth1;
            };

            return await Task.Run(() =>
            {
                while (!_isComplite)
                    Task.Delay(100);

                return account;
            });
        }
    }
}