using System.ComponentModel;
using FluentAssertions;

namespace Kenet.SimpleProcess.Test;

public class ProcessFailureTests
{
    [Fact]
    public void Process_start_should_throw_because_of_inexisting_executable()
    {
        using var process = new SimpleProcess(new SimpleProcessStartInfo("<not existing>"));
        process.Invoking(process => process.Run()).Should().Throw<Win32Exception>();
    }
}
