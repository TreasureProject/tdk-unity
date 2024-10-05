using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Treasure
{
    public class TreasureLauncherUtils
    {
        private static bool hasParsedCommandLineArgs = false;
        
        private static string tdkAuthToken = null;

        public static string GetLauncherAuthToken()
        {
            ParseArgs();
            return tdkAuthToken;
        }

        public static string GetWalletAddressFromJwt()
        {
            // TODO check token expiration date?
            var content = GetLauncherAuthToken().Split('.')[1];
            var jsonPayload = Decode(content);
            return JsonConvert.DeserializeObject<JToken>(jsonPayload)["ctx"]["smartAccountAddress"].ToString();
        }

        private static void ParseArgs()
        {
            if (hasParsedCommandLineArgs)
            {
                return;
            }
            var args = Environment.GetCommandLineArgs();
            foreach (var arg in args)
            {
                if (arg.StartsWith("--tdk-auth-token"))
                {
                    var splitArg = arg.Split("=");
                    if (splitArg.Length == 2)
                    {
                        tdkAuthToken = splitArg[1];
                    }
                }
            }
            hasParsedCommandLineArgs = true;
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
