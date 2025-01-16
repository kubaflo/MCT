﻿using System.Diagnostics;
using System.Runtime.Versioning;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Platform;

namespace CommunityToolkit.Maui.Core.Platform;

[UnsupportedOSPlatform("MacCatalyst")]
static partial class StatusBar
{
	/// <summary>
	/// Method to update the status bar size.
	/// </summary>
	public static void SetBarSize(bool isUsingSafeArea)
	{
		var statusBarTag = new IntPtr(38482);
		foreach (var window in UIApplication.SharedApplication.Windows)
		{
			var statusBar = window.ViewWithTag(statusBarTag);
			var statusBarFrame = window.WindowScene?.StatusBarManager?.StatusBarFrame;
			if (statusBarFrame is null)
			{
				continue;
			}

			statusBar ??= new UIView(statusBarFrame.Value);
			statusBar.Tag = statusBarTag;
			statusBar.Frame = isUsingSafeArea
				? new CGRect(statusBar.Frame.X, statusBar.Frame.Y, statusBar.Frame.Width, window.SafeAreaInsets.Top)
				: UIApplication.SharedApplication.StatusBarFrame;

			var statusBarSubViews = window.Subviews.Where(x => x.Tag == statusBarTag).ToList();
			foreach (var statusBarSubView in statusBarSubViews)
			{
				statusBarSubView.RemoveFromSuperview();
			}

			window.AddSubview(statusBar);

			TryUpdateStatusBarAppearance(window);
		}
	}

	static void PlatformSetColor(Color color)
	{
		var uiColor = color.ToPlatform();
		
		var statusBarTag = new IntPtr(38482);
		foreach (var window in UIApplication.SharedApplication.Windows)
		{
			var statusBar = window.ViewWithTag(statusBarTag);
			var statusBarFrame = window.WindowScene?.StatusBarManager?.StatusBarFrame;
			if (statusBarFrame is null)
			{
				continue;
			}

			// ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
			// window.ViewWithTag(tag) can return null
			var statusBarSize = statusBar?.Frame.Size;
			statusBar ??= new UIView(statusBarFrame.Value);
			statusBar.Tag = statusBarTag;
			statusBar.BackgroundColor = uiColor;
			statusBar.TintColor = uiColor;
			if (statusBarSize is null)
			{
				statusBar.Frame = new CGRect(statusBar.Frame.X, statusBar.Frame.Y, statusBar.Frame.Width, window.SafeAreaInsets.Top);
			}

			var statusBarSubViews = window.Subviews.Where(x => x.Tag == statusBarTag).ToList();
			foreach (var statusBarSubView in statusBarSubViews)
			{
				statusBarSubView.RemoveFromSuperview();
			}

			window.AddSubview(statusBar);

			TryUpdateStatusBarAppearance(window);
		}
	}

	static void PlatformSetStyle(StatusBarStyle statusBarStyle)
	{
		var uiStyle = statusBarStyle switch
		{
			StatusBarStyle.Default => UIStatusBarStyle.Default,
			StatusBarStyle.LightContent => UIStatusBarStyle.LightContent,
			StatusBarStyle.DarkContent => UIStatusBarStyle.DarkContent,
			_ => throw new NotSupportedException($"{nameof(StatusBarStyle)} {statusBarStyle} is not yet supported on iOS")
		};

		UIApplication.SharedApplication.SetStatusBarStyle(uiStyle, false);

		TryUpdateStatusBarAppearance();
	}

	static bool TryUpdateStatusBarAppearance()
	{
		if (OperatingSystem.IsIOSVersionAtLeast(13))
		{
			var didUpdateAllStatusBars = true;

			foreach (var window in UIApplication.SharedApplication.Windows)
			{
				didUpdateAllStatusBars &= TryUpdateStatusBarAppearance(window);
			}

			return didUpdateAllStatusBars;
		}
		else
		{
			var window = UIApplication.SharedApplication.KeyWindow;
			return TryUpdateStatusBarAppearance(window);
		}
	}

	static bool TryUpdateStatusBarAppearance(UIWindow? window)
	{
		var vc = window?.RootViewController ?? WindowStateManager.Default.GetCurrentUIViewController();

		if (vc is null)
		{
			Trace.WriteLine("Unable to update Status Bar Appearance because Current UIViewController is null");
			return false;
		}

		while (vc.PresentedViewController is not null)
		{
			vc = vc.PresentedViewController;
		}

		vc.SetNeedsStatusBarAppearanceUpdate();

		return true;
	}
}