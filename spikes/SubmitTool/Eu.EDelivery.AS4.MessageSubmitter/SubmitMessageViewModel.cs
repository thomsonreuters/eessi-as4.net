using System.Collections.ObjectModel;
using Eu.EDelivery.AS4.Model.PMode;

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
        
        public SendingProcessingMode SendingPMode { get; set; }

        public ObservableCollection<PayloadInfoViewModel> PayloadInformation { get; }

        public int NumberOfSubmitMessages { get; set; }

        public string SubmitLocation { get; set; }
    }

    internal class PayloadInfoViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadInfoViewModel"/> class.
        /// </summary>
        public PayloadInfoViewModel()
        {
                
        }

        public string FileName { get; set; }
        public bool IncludeSEDPartType { get; set; }
    }
}
