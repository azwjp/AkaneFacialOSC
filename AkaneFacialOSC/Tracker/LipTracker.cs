using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ViveSR;
using ViveSR.anipal.Lip;

namespace AZW.FacialOSC.Tracker
{
    internal abstract class LipTracker : Tracker<LipData_v2>
    {
        static LipTracker? instance = null;

        public static LipTracker Instance<T>() where T : LipTracker
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
                instance.Stop();
                var handler = instance.updatedHandler;
                instance = Activator.CreateInstance<T>();
                instance.updatedHandler += handler;
                return instance;
            }
        }
    }
}
