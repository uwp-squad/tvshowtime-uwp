﻿using Microsoft.Graphics.Canvas.Effects;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace TVShowTime.UWP.Controls
{
    public sealed partial class ExploreItemControl : UserControl
    {
        #region Constructor

        public ExploreItemControl()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Events

        private void OnFrostedGlassPanelLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is UIElement frostedGlass)
            {
                InitializeFrostedGlass(frostedGlass);
            }
        }

        #endregion

        #region Methods

        private void InitializeFrostedGlass(UIElement glassHost)
        {
            // Check if CompositionBackdropBrush is supported
            if (!ApiInformation.IsTypePresent("Windows.UI.Composition.CompositionBackdropBrush"))
            {
                return;
            }

            var hostVisual = ElementCompositionPreview.GetElementVisual(glassHost);
            var compositor = hostVisual.Compositor;

            // Create a glass effect, requires Win2D NuGet package
            var glassEffect = new GaussianBlurEffect
            {
                BlurAmount = 15.0f,
                BorderMode = EffectBorderMode.Hard,
                Source = new ArithmeticCompositeEffect
                {
                    MultiplyAmount = 0,
                    Source1Amount = 0.75f,
                    Source2Amount = 0.25f,
                    Source1 = new CompositionEffectSourceParameter("backdropBrush"),
                    Source2 = new ColorSourceEffect
                    {
                        Color = Color.FromArgb(255, 245, 245, 245)
                    }
                }
            };

            //  Create an instance of the effect and set its source to a CompositionBackdropBrush
            var effectFactory = compositor.CreateEffectFactory(glassEffect);
            var backdropBrush = compositor.CreateBackdropBrush();
            var effectBrush = effectFactory.CreateBrush();

            effectBrush.SetSourceParameter("backdropBrush", backdropBrush);

            // Create a Visual to contain the frosted glass effect
            var glassVisual = compositor.CreateSpriteVisual();
            glassVisual.Brush = effectBrush;

            // Add the blur as a child of the host in the visual tree
            ElementCompositionPreview.SetElementChildVisual(glassHost, glassVisual);

            // Make sure size of glass host and glass visual always stay in sync
            var bindSizeAnimation = compositor.CreateExpressionAnimation("hostVisual.Size");
            bindSizeAnimation.SetReferenceParameter("hostVisual", hostVisual);

            glassVisual.StartAnimation("Size", bindSizeAnimation);
        }

        #endregion
    }
}
