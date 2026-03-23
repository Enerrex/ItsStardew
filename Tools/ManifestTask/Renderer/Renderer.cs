using ManifestTask.Renderer.Models;

namespace ManifestTask.Renderer;

internal static class Renderer
{
    public static string Render(string template, RenderModel model)
    {
        return template.Replace
                        (
                            "$(ManifestName)",
                            Escape(model.ManifestName),
                            StringComparison.Ordinal
                        ).
                        Replace
                        (
                            "$(ManifestAuthor)",
                            Escape(model.ManifestAuthor),
                            StringComparison.Ordinal
                        ).
                        Replace
                        (
                            "$(ManifestVersion)",
                            Escape(model.ManifestVersion),
                            StringComparison.Ordinal
                        ).
                        Replace
                        (
                            "$(ManifestDescription)",
                            Escape(model.ManifestDescription),
                            StringComparison.Ordinal
                        ).
                        Replace
                        (
                            "$(ManifestUniqueId)",
                            Escape(model.ManifestUniqueId),
                            StringComparison.Ordinal
                        ).
                        Replace
                        (
                            "$(ManifestEntryDll)",
                            Escape(model.ManifestEntryDll),
                            StringComparison.Ordinal
                        ).
                        Replace
                        (
                            "$(ManifestMinimumApiVersion)",
                            Escape(model.ManifestMinimumApiVersion),
                            StringComparison.Ordinal
                        );
    }

    private static string Escape(string value)
    {
        return value.Replace
                     (
                         "\\",
                         "\\\\",
                         StringComparison.Ordinal
                     ).
                     Replace
                     (
                         "\"",
                         "\\\"",
                         StringComparison.Ordinal
                     );
    }
}