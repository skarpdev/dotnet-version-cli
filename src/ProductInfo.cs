using System.Reflection;

namespace Skarp.Version.Cli
{
    public static class ProductInfo
    {
        /// <summary>
        /// The name of the product
        /// </summary>
        public const string Name = "dotnet-version-cli";

        /// <summary>
        /// The version of the running product
        /// </summary>
        public static readonly string Version = Assembly.GetEntryAssembly().GetName().Version.ToString();
    }
}
