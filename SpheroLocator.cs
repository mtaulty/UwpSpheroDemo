namespace SpheroHelperLibrary
{
  using RobotKit;
  using System;
  using System.Threading.Tasks;
  using Windows.UI;

  public static class SpheroLocator
  {
    public static async Task<SpheroBall> LocateFirstSpheroAsync(
      TimeSpan timeout,
      bool replaceContext = true)
    {
      SpheroBall ball = await DiscoverFirstSpheroAsync(timeout);
      await ConnectDiscoveredSpheroAsync(ball, timeout, replaceContext);

      return (ball);
    }
    static async Task<SpheroBall> DiscoverFirstSpheroAsync(TimeSpan timeout)
    {
      RobotProvider provider = RobotProvider.GetSharedProvider();
      SpheroBall spheroBall = null;

      TaskCompletionSource<bool> completionSource =
        new TaskCompletionSource<bool>();

      EventHandler<RobotEventArgs> robotsHandler = null;

      robotsHandler = (s, args) =>
      {
        spheroBall = new SpheroBall(args.Robot as Sphero);

        completionSource.SetResult(true);
      };
      provider.DiscoveredRobotEvent += robotsHandler;

      provider.FindRobots();

      await Task.WhenAny(completionSource.Task, Task.Delay(timeout));
      provider.DiscoveredRobotEvent -= robotsHandler;

      return (spheroBall);
    }
    static async Task ConnectDiscoveredSpheroAsync(SpheroBall spheroBall, TimeSpan timeout,
      bool replaceContext)
    {
      RobotProvider provider = RobotProvider.GetSharedProvider();

      TaskCompletionSource<bool> completionSource =
        new TaskCompletionSource<bool>();

      EventHandler<RobotEventArgs> robotsHandler = null;

      robotsHandler = (s, args) =>
      {
        spheroBall.AddCollisionHandlers();
        completionSource.SetResult(true);
      };
      provider.ConnectedRobotEvent += robotsHandler;

      AsyncExceptionSwallowingContext ctx = new AsyncExceptionSwallowingContext();

      if (replaceContext)
      {
        ctx.Replace();
      }
      provider.ConnectRobot(spheroBall.Sphero);

      if (replaceContext)
      {
        ctx.Replace();
      }
      await Task.WhenAny(completionSource.Task, Task.Delay(timeout));

      provider.ConnectedRobotEvent -= robotsHandler;
    }
  }
}