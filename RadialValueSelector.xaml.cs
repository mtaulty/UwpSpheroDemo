namespace SpheroHelperLibrary
{
  using System;
  using System.Diagnostics;
  using Windows.Foundation;
  using Windows.UI.Input;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Input;
  using System.Linq;
  using System.Collections.Generic;

  public sealed partial class RadialValueSelector : UserControl
  {
    public RadialValueSelector()
    {
      this.InitializeComponent();
      this.SizeChanged += OnSizeChanged;
    }
    public static DependencyProperty ValueProperty =
      DependencyProperty.Register("Value", typeof(double), typeof(RadialValueSelector),
        new PropertyMetadata(0.0d, OnValueChanged));

    public static DependencyProperty MinAngleProperty =
      DependencyProperty.Register("MinAngle", typeof(double), typeof(RadialValueSelector),
        new PropertyMetadata(225.0d, OnDPAffectingPositionChanged));

    public static DependencyProperty MaxAngleProperty =
      DependencyProperty.Register("MaxAngle", typeof(double), typeof(RadialValueSelector),
        new PropertyMetadata(315.0d, OnDPAffectingPositionChanged));

    public static DependencyProperty MinimumProperty =
      DependencyProperty.Register("Minimum", typeof(double), typeof(RadialValueSelector),
        new PropertyMetadata(0.0d, OnDPAffectingPositionChanged));

    public static DependencyProperty MaximumProperty =
      DependencyProperty.Register("Maximum", typeof(double), typeof(RadialValueSelector),
        new PropertyMetadata(0.0d, OnDPAffectingPositionChanged));

    public static DependencyProperty ThumbContentProperty =
      DependencyProperty.Register("ThumbContent", typeof(object), typeof(RadialValueSelector),
        new PropertyMetadata(null, OnDPAffectingPositionChanged));

    public static DependencyProperty IncrementProperty =
      DependencyProperty.Register("Increment", typeof(double), typeof(RadialValueSelector),
        new PropertyMetadata(1.0d, null));

    public static DependencyProperty EnumTypeNameProperty =
      DependencyProperty.Register("EnumTypeName", typeof(string), typeof(RadialValueSelector),
        new PropertyMetadata(string.Empty, OnEnumTypeNameChanged));

    public static DependencyProperty EnumValueNameProperty =
      DependencyProperty.Register("EnumValueName", typeof(string), typeof(RadialValueSelector),
        null);

    public string EnumValueName
    {
      get
      {
        return ((string)base.GetValue(EnumValueNameProperty));
      }
      set
      {
        base.SetValue(EnumValueNameProperty, value);
      }
    }
    public string EnumTypeName
    {
      get
      {
        return ((string)base.GetValue(EnumTypeNameProperty));
      }
      set
      {
        base.SetValue(EnumTypeNameProperty, value);
      }
    }
    public double Increment
    {
      get
      {
        return ((double)base.GetValue(IncrementProperty));
      }
      set
      {
        base.SetValue(IncrementProperty, value);
      }
    }
    public double Value
    {
      get
      {
        return ((double)base.GetValue(ValueProperty));
      }
      set
      {
        base.SetValue(ValueProperty, value);
      }
    }
    public double MinAngle
    {
      get
      {
        return ((double)base.GetValue(MinAngleProperty));
      }
      set
      {
        base.SetValue(MinAngleProperty, value);
      }
    }
    public double MaxAngle
    {
      get
      {
        return ((double)base.GetValue(MaxAngleProperty));
      }
      set
      {
        base.SetValue(MaxAngleProperty, value);
      }
    }
    public double Minimum
    {
      get
      {
        return ((double)base.GetValue(MinimumProperty));
      }
      set
      {
        base.SetValue(MinimumProperty, value);
      }
    }
    public double Maximum
    {
      get
      {
        return ((double)base.GetValue(MaximumProperty));
      }
      set
      {
        base.SetValue(MaximumProperty, value);
      }
    }
    public object ThumbContent
    {
      get
      {
        return (base.GetValue(ThumbContentProperty));
      }
      set
      {
        base.SetValue(ThumbContentProperty, value);
      }
    }
    double ValueRange
    {
      get
      {
        return (this.Maximum - this.Minimum);
      }
    }
    double AngleRange
    {
      get
      {
        return (this.MaxAngle - this.MinAngle);
      }
    }
    double RadialLength
    {
      get
      {
        return (this.innerRadius.ActualWidth / 2.0d);
      }
    }
    Point InnerRadiusCenter
    {
      get
      {
        return (
          new Point(
            ((this.outerRadialGrid.ActualWidth - this.innerRadius.ActualWidth) / 2.0d) + this.RadialLength,
            ((this.outerRadialGrid.ActualHeight - this.innerRadius.ActualHeight) / 2.0d) + this.RadialLength));
      }
    }
    static void OnEnumTypeNameChanged(DependencyObject sender,
      DependencyPropertyChangedEventArgs args)
    {
      RadialValueSelector control = (RadialValueSelector)sender;
      control.enumType = null;
      control.enumDetails = null;

      string newValue = args.NewValue as string;
      if (!string.IsNullOrEmpty(newValue))
      {
        control.enumType = Type.GetType(newValue);
        control.enumDetails = new List<Tuple<string, double>>();
        var values = Enum.GetValues(control.enumType);
        var names = Enum.GetNames(control.enumType);

        int i = 0;
        foreach (var item in values)
        {
          control.enumDetails.Add(Tuple.Create(names[i], (double)(int)item));
          i++;
        }
        control.Minimum = control.enumDetails.Min(cd => cd.Item2);
        control.Maximum = control.enumDetails.Max(cd => cd.Item2);
        control.Value = 0.0d;
        control.Increment = 1.0d; // assumption.
        control.SetEnumValueToClosestMatch();
      }
    }
    void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.PositionBasedOnValue();
    }
    static void OnValueChanged(DependencyObject sender,
      DependencyPropertyChangedEventArgs args)
    {
      RadialValueSelector control = (RadialValueSelector)sender;
      control.PositionBasedOnValue();

      control.SetEnumValueToClosestMatch();
    }
    void SetEnumValueToClosestMatch()
    {
      if (this.enumDetails != null)
      {
        double min = double.MaxValue;
        string name = string.Empty;

        // can never figure out how to do this with min.
        foreach (var item in this.enumDetails)
        {
          if (Math.Abs(item.Item2 - this.Value) < min)
          {
            name = item.Item1;
            min = Math.Abs(item.Item2 - this.Value);
          }
        }
        this.EnumValueName = name;
      }
    }
    static void OnDPAffectingPositionChanged(DependencyObject sender,
      DependencyPropertyChangedEventArgs args)
    {
      RadialValueSelector control = (RadialValueSelector)sender;
      control.PositionBasedOnValue();
    }
    void PositionBasedOnValue()
    {
      this.PositionThumbForAngle(this.GetAngleFromValue(this.Value));
    }
    double GetAngleFromValue(double value)
    {
      return (
        ((value - this.Minimum) /
        this.ValueRange *
        this.AngleRange) + this.MinAngle);
    }
    double GetValueFromAngle(double angle)
    {
      double unCorrectedValue =
        ((angle - this.MinAngle) /
          this.AngleRange *
          this.ValueRange) + this.Minimum;

      // Nudge the value to its nearest multiple of the increment property.
      double rem = unCorrectedValue % this.Increment;
      double div = Math.Floor(unCorrectedValue / this.Increment);

      if (rem < (this.Increment / 2.0d))
      {
        unCorrectedValue = div * this.Increment;
      }
      else
      {
        unCorrectedValue = (div + 1) * this.Increment;
      }
      return (unCorrectedValue);
    }
    void PositionThumbForAngle(double angleDegrees)
    {
      Point thumbPositionPoint = new Point(
        this.InnerRadiusCenter.X + this.RadialLength * Math.Sin(DegreesToRadians(angleDegrees)),
        this.InnerRadiusCenter.Y + this.RadialLength * Math.Cos(DegreesToRadians(angleDegrees)));

      Size thumbSize = new Size(this.thumbGrid.ActualWidth, this.thumbGrid.ActualHeight);

      this.thumbGridTranslate.X = thumbPositionPoint.X - this.InnerRadiusCenter.X;
      this.thumbGridTranslate.Y = (0 - (thumbPositionPoint.Y - this.InnerRadiusCenter.Y));
    }
    void OnThumbPointerPressed(object sender, PointerRoutedEventArgs e)
    {
      // Capture the pointer in case the user wanders off with it.
      this.CapturePointer(e.Pointer);
      this.PointerCaptureLost += this.OnPointerCaptureLost;
      this.dragging = true;
    }
    void OnPointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
      this.dragging = false;
    }
    void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
      if (this.dragging)
      {
        PointerPoint pointerPointRelativeToInnerRadius = e.GetCurrentPoint(this.innerRadius);

        Point positionRelativeToInnerRadiusCenter = new Point(
          pointerPointRelativeToInnerRadius.Position.X - this.RadialLength,
          pointerPointRelativeToInnerRadius.Position.Y - this.RadialLength);

        positionRelativeToInnerRadiusCenter.Y = 0 - positionRelativeToInnerRadiusCenter.Y;

        double angleRadians = Math.Atan(
          positionRelativeToInnerRadiusCenter.X / positionRelativeToInnerRadiusCenter.Y);

        double angleDegrees = RadiansToDegrees(angleRadians);
        double increment = 0.0d;

        if (positionRelativeToInnerRadiusCenter.Y <= 0.0d)
        {
          increment = 180.0d;
        }
        else if ((positionRelativeToInnerRadiusCenter.Y >= 0.0d) &&
          (positionRelativeToInnerRadiusCenter.X <= 0.0d))
        {
          increment = 360.0d;
        }
        angleDegrees += increment;

        if ((angleDegrees >= this.MinAngle) &&
          (angleDegrees <= this.MaxAngle))
        {
          this.Value = this.GetValueFromAngle(angleDegrees);
        }
      }
    }
    void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
      this.dragging = false;
    }
    static double RadiansToDegrees(double radians)
    {
      return (radians / Math.PI * 180.0d);
    }
    static double DegreesToRadians(double degrees)
    {
      return (degrees * Math.PI / 180.0d);
    }
    bool IsDisplayingEnum
    {
      get
      {
        return (this.enumType != null);
      }
    }
    void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this.PositionBasedOnValue();
    }
    List<Tuple<string, double>> enumDetails;
    bool dragging = false;
    Type enumType = null;
  }
}
