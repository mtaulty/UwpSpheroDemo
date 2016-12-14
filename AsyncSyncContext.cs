namespace SpheroHelperLibrary
{
  using System;
  using System.Threading;

  /// <summary>
  ///  Mostly taken from this article 
  /// http://www.markermetro.com/2013/01/technical/handling-unhandled-exceptions-with-asyncawait-on-windows-8-and-windows-phone-8/
  /// </summary>
  public class AsyncExceptionSwallowingContext : SynchronizationContext
  {
    private SynchronizationContext _syncContext;

    public AsyncExceptionSwallowingContext()
    {

    }
    public void Replace()
    {
      var syncContext = Current;

      if (syncContext == null)
        throw new InvalidOperationException(
          "Ensure a synchronization context exists before calling this method.");

      var customSynchronizationContext = syncContext as AsyncExceptionSwallowingContext;

      if (customSynchronizationContext == null)
      {
        customSynchronizationContext = new AsyncExceptionSwallowingContext(syncContext);
        SetSynchronizationContext(customSynchronizationContext);
      }
      else
      {
        SetSynchronizationContext(_syncContext);
      }
    }
    public AsyncExceptionSwallowingContext(SynchronizationContext syncContext)
    {
      _syncContext = syncContext;
    }

    public override SynchronizationContext CreateCopy()
    {
      return new AsyncExceptionSwallowingContext(_syncContext.CreateCopy());
    }

    public override void OperationCompleted()
    {
      _syncContext.OperationCompleted();
    }

    public override void OperationStarted()
    {
      _syncContext.OperationStarted();
    }

    public override void Post(SendOrPostCallback d, object state)
    {
      _syncContext.Post(WrapCallback(d), state);
    }

    public override void Send(SendOrPostCallback d, object state)
    {
      _syncContext.Send(d, state);
    }

    private static SendOrPostCallback WrapCallback(SendOrPostCallback sendOrPostCallback)
    {
      return state =>
      {
        try
        {
          sendOrPostCallback(state);
        }
        catch
        {
        }
      };
    }
  }
}
