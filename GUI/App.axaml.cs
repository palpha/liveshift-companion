using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Core.Image;
using Core.Usb;
using GUI.ViewModels;
using GUI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace GUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            var services = new ServiceCollection();
            services
                .AddLogging()
                .AddSingleton<IStreamer, Streamer>()
                .AddTransient<ICaptureEventSource, DefaultCaptureEventSource>()
                .AddSingleton<IPush2Usb, Push2Usb>()
                .AddTransient<IImageConverter, ImageConverter>()
                .AddTransient<ILibUsbWrapper, DefaultLibUsbWrapper>()
                .AddTransient<MainWindowViewModel>();
            var model = services.BuildServiceProvider().GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = model
            };

            desktop.MainWindow.CanResize = false;
            desktop.MainWindow.Width = 960;
            desktop.MainWindow.Height = 400;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}