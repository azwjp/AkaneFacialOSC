using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Azw.FacialOsc.View;

namespace Azw.FacialOsc.Model
{
    [Serializable]
    public record class PreferencesV2
    {
        private const int CURRENT_VERSION = 2;

        public int version;
        public TrackingPreference trackingPreference;
        public ApplicationPreference applicationPreference;

        const string PATH = @"AZW\FacialOSC";
        const string FILE = "preferences.json";



        public PreferencesV2()
        {
            version = CURRENT_VERSION;
            trackingPreference = new TrackingPreference();
            applicationPreference = new ApplicationPreference();
        }

        private PreferencesV2(PreferencesV1 old)
        {
            version = old.version;
            if (old.version < 1)
            {
                old.faceDataPreferences = old.faceDataPreferences?.Select(p =>
                {
                    p.centerValue = 0.5f;
                    return p;
                }).ToArray();
            }
            trackingPreference = new TrackingPreference(old);
            applicationPreference = new ApplicationPreference(old);
        }


        public static PreferencesV2 Load()
        {
            var prefs = LoadV2();

            if (prefs == null)
            {
                var v1 = PreferencesV1.Load();
                if (v1 != null)
                {
                    prefs = new PreferencesV2(v1);
                    prefs?.Save();
                }
            }

            if (prefs == null)
            {
                prefs = new PreferencesV2();
            }

            return prefs;
        }

        public bool Save()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), PATH);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var path = Path.Combine(dir, FILE);
            var serializer = new DataContractJsonSerializer(typeof(PreferencesV2));
            try
            {
                using (var fs = new FileStream(path, FileMode.Create))
                using (var sw = new StreamWriter(fs, Encoding.ASCII))
                using (var ms = new MemoryStream())
                {
                    serializer.WriteObject(sw.BaseStream, this);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        static PreferencesV2? Load(string path)
        {
            try
            {
                using (var fs = new FileStream(path, FileMode.Open))
                using (var sr = new StreamReader(fs, detectEncodingFromByteOrderMarks: true))
                {
                    var stream = sr.BaseStream;
                    return new DataContractJsonSerializer(typeof(PreferencesV2)).ReadObject(stream) as PreferencesV2;
                }
            }
            catch
            {
                return null;
            }
        }


        public static PreferencesV2? LoadV2()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), PATH, FILE);
            return Load(path);
        }

        [Serializable]
        public class ApplicationPreference
        {
            public string language = "";
            public string theme = "";
            public AkaneThemes.Themes Theme
            {
                get
                {
                    AkaneThemes.Themes parsed;
                    return Enum.TryParse(theme, true, out parsed) ? parsed : AkaneThemes.Themes.AkaneTheme;
                }
            }

            public ApplicationPreference() { }

            public ApplicationPreference(PreferencesV1 old)
            {
                language = old.language;
            }
        }

        [Serializable]
        public class TrackingPreference
        {
            public List<FaceDataPreferences> faceDataPreferences;
            public string eyeTrackingType = "";
            public string lipTrackingType = "";
            public float maxAngle = 45f;
            public bool eyeAutoFps = true;
            public bool lipAutoFps = true;
            public double eyeFps = 60;
            public double lipFps = 60;

            public EyeTrackingType EyeTracker
            {
                get
                {
                    EyeTrackingType parsed;
                    return Enum.TryParse(lipTrackingType, true, out parsed) ? parsed : EyeTrackingType.ViveSRanipal;
                }
            }
            public LipTrackingType LipTracker
            {
                get
                {
                    LipTrackingType parsed;
                    return Enum.TryParse(lipTrackingType, true, out parsed) ? parsed : LipTrackingType.ViveSRanipal;
                }
            }
            public TrackingPreference()
            {
                faceDataPreferences = Enum.GetValues(typeof(FaceKey)).Cast<FaceKey>().Select(key => new FaceDataPreferences(key)).ToList();
            }
            public TrackingPreference(PreferencesV1 old)
            {
                faceDataPreferences = (old.faceDataPreferences ?? Array.Empty<PreferencesV1.FaceDataPreferences>()).Select(p => p.UpgradeToV2()).ToList();
            }
        }
        [Serializable]
        public class FaceDataPreferences
        {
            public string key = "";
            public bool isSending = true;
            public float gain = 1;
            public bool isClipping = true;
            public string range = ValueRange.Fixed.ToString();

            public FaceKey FaceKey { get { return (FaceKey)Enum.Parse(typeof(FaceKey), key); } }
            public ValueRange CenterKey { get { return (ValueRange)Enum.Parse(typeof(ValueRange), range); } }

            public FaceDataPreferences()
            {
            }

            public FaceDataPreferences(string key)
            {
                this.key = key;
                var faceKey = FaceKey;
                range = (FaceKeyUtils.GetDataType(faceKey) == DataType.Gaze ? ValueRange.ZeroCentered : FaceKeyUtils.IsBipolar(faceKey) ? ValueRange.HalfCentered : ValueRange.ZeroCentered).ToString();
            }
            public FaceDataPreferences(FaceKey key)
            {
                this.key = key.ToString();
                var faceKey = FaceKey;
                range = (FaceKeyUtils.GetDataType(faceKey) == DataType.Gaze ? ValueRange.ZeroCentered : FaceKeyUtils.IsBipolar(faceKey) ? ValueRange.HalfCentered : ValueRange.ZeroCentered).ToString();
            }
            public FaceDataPreferences(string key, bool isSending, float gain, bool isClipping, string range)
            {
                this.key = key;
                this.isSending = isSending;
                this.gain = gain;
                this.isClipping = isClipping;
                this.range = range;
            }
        }
    }
}
