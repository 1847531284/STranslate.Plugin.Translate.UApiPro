using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;
using STranslate.Plugin.Translate.UApiPro.View;
using STranslate.Plugin.Translate.UApiPro.ViewModel;

namespace STranslate.Plugin.Translate.UApiPro;

/// <summary>
/// UApiPro 翻译插件。
/// 通过 UApiPro 平台 (uapis.cn) 的 REST API 进行翻译，支持 100+ 语言。
///   - 端点: POST https://uapis.cn/api/v1/translate/text?to_lang={目标语言}
///   - 请求体: {"text": "原文"}（最大 3000 字符）
///   - 响应体: {"translate": "译文", "text": "原文"}
///   - 认证: Authorization: Bearer {ApiKey}（付费接口需要；免费层可不填）
///   - 源语言自动检测，无需传 source_lang
///   - 通过 Context.HttpService 发请求（复用宿主代理/SSL 配置）
/// </summary>
public class Main : TranslatePluginBase
{
    private Control? _settingUi;
    private SettingsViewModel? _viewModel;
    private Settings Settings { get; set; } = new();
    private IPluginContext Context { get; set; } = null!;

    // UApiPro 翻译端点（固定，不支持自定义域名）
    private const string TranslateEndpoint = "https://uapis.cn/api/v1/translate/text";

    public override Control GetSettingUI()
    {
        // 传入 this 使 ViewModel 可调用 TestAsync 进行接口校验
        _viewModel ??= new SettingsViewModel(Context, Settings, this);
        _settingUi ??= new SettingsView { DataContext = _viewModel };
        return _settingUi;
    }

