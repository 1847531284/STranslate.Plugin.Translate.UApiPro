using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace STranslate.Plugin.Translate.UApiPro.ViewModel;

public partial class SettingsViewModel(IPluginContext context, Settings settings, Main plugin) : ObservableObject
{
    private bool _isTesting;

    [ObservableProperty]
    private string _apiKey = settings.ApiKey;

    [ObservableProperty]
    private string _validateResult = string.Empty;

    partial void OnApiKeyChanged(string value)
    {
        settings.ApiKey = value;
        context.SaveSettingStorage<Settings>();
    }

    [RelayCommand(CanExecute = nameof(CanTest))]
    private async Task TestAsync()
    {
        _isTesting = true;
        ValidateResult = string.Empty;
        TestCommand.NotifyCanExecuteChanged();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        try
        {
            await plugin.TestAsync(cts.Token);
            ValidateResult = Translate("Success");
        }
        catch (OperationCanceledException)
        {
            ValidateResult = Translate("Timeout");
        }
        catch (Exception ex)
        {
            ValidateResult = Translate("Fail");
            context.Logger.LogError(ex, ValidateResult);
        }
        finally
        {
            _isTesting = false;
            TestCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanTest() => !_isTesting;

    private string Translate(string suffix) =>
        context.GetTranslation($"STranslate_Plugin_Translate_UApiPro_Validate_{suffix}");
}
