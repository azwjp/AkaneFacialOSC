using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace AZW.FacialOSC
{
    [Serializable]
    [DataContract]
    public class Preferences
    {
        const int CURRENT_VERSION = 2;

        public int version;
        public FaceDataPreferences[] faceDataPreferences;
        public bool isDebug = false;
        public bool useEyeTracking = true;
        public bool useFacialTracking = true;
        public string eyeTrackingType;
        public float maxAngle = 45f;
        public string language = "";

        const string FOLDERID_LocalAppDataLow = "{A520A1A4-1780-4FF6-BD18-167343C5AF16}";
        const string PATH = @"AZW\FacialOSC";
        const string FILE = "preferences.json";


        public static Preferences Load()
        {
            var prefs = LoadV2();
            if (prefs == null)
            {
                prefs = LoadV1();
                prefs?.Save();
            }
            return prefs;
        }

        public bool Save()
        {
            var dir = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, PATH);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var path = Path.Combine(dir, FILE);
            var serializer = new DataContractJsonSerializer(typeof(Preferences));
            try
            {
                using (var fs = new FileStream(path, FileMode.Create))
                using (var sw = new StreamWriter(fs, Encoding.ASCII))
                using (var ms = new MemoryStream())
                {
                    serializer.WriteObject(ms, this);
                    sw.Write(ms);
                }
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public static Preferences Load(string path)
        {
            try
            {
                using (var fs = new FileStream(path, FileMode.Open))
                using (var sr = new StreamReader(fs, detectEncodingFromByteOrderMarks: true))
                {
                    var stream = sr.BaseStream;
                    sr.Peek(); // need to Peek() to get the preamble
                    stream.Position = sr.CurrentEncoding.Preamble.Length; // Unity uses UTF-8 with BOM
                    return new DataContractJsonSerializer(typeof(Preferences)).ReadObject(stream) as Preferences;
                }
            }
            catch
            {
                return null;
            }
        }

        public static Preferences LoadV1()
        {
            var oldPath = Path.Combine(GetKnownFolderPath(new Guid(FOLDERID_LocalAppDataLow)), PATH, FILE);
            return Load(oldPath);
        }

        public static Preferences LoadV2()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), PATH, FILE);
            return Load(path);
        }

        static string GetKnownFolderPath(Guid knownFolderId)
        {
            IntPtr pszPath = IntPtr.Zero;
            try
            {
                int hr = SHGetKnownFolderPath(knownFolderId, 0, IntPtr.Zero, out pszPath);
                if (hr >= 0)
                    return Marshal.PtrToStringAuto(pszPath);
                throw Marshal.GetExceptionForHR(hr);
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
    public class FaceDataPreferences
    {
        public string key;
        public bool isSending = true;
        public float gain = 1;
        public bool isClipping = true;
        public float centerValue = 0.5f;

        public FaceKey faceKey { get { return (FaceKey)Enum.Parse(typeof(FaceKey), key); } }
        public Center center
        {
            get
            {
                var def = FaceKeyUtils.DefaultValue(faceKey);
                if (0.5f - 0.0009765625f < def && def < 0.5f + 0.0009765625)
                {
                    return centerValue < float.Epsilon ? Center.Zero : Center.Half;
                }
                else
                {
                    return Center.Fixed;
                }
            }
            set
            {
                switch (value)
                {
                    case Center.Fixed:
                        centerValue = 0.5f;
                        return;
                    case Center.Zero:
                        centerValue = 0;
                        return;
                    case Center.Half:
                        centerValue = 0.5f;
                        return;
                    default:
                        throw new NotImplementedException();
                }
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
    }
}
