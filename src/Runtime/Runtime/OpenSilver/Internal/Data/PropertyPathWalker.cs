
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

using System;
using System.ComponentModel;
using System.Windows.Controls;

#if MIGRATION
using System.Windows.Data;
#else
using Windows.UI.Xaml.Data;
#endif

namespace OpenSilver.Internal.Data
{
    internal class PropertyPathWalker
    {
        private readonly BindingExpression _expr;
        private readonly Binding _parentBinding;

        internal PropertyPathWalker(BindingExpression be)
        {
            _parentBinding = be.ParentBinding;

            _expr = be;
            ListenForChanges = _parentBinding.Mode != BindingMode.OneTime;

            string path = _parentBinding.XamlPath ?? _parentBinding.Path.Path ?? string.Empty;
            ParsePath(path, out IPropertyPathNode head, out IPropertyPathNode tail);

            FirstNode = head;
            FinalNode = tail;
            UpdateNotifyDataErrorInfoBinding(true);
        }

        internal bool ListenForChanges { get; }

        internal IPropertyPathNode FirstNode { get; }

        internal IPropertyPathNode FinalNode { get; }

        internal bool IsPathBroken
        {
            get
            {
                IPropertyPathNode node = FirstNode;
                while (node != null)
                {
                    if (node.IsBroken)
                    {
                        return true;
                    }

                    node = node.Next;
                }

                return false;
            }
        }

        internal void Update(object source)
        {
            UpdateNotifyDataErrorInfoBinding(false);
            FirstNode.Source = source;
            UpdateNotifyDataErrorInfoBinding(true);
        }

        internal void ValueChanged()
        {
            _expr.ValueChanged();
        }

        private void ParsePath(string path, out IPropertyPathNode head, out IPropertyPathNode tail)
        {
            head = null;
            tail = null;

            var parser = new PropertyPathParser(path);
            PropertyNodeType type;

            while ((type = parser.Step(out string typeName, out string propertyName, out string index)) != PropertyNodeType.None)
            {
                PropertyPathNode node;
                switch (type)
                {
                    case PropertyNodeType.AttachedProperty:
                    case PropertyNodeType.Property:
                        node = new StandardPropertyPathNode(this, typeName, propertyName);
                        break;
                    case PropertyNodeType.Indexed:
                        node = new IndexedPropertyPathNode(this, index);
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                if (head == null)
                {
                    head = tail = node;
                    continue;
                }

                tail.Next = node;
                tail = node;
            }

            if (head == null)
            {
                head = tail = new SourcePropertyNode(this);
            }
        }

        private static void LogMessage(string msg)
        {
            //return;
            System.Console.WriteLine(msg);
            System.Diagnostics.Debug.WriteLine(msg);
        }

        internal bool IsBoundToNotifyError { get; private set; } = false;
        private void UpdateNotifyDataErrorInfoBinding(bool attach)
        {
            if (!_parentBinding.ValidatesOnNotifyDataErrors) return;

            if (_expr.Target is TextBox)
            {
                _expr.LogMessage("This is a text box");
            }

            if (FinalNode != null && FinalNode.Source != null && FinalNode.Source is INotifyDataErrorInfo)
            {
                //LogMessage($"PPW.Source {attach}");
                INotifyDataErrorInfo notifyDataErrorInfo = FinalNode.Source as INotifyDataErrorInfo;

                if (attach)
                {
                    _expr.LogMessage("PPW IDNEI Binding to last node source");
                    IsBoundToNotifyError = true;
                    notifyDataErrorInfo.ErrorsChanged += NotifyDataErrorInfo_ErrorsChanged;
                }
                else
                {
                    IsBoundToNotifyError = false;
                    notifyDataErrorInfo.ErrorsChanged -= NotifyDataErrorInfo_ErrorsChanged;
                }
            }

            if (FinalNode != null && FinalNode.Value != null && FinalNode.Value is INotifyDataErrorInfo)
            {
                //LogMessage($"PPW.Value {attach}");

                INotifyDataErrorInfo notifyDataErrorInfo = FinalNode.Value as INotifyDataErrorInfo;

                if (attach)
                {
                    _expr.LogMessage("PPW IDNEI Binding to last node value");
                    IsBoundToNotifyError = true;
                    notifyDataErrorInfo.ErrorsChanged += NotifyDataErrorInfo_ErrorsChanged;
                }
                else
                {
                    IsBoundToNotifyError = false;
                    notifyDataErrorInfo.ErrorsChanged -= NotifyDataErrorInfo_ErrorsChanged;
                }
            }

            if (FirstNode != null)
            {
                var parentNode = FirstNode;
                var currentNode = FirstNode;
                while (currentNode != FinalNode)
                {
                    parentNode = currentNode;
                    currentNode = currentNode.Next;
                }

                var notifyDataErrorInfo = parentNode.Value as INotifyDataErrorInfo;
                if (notifyDataErrorInfo != null)
                {
                    if (attach)
                    {
                        _expr.LogMessage("PPW IDNEI Binding to second last node value");
                        IsBoundToNotifyError = true;
                        notifyDataErrorInfo.ErrorsChanged += NotifyDataErrorInfo_ErrorsChanged;
                    }
                    else
                    {
                        IsBoundToNotifyError = false;
                        notifyDataErrorInfo.ErrorsChanged -= NotifyDataErrorInfo_ErrorsChanged;
                    }
                }
            }
        }

        private void NotifyDataErrorInfo_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            var notifyDataErrorInfo = sender as INotifyDataErrorInfo;
            if (notifyDataErrorInfo != null && FinalNode is StandardPropertyPathNode propertyNode)
            {
                if (e.PropertyName == propertyNode._propertyName)
                {
                    if (notifyDataErrorInfo.HasErrors)
                    {
                        var errors = notifyDataErrorInfo.GetErrors(propertyNode._propertyName);
                        if (errors != null)
                        {
                            foreach (var error in errors)
                            {
                                if (error != null)
                                {
                                    LogMessage($"PPW IDNEI INVALID {error.ToString()}");
                                    Validation.MarkInvalid(_expr, new ValidationError(_expr) { ErrorContent = error.ToString() });
                                }
                            }
                        }
                    }
                    else
                    {
                        LogMessage($"PPW IDNEI VALID");
                        Validation.ClearInvalid(_expr);
                    }
                }                
            }
        }
    }
}
