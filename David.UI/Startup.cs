﻿using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(David.UI.Startup))]
namespace David.UI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
