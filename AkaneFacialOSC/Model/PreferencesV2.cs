using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace AZW.FacialOSC.Model
{
    [Serializable]
    public class PreferencesV2
    {
        private const int CURRENT_VERSION = 2;

        public int version;
        public List<FaceDataPreferencesV2> faceDataPreferences;
        public string eyeTrackingType;
        public float maxAngle = 45f;
        public string language = "";

        const string PATH = @"AZW\FacialOSC";
        const string FILE = "preferences.json";

        public EyeTrackingType Tracker
        {
            get
            { 
                object parsed;
                if (Enum.TryParse(typeof(EyeTrackingType), eyeTrackingType, true, out parsed)) {
                    return (EyeTrackingType)parsed!;
                }
                else
                {
                    return EyeTrackingType.ViveSRanipal;
                }
            }
        }

        public PreferencesV2()
        {
            version = CURRENT_VERSION;
            faceDataPreferences = Enum.GetValues(typeof(FaceKey)).Cast<FaceKey>().Select(key => new FaceDataPreferencesV2(key)).ToList();
            eyeTrackingType = EyeTrackingType.ViveSRanipal.ToString();
        }

        private PreferencesV2(PreferencesV1 old)
        {
            version = old.version;
            if (old.version < 1)
            {
                old.faceDataPreferences = old.faceDataPreferences?.Select(p => {
                    p.centerValue = 0.5f;
                    return p;
                }).ToArray();
            }
            faceDataPreferences = (old.faceDataPreferences ?? Array.Empty<FaceDataPreferencesV1>()).Select(p => p.UpgradeToV2()).ToList();
            eyeTrackingType = EyeTrackingType.ViveSRanipal.ToString();
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
    }



    [Serializable]
    public class FaceDataPreferencesV2
    {
        public string key = "";
        public bool isSending = true;
        public double gain = 1;
        public bool isClipping = true;
        public string range = Range.Fixed.ToString();

        public FaceKey FaceKey { get { return (FaceKey)Enum.Parse(typeof(FaceKey), key); } }
        public Range CenterKey { get { return (Range)Enum.Parse(typeof(Range), range); } }

        public FaceDataPreferencesV2()
        {
        }

        public FaceDataPreferencesV2(string key)
        {
            this.key = key;
            var faceKey = FaceKey;
            range = (FaceKeyUtils.GetDataType(faceKey) == DataType.Gaze ? Range.ZeroCentered : FaceKeyUtils.IsBipolar(faceKey) ? Range.HalfCentered : Range.ZeroCentered).ToString();
        }
        public FaceDataPreferencesV2(FaceKey key)
        {
            this.key = key.ToString();
            var faceKey = FaceKey;
            range = (FaceKeyUtils.GetDataType(faceKey) == DataType.Gaze ? Range.ZeroCentered : FaceKeyUtils.IsBipolar(faceKey) ? Range.HalfCentered : Range.ZeroCentered).ToString();
        }
        public FaceDataPreferencesV2(string key, bool isSending, float gain, bool isClipping, string center)
        {
            this.key = key;
            this.isSending = isSending;
            this.gain = gain;
            this.isClipping = isClipping;
            this.range = center;
        }
    }
}
