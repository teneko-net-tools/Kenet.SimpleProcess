using FluentAssertions;

namespace Kenet.SimpleProcess.Test
{
    public class BadExitCodeTests
    {
        private static SimpleProcessStartInfo CreateBadExitCodeLeadingProcessStartInfo() =>
            new("git") {
                Arguments = "<garbage>",
                WorkingDirectory = AppContext.BaseDirectory
            };

        [Theory]
        [InlineData(new object[] { true })]
        [InlineData(new object[] { false })]
        public async Task Run_to_completion_should_throw_bad_exit_code_with_non_null_original_message(bool synchronously)
        {
            var builder = ProcessExecutorBuilder.CreateDefault(CreateBadExitCodeLeadingProcessStartInfo());
            BadExitCodeException error;

            if (synchronously) {
                error = builder.Invoking(builder => builder.RunToCompletion()).Should().Throw<BadExitCodeException>().And;
            } else {
                error = (await builder.Awaiting(builder => builder.RunToCompletionAsync()).Should().ThrowAsync<BadExitCodeException>()).And;
            }

            error.ProcessOutput.Should().NotBeNull();
        }

        [Theory]
        [InlineData(new object[] { true })]
        [InlineData(new object[] { false })]
        public async Task Run_to_completion_should_throw_bad_exit_code_with_null_original_message(bool synchronously)
        {
            var builder = new ProcessExecutorBuilder(CreateBadExitCodeLeadingProcessStartInfo()).WithExitCode(0);
            BadExitCodeException error;

            if (synchronously) {
                error = builder.Invoking(builder => builder.RunToCompletion()).Should().Throw<BadExitCodeException>().And;
            } else {
                error = (await builder.Awaiting(builder => builder.RunToCompletionAsync()).Should().ThrowAsync<BadExitCodeException>()).And;
            }

            error.ProcessOutput.Should().BeNull();
        }

        [Theory]
        [InlineData(new object[] { true })]
        [InlineData(new object[] { false })]
        public async Task Two_execution_run_to_completion_should_pass(bool synchronously)
        {
            using var execution = new ProcessExecutorBuilder(CreateBadExitCodeLeadingProcessStartInfo()).Run();

            if (synchronously) {
                execution.RunToCompletion().Should().NotBe(0);
            } else {
                (await execution.RunToCompletionAsync()).Should().NotBe(0);
            }

            execution.RunToCompletion().Should().NotBe(0);
        }

        [Theory]
        [InlineData(new object[] { true })]
        [InlineData(new object[] { false })]
        public async Task Two_default_execution_run_to_completion_should_pass(bool synchronously)
        {
            using var execution = ProcessExecutorBuilder.CreateDefault(CreateBadExitCodeLeadingProcessStartInfo()).Run();

            if (synchronously) {
                execution.Invoking(x => x.RunToCompletion()).Should().Throw<BadExitCodeException>();
            } else {
                await execution.Awaiting(x => x.RunToCompletionAsync()).Should().ThrowAsync<BadExitCodeException>();
            }

            if (synchronously) {
                execution.Invoking(x => x.RunToCompletion()).Should().Throw<BadExitCodeException>();
            } else {
                await execution.Awaiting(x => x.RunToCompletionAsync()).Should().ThrowAsync<BadExitCodeException>();
            }
        }

        [Theory]
        [InlineData(new object[] { true })]
        [InlineData(new object[] { false })]
        public async Task Two_default_execution_run_to_completion_should_throw_bad_exit_code_then_throw_invalid_operation(bool synchronously)
        {
            var execution = ProcessExecutorBuilder.CreateDefault(CreateBadExitCodeLeadingProcessStartInfo()).Run();

            {
                using var _ = execution;

                if (synchronously) {
                    execution.Invoking(x => x.RunToCompletion()).Should().Throw<BadExitCodeException>();
                } else {
                    await execution.Awaiting(x => x.RunToCompletionAsync()).Should().ThrowAsync<BadExitCodeException>();
                }
            }

            if (synchronously) {
                execution.Invoking(x => x.RunToCompletion()).Should().Throw<InvalidOperationException>();
            } else {
                await execution.Awaiting(x => x.RunToCompletionAsync()).Should().ThrowAsync<InvalidOperationException>();
            }
        }

        [Theory]
        [InlineData(new object[] { true })]
        [InlineData(new object[] { false })]
        public async Task Two_default_execution_run_to_completion_should_return_exit_code_then_throw_invalid_operation(bool synchronously)
        {
            var execution = new ProcessExecutorBuilder(CreateBadExitCodeLeadingProcessStartInfo()).Run();

            {
                using var _ = execution;

                if (synchronously) {
                    execution.RunToCompletion().Should().NotBe(0);
                } else {
                    (await execution.RunToCompletionAsync()).Should().NotBe(0);
                }
            }

            if (synchronously) {
                execution.Invoking(x => x.RunToCompletion()).Should().Throw<InvalidOperationException>();
            } else {
                await execution.Awaiting(x => x.RunToCompletionAsync()).Should().ThrowAsync<InvalidOperationException>();
            }
        }

        [Theory]
        [InlineData(new object[] { true })]
        [InlineData(new object[] { false })]
        public async Task Two_process_run_to_completion_should_return_exit_code_then_throw_invalid_operation(bool synchronously)
        {
            var process = new SimpleProcess(CreateBadExitCodeLeadingProcessStartInfo());

            {
                using var _ = process;

                if (synchronously) {
                    process.RunToCompletion().Should().NotBe(0);
                } else {
                    (await process.RunToCompletionAsync()).Should().NotBe(0);
                }
            }

            if (synchronously) {
                process.Invoking(x => x.RunToCompletion()).Should().Throw<InvalidOperationException>();
            } else {
                await process.Awaiting(x => x.RunToCompletionAsync()).Should().ThrowAsync<InvalidOperationException>();
            }
        }
    }
}
