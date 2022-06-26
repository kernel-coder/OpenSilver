using System;
using System.Collections.ObjectModel;

#if MIGRATION
namespace System.Windows.Input
#else
namespace Windows.UI.Xaml.Input
#endif
{
    public sealed class StylusPointCollection : PresentationFrameworkCollection<StylusPoint>
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusPointCollection" /> class. </summary>
        public StylusPointCollection() : base (true)
        {
        }

        /// <summary>
        /// Adds a collection of <see cref="StylusPoint"/> objects to the collection.
        /// </summary>
        /// <param name="stylusPoints">
        /// The collection of <see cref="StylusPoint"/> objects to add to the collection.
        /// </param>
        public void Add(StylusPointCollection stylusPoints)
        {
            if (stylusPoints is null)
            {
                throw new ArgumentNullException(nameof(stylusPoints));
            }

            foreach (StylusPoint point in stylusPoints)
            {
                Add(point);
            }
        }

        internal override void AddOverride(StylusPoint point)
        {
            this.AddInternal(point);
        }

        internal override void ClearOverride()
        {
            this.ClearInternal();
        }

        internal override void RemoveAtOverride(int index)
        {
            this.RemoveAtInternal(index);
        }

        internal override void InsertOverride(int index, StylusPoint point)
        {
            this.InsertInternal(index, point);
        }

        internal override StylusPoint GetItemOverride(int index)
        {
            return this.GetItemInternal(index);
        }

        internal override void SetItemOverride(int index, StylusPoint point)
        {
            this.SetItemInternal(index, point);
        }

    }
}
