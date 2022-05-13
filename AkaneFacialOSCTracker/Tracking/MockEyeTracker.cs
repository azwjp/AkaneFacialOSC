using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using ViveSR;

namespace Azw.FacialOsc.Tracking
{
    internal class MockEyeTracker : Tracker
    {
        static MockEyeTracker? instance = null;

        public Error deviceStatus { get; private set; } = Error.NOT_INITIAL;

        bool running = false;
        nuint step = 0;

        public MockEyeTracker() { }

        public static MockEyeTracker Instance
        {
            get
            {
                if (instance == null) return instance = new MockEyeTracker();
                else return instance;
            }
        }

        protected override bool CheckWorking()
        {
            return running;
        }

        public override void StartProcess()
        {
            running = true;
        }

        public override void StopProcess()
        {
            running = false;
        }

        public override bool UpdateData()
        {
            if (step == nuint.MaxValue) step = 0;
            else step++;

            return true;
        }

        public override IDictionary<FaceKey, float> GetData()
        {
            var radian = step / 128f;
            var (sin, cos) = MathF.SinCos(radian);
            var sinSq = sin * sin;

            // Get the current value
            var gazeDirection = Vector3.Normalize(new Vector3(cos, sin, 1));

            // Gaze
            var rot = new Vector2(
                    (gazeDirection.X > 0 ? -1 : 1) * MathF.Acos(Vector2.Dot(Vector2.Normalize(new Vector2(gazeDirection.X, gazeDirection.Z)), Vector.Up2)),
                    (gazeDirection.Y > 0 ? 1 : -1) * MathF.Acos(Vector2.Dot(Vector2.Normalize(new Vector2(gazeDirection.Y, gazeDirection.Z)), Vector.Up2))
            );

            return new Dictionary<FaceKey, float>() {
                { FaceKey.Gaze_Left_Horizontal , rot.X                          },
                { FaceKey.Gaze_Right_Horizontal, rot.X                          },
                { FaceKey.Gaze_Left_Vertical   , rot.Y                          },
                { FaceKey.Gaze_Right_Vertical  , rot.Y                          },
                { FaceKey.Eye_Left_Up          , sinSq                        },
                { FaceKey.Eye_Right_Up         , sinSq                        },
                { FaceKey.Eye_Left_Down        , rot.Y < 0 ? -rot.Y : 0         },
                { FaceKey.Eye_Right_Down       , rot.Y < 0 ? -rot.Y : 0         },
                { FaceKey.Eye_Left_Left        , Math.Max(-gazeDirection.X, 0f) },
                { FaceKey.Eye_Right_Left       , Math.Max(-gazeDirection.X, 0f) },
                { FaceKey.Eye_Left_Right       , Math.Max(gazeDirection.X, 0f)  },
                { FaceKey.Eye_Right_Right      , Math.Max(gazeDirection.X, 0f)  },
                { FaceKey.Eye_Left_Blink       , sinSq                        },
                { FaceKey.Eye_Right_Blink      , sinSq                        },
                { FaceKey.Eye_Left_Wide        , sinSq                        }, // The same with Eye_Left_Up
                { FaceKey.Eye_Right_Wide       , sinSq                        }, // The same with Eye_Right_Up
                { FaceKey.Eye_Left_Frown       , sinSq                        },
                { FaceKey.Eye_Right_Frown      , sinSq                        },
                { FaceKey.Eye_Left_Squeeze     , sinSq                        },
                { FaceKey.Eye_Right_Squeeze    , sinSq                        },
            };
        }

    }
}
