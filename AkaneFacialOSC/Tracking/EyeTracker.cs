using System;
using System.Threading.Tasks;

namespace Azw.FacialOsc.Tracking
{
    internal abstract class EyeTracker : Tracker
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

                var old = instance;
                instance = Activator.CreateInstance<T>();
                instance.updatedHandler = old.updatedHandler;
                instance.checkedHandler = old.checkedHandler;
                instance.statusChangedHandler = old.statusChangedHandler;
                instance.targetInterval = old.targetInterval;
                instance.IsAutoFpsEnabled = old.IsAutoFpsEnabled;

                old.updatedHandler = null;
                old.statusChangedHandler = null;

                return instance;
            }
        }
    }
}
