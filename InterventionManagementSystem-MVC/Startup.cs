﻿using Microsoft.Owin;
using Owin;
using IMSDBLayer;

[assembly: OwinStartupAttribute(typeof(InterventionManagementSystem_MVC.Startup))]
namespace InterventionManagementSystem_MVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            //run setup scripts
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            Setup DBsetup = new Setup(connstring);

        }
    }
}
