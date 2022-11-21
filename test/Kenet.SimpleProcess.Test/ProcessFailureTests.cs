using FluentAssertions;
using System.ComponentModel;

namespace Kenet.SimpleProcess.Test;

public class ProcessFailureTests
{
    [Fact]
    public void Process_start_should_throw_because_inexisting_executable()
    {
        new SimpleProcess(new SimpleProcessStartInfo("<not existing>"))
            .Invoking(process => process.Run())
            .Should().Throw<Win32Exception>();
    }
}
