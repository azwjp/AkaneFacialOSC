using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using Azw.FacialOsc.Common;

namespace Azw.FacialOsc.Model
{
    [Serializable]
    public class PreferencesV1
    {
        public int version;
        public FaceDataPreferences[]? faceDataPreferences;
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
        [Serializable]
        public class FaceDataPreferences
        {
            public string key;
            public bool isSending = true;
            public float gain = 1;
            public bool isClipping = true;
            public float centerValue = 0.5f;

            public FaceKey faceKey { get { return (FaceKey)Enum.Parse(typeof(FaceKey), key); } }
            public ValueRange center
            {
                get
                {
                    return centerValue < float.Epsilon ? ValueRange.MinusOneToOne : ValueRange.ZeroToOne;
                }
                set
                {
                    centerValue = value switch
                    {
                        ValueRange.MinusOneToOne => centerValue = 0,
                        ValueRange.ZeroToOne => centerValue = 0.5f,
                        _ => throw new UnexpectedEnumValueException(value),
                    };
                }
            }

            public FaceDataPreferences(string key)
            {
                this.key = key;
            }
            public FaceDataPreferences(FaceKey key)
            {
                this.key = key.ToString();
            }
            public FaceDataPreferences(string key, bool isSending, float gain, bool isClipping, float centerValue)
            {
                this.key = key;
                this.isSending = isSending;
                this.gain = gain;
                this.isClipping = isClipping;
                this.centerValue = centerValue;
            }
            public PreferencesV2.FaceDataPreferences UpgradeToV2()
            {
                var v2 = new PreferencesV2.FaceDataPreferences(key);
                v2.isSending = isSending;
                v2.gain = gain;
                v2.isClipping = isClipping;
                v2.range = (centerValue < float.Epsilon ? ValueRange.MinusOneToOne : ValueRange.ZeroToOne).ToString();

                return v2;
            }
        }
    }


}
