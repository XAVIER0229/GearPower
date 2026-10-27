using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace GearPower
{
    /// <summary>
    /// Neumorphic keycap behaviour for the four top-level tabs. The visual tree
    /// is supplied by NeumorphicTabTemplate in ui.xaml; this class drives the
    /// pressed/lit state, hover lift, 2 px sink and rebound animation.
    /// </summary>
    public static class NeumorphicTabs
    {
        private sealed class State
        {
            public Button Tab;
            public bool Active;
            public bool Attached;
            public bool Hover;
            public bool FlatInactive;
            public Border Keycap;
            public Border Highlight;
            public Border Inset;
            public Ellipse Led;
            public TranslateTransform Lift;
            public DropShadowEffect Shadow;
        }

        private static readonly Dictionary<Button, State> States =
            new Dictionary<Button, State>();

        public static void Attach(Button tab)
        {
            if (tab == null) return;
            State state;
            if (States.TryGetValue(tab, out state)) return;
            state = new State();
            state.Tab = tab;
            state.Active = false;
            States[tab] = state;
            tab.Loaded += (s, e) => Build(state);
            tab.Unloaded += (s, e) => state.Attached = false;
            tab.MouseEnter += (s, e) => OnEnter(state);
            tab.MouseLeave += (s, e) => OnLeave(state);
            tab.PreviewMouseLeftButtonDown += (s, e) => OnDown(state);
            tab.PreviewMouseLeftButtonUp += (s, e) => OnUp(state);
            Build(state);
        }

        public static void AttachFlat(Button tab)
        {
            Attach(tab);
            State state;
            if (States.TryGetValue(tab, out state))
            {
                state.FlatInactive = true;
                if (state.Attached) ApplyRest(state);
            }
        }

        public static void SetActive(Button tab, bool active)
        {
            if (tab == null) return;
            Attach(tab);
            State state = States[tab];
            state.Active = active;
            if (state.Attached) ApplyRest(state);
        }

        private static void Build(State state)
        {
            Button tab = state.Tab;
            if (tab == null) return;
            tab.ApplyTemplate();
            state.Keycap = tab.Template == null ? null : tab.Template.FindName("Keycap", tab) as Border;
            state.Highlight = tab.Template == null ? null : tab.Template.FindName("Highlight", tab) as Border;
            state.Inset = tab.Template == null ? null : tab.Template.FindName("Inset", tab) as Border;
            state.Led = tab.Template == null ? null : tab.Template.FindName("Led", tab) as Ellipse;
            state.Lift = tab.Template == null ? null : tab.Template.FindName("Lift", tab) as TranslateTransform;
            if (state.Keycap == null || state.Inset == null || state.Led == null || state.Lift == null)
                return;

            // Template effects are frozen by WPF. Replace them with unfrozen
            // instances before animating shadow properties.
            var shadow = new DropShadowEffect();
            shadow.Color = ThemeManager.IsDark
                ? Color.FromRgb(0x05, 0x04, 0x03)
                : Color.FromRgb(0xBF, 0xAE, 0x9F);
            shadow.BlurRadius = 20;
            shadow.ShadowDepth = 5;
            shadow.Direction = 45;
            shadow.Opacity = 0.70;
            state.Keycap.Effect = null;
            tab.Effect = shadow;
            state.Shadow = shadow;

            var highlight = new DropShadowEffect();
            highlight.Color = ThemeManager.IsDark
                ? Color.FromRgb(0x56, 0x4B, 0x42)
                : Colors.White;
            highlight.BlurRadius = 18;
            highlight.ShadowDepth = 6;
            highlight.Direction = 225;
            highlight.Opacity = ThemeManager.IsDark ? 0.45 : 1.0;
            if (state.Highlight != null)
            {
                state.Highlight.Effect = highlight;
                state.Highlight.Background = Theme.TabBar;
            }
            state.Inset.Background = BuildInsetBrush();
            state.Led.Fill = Theme.SwitchOn;
            var glow = new DropShadowEffect();
            glow.Color = Theme.SwitchOnColor;
            glow.BlurRadius = 8;
            glow.ShadowDepth = 0;
            glow.Opacity = 0.9;
            state.Led.Effect = glow;
            state.Attached = true;
            ApplyRest(state);
        }

        private static void ApplyRest(State state)
        {
            if (!state.Attached) return;
            state.Lift.BeginAnimation(TranslateTransform.YProperty, null);
            state.Inset.BeginAnimation(UIElement.OpacityProperty, null);
            if (state.Active)
            {
                state.Lift.Y = 2;
                state.Inset.Opacity = 1;
                state.Led.Visibility = Visibility.Visible;
                state.Keycap.Background = ThemeManager.IsDark
                    ? (Brush)new SolidColorBrush(Color.FromRgb(0x2B, 0x25, 0x20))
                    : new SolidColorBrush(Color.FromRgb(0xE4, 0xD5, 0xC8));
                state.Tab.Foreground = Theme.SwitchOn;
                if (state.Highlight != null) state.Highlight.Opacity = 0;
                SetShadow(state, 0, 0, 0);
            }
            else
            {
                state.Lift.Y = 0;
                state.Inset.Opacity = 0;
                state.Led.Visibility = Visibility.Collapsed;
                if (state.FlatInactive)
                {
                    state.Keycap.Background = Brushes.Transparent;
                    state.Tab.Foreground = Theme.TabInactiveForeground;
                    if (state.Highlight != null) state.Highlight.Opacity = 0;
                    SetShadow(state, 0, 0, 0);
                }
                else
                {
                    state.Keycap.Background = Theme.TabBar;
                    state.Tab.Foreground = Theme.TabInactiveForeground;
                    if (state.Highlight != null) state.Highlight.Opacity = 1.0;
                    SetShadow(state, 20, 5, 0.70);
                }
            }
        }

        private static void OnEnter(State state)
        {
            if (!state.Attached) return;
            state.Hover = true;
            if (state.Active) return;
            if (state.FlatInactive)
            {
                state.Keycap.Background = Theme.GhostHover;
                return;
            }
            AnimateY(state, -2, 120, null);
            SetShadow(state, 22, 7, 0.92);
        }

        private static void OnLeave(State state)
        {
            if (!state.Attached) return;
            state.Hover = false;
            if (state.Active) return;
            if (state.FlatInactive)
            {
                state.Keycap.Background = Brushes.Transparent;
                return;
            }
            AnimateY(state, 0, 170, Rebound());
            SetShadow(state, 18, 6, 0.85);
        }

        private static void OnDown(State state)
        {
            if (!state.Attached) return;
            AnimateY(state, 2, 80, null);
            AnimateOpacity(state.Inset, 1, 80);
            if (state.Highlight != null) state.Highlight.Opacity = 0;
            SetShadow(state, 0, 0, 0);
        }

        private static void OnUp(State state)
        {
            if (!state.Attached) return;
            if (state.Active)
            {
                ApplyRest(state);
                return;
            }
            AnimateY(state, state.Hover ? -2 : 0, 210, Rebound());
            AnimateOpacity(state.Inset, 0, 120);
            if (state.Highlight != null) state.Highlight.Opacity = 1.0;
            SetShadow(state, state.Hover ? 24 : 20, state.Hover ? 7 : 5, state.Hover ? 0.82 : 0.70);
        }

        private static void AnimateY(State state, double to, int ms, IEasingFunction ease)
        {
            var animation = new DoubleAnimation(to, TimeSpan.FromMilliseconds(ms));
            animation.EasingFunction = ease;
            animation.FillBehavior = FillBehavior.Stop;
            state.Lift.BeginAnimation(TranslateTransform.YProperty, animation);
            state.Lift.Y = to;
        }

        private static void AnimateOpacity(UIElement element, double to, int ms)
        {
            var animation = new DoubleAnimation(to, TimeSpan.FromMilliseconds(ms));
            animation.FillBehavior = FillBehavior.Stop;
            element.BeginAnimation(UIElement.OpacityProperty, animation);
            element.Opacity = to;
        }

        private static IEasingFunction Rebound()
        {
            return new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.35 };
        }

        private static void SetShadow(State state, double blur, double depth, double opacity)
        {
            if (state.Shadow == null) return;
            state.Shadow.BlurRadius = blur;
            state.Shadow.ShadowDepth = depth;
            state.Shadow.Opacity = opacity;
        }

        private static Brush BuildInsetBrush()
        {
            var brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(1, 1);
            if (ThemeManager.IsDark)
            {
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xAA, 0x05, 0x04, 0x03), 0));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x55, 0x05, 0x04, 0x03), 0.38));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 0x56, 0x4B, 0x42), 0.66));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x33, 0x56, 0x4B, 0x42), 1));
            }
            else
            {
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xCC, 0xB8, 0xA4, 0x93), 0));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x66, 0xB8, 0xA4, 0x93), 0.38));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF), 0.66));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xCC, 0xFF, 0xFF, 0xFF), 1));
            }
            return brush;
        }
    }
}
