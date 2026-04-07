using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace RentalClient;

public static class Designer
    {
        
        //Watermark Text
        public static readonly DependencyProperty IsMonitoringProperty =
            DependencyProperty.RegisterAttached("IsMonitoring", 
                typeof(bool), 
                typeof(Designer), 
                new UIPropertyMetadata(false, OnIsMonitoringChanged));

        public static void SetIsMonitoring(DependencyObject obj, bool value) => obj.SetValue(IsMonitoringProperty, value);
        public static bool GetIsMonitoring(DependencyObject obj) => (bool)obj.GetValue(IsMonitoringProperty);
        
        public static readonly DependencyProperty HasTextProperty =
            DependencyProperty.RegisterAttached("HasText",
                typeof(bool), 
                typeof(Designer), 
                new UIPropertyMetadata(false));

        public static void SetHasText(DependencyObject obj, bool value) => obj.SetValue(HasTextProperty, value);
        public static bool GetHasText(DependencyObject obj) => (bool)obj.GetValue(HasTextProperty);
        
        private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool isMonitoring = (bool)e.NewValue;
            
            if (d is TextBox tb)
            {
                if (isMonitoring) tb.TextChanged += TextBox_TextChanged;
                else tb.TextChanged -= TextBox_TextChanged;
            }
            else if (d is PasswordBox pb)
            {
                if (isMonitoring) pb.PasswordChanged += PasswordBox_PasswordChanged;
                else pb.PasswordChanged -= PasswordBox_PasswordChanged;
            }
        }
        
        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
                SetHasText(tb, tb.Text.Length > 0);
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox pb)
                SetHasText(pb, pb.Password.Length > 0);
        }
        
        
        //TextBox Border Radius
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.RegisterAttached(
                "CornerRadius", 
                typeof(CornerRadius), 
                typeof(Designer), 
                new PropertyMetadata(new CornerRadius(0)));

        public static void SetCornerRadius(DependencyObject element, CornerRadius value) => element.SetValue(CornerRadiusProperty, value);

        public static CornerRadius GetCornerRadius(DependencyObject element) => (CornerRadius)element.GetValue(CornerRadiusProperty);
        
    }