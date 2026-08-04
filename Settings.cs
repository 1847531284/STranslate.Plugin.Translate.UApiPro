namespace STranslate.Plugin.Translate.UApiPro;

/// <summary>
/// 插件设置：UApiPro 翻译服务配置。
/// ApiKey: 在 https://uapis.cn/console 获取，通过 Authorization: Bearer 头部传入。
/// </summary>
public class Settings
{
    /// <summary>
    /// UApiPro API Key（在 https://uapis.cn/console 注册获取）。
    /// 付费接口需要 Key 才能调用；免费层可不填（按 IP 限流）。
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
