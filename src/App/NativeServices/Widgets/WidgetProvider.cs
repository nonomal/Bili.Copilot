// Copyright (c) Bili Copilot. All rights reserved.

using System.Runtime.InteropServices;

namespace Bili.Copilot.App.NativeServices.Widgets;

/// <summary>
/// 小组件提供器.
/// </summary>
[Guid("40A52E4C-AD4A-46B5-9752-3A90D784C0EA")]
public class WidgetProvider : WidgetProviderBase
{
    static WidgetProvider()
    {
        RegisterWidget<RankWidget>("BiliCopilotRank");
    }
}
