using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Azw.FacialOsc.Model
{
    internal record class VersionCheck
    {
        internal static readonly Uri Url = new Uri(@"https://version.azw.jp/Azw.FacialOsc");

        public readonly string Latest;

        private VersionCheck(string rawVersion)
        {
            Latest = rawVersion;
        }

        public static async Task<VersionCheck> CheckAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(15);
                var res = await client.GetStringAsync(Url).ConfigureAwait(false);
                return new VersionCheck(res);
            }
            catch
            {
            }
            return new VersionCheck("");
        }

        private static nint[]? VersionNumbners (string version)
        {
            if (string.IsNullOrEmpty(version)) return null;
            try
            {
                var re = new Regex(@"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)");
                var match = re.Match(version).Groups;
                return new nint[] { nint.Parse(match["major"].Value), nint.Parse(match["minor"].Value), nint.Parse(match["patch"].Value) };
            }
            catch { 
                return null;
            }
        }

        public bool UpdateExist()
        {
            try
            {
                var re = new Regex(@"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)");
                var current = VersionNumbners(CurrentVersion());
                var latest = VersionNumbners(Latest);
                if (current == null || latest == null) return false;

                nint length = Math.Max(current.Length, latest.Length);
                for (nint i = 0; i < length; i++)
                {
                    var c = i < current.Length ? current[i] : 0;
                    var l = i < latest.Length ? latest[i] : 0;

                    if (c < l) return true;
                    if (c > l) return false;
                }
                return false;
            }
            catch
            {
                return false;
            }


        }
        public static string CurrentVersion()
        {
            var current = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return current != null ? current.InformationalVersion : "";
        }
    }
}
