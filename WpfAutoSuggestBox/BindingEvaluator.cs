using System.Windows;
using System.Windows.Data;

namespace WpfAutoSuggestBox
{
    public class BindingEvaluator : FrameworkElement
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(string), typeof(BindingEvaluator), new FrameworkPropertyMetadata(string.Empty));

        public BindingEvaluator(Binding binding)
        {
            ValueBinding = binding;
        }

        public string Value
        {
            get => (string)GetValue(ValueProperty);

            set => SetValue(ValueProperty, value);
        }

        public Binding ValueBinding { get; set; }

        public string Evaluate(object dataItem)
        {
            DataContext = dataItem;
            SetBinding(ValueProperty, ValueBinding);
            return Value;
        }
    }
}
