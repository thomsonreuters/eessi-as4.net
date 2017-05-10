using System.Collections.ObjectModel;

namespace Eu.EDelivery.AS4.MessageSubmitter
{
    internal class SubmitMessageViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitMessageViewModel"/> class.
        /// </summary>
        public SubmitMessageViewModel()
        {
             PayloadInformation = new ObservableCollection<PayloadInfoViewModel>();
            NumberOfSubmitMessages = 1;
        }

        public string SendingProcessingModeName { get; set; }
        public ObservableCollection<PayloadInfoViewModel> PayloadInformation { get; }

        public int NumberOfSubmitMessages { get; set; }

        public string SubmitLocation { get; set; }
    }

    internal class PayloadInfoViewModel
    {
        public string FileName { get; set; }
    }
}
