﻿using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Contoso.UI.Views
{
	public partial class MainPage : ContentPage, IPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

		int count = 0;
		private void OnCounterClicked(object sender, EventArgs e)
		{
			count++;
			CounterLabel.Text = $"Current count: {count}";
		}
	}
}
