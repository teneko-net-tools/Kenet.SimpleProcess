using FluentAssertions;

namespace Kenet.SimpleProcess.Test
{
    public class BadExitCodeTests
    {
        private static SimpleProcessStartInfo CreateBadExitCodeLeadingProcessStartInfo() =>
            new("git")
            {
                Arguments = "<garbage>",
                WorkingDirectory = AppContext.BaseDirectory
            };

        [Fact]
        public void Run_to_completion_should_throw_bad_exit_code()
        {
            ProcessExecutorBuilder.CreateDefault(CreateBadExitCodeLeadingProcessStartInfo())
                .Invoking(builder => builder.RunToCompletion())
                .Should().Throw<BadExitCodeException>();
        }

        [Fact]
        public void Two_execution_run_to_completion_should_pass()
        {
            using var execution = new ProcessExecutorBuilder(CreateBadExitCodeLeadingProcessStartInfo()).Run();
            execution.RunToCompletionAsync().Should().NotBe(0);
            execution.RunToCompletionAsync().Should().NotBe(0);
        }

        [Fact]
        public void Two_default_execution_run_to_completion_should_pass()
        {
            using var execution = ProcessExecutorBuilder.CreateDefault(CreateBadExitCodeLeadingProcessStartInfo()).Run();

            execution.Invoking(x => x.RunToCompletionAsync())
                 .Should().ThrowAsync<BadExitCodeException>();

            execution.Invoking(x => x.RunToCompletionAsync())
                 .Should().ThrowAsync<BadExitCodeException>();
        }

        [Fact]
        public void Two_default_execution_run_to_completion_should_throw_bad_exit_code_then_throw_invalid_operation()
        {
            ProcessExecution execution = ProcessExecutorBuilder.CreateDefault(CreateBadExitCodeLeadingProcessStartInfo()).Build().Run();

            execution.Invoking(x => x.RunToCompletion(ProcessCompletionOptions.DisposeOnFailure))
                .Should().Throw<BadExitCodeException>();

            execution.Invoking(x => x.RunToCompletion())
                .Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Two_default_execution_run_to_completion_should_return_exit_code_then_throw_invalid_operation()
        {
            ProcessExecution execution = new ProcessExecutorBuilder(CreateBadExitCodeLeadingProcessStartInfo()).Build().Run();
            execution.RunToCompletion(ProcessCompletionOptions.DisposeOnCompleted).Should().NotBe(0);

            execution.Invoking(x => x.RunToCompletion())
                .Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Two_process_run_to_completion_should_return_exit_code_then_throw_invalid_operation()
        {
            SimpleProcess process = new(CreateBadExitCodeLeadingProcessStartInfo());
            process.RunToCompletion(ProcessCompletionOptions.DisposeOnCompleted).Should().NotBe(0);

            process.Invoking(x => x.RunToCompletion())
                .Should().Throw<InvalidOperationException>();
        }
    }
}
