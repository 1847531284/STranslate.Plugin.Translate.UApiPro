using System.Windows.Controls;

namespace STranslate.Plugin.Translate.UApiPro.View;

/// <summary>
/// 设置页 UI：使用 iNKORE SettingsCard 卡片式布局（与内置插件及 OpenAICompetitive 一致）。
///   - API Key 卡片：PasswordBox 通过 PasswordBoxAssistant 直接双向绑定 ViewModel.ApiKey
///   - 官网卡片：HyperlinkButton 超链接按钮
///   - 校验卡片：AccentButton 样式按钮 + IconAndText
/// </summary>
public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }
}
