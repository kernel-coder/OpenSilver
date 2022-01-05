

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


//#define CHECK_THAT_ID_EXISTS 
//#define PERFORMANCE_ANALYSIS

using OpenSilver.Internal;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace CSHTML5.Internal
{
    // Note: this class is intented to be used by the Simulator only, not when compiled to JavaScript.
#if BRIDGE
    [Bridge.External] //we exclude this class
#endif

#if CSHTML5NETSTANDARD
    public class INTERNAL_Html2dContextReference
#else
    internal class INTERNAL_Html2dContextReference 
#endif
    {
        private static double? _dpi = null;
        internal static double DPI
        {
            get
            {
                if (_dpi == null)
                {
                    _dpi = Convert.ToDouble(OpenSilver.Interop.ExecuteJavaScript("window.devicePixelRatio"));
                }
                return _dpi.Value;
            }
        }

        static Dictionary<string, INTERNAL_Html2dContextReference> IdToInstance = new Dictionary<string, INTERNAL_Html2dContextReference>();

        public static INTERNAL_Html2dContextReference GetInstance(string elementId)
        {
            if (IdToInstance.ContainsKey(elementId))
                return IdToInstance[elementId];
            else
            {
                var newInstance = new INTERNAL_Html2dContextReference(elementId);
                IdToInstance[elementId] = newInstance;
                return newInstance;
            }
        }

        string _domElementUniqueIdentifier;

        // Note: It's important that the constructor stays Private because we need to recycle the instances that correspond to the same ID using the "GetInstance" public static method, so that each ID always corresponds to the same instance. This is useful to ensure that private fields such as "_display" work propertly.
        private INTERNAL_Html2dContextReference(string elementId)
        {
            _domElementUniqueIdentifier = elementId;
        }

        void SetPropertyValue(string propertyName, string propertyValue)
        {
            string javaScriptCodeToExecute = "document.set2dContextProperty(\"" + _domElementUniqueIdentifier + "\",\"" + propertyName + "\",\"" + propertyValue + "\")";
            //INTERNAL_SimulatorPerformanceOptimizer.QueueJavaScriptCode(javaScriptCodeToExecute);
            INTERNAL_SimulatorExecuteJavaScript.ExecuteJavaScriptAsync(javaScriptCodeToExecute);
        }

        object InvokeMethod(string methodName, object[] args)
        {
            string methodArgumentsFormattedForJavaScript = string.Join(", ", args.Select(x => INTERNAL_HtmlDomManager.ConvertToStringToUseInJavaScriptCode(x)));
            string javaScriptCodeToExecute =
                $@"document.invoke2dContextMethod(""{_domElementUniqueIdentifier}"" , ""{methodName}"", ""{methodArgumentsFormattedForJavaScript}"")";

            var result = CSHTML5.Interop.ExecuteJavaScriptAsync(javaScriptCodeToExecute);
            return result;
            //INTERNAL_SimulatorPerformanceOptimizer.QueueJavaScriptCode(javaScriptCodeToExecute);
            //result = null;
        }

        public string fillStyle 
        { 
            set 
            {
                if (Ctx == null) SetPropertyValue("fillStyle", value);
                else AddProp("fillStyle", value);
            } 
        }
        public string strokeStyle
        {
            set
            {
                if (Ctx == null) SetPropertyValue("strokeStyle", value);
                else AddProp("strokeStyle", value);
            }
        }

        public string lineJoin
        {
            set
            {
                if (Ctx == null) SetPropertyValue("lineJoin", value);
                else AddProp("lineJoin", value);
            }
        }

        public double miterLimit
        {
            set
            {
                if (Ctx == null) SetPropertyValue("miterLimit", (DPI * value).ToInvariantString());
                else AddProp("miterLimit", DPI * value);
            }
        }

        public double lineWidth
        {
            set
            {
                if (Ctx == null) SetPropertyValue("lineWidth", (DPI * value).ToInvariantString());
                else AddProp("lineWidth", DPI * value);
            }
        }
        
        public double lineDashOffset
        {
            set
            {
                if (Ctx == null) SetPropertyValue("lineDashOffset", (DPI * value).ToInvariantString());
                else AddProp("lineDashOffset", DPI * value);
            }
        }
        
        public void transform(double a, double b, double c, double d, double e, double f)
        {
            var args = new object[] { a, b, c , d,  e * DPI, f * DPI };
            if (Ctx == null) InvokeMethod("transform", args);
            else AddLineEmbArg("transform", args);
        }

        public void translate(double x, double y)
        {
            var args = new object[] { x * DPI, y * DPI };
            if (Ctx == null) InvokeMethod("translate", args);
            else AddLineEmbArg("translate", args);
        }

        public void rotate(double angle)
        {
            var args = new object[] { angle };
            if (Ctx == null) InvokeMethod("rotate", args);
            else AddLineEmbArg("rotate", args);
        }

        public void scale(double x, double y)
        {
            var args = new object[] { x * DPI, y * DPI };
            if (Ctx == null) InvokeMethod("scale", args);
            else AddLineEmbArg("scale", args);
        }

        public void save()
        {
            if (Ctx == null) InvokeMethod("save", new object[] { });
            else AddLineEmbArg("save", new object[] { });
        }

        public void restore()
        {
            if (Ctx == null) InvokeMethod("restore", new object[] { });
            else AddLineEmbArg("restore", new object[] { });
        }

        public void fill(string fillRule = "evenodd")
        {
            if (Ctx == null) InvokeMethod("fill", new object[] { "'" + fillRule + "'" });
            else AddLineEmbArg("fill", new object[] { "'" + fillRule + "'" });
        }

        public void stroke() 
        {
            if (Ctx == null) InvokeMethod("stroke", new object[] { });
            else AddLineEmbArg("stroke", new object[] { });
        }

        public void setLineDash(params object[] args) 
        {
            if (Ctx == null) InvokeMethod("setLineDash", args);
            else AddLine("setLineDash", args);
        }

        public void beginPath()
        {
            if (Ctx == null) InvokeMethod("beginPath", new object[] { });
            else AddLineEmbArg("beginPath", new object[] { });
        }

        public void closePath() 
        {
            if (Ctx == null) InvokeMethod("closePath", new object[] { });
            else AddLineEmbArg("closePath", new object[] { });
        }

        public void createLinearGradient(params object[] args) 
        {
            if (Ctx == null) InvokeMethod("createLinearGradient", args);
            else AddLine("createLinearGradient", args);
        }

        public void arc(double x, double y, double r, double sAngle, double eAngle, bool counterclockwise = false)
        {
            var args = new object[] { x * DPI, y * DPI, r * DPI, sAngle, eAngle, counterclockwise ? "true" : "false" };
            if (Ctx == null) InvokeMethod("arc", args);
            else AddLineEmbArg("arc", args);
        }

        public void arcTo(double x1, double y1, double x2, double y2, double r)
        {
            var args = new object[] { x1 * DPI, y1 * DPI, x2 * DPI, y2 * DPI, r * DPI };
            if (Ctx == null) InvokeMethod("arcTo", args);
            else AddLineEmbArg("arcTo", args);
        }

        public void ellipse(double x, double y, double radx, double rady, double xAngle, double yAngle, double angle)
        {
            var args = new object[] { x * DPI, y * DPI, radx * DPI, rady * DPI, xAngle, yAngle, angle };
            if (Ctx == null) InvokeMethod("ellipse", args);
            else AddLineEmbArg("ellipse", args);
        }

        public void rect(double x, double y, double w, double h)
        {
            var args = new object[] { x * DPI, y * DPI, w * DPI, h * DPI };
            if (Ctx == null) InvokeMethod("rect", args);
            else AddLineEmbArg("rect", args);
        }

        public void clearRect(double x, double y, double w, double h)
        {
            var args = new object[] { x * DPI, y * DPI, w * DPI, h * DPI };
            if (Ctx == null) InvokeMethod("clearRect", args);
            else AddLineEmbArg("clearRect", args);
        }

        public void moveTo(double x, double y)
        {
            var args = new object[] { x * DPI, y * DPI };
            if (Ctx == null) InvokeMethod("moveTo", args);
            else AddLineEmbArg("moveTo", args);
        }

        public void lineTo(double x, double y)
        {
            var args = new object[] { x * DPI, y * DPI };
            if (Ctx == null) InvokeMethod("lineTo", args);
            else AddLineEmbArg("lineTo", args);
        }

        public void bezierCurveTo(double cp1x, double cp1y, double cp2x, double cp2y, double x, double y)
        {
            var args = new object[] { cp1x * DPI, cp1y * DPI, cp2x * DPI, cp2y * DPI, x * DPI, y * DPI };
            if (Ctx == null) InvokeMethod("bezierCurveTo", args);
            else AddLineEmbArg("bezierCurveTo", args);
        }

        public void quadraticCurveTo(double cpx, double cpy, double x, double y)
        {
            var args = new object[] { cpx * DPI, cpy * DPI, x * DPI, y * DPI };
            if (Ctx == null) InvokeMethod("quadraticCurveTo", args);
            else AddLineEmbArg("quadraticCurveTo", args);
        }

        public object Ctx { get; private set; }
        private StringBuilder _lines = new StringBuilder();
        private List<object> _args = new List<object>();

        private void AddLine(string cmd, object[] args)
        {
            StringBuilder line = new StringBuilder();
            line.Append("$0." + cmd);
            line.Append("(");
            for (int i = 0; i < args.Length; i++)
            {
                line.Append( "$" + _args.Count.ToString());
                if (i < args.Length - 1)
                {
                    line.Append(',');
                }

                _args.Add(args[i]);
            }

            line.Append(");");
            _lines.Append(line);
        }

        private void AddLineEmbArg(string cmd, object[] args)
        {
            StringBuilder line = new StringBuilder();
            line.Append("$0." + cmd);
            line.Append("(");
            for(int i = 0; i < args.Length; i++)
            {
                line.Append(args[i].ToInvariantString());
                if (i < args.Length - 1)
                {
                    line.Append(',');
                }
            }

            line.Append(");");
            _lines.Append(line);
        }

        private void AddProp(string prop, object arg)
        {
            StringBuilder line = new StringBuilder();
            line.Append("$0." + prop + "=");
            if (arg is string)
            {
                line.Append("'");
            }

            line.Append(arg.ToInvariantString());

            if (arg is string)
            {
                line.Append("'");
            }

            line.Append(";");
            _lines.Append(line);
        }

        public void BeginCache(Object canvas)
        {
            if (canvas == null || Ctx != null) return;

            Ctx = OpenSilver.Interop.ExecuteJavaScriptAsync(@"$0.getContext('2d')", canvas);
            _args.Add(Ctx);
        }

        public void FlushCache()
        {
            if (Ctx == null) return;
            OpenSilver.Interop.ExecuteJavaScriptAsync(_lines.ToString(), _args.ToArray());
            Ctx = null;
            _lines.Clear();
            _args.Clear();
        }
    }
}
