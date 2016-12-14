namespace SpheroHelperLibrary
{
  using Windows.UI.Core;
  using Windows.UI.Xaml;

  public enum AspectRatio
  {
    None,
    Portrait,
    Landscape
  }
  public class AspectRatioTrigger : StateTriggerBase
  {
    public static readonly DependencyProperty AspectRatioProperty =
      DependencyProperty.Register(
        "AspectRatio", typeof(AspectRatio), typeof(AspectRatioTrigger),
        new PropertyMetadata(AspectRatio.None, OnAspectRatioChanged));

    public AspectRatioTrigger()
    {

    }
    public AspectRatio AspectRatio
    {
      get
      {
        return (AspectRatio)base.GetValue(AspectRatioProperty);
      }
      set
      {
        base.SetValue(AspectRatioProperty, value);
      }
    }
    static void OnAspectRatioChanged(DependencyObject sender,
      DependencyPropertyChangedEventArgs args)
    {
      AspectRatioTrigger trigger = (AspectRatioTrigger)sender;

      if (trigger.window == null)
      {
        trigger.window = Window.Current;
        trigger.window.SizeChanged += trigger.OnSizeChanged;
        trigger.window.Activated += trigger.OnActivated;
      }
    }
    void OnActivated(object sender,
      WindowActivatedEventArgs args)
    {
      this.CheckActive();
    }
    void OnSizeChanged(object sender, 
      WindowSizeChangedEventArgs e)
    {
      this.CheckActive();
    }
    void CheckActive()
    {
      var bounds = Window.Current.Bounds;

      base.SetActive(
        ((this.AspectRatio == AspectRatio.Landscape) && (bounds.Width > bounds.Height)) ||
        ((this.AspectRatio == AspectRatio.Portrait) && (bounds.Width <= bounds.Height)));
    }
    Window window;
  }
}
