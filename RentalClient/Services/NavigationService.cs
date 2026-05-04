using System;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace RentalClient.Services;

public interface INavigationService
{
    UserControl CurrentRootView { get; }
    event Action? RootViewChanged;
    void NavigateRootTo<TView>() where TView : UserControl;
    
    UserControl CurrentWorkplaceView { get; }
    event Action? WorkplaceViewChanged;
    void NavigateWorkplaceTo<TView>() where TView : UserControl;
}

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public UserControl CurrentRootView { get; private set; }
    public event Action? RootViewChanged;

    public UserControl CurrentWorkplaceView { get; private set; }
    public event Action? WorkplaceViewChanged;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateRootTo<TView>() where TView : UserControl
    {
        CurrentRootView = _serviceProvider.GetRequiredService<TView>();
        RootViewChanged?.Invoke();
    }

    public void NavigateWorkplaceTo<TView>() where TView : UserControl
    {
        CurrentWorkplaceView = _serviceProvider.GetRequiredService<TView>();
        WorkplaceViewChanged?.Invoke();
    }
}