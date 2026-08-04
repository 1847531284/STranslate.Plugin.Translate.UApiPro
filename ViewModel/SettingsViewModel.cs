using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace STranslate.Plugin.Translate.UApiPro.ViewModel;

/// <summary>
/// 设置页 ViewModel：使用 iNKORE SettingsCard 卡片式 UI（与内置插件及 OpenAICompetitive 一致）。
///   - ApiKey 通过 PasswordBoxAssistant 直接双向绑定
///   - 卡片标题/描述通过 Context.GetTranslation 获取本地化字符串
///   - TestCommand 校验接口连通性，通过 Context.Snackbar 反馈结果
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly IPluginContext _context;
    private readonly Settings _settings;
    private readonly Main _plugin;

    public SettingsViewModel(IPluginContext context, Settings settings, Main plugin)
    {
        _context = context;
        _settings = settings;
        _plugin = plugin;
        _apiKey = settings.ApiKey;

        // 初始化本地化标签（Header / Description）
        ApiKeyLabel = context.GetTranslation("STranslate_Plugin_Translate_UApiPro_ApiKey");
        ApiKeyDescription = context.GetTranslation("STranslate_Plugin_Translate_UApiPro_ApiKey_Description");
        ApiKeyPlaceholder = context.GetTranslation("STranslate_Plugin_Translate_UApiPro_ApiKey_Placeholder");
        WebsiteLabel = context.GetTranslation("STranslate_Plugin_Translate_UApiPro_Website");
        WebsiteDescription = context.GetTranslation("STranslate_Plugin_Translate_UApiPro_Website_Description");
        VerifyLabel = context.GetTranslation("STranslate_Plugin_Translate_UApiPro_Verify");
        VerifyDescription = context.GetTranslation("STranslate_Plugin_Translate_UApiPro_Verify_Description");
        UpdateTestButtonText();
    }

    /// <summary>
    /// API Key：通过 PasswordBoxAssistant 与 PasswordBox 双向绑定。
    /// </summary>
    [ObservableProperty]
    private string _apiKey;

    /// <summary>
    /// 是否正在测试中：控制按钮禁用状态与文案
    /// </summary>
    [ObservableProperty]
    private bool _isTesting;

    // ===== SettingsCard Header / Description（本地化字符串） =====
    public string ApiKeyLabel { get; }
    public string ApiKeyDescription { get; }
    public string ApiKeyPlaceholder { get; }
    public string WebsiteLabel { get; }
    public string WebsiteDescription { get; }
    public string VerifyLabel { get; }
    public string VerifyDescription { get; }

    /// <summary>
    /// 校验按钮文案：测试中显示"测试中..."，否则显示"校验"
    /// </summary>
    [ObservableProperty]
    private string _testButtonText = "校验";

    private void UpdateTestButtonText()
    {
        TestButtonText = IsTesting
            ? _context.GetTranslation("STranslate_Plugin_Translate_UApiPro_Testing")
            : _context.GetTranslation("STranslate_Plugin_Translate_UApiPro_Test");
    }

    /// <summary>
    /// 校验命令：调用插件 TestAsync 发送测试翻译请求，通过 Snackbar 反馈结果
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanTest))]
    private async Task TestAsync()
    {
        IsTesting = true;
        UpdateTestButtonText();
        TestCommand.NotifyCanExecuteChanged();

        // 测试用取消令牌：15s 超时（UApiPro 响应可能较慢）
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        try
        {
            var translated = await _plugin.TestAsync(cts.Token);
            _context.Snackbar.ShowSuccess($"校验成功，测试译文: {translated}");
        }
        catch (OperationCanceledException)
        {
            _context.Snackbar.ShowWarning("校验超时，请检查网络或 API Key 是否正确");
        }
        catch (Exception ex)
        {
            _context.Snackbar.ShowError($"校验失败: {ex.Message}");
        }
        finally
        {
            IsTesting = false;
            UpdateTestButtonText();
            TestCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// 测试中时禁用按钮，避免重复点击
    /// </summary>
    private bool CanTest() => !IsTesting;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(ApiKey))
        {
            _settings.ApiKey = ApiKey;
            _context.SaveSettingStorage<Settings>();
        }
        else if (e.PropertyName == nameof(IsTesting))
        {
            // IsTesting 变化时刷新命令可用状态
            TestCommand.NotifyCanExecuteChanged();
        }
    }
}
