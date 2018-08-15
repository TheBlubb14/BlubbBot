﻿using System;

namespace BlubbBot.Autorization
{
    public class AccessToken
    {
        public string access_token { get; set; }

        public string refresh_token { get; set; }

        public DateTime expires_at { get; set; }

        public int expires_in { get; set; }

        public string scope { get; set; }

        public string token_type { get; set; }
    }
}
