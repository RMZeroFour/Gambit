using System.Windows.Threading;
using System;
using System.Timers;

namespace Gambit.UI.Services;

public interface ITimerService
{
    bool Running { get; }

    ITimerService SetInterval(TimeSpan interval);
    ITimerService SetCallback(Action callback);

    void Start();
    void Stop();
}

public class TimerService : ITimerService
{
    private Timer timer = new();

    public bool Running => timer.Enabled;

    public ITimerService SetCallback(Action callback)
    {
        timer.Elapsed += (_, _) => callback();
        return this;
    }

    public ITimerService SetInterval(TimeSpan interval)
    {
        timer.Interval = interval.TotalMilliseconds;
        return this;
    }

    public void Start() => timer.Start();
    public void Stop() => timer.Stop();
}

public class DispatcherTimerService : ITimerService
{
    private DispatcherTimer timer = new();

    public bool Running => timer.IsEnabled;

    public ITimerService SetCallback(Action callback)
    {
        timer.Tick += (_, _) => callback();
        return this;
    }

    public ITimerService SetInterval(TimeSpan interval)
    {
        timer.Interval = interval;
        return this;
    }

    public void Start() => timer.Start();
    public void Stop() => timer.Stop();
}