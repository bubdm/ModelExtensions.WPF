using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ModelExtensions.WPF
{
    /// <summary>
    /// validation error handler for window with view model of type ModelExtensions.BindableModel
    /// controls validation settings should be configured like this for validation rules (view) only:
    ///     UpdateSourceTrigger="PropertyChanged"
    ///     ValidatesOnNotifyDataErrors="False"
    ///     ValidatesOnDataErrors="False"      
    ///     ValidatesOnExceptions="True"
    ///     NotifyOnValidationError="True"      * mandatory
    ///     
    /// if mixing validation rules (view) and view model property validation (data annotations) :
    ///     UpdateSourceTrigger="PropertyChanged"
    ///     ValidatesOnNotifyDataErrors="False"
    ///     ValidatesOnDataErrors="False"      
    ///     ValidatesOnExceptions="True"        * to be checked
    ///     NotifyOnValidationError="True"      * mandatory
    ///     IsModelPropertyValidationEnabled=true in constructor is mandatory
    /// </summary>
    public class ValidationErrorHandler
    {
        readonly bool _IsModelPropertyValidationEnabled = false;

        public ValidationErrorHandler(Window window,bool isModelPropertyValidationEnabled = false)
        {
            _IsModelPropertyValidationEnabled = isModelPropertyValidationEnabled;
            Validation.AddErrorHandler(window, ValidationErrorHandlerImpl);
        }

        void ValidationErrorHandlerImpl(object sender, ValidationErrorEventArgs e)
        {
            BindingExpression binding = e.Error.BindingInError as BindingExpression;
            if (binding != null)
            {
                var viewModel = binding.ResolvedSource as BindableModel;             // object
                if (viewModel != null)
                {
                    var property = binding.ResolvedSourcePropertyName;  // string
                    viewModel.NotifyErrorsChangedEnabled = false;

                    bool addMultiple = e.Action == ValidationErrorEventAction.Added
                        &&
                        (!_IsModelPropertyValidationEnabled ||
                        !(e.Error.RuleInError is NotifyDataErrorValidationRule));

                    /*
                     * if source has property validation throught data model annotation,
                     * clear property validation if Added & e.Error.RuleInError isn't a System.Windows.Controls.NotifyDataErrorValidationRule
                     */
                    if (_IsModelPropertyValidationEnabled
                        && e.Action==ValidationErrorEventAction.Added
                        && !(e.Error.RuleInError is NotifyDataErrorValidationRule))
                    {
                        // if validation rule
                        var validationAttributes = viewModel.GetType().GetProperty(property).GetCustomAttributes(true)
                            .OfType<ValidationAttribute>();
                        if (validationAttributes.Count()>0)
                        {
                            if (viewModel.HasPropertyValivationError(property))
                            {
                                viewModel.NotifyErrorsChangedEnabled = true;
                                viewModel.ClearPropertyValidationErrors(property);    // must clear only NotifyDataErrorValidationRule messages
                                                                                      // ?? heuristic: try clear, at least keep 1 - coz clear needed for avoiding validation rules repeat
                                                                                      //viewModel.ClearErrors(property);
                                viewModel.NotifyErrorsChangedEnabled = false;
                            }
                        }
                    }

                    switch (e.Action)
                    {
                        case ValidationErrorEventAction.Added:
                            viewModel.AddError(
                                e.Error.ErrorContent as string,
                                e.Error.RuleInError.GetType(),
                                property, 
                                e.Error.RuleInError is NotifyDataErrorValidationRule );
                            break;
                        case ValidationErrorEventAction.Removed:
                            //viewModel.NotifyErrorsChangedEnabled = true;
                            viewModel.RemoveError(e.Error.ErrorContent as string, property);
                            break;
                    }
                    viewModel.NotifyErrorsChangedEnabled = true;
                }
            }
        }
    }
}
