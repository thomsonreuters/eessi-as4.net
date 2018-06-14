using System.Collections;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Strategies.Sender;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Deliver
{
    public class DeliverRetryData : IEnumerable<object[]>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[]
            {
                new DeliverRetry(
                    currentRetryCount: 1,
                    maxRetryCount: 3,
                    sendResult: SendResult.Success,
                    expectedCurrentRetryCount: 1,
                    expectedOperation: Operation.Delivered,
                    expectedStatus: InStatus.Delivered)
            },
            new object[]
            {
                new DeliverRetry(
                    currentRetryCount: 1,
                    maxRetryCount: 3,
                    sendResult: SendResult.RetryableFail,
                    expectedCurrentRetryCount: 2,
                    expectedOperation: Operation.ToBeRetried,
                    expectedStatus: InStatus.Received)
            },
            new object[]
            {
                new DeliverRetry(
                    currentRetryCount: 1,
                    maxRetryCount: 3,
                    sendResult: SendResult.FatalFail,
                    expectedCurrentRetryCount: 1,
                    expectedOperation: Operation.DeadLettered,
                    expectedStatus: InStatus.Exception)
            },
            new object[]
            {
                new DeliverRetry(
                    currentRetryCount: 3,
                    maxRetryCount: 3,
                    sendResult: SendResult.FatalFail,
                    expectedCurrentRetryCount: 3,
                    expectedOperation: Operation.DeadLettered,
                    expectedStatus: InStatus.Exception)
            }
        };

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<object[]> GetEnumerator()
        {
            return Inputs.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class DeliverRetry
    {
        public DeliverRetry(
            int currentRetryCount,
            int maxRetryCount,
            SendResult sendResult,
            int expectedCurrentRetryCount,
            Operation expectedOperation,
            InStatus expectedStatus)
        {
            CurrentRetryCount = currentRetryCount;
            MaxRetryCount = maxRetryCount;
            SendResult = sendResult;
            ExpectedCurrentRetryCount = expectedCurrentRetryCount;
            ExpectedOperation = expectedOperation;
            ExpectedStatus = expectedStatus;
        }

        public int CurrentRetryCount { get; }

        public int MaxRetryCount { get; }

        public SendResult SendResult { get; }

        public int ExpectedCurrentRetryCount { get; }

        public Operation ExpectedOperation { get; }

        public InStatus ExpectedStatus { get; }
    }
}