﻿// Copyright (c) Bili Copilot. All rights reserved.

using Richasy.WinUIKernel.AI.ViewModels;

namespace BiliCopilot.UI.Controls.AI;

/// <summary>
/// AI 头部.
/// </summary>
public sealed partial class AIHeader : AIControlBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AIHeader"/> class.
    /// </summary>
    public AIHeader() => InitializeComponent();

    protected override void OnControlUnloaded()
        => ServiceRepeater.ItemsSource = null;

    private void OnServiceItemClick(object sender, RoutedEventArgs e)
    {
        var context = (sender as FrameworkElement).DataContext as ChatServiceItemViewModel;
        ViewModel.SelectServiceCommand.Execute(context);
        ServiceFlyout.Hide();
    }
}
