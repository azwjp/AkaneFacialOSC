using System;
using System.Threading;
using System.Threading.Tasks;
using ViveSR;
using ViveSR.anipal.Eye;

namespace AZW.FacialOSC.Tracking
{
    internal abstract class EyeTracker : Tracker<EyeData_v2>
    {
        static EyeTracker? instance = null;

        public static async Task<EyeTracker> Instance<T>() where T : EyeTracker
        {
            if (instance == null)
            {
                return instance = Activator.CreateInstance<T>();
            }
            else if (instance.GetType() == typeof(T))
            {
                return (T)instance;
            }
            else
            {
                await instance.Stop();
                var handler = instance.updatedHandler;
                instance = Activator.CreateInstance<T>();
                instance.updatedHandler += handler;
                return instance;
            }
        }
    }
}
