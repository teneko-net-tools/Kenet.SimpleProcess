﻿using FluentAssertions;

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
        public async Task Two_execution_run_to_completion_should_pass()
        {
            using var execution = new ProcessExecutorBuilder(CreateBadExitCodeLeadingProcessStartInfo()).Run();
            (await execution.RunToCompletionAsync()).Should().NotBe(0);
            (await execution.RunToCompletionAsync()).Should().NotBe(0);
        }

        [Fact]
        public async Task Two_default_execution_run_to_completion_should_pass()
        {
            using var execution = ProcessExecutorBuilder.CreateDefault(CreateBadExitCodeLeadingProcessStartInfo()).Run();

            await execution.Awaiting(x => x.RunToCompletionAsync())
                 .Should().ThrowAsync<BadExitCodeException>();

            await execution.Awaiting(x => x.RunToCompletionAsync())
                 .Should().ThrowAsync<BadExitCodeException>();
        }

        [Fact]
        public async Task Two_default_execution_run_to_completion_should_throw_bad_exit_code_then_throw_invalid_operation()
        {
            using var execution = ProcessExecutorBuilder.CreateDefault(CreateBadExitCodeLeadingProcessStartInfo()).Run();

            await execution.Awaiting(x => x.RunToCompletionAsync(ProcessCompletionOptions.DisposeOnFailure))
                .Should().ThrowAsync<BadExitCodeException>();

            await execution.Awaiting(x => x.RunToCompletionAsync())
                .Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Two_execution_run_to_completion_should_return_exit_code_then_throw_invalid_operation()
        {
            using var execution = new ProcessExecutorBuilder(CreateBadExitCodeLeadingProcessStartInfo()).Run();
            (await execution.RunToCompletionAsync(ProcessCompletionOptions.DisposeOnCompleted)).Should().NotBe(0);

            await execution.Awaiting(x => x.RunToCompletionAsync())
                .Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Two_process_run_to_completion_should_return_exit_code_then_throw_invalid_operation()
        {
            using var process = new SimpleProcess(CreateBadExitCodeLeadingProcessStartInfo());
            (await process.RunToCompletionAsync(ProcessCompletionOptions.DisposeOnCompleted)).Should().NotBe(0);

            await process.Awaiting(x => x.RunToCompletionAsync())
                .Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
