using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCloudShared.Shared
{
    public interface IInternetState
    {
        NetworkState State { get; set; }
        AuthState AuthState { get; set; }


        void ShowNoInternetMessage();
        void OnShowNoInternetMessageSuccess();
        void LogoffRedirectToLogin();
    }
}