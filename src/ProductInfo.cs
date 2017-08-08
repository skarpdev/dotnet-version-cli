using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Skarp.Version.Cli
{
    public static class ProductInfo
    {
        /// <summary>
        /// The name of the product
        /// </summary>
        public static string Name = "dotnet-version-cli";

        /// <summary>
        /// The version of the running product
        /// </summary>
        public static string Version = Assembly.GetEntryAssembly().GetName().Version.ToString();
    }
}
