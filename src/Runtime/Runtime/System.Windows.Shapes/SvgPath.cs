

/*===================================================================================
* 
*   Copyright (c) Userware/OpenSilver.net
*      
*   This file is part of the OpenSilver Runtime (https://opensilver.net), which is
*   licensed under the MIT license: https://opensource.org/licenses/MIT
*   
*   As stated in the MIT license, "the above copyright notice and this permission
*   notice shall be included in all copies or substantial portions of the Software."
*  
\*====================================================================================*/

using CSHTML5.Internal;
using System;

#if MIGRATION
using System.Windows.Media;
#else
using Windows.UI.Xaml.Media;
using Windows.Foundation;
#endif

#if MIGRATION
namespace System.Windows.Shapes
#else
namespace Windows.UI.Xaml.Shapes
#endif
{
    /// <summary>
    /// Draws a series of connected lines and curves. The line and curve dimensions
    /// are declared through the Data property, and can be specified either with
    /// a path-specific mini-language, or with an object model.
    /// </summary>
    public partial class SvgPath : Shape
    {
        private object _svgTag, _pathTag;
        public override object CreateDomElement(object parentRef, out object domElementWhereToPlaceChildren)
        {
            UIElement associatedUIElement = this;

            domElementWhereToPlaceChildren = null;
            _svgTag = INTERNAL_HtmlDomManager.CreateDomElementAndAppendIt("svg", parentRef, associatedUIElement);
            var divStyle = INTERNAL_HtmlDomManager.GetDomElementStyleForModification(_svgTag);
            //divStyle.overflow = "hidden";
            divStyle.lineHeight = "0"; // Line height is not needed in shapes because it causes layout issues.
            //divStyle.width = "100%";
            //divStyle.height = "100%";
            divStyle.fontSize = "0px"; //this allows this div to be as small as we want (for some reason in Firefox, what contains a canvas has a height of at least about (1 + 1/3) * fontSize)
            _pathTag = INTERNAL_HtmlDomManager.CreateDomElementAndAppendIt("path", _svgTag, associatedUIElement);
            var style = INTERNAL_HtmlDomManager.GetDomElementStyleForModification(_pathTag);
            style.width = "100%";
            style.height = "100%";
            return _svgTag;
        }

        protected internal override void INTERNAL_OnAttachedToVisualTree()
        {
            base.INTERNAL_OnAttachedToVisualTree();

            if (Fill != null)
                OnFillChanged();

            if (!string.IsNullOrEmpty(Data))
                OnDataChanged();
        }

        /// <summary>
        /// Gets or sets a Geometry that specifies the shape to be drawn.
        /// </summary>
        public string Data
        {
            get { return (string)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="SvgPath.Data"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(
                nameof(Data),
                typeof(string),
                typeof(SvgPath),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, Data_Changed));

        private static void Data_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SvgPath path = (SvgPath)d;
            path?.OnDataChanged();           
        }

        private void OnDataChanged()
        {
            try
            {
                if (!string.IsNullOrEmpty(Data) && _pathTag != null)
                {
                    INTERNAL_HtmlDomManager.SetDomElementAttribute(_pathTag, "d", Data);
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine("SvgPath: path has no d prop");
            }
        }

        protected override void OnFillChanged()
        {
            try
            {
                if (Fill is SolidColorBrush && _pathTag != null)
                {
                    INTERNAL_HtmlDomManager.SetDomElementAttribute(_pathTag, "fill", ((SolidColorBrush)Fill).INTERNAL_ToHtmlString());
                }
            }
            catch(Exception exc)
            {
                System.Diagnostics.Debug.WriteLine("SvgPath: path has no fill prop");
            }
        }

    }
}