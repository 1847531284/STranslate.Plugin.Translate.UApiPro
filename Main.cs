using System.Text.Json;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;
using STranslate.Plugin.Translate.UApiPro.View;
using STranslate.Plugin.Translate.UApiPro.ViewModel;

namespace STranslate.Plugin.Translate.UApiPro;

public class Main : TranslatePluginBase
{
    private const string TranslateEndpoint = "https://uapis.cn/api/v1/translate/text";
    private Control? _settingUi;
    private Settings Settings { get; set; } = new();
    private IPluginContext Context { get; set; } = null!;

    public override Control GetSettingUI() => _settingUi ??= new SettingsView
    {
        DataContext = new SettingsViewModel(Context, Settings, this),
    };

    public async Task<string> TestAsync(CancellationToken cancellationToken = default)
    {
        var result = new TranslateResult();
        await TranslateAsync(
            new TranslateRequest("hi", LangEnum.English, LangEnum.ChineseSimplified),
            result,
            cancellationToken);

        if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Text))
            throw new Exception(result.Text?.Trim() ?? "UApiPro 返回空响应");

        return result.Text.Trim();
    }

    public override string? GetSourceLanguage(LangEnum langEnum) => langEnum switch
    {
        LangEnum.Auto => "auto",
        LangEnum.ChineseSimplified => "zh",
        LangEnum.ChineseTraditional or LangEnum.Cantonese => "zh-TW",
        LangEnum.English => "en",
        LangEnum.Japanese => "ja",
        LangEnum.Korean => "ko",
        LangEnum.French => "fr",
        LangEnum.Spanish => "es",
        LangEnum.Russian => "ru",
        LangEnum.German => "de",
        LangEnum.Italian => "it",
        LangEnum.Turkish => "tr",
        LangEnum.PortuguesePortugal or LangEnum.PortugueseBrazil => "pt",
        LangEnum.Vietnamese => "vi",
        LangEnum.Indonesian => "id",
        LangEnum.Thai => "th",
        LangEnum.Malay => "ms",
        LangEnum.Arabic => "ar",
        LangEnum.Hindi => "hi",
        LangEnum.MongolianCyrillic or LangEnum.MongolianTraditional => "mn",
        LangEnum.Khmer => "km",
        LangEnum.NorwegianBokmal or LangEnum.NorwegianNynorsk => "no",
        LangEnum.Persian => "fa",
        LangEnum.Swedish => "sv",
        LangEnum.Polish => "pl",
        LangEnum.Dutch => "nl",
        LangEnum.Ukrainian => "uk",
        _ => null,
    };

    public override string? GetTargetLanguage(LangEnum langEnum) => GetSourceLanguage(langEnum);

    public override void Init(IPluginContext context)
    {
        Context = context;
        Settings = context.LoadSettingStorage<Settings>();
    }

    public override void Dispose() { }

    public override async Task TranslateAsync(
        TranslateRequest request,
        TranslateResult result,
        CancellationToken cancellationToken = default)
    {
        if (GetTargetLanguage(request.TargetLang) is not { } targetLanguage)
        {
            result.Fail(Context.GetTranslation("UnsupportedTargetLang"));
            return;
        }

        try
        {
            var headers = new Dictionary<string, string> { ["Accept"] = "application/json" };
            if (!string.IsNullOrWhiteSpace(Settings.ApiKey))
                headers["Authorization"] = $"Bearer {Settings.ApiKey}";

            var response = await Context.HttpService.PostAsync(
                $"{TranslateEndpoint}?to_lang={targetLanguage}",
                JsonSerializer.Serialize(new { text = request.Text }),
                new Options { Headers = headers, ContentType = "application/json" },
                cancellationToken);

            if (string.IsNullOrEmpty(response))
            {
                result.Fail("UApiPro 返回空响应");
                return;
            }

            using var document = JsonDocument.Parse(response);
            var root = document.RootElement;

            if (root.TryGetProperty("code", out var code) && code.ValueKind == JsonValueKind.String)
            {
                result.Fail($"UApiPro 错误: {(root.TryGetProperty("message", out var message) ? message.GetString() : "未知错误")}");
                return;
            }

            if (!root.TryGetProperty("translate", out var translation) ||
                translation.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(translation.GetString()))
            {
                result.Fail("UApiPro 返回格式异常或空译文");
                return;
            }

            result.Success(translation.GetString()!.Trim());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Context.Logger.LogError(ex, "[UApiPro] 翻译失败");
            result.Fail($"UApiPro 翻译失败: {ex.Message}");
        }
    }
}
