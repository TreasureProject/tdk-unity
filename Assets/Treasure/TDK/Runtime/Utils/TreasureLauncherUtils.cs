using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Treasure
{
    public class TreasureLauncherUtils
    {
        private static bool hasParsedCommandLineArgs = false;

        private static string tdkAuthToken = null;
        private static string serverPort = null;
        private static string tdkAuthCookie = null;
        private static string tdkAuthProvider = null;

        public static string GetLauncherAuthToken()
        {
            ParseArgs();
            return tdkAuthToken;
        }

        public static string GetLauncherServerUrl()
        {
            ParseArgs();
            return $"http://localhost:{serverPort}";
        }

        public static string GetWalletAddressFromJwt()
        {
            var content = GetLauncherAuthToken().Split('.')[1];
            var jsonPayload = Decode(content);
            return JsonConvert.DeserializeObject<JToken>(jsonPayload)["ctx"]["address"].ToString();
        }

        internal static string GetLauncherAuthCookie()
        {
            ParseArgs();
            return tdkAuthCookie;
        }

        public static string GetEmailAddressFromAuthCookie()
        {
            var content = GetLauncherAuthCookie().Split('.')[1];
            var jsonPayload = Decode(content);
            return JsonConvert.DeserializeObject<JToken>(jsonPayload)["storedToken"]["authDetails"]["email"].ToString();
        }

        internal static Thirdweb.AuthProvider? GetLauncherAuthProvider()
        {
            ParseArgs();
            return ParseAuthProviderString(tdkAuthProvider);
        }

        public static Thirdweb.AuthProvider? ParseAuthProviderString(string authProvider)
        {
            return authProvider?.ToLower() switch
            {
                "google" => Thirdweb.AuthProvider.Google,
                "google_v2" => Thirdweb.AuthProvider.Google,
                "discord" => Thirdweb.AuthProvider.Discord,
                "x" => Thirdweb.AuthProvider.X,
                "apple" => Thirdweb.AuthProvider.Apple,
                "siwe" => Thirdweb.AuthProvider.Siwe,
                "email" => Thirdweb.AuthProvider.Default,
                _ => Thirdweb.AuthProvider.Siwe,
            };
        }

        private static void ParseArgs()
        {
            if (hasParsedCommandLineArgs)
            {
                return;
            }
            var args = Environment.GetCommandLineArgs();
            tdkAuthToken = TryParseArg(args, "--tdk-auth-token", defaultValue: null);
            serverPort = TryParseArg(args, "--server-port", defaultValue: "16001");
            tdkAuthCookie = TryParseArg(args, "--tdk-auth-cookie", defaultValue: null);
            tdkAuthProvider = TryParseArg(args, "--tdk-auth-provider", defaultValue: null);
            hasParsedCommandLineArgs = true;
        }

        private static string TryParseArg(string[] args, string argName, string defaultValue)
        {
            foreach (var arg in args)
            {
                if (arg.StartsWith(argName))
                {
                    var splitArg = arg.Split("=");
                    if (splitArg.Length == 2)
                    {
                        return splitArg[1];
                    }
                }
            }
            return defaultValue;
        }

        private static string Decode(string input)
        {
            var output = input;
            output = output.Replace('-', '+'); // 62nd char of encoding
            output = output.Replace('_', '/'); // 63rd char of encoding
            switch (output.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: output += "=="; break; // Two pad chars
                case 3: output += "="; break; // One pad char
                default: throw new ArgumentOutOfRangeException("input", "Illegal base64url string!");
            }
            var converted = Convert.FromBase64String(output); // Standard base64 decoder
            var decoded = System.Text.Encoding.UTF8.GetString(converted);
            return decoded;
        }
    }
}
