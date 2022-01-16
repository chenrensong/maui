﻿using Microsoft.Maui.Graphics;
using static Microsoft.Maui.Primitives.Dimension;

namespace Microsoft.Maui
{
	public static partial class ViewHandlerExtensions
	{
		internal static Size GetDesiredSizeFromHandler(this IViewHandler viewHandler, double widthConstraint, double heightConstraint)
		{
			var VirtualView = viewHandler.VirtualView;

			var nativeView = viewHandler.GetWrappedNativeView();

			if (nativeView == null || VirtualView == null)
			{
				return new Size(widthConstraint, heightConstraint);
			}

			var sizeThatFits = nativeView.SizeThatFits(new CoreGraphics.CGSize((float)widthConstraint, (float)heightConstraint));

			var size = new Size(
				sizeThatFits.Width == float.PositiveInfinity ? double.PositiveInfinity : sizeThatFits.Width,
				sizeThatFits.Height == float.PositiveInfinity ? double.PositiveInfinity : sizeThatFits.Height);

			if (double.IsInfinity(size.Width) || double.IsInfinity(size.Height))
			{
				nativeView.SizeToFit();
				size = new Size(nativeView.Frame.Width, nativeView.Frame.Height);
			}

			var finalWidth = ResolveConstraints(size.Width, VirtualView.Width, VirtualView.MinimumWidth, VirtualView.MaximumWidth);
			var finalHeight = ResolveConstraints(size.Height, VirtualView.Height, VirtualView.MinimumHeight, VirtualView.MaximumHeight);

			return new Size(finalWidth, finalHeight);

		}

		internal static void NativeArrangeHandler(this IViewHandler viewHandler, Rectangle rect)
		{
			var nativeView = viewHandler.GetWrappedNativeView();

			if (nativeView == null)
				return;

			// We set Center and Bounds rather than Frame because Frame is undefined if the CALayer's transform is 
			// anything other than the identity (https://developer.apple.com/documentation/uikit/uiview/1622459-transform)
			nativeView.Center = new CoreGraphics.CGPoint(rect.Center.X, rect.Center.Y);

			// The position of Bounds is usually (0,0), but in some cases (e.g., UIScrollView) it's the content offset.
			// So just leave it a whatever value iOS thinks it should be.
			nativeView.Bounds = new CoreGraphics.CGRect(nativeView.Bounds.X, nativeView.Bounds.Y, rect.Width, rect.Height);

			viewHandler.Invoke(nameof(IView.Frame), rect);
		}

		static double ResolveConstraints(double measured, double exact, double min, double max)
		{
			var resolved = measured;

			if (IsExplicitSet(exact))
			{
				// If an exact value has been specified, try to use that
				resolved = exact;
			}

			if (resolved > max)
			{
				// Apply the max value constraint (if any)
				// If the exact value is in conflict with the max value, the max value should win
				resolved = max;
			}

			if (resolved < min)
			{
				// Apply the min value constraint (if any)
				// If the exact or max value is in conflict with the min value, the min value should win
				resolved = min;
			}

			return resolved;
		}
	}
}
