using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace AZW.FacialOSC.Model
{
    [Serializable]
    internal class PreferencesV1
    {
        public int version;
        public FaceDataPreferencesV1[]? faceDataPreferences;
        public bool isDebug = false;
        public bool useEyeTracking = true;
        public bool useFacialTracking = true;
        public string? eyeTrackingType;
        public float maxAngle = 45f;
        public string language = "";

        const string FOLDERID_LocalAppDataLow = "{A520A1A4-1780-4FF6-BD18-167343C5AF16}";
        const string PATH = @"AZW\FacialOSC";
        const string FILE = "preferences.json";

        public static PreferencesV1? Load()
        {
            var dir = GetKnownFolderPath(new Guid(FOLDERID_LocalAppDataLow));
            if (dir == null) return null;
            var oldPath = Path.Combine(dir, PATH, FILE);
            return Load(oldPath);
        }

        static PreferencesV1? Load(string path)
        {
            try
            {
                using (var fs = new FileStream(path, FileMode.Open))
                using (var sr = new StreamReader(fs, detectEncodingFromByteOrderMarks: true))
                {
                    var stream = sr.BaseStream;
                    sr.Peek(); // need to Peek() to get the preamble
                    stream.Position = sr.CurrentEncoding.Preamble.Length; // Unity uses UTF-8 with BOM
                    return new DataContractJsonSerializer(typeof(PreferencesV1)).ReadObject(stream) as PreferencesV1;
                }
            }
            catch
            {
                return null;
            }
        }

        static string? GetKnownFolderPath(Guid knownFolderId)
        {
            IntPtr pszPath = IntPtr.Zero;
            try
            {
                int hr = SHGetKnownFolderPath(knownFolderId, 0, IntPtr.Zero, out pszPath);
                if (hr >= 0)
                    return Marshal.PtrToStringAuto(pszPath);
                throw Marshal.GetExceptionForHR(hr) ?? new Exception();
            }
            finally
            {
                if (pszPath != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(pszPath);
            }
        }

        [DllImport("shell32.dll")]
        static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);
    }

    [Serializable]
    public class FaceDataPreferencesV1
    {
        public string key;
        public bool isSending = true;
        public float gain = 1;
        public bool isClipping = true;
        public float centerValue = 0.5f;

        public FaceKey faceKey { get { return (FaceKey)Enum.Parse(typeof(FaceKey), key); } }
        public Range center
        {
            get
            {
                var def = FaceKeyUtils.DefaultValue(faceKey);
                if (0.5f - 0.0009765625f < def && def < 0.5f + 0.0009765625)
                {
                    return centerValue < float.Epsilon ? Range.ZeroCentered : Range.HalfCentered;
                }
                else
                {
                    return Range.Fixed;
                }
            }
            set
            {
                switch (value)
                {
                    case Range.Fixed:
                        centerValue = 0.5f;
                        return;
                    case Range.ZeroCentered:
                        centerValue = 0;
                        return;
                    case Range.HalfCentered:
                        centerValue = 0.5f;
                        return;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public FaceDataPreferencesV1(string key)
        {
            this.key = key;
        }
        public FaceDataPreferencesV1(FaceKey key)
        {
            this.key = key.ToString();
        }
        public FaceDataPreferencesV1(string key, bool isSending, float gain, bool isClipping, float centerValue)
        {
            this.key = key;
            this.isSending = isSending;
            this.gain = gain;
            this.isClipping = isClipping;
            this.centerValue = centerValue;
        }
        public FaceDataPreferencesV2 UpgradeToV2()
        {
            var v2 = new FaceDataPreferencesV2(key);
            v2.isSending = isSending;
            v2.gain = gain;
            v2.isClipping = isClipping;
            var defaultValue = FaceKeyUtils.DefaultValue(faceKey);

            if (0.5f - 0.0009765625f < defaultValue && defaultValue < 0.5f + 0.0009765625)
            {
                v2.range = (centerValue < float.Epsilon ? Range.ZeroCentered : Range.HalfCentered).ToString();
            }
            else
            {
                v2.range = Range.Fixed.ToString();
            }

            return v2;
        }
    }
}
