using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Azw.FacialOsc.Tracking
{
    class DroolonPi1EyeTracker : EyeTracker
    {
        static DroolonPi1EyeTracker? instance = null;
        private Pimax.EyeTracking.EyeTracker pimaxAseeEye = new Pimax.EyeTracking.EyeTracker();

        private long lastUpdate = 0;
        private bool started = false;

        Vector2 pimaxAseeCenterRight = Vector2.Zero;
        Vector2 pimaxAseeCenterLeft = Vector2.Zero;

        public DroolonPi1EyeTracker() { }

        public static DroolonPi1EyeTracker Instance
        {
            get
            {
                if (instance == null) return instance = new DroolonPi1EyeTracker();
                else return instance;
            }
        }

        protected override bool CheckWorking()
        {
            return started;
        }

        public override Task<bool> StartProcess()
        {
            return Task.Run(() => started = pimaxAseeEye.Start());
        }

        public override Task StopProcess()
        {
            return Task.Run(() => {
                pimaxAseeEye.Stop();
                started = false;
            });
        }

        public override bool UpdateData()
        {
            if (pimaxAseeEye.LastUpdateTick <= lastUpdate)
            {
                return false;
            }
            else
            {
                lastUpdate = pimaxAseeEye.LastUpdateTick;
                return true;
            }
        }

        public override IDictionary<FaceKey, float> GetData()
        {
            var left = pimaxAseeEye.LeftEye;
            var right = pimaxAseeEye.RightEye;

            var leftRot = left.Expression.PupilCenter - pimaxAseeCenterLeft + Vector.Half2;
            var rightRot = right.Expression.PupilCenter - pimaxAseeCenterRight + Vector.Half2;

            var leftExpression = left.Expression;
            var rightExpression = right.Expression;

            return new Dictionary<FaceKey, float>() {
                { FaceKey.Gaze_Left_Horizontal , leftRot.X                                    },
                { FaceKey.Gaze_Right_Horizontal, rightRot.X                                   },
                { FaceKey.Gaze_Left_Vertical   , leftRot.Y                                    },
                { FaceKey.Gaze_Right_Vertical  , rightRot.Y                                   },
                { FaceKey.Eye_Left_Up          , left.UpperEyelid                             },
                { FaceKey.Eye_Right_Up         , right.UpperEyelid                            },
                { FaceKey.Eye_Left_Down        , leftRot.Y < 0 ? -leftRot.Y : 0               },
                { FaceKey.Eye_Right_Down       , rightRot.Y < 0 ? -rightRot.Y : 0             },
                { FaceKey.Eye_Left_Left        , Math.Max(-leftExpression.PupilCenter.X, 0f)  },
                { FaceKey.Eye_Right_Left       , Math.Max(-rightExpression.PupilCenter.X, 0f) },
                { FaceKey.Eye_Left_Right       , Math.Max(leftExpression.PupilCenter.X, 0f)   },
                { FaceKey.Eye_Right_Right      , Math.Max(rightExpression.PupilCenter.X, 0f)  },
                { FaceKey.Eye_Left_Blink       , 1 - leftExpression.Openness                  },
                { FaceKey.Eye_Right_Blink      , 1 - rightExpression.Openness                 },
                { FaceKey.Eye_Left_Wide        , left.UpperEyelid                             }, // The same with Eye_Left_Up
                { FaceKey.Eye_Right_Wide       , right.UpperEyelid                            }, // The same with Eye_Right_Up
                { FaceKey.Eye_Left_Frown       , 0                                            },
                { FaceKey.Eye_Right_Frown      , 0                                            },
                { FaceKey.Eye_Left_Squeeze     , 0                                            },
                { FaceKey.Eye_Right_Squeeze    , 0                                            },
            };
        }
    }
}
