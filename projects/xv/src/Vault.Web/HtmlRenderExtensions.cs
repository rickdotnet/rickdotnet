namespace Vault.Web;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

public static class HtmlRendererExtensions
{
    public static async Task<string> RenderHtmlAsync<TComponent>(
        this HtmlRenderer htmlRenderer, ParameterView? parameters = null)
        where TComponent : IComponent
    {
        return await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await htmlRenderer.RenderComponentAsync<TComponent>(parameters ?? ParameterView.Empty);
            return output.ToHtmlString();
        });
    }
    
    public static Task<string> RenderHtmlAsync<TComponent>(
        this HtmlRenderer htmlRenderer, Dictionary<string, object?> paramDict)
        where TComponent : IComponent
    {
        return htmlRenderer.RenderHtmlAsync<TComponent>(ParameterView.FromDictionary(paramDict));
    }
}

