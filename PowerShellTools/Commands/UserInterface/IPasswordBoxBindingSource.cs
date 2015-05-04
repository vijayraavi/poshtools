using System.Security;

namespace PowerShellTools.Commands.UserInterface
{
    /// <summary>
    /// Provides passwords to PasswordBoxBinding
    /// </summary>
    internal interface IPasswordBoxBindingSource
    {
        SecureString SecurePassword { get; set; }
    }
}
