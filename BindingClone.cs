using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace ModelExtensions.WPF
{
    /// <summary>
    /// binding cloner with new target assignment
    /// </summary>
    public static class BindingClone
    {
        public static Binding Get(
            DependencyObject target,
            DependencyProperty property,
            string path)
        {
            var b = BindingOperations.GetBinding(target,property);
            var nb = new Binding(path)
            {
                UpdateSourceTrigger = b.UpdateSourceTrigger,
                Mode = b.Mode,
                NotifyOnValidationError = b.NotifyOnValidationError
            };
            foreach (var vr in b.ValidationRules)
                nb.ValidationRules.Add(vr);

            b.ValidationRules.Clear();
            BindingOperations.ClearBinding(target, property);
            BindingOperations.SetBinding(target, property, nb);
            return b;
        }
    }
}
