using FluentAssertions;

namespace Kenet.SimpleProcess.Test
{
    public class AsyncBadExitCodeTests
    {
        private static SimpleProcessStartInfo CreateBadExitCodeLeadingProcessStartInfo() =>
            new("git")
            {
                Arguments = "<garbage>",
                WorkingDirectory = AppContext.BaseDirectory
            };

        [Fact]
        public async Task Run_to_completion_should_throw_bad_exit_code()
        {
            await ProcessExecutorBuilder.CreateDefault(CreateBadExitCodeLeadingProcessStartInfo())
                .Awaiting(builder => builder.RunToCompletionAsync())
                .Should().ThrowAsync<BadExitCodeException>();
        }

        [Fact]
        public async Task Second_default_execution_run_to_completion_should_throw_bad_exit_code_then_throw_invalid_operation()
        {
            ProcessExecution execution = ProcessExecutorBuilder.CreateDefault(CreateBadExitCodeLeadingProcessStartInfo()).Build().Run();

            await execution.Awaiting(x => x.RunToCompletionAsync())
                 .Should().ThrowAsync<BadExitCodeException>();

            await execution.Invoking(x => x.RunToCompletionAsync())
                .Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Second_default_execution_run_to_completion_should_return_exit_code_then_throw_invalid_operation()
        {
            ProcessExecution execution = new ProcessExecutorBuilder(CreateBadExitCodeLeadingProcessStartInfo()).Build().Run();
            (await execution.RunToCompletionAsync(ProcessCompletionOptions.DisposeOnCompleted)).Should().NotBe(0);

            await execution.Awaiting(x => x.RunToCompletionAsync())
                .Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Second_process_run_to_completion_should_return_exit_code_then_throw_invalid_operation()
        {
            SimpleProcess process = new(CreateBadExitCodeLeadingProcessStartInfo());
            (await process.RunToCompletionAsync(ProcessCompletionOptions.DisposeOnCompleted)).Should().NotBe(0);

            await process.Awaiting(x => x.RunToCompletionAsync())
                .Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
