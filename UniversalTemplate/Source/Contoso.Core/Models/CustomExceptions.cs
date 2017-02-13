using System;

namespace Contoso.Core.Models
{
    public class UserFriendlyException : Exception
    {
        public UserFriendlyException(string userMessage, DisplayStyles displayStyle = DisplayStyles.NonBlockingMessage)
        {
            this.UserMessage = userMessage;
            this.DisplayStyle = displayStyle;
        }

        public string UserMessage { get; private set; }
        public DisplayStyles DisplayStyle { get; private set; }

        public enum DisplayStyles
        {
            NonBlockingMessage,
            BlockingMessage,
            MessageBox
        }
    }

    public class UserUnauthorizedException : Exception
    {
    }
}
