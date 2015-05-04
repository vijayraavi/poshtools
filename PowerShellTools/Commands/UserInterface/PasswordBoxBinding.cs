using System.Security;
using System.Windows;
using System.Windows.Controls;
using PowerShellTools.Common;

namespace PowerShellTools.Commands.UserInterface
{
    /// <summary>
    /// Provides a way to bind to the SecurePassword property of PasswordBox without having it be
    /// exposed as a dependency property, which the .net team considers a security risk.
    /// </summary>
    internal static class PasswordBoxBinding
    {
        public static IPasswordBoxBindingSource GetPasswordSource(DependencyObject d)
        {
            return (IPasswordBoxBindingSource)d.GetValue(PasswordSourceProperty);
        }

        public static void SetPasswordSource(DependencyObject d, IPasswordBoxBindingSource value)
        {
            d.SetValue(PasswordSourceProperty, value);
        }

        public static readonly DependencyProperty PasswordSourceProperty =
            DependencyProperty.RegisterAttached("PasswordSource",
                typeof(IPasswordBoxBindingSource),
                typeof(PasswordBoxBinding),
                new UIPropertyMetadata(null, (d, e) =>
                {
                    OnPasswordSourceChanged(d, (IPasswordBoxBindingSource)e.NewValue);
                }));

        private static void OnPasswordSourceChanged(DependencyObject d, IPasswordBoxBindingSource source)
        {
            PasswordBox passwordBox = (PasswordBox)d;

            if (source == null)
            {
                return;
            }

            SecureString initialValue = source.SecurePassword;
            if (initialValue != null && initialValue.Length > 0)
            {
                // If we already had a password, we have no choice but to convert it to plaintext with
                //   the current implementation of PasswordBox in order to set the initial value.
                passwordBox.Password = initialValue.ConvertToUnsecureString();
            }

            passwordBox.PasswordChanged += (s, e) =>
            {
                source.SecurePassword = passwordBox.SecurePassword;
            };
        }
    }
}
