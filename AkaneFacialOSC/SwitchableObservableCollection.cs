using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azw.FacialOsc
{
    internal class SwitchableObservableCollection<T> : ObservableCollection<T>
    {
        public bool isNotifying = true;
        public SwitchableObservableCollection(IEnumerable<T> collection) : base(collection) { }
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (isNotifying)
            {
                base.OnCollectionChanged(e);
            }
        }
    }
}
