﻿using System;

namespace AppFramework.Core.Commands
{
    /// <summary>
    /// Command for navigating to a page.
    /// </summary>
    public sealed class NavigationCommand : GenericCommand<object>
    {
        #region Constructors

        /// <summary>
        /// Command for navigating to a page based on the IModel parameter passed into the Execute method.
        /// </summary>
        public NavigationCommand()
            : base("NavigateToModelCommand", BasePlatform.CurrentCore.NavigationBase.Model, null)
        {
        }

        /// <summary>
        /// Command for navigation to a page.
        /// </summary>
        /// <param name="commandName">Identifies this command instance, used for logging.</param>
        /// <param name="execute">Logic to execute, Execute command object will be ignored.</param>
        /// <param name="canExecute">Logic to determine if this command can execute, CanExecute parameter will be ignored.</param>
        public NavigationCommand(string commandName, Action execute, Func<bool> canExecute = null)
            : base(commandName, execute, canExecute)
        {
        }

        /// <summary>
        /// Command for navigation to a page.
        /// </summary>
        /// <param name="commandName">Identifies this command instance, used for logging.</param>
        /// <param name="execute">Logic to execute, Execute command object must be an IModel type.</param>
        /// <param name="canExecute">Logic to determine if this command can execute, CanExecute parameter must be an IModel type.</param>
        public NavigationCommand(string commandName, Action<object> execute, Func<object, bool> canExecute = null)
            : base(commandName, execute, canExecute)
        {
        }

        #endregion
    }
}