﻿// Copyright (c) Bili Copilot. All rights reserved.

using BiliCopilot.UI.ViewModels.Components;

namespace BiliCopilot.UI.Controls.Message;

/// <summary>
/// 通知消息主体.
/// </summary>
public sealed partial class NotifyMessageBody : NotifyMessageControlBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotifyMessageBody"/> class.
    /// </summary>
    public NotifyMessageBody() => InitializeComponent();

    /// <inheritdoc/>
    protected override void OnControlLoaded()
    {
        MessageScrollView.ViewChanged += OnViewChanged;
        MessageScrollView.SizeChanged += OnScrollViewSizeChanged;
        if (ViewModel is null)
        {
            return;
        }

        CheckMessageCount();
    }

    /// <inheritdoc/>
    protected override void OnControlUnloaded()
    {
        if (ViewModel is not null)
        {
            ViewModel.ListUpdated -= OnMessageListUpdatedAsync;
        }

        MessageRepeater.ItemsSource = null;
        MessageScrollView.ViewChanged -= OnViewChanged;
        MessageScrollView.SizeChanged -= OnScrollViewSizeChanged;
    }

    /// <inheritdoc/>
    protected override void OnViewModelChanged(NotifyMessageSectionDetailViewModel? oldValue, NotifyMessageSectionDetailViewModel? newValue)
    {
        if (oldValue is not null)
        {
            oldValue.ListUpdated -= OnMessageListUpdatedAsync;
        }

        if (newValue is null)
        {
            return;
        }

        newValue.ListUpdated += OnMessageListUpdatedAsync;
        MessageScrollView?.ChangeView(0, 0, default);
    }

    private async void OnMessageListUpdatedAsync(object? sender, EventArgs e)
    {
        await Task.Delay(500);
        CheckMessageCount();
    }

    private void OnViewChanged(object? sender, ScrollViewerViewChangedEventArgs args)
    {
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
        {
            if (MessageScrollView.ExtentHeight - MessageScrollView.ViewportHeight - MessageScrollView.VerticalOffset <= 240)
            {
                ViewModel.LoadItemsCommand.Execute(default);
            }
        });
    }

    private void OnScrollViewSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width > 100 && ViewModel is not null)
        {
            CheckMessageCount();
        }
    }

    private void CheckMessageCount()
    {
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
        {
            if (MessageScrollView.ScrollableHeight <= 240 && ViewModel is not null)
            {
                ViewModel.LoadItemsCommand.Execute(default);
            }
        });
    }
}
