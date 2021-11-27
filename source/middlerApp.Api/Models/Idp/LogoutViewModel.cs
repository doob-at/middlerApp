// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace middlerApp.Api.Models.Idp
{
    public class LogoutViewModel
    {
        public string ClientName { get; set; }
        public bool ShowLogoutPrompt { get; set; } = true;
        public string PostLogoutRedirectUri { get; set; }
    }
}
