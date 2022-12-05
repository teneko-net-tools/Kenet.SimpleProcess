namespace Kenet.SimpleProcess.Test;

public class ProcessBoundaryTests
{
    [Fact]
    public void Ensure_default_is_faulted()
    {
        default(ProcessBoundary).IsFaulted.Should().BeTrue();
    }

    [Fact]
    public void Ensure_initialized_is_not_faulted()
    {
        new ProcessBoundary().IsFaulted.Should().BeFalse();
    }

    [Fact]
    public void Ensure_initialized_is_not_disposed()
    {
        new ProcessBoundary().IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void Ensure_initialized_is_disposed()
    {
        var boundary = new ProcessBoundary();
        boundary.Dispose();
        boundary.IsDisposed.Should().BeTrue();
    }
}