    /// <summary>
    /// 测试接口连通性：发送一个简单的翻译请求（en -> zh, 文本 "hi"）。
    /// 返回译文字符串表示成功，抛出异常表示失败。
    /// 供设置页"校验"按钮调用。
    /// </summary>
    public async Task<string> TestAsync(CancellationToken cancellationToken = default)
    {
        // 构造测试请求：英文 "hi" -> 简体中文
        var request = new TranslateRequest("hi", LangEnum.English, LangEnum.ChineseSimplified);
        var result = new TranslateResult();

        await TranslateAsync(request, result, cancellationToken);

        if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Text))
        {
            throw new Exception(result.Text?.Trim() ?? "UApiPro 翻译返回空响应");
        }

        return result.Text.Trim();
    }

    /// <summary>
    /// 获取源语言代码：UApiPro 自动检测源语言，对所有语言返回非 null 表示支持。
    /// Auto 返回 "auto"，其他语言返回对应代码（API 实际不使用 source_lang 参数）。
    /// </summary>
    public override string? GetSourceLanguage(LangEnum langEnum) => langEnum switch
    {
        LangEnum.Auto => "auto",
        LangEnum.ChineseSimplified => "zh",
        LangEnum.ChineseTraditional => "zh-TW",
        LangEnum.Cantonese => "zh-TW",
        LangEnum.English => "en",
        LangEnum.Japanese => "ja",
        LangEnum.Korean => "ko",
        LangEnum.French => "fr",
        LangEnum.Spanish => "es",
        LangEnum.Russian => "ru",
        LangEnum.German => "de",
        LangEnum.Italian => "it",
        LangEnum.Turkish => "tr",
        LangEnum.PortuguesePortugal => "pt",
        LangEnum.PortugueseBrazil => "pt",
        LangEnum.Vietnamese => "vi",
        LangEnum.Indonesian => "id",
        LangEnum.Thai => "th",
        LangEnum.Malay => "ms",
        LangEnum.Arabic => "ar",
        LangEnum.Hindi => "hi",
        LangEnum.MongolianCyrillic => "mn",
        LangEnum.MongolianTraditional => "mn",
        LangEnum.Khmer => "km",
        LangEnum.NorwegianBokmal => "no",
        LangEnum.NorwegianNynorsk => "no",
        LangEnum.Persian => "fa",
        LangEnum.Swedish => "sv",
        LangEnum.Polish => "pl",
        LangEnum.Dutch => "nl",
        LangEnum.Ukrainian => "uk",
        _ => null,
    };

    /// <summary>
    /// 获取目标语言代码：映射到 UApiPro 支持的语言代码。
    /// </summary>
    public override string? GetTargetLanguage(LangEnum langEnum) => GetSourceLanguage(langEnum);

    public override void Init(IPluginContext context)
    {
        Context = context;
        Settings = context.LoadSettingStorage<Settings>();
        Context.Logger.LogInformation("[UApiPro] 插件初始化, ApiKey: {HasKey}", string.IsNullOrEmpty(Settings.ApiKey) ? "未配置" : "已配置");
    }

    public override void Dispose() { }

    public override async Task TranslateAsync(TranslateRequest request, TranslateResult result, CancellationToken cancellationToken = default)
    {
        // 校检：目标语言映射失败直接返回
        if (GetTargetLanguage(request.TargetLang) is not string targetLanguage)
        {
            result.Fail(Context.GetTranslation("UnsupportedTargetLang"));
            return;
        }

        try
        {
            Context.Logger.LogDebug("[UApiPro] 开始翻译: -> {Tgt}, 文本长度={Len}",
                targetLanguage, request.Text.Length);

            // 构建请求 URL：to_lang 作为 query 参数
            var url = $"{TranslateEndpoint}?to_lang={targetLanguage}";

            // 构建请求体：{"text": "原文"}
            var body = JsonSerializer.Serialize(new { text = request.Text });

            // 构建请求头
            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Accept", "application/json" },
            };

            // 如果配置了 API Key，添加认证头
            if (!string.IsNullOrWhiteSpace(Settings.ApiKey))
            {
                headers["Authorization"] = $"Bearer {Settings.ApiKey}";
            }

            var options = new Options
            {
                Headers = headers,
                ContentType = "application/json",
            };

            // 通过 Context.HttpService.PostAsync 发送 POST 请求
            // 复用宿主代理/SSL 配置
            var jsonStr = await Context.HttpService.PostAsync(url, body, options, cancellationToken);

            if (string.IsNullOrEmpty(jsonStr))
            {
                result.Fail("UApiPro 翻译返回空响应");
                return;
            }

            // 解析响应：{"translate": "译文", "text": "原文"}
            using var doc = JsonDocument.Parse(jsonStr);
            var root = doc.RootElement;

            // 检查错误响应：{"code": "...", "message": "..."}
            if (root.TryGetProperty("code", out var codeEl) && codeEl.ValueKind == JsonValueKind.String)
            {
                var errorCode = codeEl.GetString();
                var errorMsg = root.TryGetProperty("message", out var msgEl) ? msgEl.GetString() : "未知错误";
                Context.Logger.LogWarning("[UApiPro] 翻译失败, code={Code}, message={Msg}", errorCode, errorMsg);
                result.Fail($"UApiPro 错误: {errorMsg}");
                return;
            }

            // 成功响应：提取 translate 字段
            if (!root.TryGetProperty("translate", out var translateEl) || translateEl.ValueKind != JsonValueKind.String)
            {
                Context.Logger.LogWarning("[UApiPro] 响应中缺少 translate 字段, 原始: {Raw}",
                    jsonStr[..Math.Min(200, jsonStr.Length)]);
                result.Fail("UApiPro 翻译返回格式异常");
                return;
            }

            var translated = translateEl.GetString()?.Trim();

            if (string.IsNullOrEmpty(translated))
            {
                result.Fail("UApiPro 翻译返回空译文");
                return;
            }

            Context.Logger.LogInformation("[UApiPro] 翻译成功, 译文长度={Len}", translated.Length);
            result.Success(translated);
        }
        catch (OperationCanceledException)
        {
            // 用户主动取消或宿主超时：不记录错误，不显示失败信息
            Context.Logger.LogInformation("[UApiPro] 翻译请求已取消");
            throw;
        }
        catch (Exception ex)
        {
            Context.Logger.LogError(ex, "[UApiPro] 翻译失败");
            result.Fail($"UApiPro 翻译失败: {ex.Message}");
        }
    }
}
