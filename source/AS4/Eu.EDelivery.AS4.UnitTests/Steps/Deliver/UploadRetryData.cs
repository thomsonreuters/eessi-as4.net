using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.Strategies.Uploader;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Deliver
{
    public class UploadRetryData : IEnumerable<object[]>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[]
            {
                new UploadRetry(
                    currentRetryCount: 1,
                    maxRetryCount: 3,
                    uploadResult: UploadResult.SuccessWithIdAndUrl("", ""),
                    expectedCurrentRetryCount: 1,
                    expectedOperation: Operation.Delivering,
                    expectedStatus: InStatus.Received),
            },
            new object[]
            {
                new UploadRetry(
                    currentRetryCount: 1,
                    maxRetryCount: 3,
                    uploadResult: UploadResult.RetryableFail,
                    expectedCurrentRetryCount: 2,
                    expectedOperation: Operation.ToBeDelivered,
                    expectedStatus: InStatus.Received)
            },
            new object[]
            {
                new UploadRetry(
                    currentRetryCount: 1,
                    maxRetryCount: 3,
                    uploadResult: UploadResult.FatalFail,
                    expectedCurrentRetryCount: 1,
                    expectedOperation: Operation.DeadLettered,
                    expectedStatus: InStatus.Exception)
            },
            new object[]
            {
                new UploadRetry(
                    currentRetryCount: 3,
                    maxRetryCount: 3,
                    uploadResult: UploadResult.RetryableFail,
                    expectedCurrentRetryCount: 3,
                    expectedOperation: Operation.DeadLettered,
                    expectedStatus: InStatus.Exception)
            },
            new object[]
            {
                new UploadRetry(
                    currentRetryCount: 3,
                    maxRetryCount: 3,
                    uploadResult: UploadResult.FatalFail,
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

    public class UploadRetry
    {
        public UploadRetry(
            int currentRetryCount,
            int maxRetryCount,
            UploadResult uploadResult,
            int expectedCurrentRetryCount,
            Operation expectedOperation,
            InStatus expectedStatus)
        {
            CurrentRetryCount = currentRetryCount;
            MaxRetryCount = maxRetryCount;
            UploadResult = uploadResult;
            ExpectedCurrentRetryCount = expectedCurrentRetryCount;
            ExpectedOperation = expectedOperation;
            ExpectedStatus = expectedStatus;
        }

        public int CurrentRetryCount { get; }

        public int MaxRetryCount { get; }

        public UploadResult UploadResult { get; }

        public int ExpectedCurrentRetryCount { get; }

        public Operation ExpectedOperation { get; }

        public InStatus ExpectedStatus { get; }
    }
}
