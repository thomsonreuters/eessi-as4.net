using System;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions.Handlers;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Services.Journal;
using Eu.EDelivery.AS4.Steps;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps
{
    public class GivenStepExecutionerFacts
    {
        [Fact]
        public async Task Collects_Journals_Throughout_Execution()
        {
            // Arrange
            var sut = new StepExecutioner(
                new StepConfiguration
                {
                    NormalPipeline = new[]
                    {
                        new Step { Type = typeof(AddJournalLogEntryStep1).AssemblyQualifiedName },
                        new Step { Type = typeof(AddJournalLogEntryStep2).AssemblyQualifiedName },
                        new Step() { Type = typeof(AddJournalLogEntryReceiptStep1).AssemblyQualifiedName },
                        new Step { Type = typeof(AddJournalLogEntryStep3).AssemblyQualifiedName },
                    },
                    ErrorPipeline = new[]
                    {
                        new Step { Type = typeof(AddJournalLogEntryStep4).AssemblyQualifiedName },
                    }
                },
                new LogExceptionHandler());

            var userMessage = new UserMessage($"user-{Guid.NewGuid()}");

            // Act
            StepResult result = 
                await sut.ExecuteStepsAsync(
                    new MessagingContext(
                        AS4Message.Create(userMessage), 
                        MessagingContextMode.Unknown));

            // Assert
            Assert.Collection(
                result.Journal.First(j => j.EbmsMessageId == userMessage.MessageId).LogEntries,
                e => Assert.Equal("Log entry 1", e),
                e => Assert.Equal("Log entry 2", e),
                e => Assert.Equal("Log entry 3", e),
                e => Assert.Equal("Log entry 4", e));

            Assert.Collection(
                result.Journal.First(j => j.RefToMessageId == userMessage.MessageId).LogEntries,
                e => Assert.Equal("Log entry 1", e));
        }
    }

    public class AddJournalLogEntryStep1 : IStep
    {
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            return StepResult
                .Success(messagingContext)
                .WithJournalAsync(
                    JournalLogEntry.CreateFrom(
                        messagingContext.AS4Message,
                        "Log entry 1"));
        }
    }

    public class AddJournalLogEntryStep2 : IStep
    {
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            return StepResult
                   .Success(messagingContext)
                   .WithJournalAsync(
                       JournalLogEntry.CreateFrom(
                           messagingContext.AS4Message,
                           "Log entry 2"));
        }
    }

    public class AddJournalLogEntryStep3 : IStep
    {
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            return StepResult
                   .Failed(messagingContext)
                   .WithJournalAsync(
                       JournalLogEntry.CreateFrom(
                           messagingContext.AS4Message,
                           "Log entry 3"));
        }
    }

    public class AddJournalLogEntryStep4 : IStep
    {
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            return StepResult
                   .Success(messagingContext)
                   .WithJournalAsync(
                       JournalLogEntry.CreateFrom(
                           messagingContext.AS4Message,
                           "Log entry 4"));
        }
    }

    public class AddJournalLogEntryReceiptStep1 : IStep
    {
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            return StepResult
                   .Success(messagingContext)
                   .WithJournalAsync(
                       JournalLogEntry.CreateFrom(
                           AS4Message.Create(new Receipt(
                               $"receipt-{Guid.NewGuid()}", 
                               messagingContext.AS4Message.PrimaryMessageUnit.MessageId)), 
                           "Log entry 1"));
        }
    }
}
