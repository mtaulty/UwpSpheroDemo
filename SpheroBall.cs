namespace SpheroHelperLibrary
{
  using RobotKit;
  using System;
  using System.Threading.Tasks;
  using Windows.UI;

  public class SpheroBallCrashedEventArgs : EventArgs
  {
    public SpheroBallCrashedEventArgs(float powerX,
      float powerY, float speed)
    {
      this.PowerX = powerX;
      this.PowerY = powerY;
      this.Speed = speed;
    }
    public float PowerX { get; }
    public float PowerY { get; }
    public float Speed { get; }
  }
  public class SpheroBall
  {
    public event EventHandler<SpheroBallCrashedEventArgs> Crashed;

    public SpheroBall(Sphero sphero)
    {
      this.sphero = sphero;
      this.sphero.OnConnectionStateChanged += this.OnSpheroConnectionStateChanged;
    }
    void OnCollisionDetected(object sender, CollisionData e)
    {
      if (this.Crashed != null)
      {
        this.Crashed(this, new SpheroBallCrashedEventArgs(
          e.CollisionPower.X, e.CollisionPower.Y, e.SpheroSpeed));
      }
    }
    public Sphero Sphero
    {
      get
      {
        return (this.sphero);
      }
    }
    public void Stop()
    {
      this.speed = 0.0f;
      this.Roll();
    }
    public void Forward(double speed)
    {
      this.speed = (float)speed / 10.0f;
      this.Roll();
    }
    public void IncrementalClockwiseTurn(int angle)
    {
      this.angle = ((this.angle + angle) % 360);
      this.Roll();
    }
    public void IncrementalAnticlockwiseTurn(int angle)
    {
      this.angle = (this.angle - angle);
      if (this.angle < 0)
      {
        this.angle = 360 + this.angle;
      }
      this.Roll();
    }
    public void ChangeHeading(int angle)
    {
      this.angle = angle;
      this.Roll();
    }
    public void SwitchBacklightLED(bool on)
    {
      this.sphero.SetBackLED(on ? 1.0f : 0.0f);
    }
    public async Task FlashRedGreenBlueAsync(TimeSpan timeSpan)
    {
      await this.FlashAsync(Colors.Red, timeSpan);
      await this.FlashAsync(Colors.Green, timeSpan);
      await this.FlashAsync(Colors.Blue, timeSpan);
    }
    public async Task FlashAsync(Color color, TimeSpan timeSpan)
    {
      this.sphero.SetRGBLED(color.R, color.G, color.B);
      await Task.Delay(timeSpan);
    }
    void Roll()
    {
      // NB: using roll for both rotate and drive as SetHeading doesn't
      // seem to work for me.
      this.sphero.Roll(this.angle, this.speed);
    }
    void OnSpheroConnectionStateChanged(Robot sender, ConnectionState state)
    {
      var on = false;
      switch (state)
      {
        case ConnectionState.Failed:
          break;
        case ConnectionState.Disconnected:
          this.SwitchBacklightLED(false);
          break;
        case ConnectionState.Connecting:
          break;
        case ConnectionState.Connected:
          this.SwitchBacklightLED(true);
          break;
        default:
          break;
      }
      this.SwitchBacklightLED(on);
    }
    public void AddCollisionHandlers()
    {
      if (this.Sphero?.CollisionControl != null)
      {
        this.sphero.CollisionControl.CollisionDetectedEvent += OnCollisionDetected;
        this.sphero.CollisionControl.StartDetection(50, 30, 200, 0, 1000);
      }
    }
    public void SetColour(byte red, byte green, byte blue)
    {
      this.sphero.SetRGBLED(red, green, blue);
    }
    float speed;
    int angle;
    Sphero sphero;
  }
}
