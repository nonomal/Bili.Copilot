// Copyright (c) Bili Copilot. All rights reserved.

using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Bili.Copilot.Libs.Provider;
using Microsoft.Windows.Widgets.Providers;

namespace Bili.Copilot.App.NativeServices.Widgets;

internal class RankWidget : WidgetBase
{
    // This function wil be invoked when the Increment button was clicked by the user.
    public override void OnActionInvoked(WidgetActionInvokedArgs actionInvokedArgs)
    {
        var context = actionInvokedArgs.Data;
        Debug.WriteLine(context);
    }

    public override string GetTemplateForWidget()
    {
        // This widget has the same template for all the sizes/themes so we load it only once.
        var widgetTemplate = GetTemplateFromFile("ms-appx:///Assets/Widgets/RankWidgetTemplate.json");
        return widgetTemplate;
    }

    public override string GetDataForWidget()
    {
        var rankData = HomeProvider.GetRankDetailAsync("0").Result;
        var displayData = rankData.Take(1).Select(p => new
        {
            title = p.Identifier.Title,
            id = p.Identifier.Id,
            subtitle = p.Subtitle,
            cover = p.Identifier.Cover.Uri,
        });

        return JsonSerializer.Serialize(displayData);
    }
}
