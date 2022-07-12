using System;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows.Markup;

[assembly: NeutralResourcesLanguage("en-US")]
[assembly: ComVisible(false)]

#if __CLSCOMPLIANT__
[assembly: CLSCompliant(true)]
#endif
[assembly: XmlnsPrefix("http://schemas.microsoft.com/winfx/2006/xaml/presentation/toolkit", "toolkit")]
#if MIGRATION
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation/toolkit", "System.Windows.Controls")]
#else
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation/toolkit", "Windows.UI.Xaml.Controls")]
#endif



