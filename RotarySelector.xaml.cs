namespace SpheroHelperLibrary
{
  using System;
  using Windows.UI.Xaml.Controls;

  public class RotarySelectorValueChangedEventArgs : EventArgs
  {
    public RotarySelectorValueChangedEventArgs(int value)
    {
      this.NewValue = value;
    }
    public int NewValue { get; private set; }
  }
  public sealed partial class RotarySelector : UserControl
  {
    public event EventHandler<RotarySelectorValueChangedEventArgs> RotationChanged;

    public RotarySelector()
    {
      this.InitializeComponent();
      this.DataContext = this;
    }
    public double Value
    {
      get
      {
        return (this.value);
      }
      set
      {
        if (this.value != value)
        {
          this.value = value;
          var handlers = this.RotationChanged;
          if (handlers != null)
          {
            handlers(this, new RotarySelectorValueChangedEventArgs((int)this.value));
          }
        }
      }
    }
    double value;
  }
}