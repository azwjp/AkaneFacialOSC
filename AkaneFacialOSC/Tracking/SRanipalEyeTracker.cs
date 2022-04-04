using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using ViveSR;
using ViveSR.anipal;
using ViveSR.anipal.Eye;

namespace Azw.FacialOsc.Tracking
{
    internal class SRanipalEyeTracker : EyeTracker
    {
        static SRanipalEyeTracker? instance = null;

        public Error deviceStatus { get; private set; } = Error.NOT_INITIAL;
        int lastUpdate = -1;
        EyeData_v2 eyeData = new EyeData_v2();

        public SRanipalEyeTracker() { }

        public static SRanipalEyeTracker Instance
        {
            get
            {
                if (instance == null) return instance = new SRanipalEyeTracker();
                else return instance;
            }
        }

        protected override bool CheckWorking()
        {
            return deviceStatus == Error.WORK;
        }

        public override Task<Error> StartProcess()
        {
            return Task.Run(() => deviceStatus = SRanipal_API.Initial(SRanipal_Eye.ANIPAL_TYPE_EYE_V2, IntPtr.Zero));
        }

        public override Task<Error> StopProcess()
        {
            return Task.Run(() => deviceStatus = SRanipal_API.Release(SRanipal_Eye.ANIPAL_TYPE_EYE_V2));
        }

        public override bool UpdateData()
        {
            var data = new EyeData_v2();
            deviceStatus = SRanipal_Eye.GetEyeData_v2(ref data);

            if (lastUpdate == data.frame_sequence) return false;
            else lastUpdate = data.frame_sequence;

            if (CheckWorking())
            {
                eyeData = data;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override IDictionary<FaceKey, float> GetData()
        {
            // Get the current value
            var leftEye = eyeData.verbose_data.left;
            var rightEye = eyeData.verbose_data.right;
            var leftExpression = eyeData.expression_data.left;
            var rightExpression = eyeData.expression_data.right;

            // Gaze
            // Eye balls usually don't rotate by z-axis
            // If invalid data such as closed eyes, return the centre
            // The data from tracler is right-handed coordinate system: the x-value is positive when looking to the left, the y-value is positive when looking up
            var leftRot = leftEye.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY)
                ? new Vector2(
                    (leftEye.gaze_direction_normalized.X > 0 ? -1 : 1) * MathF.Acos(Vector2.Dot(Vector2.Normalize(new Vector2(leftEye.gaze_direction_normalized.X, leftEye.gaze_direction_normalized.Z)), Vector.Up2)),
                    (leftEye.gaze_direction_normalized.Y > 0 ? 1 : -1) * MathF.Acos(Vector2.Dot(Vector2.Normalize(new Vector2(leftEye.gaze_direction_normalized.Y, leftEye.gaze_direction_normalized.Z)), Vector.Up2))
                )
                : Vector2.Zero;
            var rightRot = rightEye.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY)
                ? new Vector2(
                    (rightEye.gaze_direction_normalized.X > 0 ? -1 : 1) * MathF.Acos(Vector2.Dot(Vector2.Normalize(new Vector2(rightEye.gaze_direction_normalized.X, rightEye.gaze_direction_normalized.Z)), Vector.Up2)),
                    (rightEye.gaze_direction_normalized.Y > 0 ? 1 : -1) * MathF.Acos(Vector2.Dot(Vector2.Normalize(new Vector2(rightEye.gaze_direction_normalized.Y, rightEye.gaze_direction_normalized.Z)), Vector.Up2))
                )
                : Vector2.Zero;

            // Pupil
            // the same value with SRanipal_Eye_v2.GetPupilPosition
            // If invalid data such as closed eyes, return the centre
            // pupil_position_in_sensor_area: the left top corner is (0, 0), the right down corner is (1, 1)
            var leftPupil = leftEye.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY)
                ? new Vector2(
                    leftEye.pupil_position_in_sensor_area.X * 2 - 1,
                    leftEye.pupil_position_in_sensor_area.Y * -2 + 1
                )
                : Vector2.Zero;
            var rightPupil = rightEye.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY)
                ? new Vector2(
                    rightEye.pupil_position_in_sensor_area.X * 2 - 1,
                    rightEye.pupil_position_in_sensor_area.Y * -2 + 1
                )
                : Vector2.Zero;

            // Openness
            // If invalid data such as closed eyes, return 0 which means a closed eye
            var leftOpenness = leftEye.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY) ? leftEye.eye_openness : 0;
            var rightOpenness = leftEye.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY) ? rightEye.eye_openness : 0;

            return new Dictionary<FaceKey, float>() {
                { FaceKey.Gaze_Left_Horizontal , leftRot.X                        },
                { FaceKey.Gaze_Right_Horizontal, rightRot.X                       },
                { FaceKey.Gaze_Left_Vertical   , leftRot.Y                        },
                { FaceKey.Gaze_Right_Vertical  , rightRot.Y                       },
                { FaceKey.Eye_Left_Up          , leftExpression.eye_wide          },
                { FaceKey.Eye_Right_Up         , rightExpression.eye_wide         },
                { FaceKey.Eye_Left_Down        , leftRot.Y < 0 ? -leftRot.Y : 0   },
                { FaceKey.Eye_Right_Down       , rightRot.Y < 0 ? -rightRot.Y : 0 },
                { FaceKey.Eye_Left_Left        , Math.Max(-leftPupil.X, 0f)       },
                { FaceKey.Eye_Right_Left       , Math.Max(-rightPupil.X, 0f)      },
                { FaceKey.Eye_Left_Right       , Math.Max(leftPupil.X, 0f)        },
                { FaceKey.Eye_Right_Right      , Math.Max(rightPupil.X, 0f)       },
                { FaceKey.Eye_Left_Blink       , 1 - leftOpenness                 },
                { FaceKey.Eye_Right_Blink      , 1 - rightOpenness                },
                { FaceKey.Eye_Left_Wide        , leftExpression.eye_wide          }, // The same with Eye_Left_Up
                { FaceKey.Eye_Right_Wide       , rightExpression.eye_wide         }, // The same with Eye_Right_Up
                { FaceKey.Eye_Left_Frown       , leftExpression.eye_frown         },
                { FaceKey.Eye_Right_Frown      , rightExpression.eye_frown        },
                { FaceKey.Eye_Left_Squeeze     , leftExpression.eye_squeeze       },
                { FaceKey.Eye_Right_Squeeze    , rightExpression.eye_squeeze      },
            };
        }
        public EyeData_v2 GetRawData()
        {
            return eyeData;
        }

    }
}
